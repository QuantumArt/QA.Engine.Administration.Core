using QA.Engine.Administration.Data.Core.Qp;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using QP.ConfigurationService.Models;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using DatabaseType = Quantumart.QP8.Constants.DatabaseType;

namespace QA.Engine.Administration.Data.Core
{
    public class QpDataProvider : IQpDataProvider
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IQpMetadataManager _qpMetadataManager;
        private readonly IQpContentManager _qpContentManager;
        private readonly CustomerConfiguration _configuration;

        public QpDataProvider(IQpMetadataManager qpMetadataManager, IQpContentManager qpContentManager, CustomerConfiguration configuration)
        {
            _qpMetadataManager = qpMetadataManager;
            _qpContentManager = qpContentManager;
            _configuration = configuration;
        }

        private ArticleService GetArticleService(int userId)
        {
            var info = new QpConnectionInfo(
                _configuration.ConnectionString, 
                (DatabaseType)(int)_configuration.DbType
            );
            return new ArticleService(info, userId);
        }

        public void Edit(int siteId, int contentId, int userId, EditData editData)
        {
            Logger.ForDebugEvent()
                .Message("Edit")
                .Property("siteId", siteId)
                .Property("contentId", contentId)
                .Property("userId", userId)
                .Property("editData", editData)
                .Log();

            var fields = GetFields(siteId, contentId)
                .ToDictionary(k => k.Name, v => v.Id);
            if (!fields.ContainsKey("Title"))
                throw new Exception("NetName for field Title not found");
            if (!fields.ContainsKey("IsInSiteMap"))
                throw new Exception("NetName for field IsInSiteMap not found");

            var articleData = new[]
            {
                new ArticleData { ContentId = contentId, Id = editData.ItemId, Fields = new List<FieldData>()
                {
                    new() { Id = fields["Title"], Value = editData.Title },
                    new() { Id = fields["IsInSiteMap"], Value = Convert.ToInt32(editData.IsInSiteMap).ToString() }
                } }
            };
            GetArticleService(userId).BatchUpdate(articleData, true);
        }

        public void Publish(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items, int statusId)
        {
            var itemIds = items.Select(x => new {x.Id, x.ExtensionId}).ToArray();
            Logger.ForDebugEvent()
                .Message("Publish")
                .Property("siteId", siteId)
                .Property("contentId", contentId)
                .Property("userId", userId)
                .Property("itemIds", itemIds)
                .Log();              

            var result = GetArticleService(userId).Publish(contentId, itemIds.Select(n => n.Id).ToArray());
            if (result != null && result.Type == ActionMessageType.Error)
            {
                throw new ApplicationException(result.Text);
            }            
        }

        public void Reorder(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items)
        {
            var data = items.Select(x => new {x.Id, x.IndexOrder}).ToArray();
            Logger.ForDebugEvent()
                .Message("Reorder")
                .Property("siteId", siteId)
                .Property("contentId", contentId)
                .Property("userId", userId)
                .Property("data", data)
                .Log();               
            
            var fields = GetFields(siteId, contentId)
                .ToDictionary(k => k.Name, v => v.Id);
            if (!fields.ContainsKey("IndexOrder"))
                throw new Exception("NetName for field IndexOrder not found");
            
            var articleData = data
                .Where(x => x.IndexOrder.HasValue)
                .Select(n => new ArticleData()
                {
                    ContentId = contentId, Id = n.Id, Fields = new List<FieldData>()
                    {
                        new() {Id = fields["IndexOrder"], Value = n.IndexOrder.Value.ToString()},
                    }
                }).ToArray();
            
            GetArticleService(userId).BatchUpdate(articleData, true);
        }

        public void Move(int siteId, int contentId, int userId, int itemId, int newParentId)
        {
            Logger.ForDebugEvent()
                .Message("Edit")
                .Property("siteId", siteId)
                .Property("contentId", contentId)
                .Property("userId", userId)
                .Property("itemId", itemId)
                .Property("newParentId", newParentId)
                .Log();              

            var fields = GetFields(siteId, contentId)
                .ToDictionary(k => k.Name, v => v.Id);
            if (!fields.ContainsKey("Parent"))
                throw new Exception("NetName for field Parent not found");

            var articleData = new[]
            {
                new ArticleData { ContentId = contentId, Id = itemId, Fields = new List<FieldData>()
                {
                    new() { Id = fields["Parent"], Value = newParentId.ToString() },
                } }
            };
            GetArticleService(userId).BatchUpdate(articleData, true);
        }

