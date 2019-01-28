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
        private readonly IQpMetadataManager _qpMetadataManager;
        private readonly IQpContentManager _qpContentManager;

        public QpDataProvider(IQpMetadataManager qpMetadataManager, IQpContentManager qpContentManager)
        {
            _qpMetadataManager = qpMetadataManager;
            _qpContentManager = qpContentManager;
        }

        public void Publish(int siteId, int contentId, IEnumerable<AbstractItemData> items, int statusId)
        {
            if (!items.Any())
                return;

            if (!items.All(x => x.ExtensionId.HasValue))
                return;

            var item = items.First();
            var siteName = _qpMetadataManager.GetSiteName(siteId);
            var statusNames = string.Join(",", new[]
                {
                                QpContentItemStatus.None,
                                QpContentItemStatus.Published,
                                QpContentItemStatus.Created,
                                QpContentItemStatus.Approved
                });
            _qpContentManager
                .Connection(_qpContentManager.DbConnection.InstanceConnectionString)
                .SiteName(siteName)
                .IsIncludeArchive(true)
                .IsShowSplittedArticle(true);

            // update content
            var contentName = _qpMetadataManager.GetContentName(contentId);
            _qpContentManager
                .StatusName(statusNames)
                .ContentId(contentId)
                .ContentName(contentName)
                .Where($"CONTENT_ITEM_ID = {item.Id}")
                .ChangeStatus(statusId);

            //update extantion
            var contentNameExtantion = _qpMetadataManager.GetContentName(contentId);
            _qpContentManager
                .StatusName(statusNames)
                .ContentId(item.ExtensionId.Value)
                .ContentName(contentNameExtantion)
                .Where($"ItemId in ({string.Join(",", items.Select(x => x.Id))})")
                .ChangeStatus(statusId);
        }
    }
}
