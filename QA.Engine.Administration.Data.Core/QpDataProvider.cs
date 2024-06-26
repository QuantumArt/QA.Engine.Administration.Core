﻿using QA.Engine.Administration.Data.Core.Qp;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Options;
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
        private readonly CustomerConfiguration _configuration;
        private readonly S3Options _s3Options;

        public QpDataProvider(IQpMetadataManager qpMetadataManager, CustomerConfiguration configuration, IOptions<S3Options> s3Options)
        {
            _qpMetadataManager = qpMetadataManager;
            _configuration = configuration;
            _s3Options = s3Options.Value;
        }

        private ArticleService GetArticleService(int userId)
        {
            var info = new QpConnectionInfo(
                _configuration.ConnectionString,
                (DatabaseType)(int)_configuration.DbType
            );
            return new ArticleService(info, userId) { S3Options = _s3Options };
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
                new ArticleData
                {
                    ContentId = contentId, Id = editData.ItemId, Fields = new List<FieldData>()
                    {
                        new() { Id = fields["Title"], Value = editData.Title },
                        new() { Id = fields["IsInSiteMap"], Value = Convert.ToInt32(editData.IsInSiteMap).ToString() }
                    }
                }
            };

            var model = new BatchUpdateModel { Articles = articleData, CreateVersions = true };
            var result = GetArticleService(userId).BatchUpdate(model);
            if (result != null && result.Type == ActionMessageType.Error)
            {
                throw new ApplicationException(result.Text);
            }
        }

        public void Publish(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items, int statusId)
        {
            var itemIds = items.Select(x => new { x.Id, x.ExtensionId }).ToArray();
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
            var data = items.Select(x => new { x.Id, x.IndexOrder }).ToArray();
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
                        new() { Id = fields["IndexOrder"], Value = n.IndexOrder.Value.ToString() },
                    }
                }).ToArray();

            var model = new BatchUpdateModel { Articles = articleData, CreateVersions = true };
            var result = GetArticleService(userId).BatchUpdate(model);
            if (result != null && result.Type == ActionMessageType.Error)
            {
                throw new ApplicationException(result.Text);
            }
        }

        public void Move(int siteId, int contentId, int userId, int itemId, int newParentId)
        {
            Logger.ForDebugEvent()
                .Message("Move")
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
                new ArticleData
                {
                    ContentId = contentId, Id = itemId, Fields = new List<FieldData>()
                    {
                        new() { Id = fields["Parent"], ArticleIds = new[] { newParentId } },
                    }
                }
            };
            var model = new BatchUpdateModel { Articles = articleData, CreateVersions = true };
            var result = GetArticleService(userId).BatchUpdate(model);
            if (result != null && result.Type == ActionMessageType.Error)
            {
                throw new ApplicationException(result.Text);
            }
        }

        public void Archive(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items,
            AbstractItemData moveContentVersion)
        {
            var itemIds = items.Select(x => new { x.Id, x.ExtensionId }).ToArray();
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
            var itemIds = items.Select(x => new { x.Id, x.ExtensionId }).ToArray();
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
            if (result != null && result.Type == ActionMessageType.Error)
            {
                throw new ApplicationException(result.Text);
            }
        }

        public List<QpFieldData> GetFields(int siteId, int contentId)
        {
            var result = _qpMetadataManager.GetRealData(
                    "CONTENT_ATTRIBUTE",
                    "ATTRIBUTE_ID, CONTENT_ID, ATTRIBUTE_NAME, NET_ATTRIBUTE_NAME",
                    $"NET_ATTRIBUTE_NAME is not null AND CONTENT_ID = {contentId}"
                )
                .AsEnumerable()
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
            if (!item.ParentId.HasValue)
            {
                Logger.ForErrorEvent()
                    .Message("item.ParentId is null in moveUpContentVersion")
                    .Property("siteId", siteId)
                    .Property("contentId", contentId)
                    .Property("userId", userId)
                    .Property("id", item.Id)
                    .Property("alias", item.Alias)
                    .Log();
                return;
            }

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
            if (!fields.ContainsKey("IndexOrder"))
                throw new Exception("NetName for field IndexOrder not found");

            var articleData = new[]
            {
                new ArticleData
                {
                    ContentId = contentId, Id = item.Id, Fields = new List<FieldData>
                    {
                        new() { Id = fields["Parent"], ArticleIds = new[] { item.ParentId.Value } },
                        new() { Id = fields["Name"], Value = item.Alias },
                        new() { Id = fields["IndexOrder"], Value = item.IndexOrder?.ToString() },
                        new() { Id = fields["VersionOf"] },
                        new() { Id = fields["IsPage"], Value = "1" },
                    }
                }
            };
            var model = new BatchUpdateModel { Articles = articleData, CreateVersions = true };
            var result = GetArticleService(userId).BatchUpdate(model);
            if (result != null && result.Type == ActionMessageType.Error)
            {
                throw new ApplicationException(result.Text);
            }
        }
    }
}