        public void Archive(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items, AbstractItemData moveContentVersion)
        {
            var itemIds = items.Select(x => new {x.Id, x.ExtensionId}).ToArray();
            Logger.ForDebugEvent()
                .Message("Archive")
                .Property("siteId", siteId)
                .Property("contentId", contentId)
                .Property("userId", userId)
                .Property("itemIds", itemIds)
                .Property("moveContentVersion", moveContentVersion)
                .Log();
            
            var ids = itemIds.Select(n => n.Id).ToArray();
            var result = GetArticleService(userId).SetArchiveFlag(contentId, ids, true);
            if (result != null && result.Type == ActionMessageType.Error)
            {
                throw new ApplicationException(result.Text);
            }

            if (moveContentVersion != null)
            {
                MoveUpContentVersion(siteId, contentId, userId, moveContentVersion);
            }
        }

        public void Restore(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items)
        {
            var itemIds = items.Select(x => new {x.Id, x.ExtensionId}).ToArray();
            Logger.ForDebugEvent()
                .Message("Restore")
                .Property("siteId", siteId)
                .Property("contentId", contentId)
                .Property("userId", userId)
                .Property("itemIds", itemIds)
                .Log();
            
            var ids = itemIds.Select(n => n.Id).ToArray();
            var result = GetArticleService(userId).SetArchiveFlag(contentId, ids, false);
            if (result != null && result.Type == ActionMessageType.Error)
            {
                throw new ApplicationException(result.Text);
            }
        }

        public void Delete(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items)
        {
            var itemIds = items.Select(n => n.Id).ToArray();
            Logger.ForDebugEvent()
                .Message("Restore")
                .Property("siteId", siteId)
                .Property("contentId", contentId)
                .Property("userId", userId)
                .Property("items", itemIds)
                .Log();
            
            var result = GetArticleService(userId).Delete(contentId, itemIds);
            if (result.Type == ActionMessageType.Error)
            {
                throw new ApplicationException(result.Text);
            }
        }
        
        public List<QpFieldData> GetFields(int siteId, int contentId)
        {
            var siteName = _qpMetadataManager.GetSiteName(siteId);
            var fields = _qpContentManager
                .Connect()
                .SiteName(siteName)
                .ContentName("CONTENT_ATTRIBUTE")
                .Fields("ATTRIBUTE_ID, CONTENT_ID, ATTRIBUTE_NAME, NET_ATTRIBUTE_NAME")
                .Where($"NET_ATTRIBUTE_NAME is not null AND CONTENT_ID = {contentId}")
                .GetRealData();

            var result = fields.PrimaryContent.Select()
                .Select(x => new QpFieldData
                {
                    Id = int.Parse(x["ATTRIBUTE_ID"].ToString() ?? "0"),
                    Name = x["NET_ATTRIBUTE_NAME"].ToString()
                }).ToList();

            Logger.ForDebugEvent()
                .Message("GetFields")
                .Property("siteId", siteId)
                .Property("contentId", contentId)
                .Property("result", result)
                .Log();

            return result;
        }

        private void MoveUpContentVersion(int siteId, int contentId, int userId, AbstractItemData item)
        {
            Logger.ForDebugEvent()
                .Message("moveUpContentVersion")
                .Property("siteId", siteId)
                .Property("contentId", contentId)
                .Property("userId", userId)
                .Property("id", item.Id)
                .Property("alias", item.Alias)
                .Property("parentId", item.ParentId)
                .Log();
            
            var fields = GetFields(siteId, contentId)
                .ToDictionary(k => k.Name, v => v.Id);
            if (!fields.ContainsKey("Name"))
                throw new Exception("NetName for field Name not found");
            if (!fields.ContainsKey("Parent"))
                throw new Exception("NetName for field Parent not found");
            if (!fields.ContainsKey("VersionOf"))
                throw new Exception("NetName for field VersionOf not found");
            if (!fields.ContainsKey("IsPage"))
                throw new Exception("NetName for field IsPage not found");

            var articleData = new[]
            {
                new ArticleData { ContentId = contentId, Id = item.Id, Fields = new List<FieldData>
                {
                    new() { Id = fields["Parent"], Value = item.ParentId.ToString() },
                    new() { Id = fields["Name"], Value = item.Alias },
                    new() { Id = fields["VersionOf"], Value = null },
                    new() { Id = fields["IsPage"], Value = "1" },
                } }
            };
            GetArticleService(userId).BatchUpdate(articleData, true);
        }
    }
}
