using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Core.Qp;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace QA.Engine.Administration.Data.Core
{
    public class QpDataProvider : IQpDataProvider
    {
        private readonly IQpDbConnector _qpDbConnector;
        private readonly IQpMetadataManager _qpMetadataManager;
        private readonly IQpContentManager _qpContentManager;
        private readonly IMetaInfoRepository _metaInfoRepository;

        private readonly string _statusNames = string.Join(",", new[]
            {
                QpContentItemStatus.None,
                QpContentItemStatus.Published,
                QpContentItemStatus.Created,
                QpContentItemStatus.Approved
            });

        public QpDataProvider(
            IQpDbConnector qpDbConnector, IQpMetadataManager qpMetadataManager, IQpContentManager qpContentManager,
            IMetaInfoRepository metaInfoRepository)
        {
            _qpDbConnector = qpDbConnector;
            _qpMetadataManager = qpMetadataManager;
            _qpContentManager = qpContentManager;
            _metaInfoRepository = metaInfoRepository;
        }

        public void Publish(int siteId, int contentId, IEnumerable<AbstractItemData> items, int statusId, int userId)
        {
            if (!items.Any())
                return;

            if (!items.All(x => x.ExtensionId.HasValue))
                return;

            var item = items.First();
            var siteName = _qpMetadataManager.GetSiteName(siteId);
            var contentName = _qpMetadataManager.GetContentName(contentId);
            var contentNameExtantion = _qpMetadataManager.GetContentName(item.ExtensionId.Value);

            _qpContentManager
                .Connection(_qpDbConnector.InstanceConnectionString)
                .SiteName(siteName)
                .IsIncludeArchive(true)
                .IsShowSplittedArticle(true);

            // update content
            _qpContentManager
                .StatusName(_statusNames)
                .ContentId(contentId)
                .ContentName(contentName)
                .Where($"CONTENT_ITEM_ID = {item.Id}")
                .ChangeStatus(statusId, userId);

            //update extantion
            _qpContentManager
                .StatusName(_statusNames)
                .ContentId(item.ExtensionId.Value)
                .ContentName(contentNameExtantion)
                .Where($"ItemId in ({string.Join(",", items.Select(x => x.Id))})")
                .ChangeStatus(statusId, userId);
        }

        public void Reorder(int siteId, int contentId, IEnumerable<AbstractItemData> items, int userId)
        {
            if (!items.Any())
                return;

            var siteName = _qpMetadataManager.GetSiteName(siteId);
            var contentName = _qpMetadataManager.GetContentName(contentId);

            var columnName = GetColumnNameByNteName(siteId, "IndexOrder");

            _qpContentManager
                .Connection(_qpDbConnector.InstanceConnectionString)
                .SiteName(siteName)
                .IsIncludeArchive(true)
                .IsShowSplittedArticle(true)
                .StatusName(_statusNames)
                .ContentId(contentId)
                .ContentName(contentName)
                .Where($"CONTENT_ITEM_ID IN ({string.Join(",", items.Select(x => x.Id))})")
                .Reorder(items, columnName, userId);
        }

        private string GetColumnNameByNteName(int siteId, string columnNetName)
        {
            var contentMetaInfo = _metaInfoRepository.GetContent("QPAbstractItem", siteId);
            if (contentMetaInfo == null)
            {
                throw new Exception($"Content with netname 'QPAbstractItem' was not found for site {siteId}");
            }
            var contentAttribute = contentMetaInfo.ContentAttributes.FirstOrDefault(ca => ca.NetName == columnNetName);
            if (contentAttribute == null)
            {
                throw new Exception($"Content attribute with netname '{columnNetName}' was not found for table 'QPAbstractItem' and site {siteId}");
            }
            return contentAttribute.ColumnName;
        }
    }
}
