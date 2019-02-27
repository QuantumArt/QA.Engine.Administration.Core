﻿using QA.DotNetCore.Engine.Persistent.Interfaces;
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

        public void Edit(int siteId, int contentId, int userId, EditData editData)
        {
            var columnNames = GetColumnNamesByNetNames(siteId, new List<string> { nameof(editData.Title), nameof(editData.IsVisible), nameof(editData.IsInSiteMap) });
            if (!columnNames.ContainsKey(nameof(editData.Title)))
                throw new Exception("NetName для поля Title не найдено.");
            if (!columnNames.ContainsKey(nameof(editData.IsVisible)))
                throw new Exception("NetName для поля IsVisible не найдено.");
            if (!columnNames.ContainsKey(nameof(editData.IsInSiteMap)))
                throw new Exception("NetName для поля IsInSiteMap не найдено.");

            _qpDbConnector.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var value = new Dictionary<string, string>
                {
                    { ContentItemIdFieldName, editData.ItemId.ToString(CultureInfo.InvariantCulture) },
                    { columnNames[nameof(editData.Title)], editData.Title },
                    { columnNames[nameof(editData.IsVisible)], Convert.ToInt32(editData.IsVisible).ToString() },
                    { columnNames[nameof(editData.IsInSiteMap)], Convert.ToInt32(editData.IsInSiteMap).ToString() }
                };

                _qpDbConnector.DbConnector.MassUpdate(contentId, new[] { value }, userId);

                if (editData.ExtensionId.HasValue && editData.Fields.Any())
                {
                    var siteName = _qpMetadataManager.GetSiteName(siteId);
                    var extentionContent = _qpContentManager
                          .Connect()
                          .SiteName(siteName)
                          .ContentName($"content_{editData.ExtensionId.Value}_united")
                          .Fields($"{ContentItemIdFieldName}")
                          .Where($"[ItemId] = '{editData.ItemId}'")
                          .GetRealData();
                    var extensionContentId = extentionContent.PrimaryContent.Select().Select(x => x[ContentItemIdFieldName].ToString()).FirstOrDefault();

                    var extensionValue = new Dictionary<string, string>
                {
                    { ContentItemIdFieldName, extensionContentId }
                };
                    foreach (var field in editData.Fields)
                        extensionValue.Add(field.FieldName, field.Value.ToString());

                    _qpDbConnector.DbConnector.MassUpdate(editData.ExtensionId.Value, new[] { extensionValue }, userId);
                }
                _qpDbConnector.CommitTransaction();
            }
            catch
            {
                _qpDbConnector.RollbackTransaction();
                throw;
            }
        }

        public void Publish(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items, int statusId)
        {
            _qpDbConnector.BeginTransaction(IsolationLevel.Serializable);

            try
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
                        .Connect()
                        .SiteName(siteName)
                        .IsIncludeArchive(true)
                        .IsShowSplittedArticle(true)
                        .StatusName(_statusNames)
                        .ContentId(item.Key)
                        .ContentName(contentName)
                        .Where($"ItemId in ({string.Join(",", item.Select(x => x))})")
                        .ChangeStatus(userId, statusId);
                }
                _qpDbConnector.CommitTransaction();
            }
            catch
            {
                _qpDbConnector.RollbackTransaction();
                throw;
            }
        }

        public void Reorder(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items)
        {
            _qpDbConnector.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var columnName = GetColumnNameByNetName(siteId, "IndexOrder");
                if (string.IsNullOrWhiteSpace(columnName))
                    throw new Exception("NetName для поля IndexOrder не найдено.");

                var values = items
                    .Where(x => x.IndexOrder.HasValue)
                    .Select(x => new Dictionary<string, string>
                    {
                    { ContentItemIdFieldName, x.Id.ToString(CultureInfo.InvariantCulture) },
                    { columnName, x.IndexOrder.Value.ToString(CultureInfo.InvariantCulture) }
                    });

                _qpDbConnector.DbConnector.MassUpdate(contentId, values, userId);
                _qpDbConnector.CommitTransaction();
            }
            catch
            {
                _qpDbConnector.RollbackTransaction();
                throw;
            }
        }

        public void Move(int siteId, int contentId, int userId, int itemId, int newParentId)
        {
            _qpDbConnector.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var columnName = GetColumnNameByNetName(siteId, "Parent");
                if (string.IsNullOrWhiteSpace(columnName))
                    throw new Exception("NetName для поля Parent не найдено.");

                var values = new Dictionary<string, string>
                {
                    { ContentItemIdFieldName, itemId.ToString(CultureInfo.InvariantCulture) },
                    { columnName, newParentId.ToString(CultureInfo.InvariantCulture) }
                };

                _qpDbConnector.DbConnector.MassUpdate(contentId, new[] { values }, userId);
                _qpDbConnector.CommitTransaction();
            }
            catch
            {
                _qpDbConnector.RollbackTransaction();
                throw;
            }
        }

        public void Remove(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items, AbstractItemData moveContentVersion)
        {
            _qpDbConnector.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var siteName = _qpMetadataManager.GetSiteName(siteId);

                if (items.Any())
                {
                    // update content
                    var values = items.Select(x => new Dictionary<string, string>
                    {
                        { ContentItemIdFieldName, x.Id.ToString(CultureInfo.InvariantCulture) },
                        { ArchiveFieldName, "1" }
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
                            .Connect()
                            .SiteName(siteName)
                            .IsIncludeArchive(true)
                            .IsShowSplittedArticle(true)
                            .StatusName(_statusNames)
                            .ContentId(item.Key)
                            .ContentName(contentName)
                            .Where($"ItemId in ({string.Join(",", item.Select(x => x))})")
                            .Archive(userId);
                    }
                }

                if (moveContentVersion != null)
                    MoveUpContentVersion(siteId, contentId, userId, moveContentVersion, siteName);

                _qpDbConnector.CommitTransaction();
            }
            catch
            {
                _qpDbConnector.RollbackTransaction();
                throw;
            }
        }

        public void Restore(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items)
        {
            _qpDbConnector.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var siteName = _qpMetadataManager.GetSiteName(siteId);

                // update content
                var values = items.Select(x => new Dictionary<string, string>
                {
                    { ContentItemIdFieldName, x.Id.ToString(CultureInfo.InvariantCulture) },
                    { ArchiveFieldName, "0" }
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
                        .Connect()
                        .SiteName(siteName)
                        .IsIncludeArchive(true)
                        .IsShowSplittedArticle(true)
                        .StatusName(_statusNames)
                        .ContentId(item.Key)
                        .ContentName(contentName)
                        .Where($"ItemId in ({string.Join(",", item.Select(x => x))}) AND Archive = 1")
                        .Restore(userId);
                }
                _qpDbConnector.CommitTransaction();
            }
            catch
            {
                _qpDbConnector.RollbackTransaction();
                throw;
            }
        }

        public void Delete(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items)
        {
            _qpDbConnector.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var siteName = _qpMetadataManager.GetSiteName(siteId);

                // update content
                var contentName = _qpMetadataManager.GetContentName(contentId);
                _qpContentManager
                    .Connect()
                    .SiteName(siteName)
                    .IsIncludeArchive(true)
                    .IsShowSplittedArticle(true)
                    .StatusName(_statusNames)
                    .ContentId(contentId)
                    .ContentName(contentName)
                    .Where($"{ContentItemIdFieldName} in ({string.Join(",", items.Select(x => x.Id))}) AND Archive = 1")
                    .Delete(userId);

                //update extantion
                var extantionValues = items
                    .Where(x => x.ExtensionId.HasValue)
                    .GroupBy(x => x.ExtensionId.Value, x => x.Id);
                foreach (var item in extantionValues)
                {
                    var contentExtantionName = _qpMetadataManager.GetContentName(item.Key);
                    _qpContentManager
                        .Connect()
                        .SiteName(siteName)
                        .IsIncludeArchive(true)
                        .IsShowSplittedArticle(true)
                        .StatusName(_statusNames)
                        .ContentId(item.Key)
                        .ContentName(contentExtantionName)
                        .Where($"ItemId in ({string.Join(",", item.Select(x => x))})")
                        .Delete(userId);
                }
                _qpDbConnector.CommitTransaction();
            }
            catch (Exception e)
            {
                _qpDbConnector.RollbackTransaction();
                throw e;
            }
        }

        private void MoveUpContentVersion(int siteId, int contentId, int userId, AbstractItemData item, string siteName)
        {
            var columnNames = GetColumnNamesByNetNames(siteId, new List<string> { "Name", "Parent", "VersionOf", "IsPage" });
            if (!columnNames.ContainsKey("Name"))
                throw new Exception("NetName для поля Name не найдено.");
            if (!columnNames.ContainsKey("Parent"))
                throw new Exception("NetName для поля Parent не найдено.");
            if (!columnNames.ContainsKey("VersionOf"))
                throw new Exception("NetName для поля VersionOf не найдено.");
            if (!columnNames.ContainsKey("IsPage"))
                throw new Exception("NetName для поля IsPage не найдено.");

            var value = new Dictionary<string, string> { { ContentItemIdFieldName, item.Id.ToString(CultureInfo.InvariantCulture) } };
            foreach (var x in columnNames)
            {
                switch (x.Key)
                {
                    case "Name":
                        value.Add(x.Value, item.Alias);
                        break;
                    case "Parent":
                        value.Add(x.Value, item.ParentId.ToString());
                        break;
                    case "VersionOf":
                        value.Add(x.Value, null);
                        break;
                    case "IsPage":
                        value.Add(x.Value, "1");
                        break;
                    default:
                        break;
                }
            }
            _qpDbConnector.DbConnector.MassUpdate(contentId, new[] { value }, userId);
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

        private Dictionary<string, string> GetColumnNamesByNetNames(int siteId, List<string> columnNetNames)
        {
            var contentMetaInfo = _metaInfoRepository.GetContent("QPAbstractItem", siteId);
            if (contentMetaInfo == null)
            {
                throw new Exception($"Content with netname 'QPAbstractItem' was not found for site {siteId}");
            }
            var contentAttribute = contentMetaInfo.ContentAttributes.Where(ca => columnNetNames.Contains(ca.NetName)).ToDictionary(k => k.NetName, v => v.ColumnName);
            if (contentAttribute.Count() != columnNetNames.Count())
            {
                throw new Exception($"One of the content attribute with netnames '{string.Join(",", columnNetNames)}' was not found for table 'QPAbstractItem' and site {siteId}");
            }
            return contentAttribute;
        }
    }
}
