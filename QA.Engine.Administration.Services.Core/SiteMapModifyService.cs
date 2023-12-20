using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core
{
    public class SiteMapModifyService : ISiteMapModifyService
    {
        private readonly ISiteMapProvider _siteMapProvider;
        private readonly IDictionaryProvider _dictionaryProvider;
        private readonly IQpDataProvider _qpDataProvider;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IMapper _mapper;

        public SiteMapModifyService(
            ISiteMapProvider siteMapProvider, IDictionaryProvider dictionaryProvider,
            IQpDataProvider qpDataProvider, ISettingsProvider settingsProvider, IMapper mapper)
        {
            _siteMapProvider = siteMapProvider;
            _dictionaryProvider = dictionaryProvider;
            _qpDataProvider = qpDataProvider;
            _settingsProvider = settingsProvider;
            _mapper = mapper;
        }

        public void EditSiteMapItem(int siteId, int userId, EditModel editModel)
        {
            if (editModel.ItemId <= 0)
            {
                throw new ArgumentException("itemId <= 0");
            }

            var item = _siteMapProvider.GetByIds(siteId, false, new[] { editModel.ItemId })?.FirstOrDefault();
            if (item == null)
            {
                throw new InvalidOperationException($"Element {editModel.ItemId} not found");
            }

            var contentId = _settingsProvider.GetContentId(siteId);

            var model = _mapper.Map<EditData>(editModel);
            _qpDataProvider.Edit(siteId, contentId, userId, model);
        }

        public void PublishSiteMapItems(int siteId, int userId, List<int> itemIds)
        {
            if (itemIds == null || !itemIds.Any())
            {
                throw new ArgumentNullException(nameof(itemIds));
            }

            if (itemIds.Any(x => x <= 0))
            {
                throw new ArgumentException("itemId <= 0");
            }

            var items = _siteMapProvider.GetByIds(siteId, false, itemIds);
            if (!items.Any())
            {
                throw new InvalidOperationException($"Elements {string.Join(", ", itemIds)} not found");
            }

            var status = _dictionaryProvider.GetStatusType(siteId, QpContentItemStatus.Published);
            var contentId = _settingsProvider.GetContentId(siteId);

            _qpDataProvider.Publish(siteId, contentId, userId, items, status.Id);
        }

        public void ReorderSiteMapItems(int siteId, int userId, int itemId, int relatedItemId, bool isInsertBefore, int step)
        {
            if (itemId <= 0)
            {
                throw new ArgumentException("itemId <= 0");
            }

            if (relatedItemId <= 0)
            {
                throw new ArgumentException("relatedItemId <= 0");
            }

            var itemIds = new[] {itemId, relatedItemId};
            var items = _siteMapProvider.GetByIds(siteId, false, itemIds);

            if (items.All(x => x.Id != itemId) || items.All(x => x.Id != relatedItemId))
            {
                var message = $"itemId or relatedItemId doesn't exist (itemId={itemId}, relatedItemId={relatedItemId})";
                throw new InvalidOperationException(message);
            }

            var item = items.First(x => x.Id == itemId);

            var parentId = item.ParentId.HasValue ? new[] {item.ParentId.Value} : Array.Empty<int>();
            var list = _siteMapProvider.GetItems(siteId, false, parentId , false);

            var result = list.Select(x => _mapper.Map<PageModel>(x)).ToList();
            result.Remove(result.SingleOrDefault(x => x.Id == itemId));
            var relatedIndex = result.IndexOf(result.SingleOrDefault(x => x.Id == relatedItemId));
            var newIndex = isInsertBefore
                ? relatedIndex
                : relatedIndex + 1 >= result.Count ? result.Count : relatedIndex + 1;
            result.Insert(newIndex, _mapper.Map<PageModel>(item));

            for (var i = 0; i < result.Count; i++)
            {
                var i1 = i;
                var listItem = list.SingleOrDefault(x => x.Id == result[i1].Id);
                if (listItem != null)
                {
                    listItem.IndexOrder = i * step;
                }
            }

            var contentId = _settingsProvider.GetContentId(siteId);

            _qpDataProvider.Reorder(siteId, contentId, userId, list);
        }

        public void MoveSiteMapItem(int siteId, int userId, int itemId, int newParentId)
        {
            if (itemId <= 0)
            {
                throw new ArgumentException("itemId <= 0");
            }

            if (newParentId <= 0)
            {
                throw new ArgumentException("newParentId <= 0");
            }

            var itemIds = new[] {itemId, newParentId};
            var items = _siteMapProvider.GetByIds(siteId, false, itemIds);
            if (items.All(x => x.Id != itemId) || items.All(x => x.Id != newParentId))
            {
                var message = $"itemId or newParentId doesn't exist (itemId={itemId}, newParentId={newParentId})";
                throw new InvalidOperationException(message);
            }
            var contentId = _settingsProvider.GetContentId(siteId);
            
            _qpDataProvider.Move(siteId, contentId, userId, itemId, newParentId);
        }

        public void ArchiveSiteMapItems(
            int siteId, int userId, int itemId,
            bool isDeleteAllVersions, bool isDeleteContentVersion, int? contentVersionId)
        {
            if (itemId <= 0)
            {
                throw new ArgumentException("itemId <= 0");
            }

            if (!isDeleteContentVersion && contentVersionId == null)
            {
                throw new InvalidOperationException("Field contentVersionId is required if isDeleteContentVersion is false");
            }

            var item = _siteMapProvider.GetByIds(siteId, false, new[] { itemId })?.FirstOrDefault();
            if (item == null || item.Id == 0)
            {
                throw new InvalidOperationException($"Element {itemId} not found");
            }

            if (contentVersionId.HasValue)
            {
                var itemIds = new[] {contentVersionId.Value};
                var contentVersion = _siteMapProvider.GetByIds(siteId, false, itemIds)?.FirstOrDefault();
                if (contentVersion == null || contentVersion.Id == 0)
                {
                    throw new InvalidOperationException($"Element {contentVersionId} not found");
                }
            }

            var rootPageId = _siteMapProvider.GetRootPage(siteId)?.Id;
            if (itemId == rootPageId)
            {
                throw new InvalidOperationException("Cannot archive the root page");
            }

            var itemsToArchive = new List<AbstractItemData>();
            AbstractItemData moveContentVersion = null;

            var allItems = _siteMapProvider.GetAllItems(siteId, false, false);
            var pages = allItems
                .Where(x => x.IsPage)
                .Select(x => _mapper.Map<PageModel>(x))
                .OrderBy(x => x.IndexOrder)
                .ToList();
            var widgets = allItems
                .Where(x => !x.IsPage)
                .Select(x => _mapper.Map<WidgetModel>(x))
                .OrderBy(x => x.IndexOrder)
                .ToList();
            var pageStructure = item.IsPage ? SiteMapStructureBuilder.GetPageSubTree(itemId, pages, widgets) : new List<PageModel>();
            var widgetStructure = !item.IsPage ? SiteMapStructureBuilder.GetWidgetSubTree(itemId, widgets) : new List<WidgetModel>();

            if (isDeleteAllVersions)
            {
                var structuralVersions = allItems.Where(x => x.ParentId == item.ParentId && x.Alias == item.Alias && x.Id != item.Id).ToList();
                foreach (var structuralVersion in structuralVersions)
                {
                    if (!structuralVersion.IsPage)
                    {
                        continue;
                    }

                    var structuralVersionPageStructure = SiteMapStructureBuilder.GetPageSubTree(structuralVersion.Id, pages, widgets);
                    FuncPages(structuralVersionPageStructure);
                }
            }

            FuncPages(pageStructure);
            FuncWidgets(widgetStructure);

            var contentId = _settingsProvider.GetContentId(siteId);

            _qpDataProvider.Archive(siteId, contentId, userId, itemsToArchive, moveContentVersion);
            return;

            void FuncPages(List<PageModel> items)
            {
                foreach (var i in items)
                {
                    itemsToArchive.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (!isDeleteAllVersions & !isDeleteContentVersion)
                    {
                        moveContentVersion = allItems
                            .FirstOrDefault(x => x.Id == contentVersionId && x.VersionOfId != null);
                        if (moveContentVersion != null)
                        {
                            moveContentVersion.Alias = item.Alias;
                            moveContentVersion.ParentId = item.ParentId;
                            moveContentVersion.IndexOrder = item.IndexOrder;
                            moveContentVersion.VersionOfId = null;
                        }
                    }
                    else
                    {
                        if (i.HasContentVersion)
                        {
                            itemsToArchive.AddRange(allItems.Where(x => i.ContentVersions.Any(y => x.Id == y.Id)));
                        }
                    }

                    if (i.HasWidgets)
                    {
                        FuncWidgets(i.Widgets);
                    }

                    if (i.HasChildren)
                    {
                        FuncPages(i.Children);
                    }
                }
            }

            void FuncWidgets(List<WidgetModel> items)
            {
                foreach (var i in items)
                {
                    itemsToArchive.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (i.HasChildren)
                    {
                        FuncWidgets(i.Children);
                    }
                }
            }
        }

        public void RestoreSiteMapItems(
            int siteId, int userId, int itemId,
            bool isRestoreAllVersions, bool isRestoreChildren, bool isRestoreContentVersions, bool isRestoreWidgets)
        {
            if (itemId <= 0)
            {
                throw new ArgumentException("itemId <= 0");
            }

            var item = _siteMapProvider.GetByIds(siteId, true, new[] { itemId })?.FirstOrDefault();
            if (item == null || item.Id == 0)
            {
                throw new InvalidOperationException($"Element {itemId} not found");
            }

            if (item.VersionOfId != null || item.ParentId != null)
            {
                var parent = _siteMapProvider.GetByIds(siteId, false, new[] { item.VersionOfId ?? item.ParentId.Value }).FirstOrDefault();
                if (parent == null || parent.Id == 0)
                {
                    throw new InvalidOperationException("Cannot restore element without parent element");
                }
            }

            var itemsToRestore = new List<AbstractItemData>();

            var allItems = _siteMapProvider.GetAllItems(siteId, true, false);
            var pages = allItems.Where(x => x.IsPage).Select(x => _mapper.Map<PageModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = allItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var pageStructure = item.IsPage ? SiteMapStructureBuilder.GetPageSubTree(itemId, pages, widgets) : new List<PageModel>();
            var widgetStructure = !item.IsPage ? SiteMapStructureBuilder.GetWidgetSubTree(itemId, widgets) : new List<WidgetModel>();

            if (isRestoreAllVersions && item.IsPage && item.VersionOfId == null)
            {
                var structuralVersions = allItems.Where(x => x.ParentId == item.ParentId && x.Alias == item.Alias && x.Id != item.Id).ToList();
                foreach (var structuralVersion in structuralVersions)
                {
                    if (!structuralVersion.IsPage)
                    {
                        continue;
                    }

                    var structuralVersionPageStructure = SiteMapStructureBuilder.GetPageSubTree(structuralVersion.Id, pages, widgets);
                    FuncPages(structuralVersionPageStructure);
                }
            }

            FuncPages(pageStructure);
            FuncWidgets(widgetStructure);

            var contentId = _settingsProvider.GetContentId(siteId);

            if (itemsToRestore.Any())
            {
                _qpDataProvider.Restore(siteId, contentId, userId, itemsToRestore);
            }

            return;

            void FuncPages(List<PageModel> items)
            {
                foreach (var i in items)
                {
                    itemsToRestore.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (isRestoreContentVersions && i.HasContentVersion)
                    {
                        itemsToRestore.AddRange(allItems.Where(x => i.ContentVersions.Any(y => x.Id == y.Id)));
                    }

                    if (isRestoreWidgets && i.HasWidgets)
                    {
                        FuncWidgets(i.Widgets);
                    }

                    if (isRestoreChildren && i.HasChildren)
                    {
                        FuncPages(i.Children);
                    }
                }
            }

            void FuncWidgets(List<WidgetModel> items)
            {
                foreach (var i in items)
                {
                    itemsToRestore.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (i.HasChildren)
                    {
                        FuncWidgets(i.Children);
                    }
                }
            }
        }

        public void DeleteSiteMapItems(
            int siteId, int userId, int itemId,
            bool isDeleteAllVersions)
        {
            if (itemId <= 0)
            {
                throw new ArgumentException("itemId <= 0");
            }

            var item = _siteMapProvider.GetByIds(siteId, true, new[] { itemId })?.FirstOrDefault();
            if (item == null || item.Id == 0)
            {
                throw new InvalidOperationException($"Element {itemId} not found");
            }

            var rootPageId = _siteMapProvider.GetRootPage(siteId)?.Id;
            if (itemId == rootPageId)
            {
                throw new InvalidOperationException("Cannot delete the root page");
            }

            var itemsToDelete = new List<AbstractItemData>();

            var allItems = _siteMapProvider.GetAllItems(siteId, true, false);
            var pages = allItems.Where(x => x.IsPage).Select(x => _mapper.Map<PageModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = allItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var pageStructure = item.IsPage ? SiteMapStructureBuilder.GetPageSubTree(itemId, pages, widgets) : new List<PageModel>();
            var widgetStructure = !item.IsPage ? SiteMapStructureBuilder.GetWidgetSubTree(itemId, widgets) : new List<WidgetModel>();

            if (isDeleteAllVersions)
            {
                var structuralVersions = allItems.Where(x => x.ParentId == item.ParentId && x.Alias == item.Alias && x.Id != item.Id).ToList();
                foreach (var structuralVersion in structuralVersions)
                {
                    if (!structuralVersion.IsPage)
                    {
                        continue;
                    }

                    var structuralVersionPageStructure = SiteMapStructureBuilder.GetPageSubTree(structuralVersion.Id, pages, widgets);
                    FuncPages(structuralVersionPageStructure);
                }
            }

            FuncPages(pageStructure);
            FuncWidgets(widgetStructure);

            var contentId = _settingsProvider.GetContentId(siteId);

            if (itemsToDelete.Any())
            {
                _qpDataProvider.Delete(siteId, contentId, userId, itemsToDelete);
            }

            return;

            void FuncWidgets(List<WidgetModel> items)
            {
                foreach (var i in items)
                {
                    itemsToDelete.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (i.HasChildren)
                    {
                        FuncWidgets(i.Children);
                    }
                }
            }

            void FuncPages(List<PageModel> items)
            {
                foreach (var i in items)
                {
                    itemsToDelete.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (i.HasContentVersion)
                    {
                        itemsToDelete.AddRange(allItems.Where(x => i.ContentVersions.Any(y => x.Id == y.Id)));
                    }

                    if (i.HasWidgets)
                    {
                        FuncWidgets(i.Widgets);
                    }

                    if (i.HasChildren)
                    {
                        FuncPages(i.Children);
                    }
                }
            }
        }

    }
}
