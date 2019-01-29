using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Core.Qp;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

        protected const string ContentItemIdFieldName = "CONTENT_ITEM_ID";
        protected const string ArchiveFieldName = "ARCHIVE";
        protected const string StatusTypeIdFieldName = "STATUS_TYPE_ID";

        public QpDataProvider(
            IQpDbConnector qpDbConnector, IQpMetadataManager qpMetadataManager, IQpContentManager qpContentManager,
            IMetaInfoRepository metaInfoRepository)
        {
            _qpDbConnector = qpDbConnector;
            _qpMetadataManager = qpMetadataManager;
            _qpContentManager = qpContentManager;
            _metaInfoRepository = metaInfoRepository;
        }

        public void Publish(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items, int statusId)
        {
            var siteName = _qpMetadataManager.GetSiteName(siteId);

            // update content
            var values = items.Select(x => new Dictionary<string, string>
            {
                { ContentItemIdFieldName, x.Id.ToString(CultureInfo.InvariantCulture) },
                { StatusTypeIdFieldName, statusId.ToString(CultureInfo.InvariantCulture) }
            });
            _qpDbConnector.DbConnector.MassUpdate(contentId, values, userId);

            //update extantion
            var extantionValues = items
                .Where(x => x.ExtensionId.HasValue)
                .GroupBy(x => x.ExtensionId.Value, x => x.Id);
            foreach (var item in extantionValues)
            {
                var contentName = _qpMetadataManager.GetContentName(item.Key);
                _qpContentManager
                    .Connection(_qpDbConnector.InstanceConnectionString)
                    .SiteName(siteName)
                    .IsIncludeArchive(true)
                    .IsShowSplittedArticle(true)
                    .StatusName(_statusNames)
                    .ContentId(item.Key)
                    .ContentName(contentName)
                    .Where($"ItemId in ({string.Join(",", item.Select(x => x))})")
                    .ChangeStatus(userId, statusId);
            }
        }

        public void Reorder(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items)
        {
            var columnName = GetColumnNameByNetName(siteId, "IndexOrder");

            var values = items
                .Where(x => x.IndexOrder.HasValue)
                .Select(x => new Dictionary<string, string>
                {
                    { ContentItemIdFieldName, x.Id.ToString(CultureInfo.InvariantCulture) },
                    { columnName, x.IndexOrder.Value.ToString(CultureInfo.InvariantCulture) }
                });

            _qpDbConnector.DbConnector.MassUpdate(contentId, values, userId);
        }

        public void Move(int siteId, int contentId, int userId, int itemId, int newParentId)
        {
            var columnName = GetColumnNameByNetName(siteId, "Parent");

            var values = new Dictionary<string, string>
            {
                { ContentItemIdFieldName, itemId.ToString(CultureInfo.InvariantCulture) },
                { columnName, newParentId.ToString(CultureInfo.InvariantCulture) }
            };

            _qpDbConnector.DbConnector.MassUpdate(contentId, new[] { values }, userId);
        }

        private string GetColumnNameByNetName(int siteId, string columnNetName)
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
