using System;
using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Models;

namespace QA.Engine.Administration.Services.Core
{
    public class SiteMapService : ISiteMapService
    {
        private readonly ISiteMapProvider _siteMapProvider;
        private readonly IWidgetProvider _widgetProvider;
        private readonly IItemDifinitionProvider _itemDifinitionProvider;
        private readonly IStatusTypeProvider _statusTypeProvider;
        private readonly IQpDataProvider _qpDataProvider;
        private readonly IMapper _mapper;

        public SiteMapService(
            ISiteMapProvider siteMapProvider, IWidgetProvider widgetProvider, IItemDifinitionProvider itemDifinitionProvider, 
            IQpDataProvider qpDataProvider, IStatusTypeProvider statusTypeProvider,
            IMapper mapper)
        {
            _siteMapProvider = siteMapProvider;
            _widgetProvider = widgetProvider;
            _itemDifinitionProvider = itemDifinitionProvider;
            _statusTypeProvider = statusTypeProvider;
            _qpDataProvider = qpDataProvider;
            _mapper = mapper;
        }

        public List<SiteTreeModel> GetSiteMapItems(int siteId, bool isArchive, int? parentId)
        {
            var items = _siteMapProvider.GetItems(siteId, isArchive, parentId.HasValue ? new[] { parentId.Value } : null)
                .Select(x => _mapper.Map<SiteTreeModel>(x))
                .ToList();
            var children = _siteMapProvider.GetItems(siteId, isArchive, items.Select(x => x.Id))
                .Select(x => _mapper.Map<SiteTreeModel>(x))
                .ToList();
            var discriminators = _itemDifinitionProvider.GetAllItemDefinitions(siteId)
                .Select(x => _mapper.Map<DiscriminatorModel>(x))
                .ToList();

            foreach (var item in items)
            {
                item.Children = children.Where(x => x.ParentId == item.Id).ToList();
                item.Discriminator = discriminators.FirstOrDefault(x => x.Id == item.DiscriminatorId);
                item.Children.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
            }

            return items;
        }

        public List<WidgetTreeModel> GetWidgetItems(int siteId, bool isArchive, int parentId)
        {
            var items = _widgetProvider.GetItems(siteId, isArchive, new[] { parentId })
                .Select(x => _mapper.Map<WidgetTreeModel>(x))
                .ToList();
            var children = _widgetProvider.GetItems(siteId, isArchive, items.Select(x => x.Id))
                .Select(x => _mapper.Map<WidgetTreeModel>(x))
                .ToList();
            var discriminators = _itemDifinitionProvider.GetAllItemDefinitions(siteId)
                .Select(x => _mapper.Map<DiscriminatorModel>(x))
                .ToList();

            foreach (var item in items)
            {
                item.Children = children.Where(x => x.ParentId == item.Id).ToList();
                item.Discriminator = discriminators.FirstOrDefault(x => x.Id == item.DiscriminatorId);
                item.Children.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
            }

            return items;
        }

        public List<SiteTreeModel> GetSiteMapStructure(int siteId)
        {
            var abstractItems = _siteMapProvider.GetAllItems(siteId, false);

            var pages = abstractItems.Where(x => x.IsPage).Select(x => _mapper.Map<SiteTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = abstractItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var discriminators = _itemDifinitionProvider.GetAllItemDefinitions(siteId)
                .Select(x => _mapper.Map<DiscriminatorModel>(x))
                .ToList();

            var pageStructure = SiteMapStructureBuilder.GetPageStructure(pages, widgets, discriminators);

            return pageStructure;
        }

        public ArchiveTreeModel GetArchiveStructure(int siteId)
        {
            var abstractItems = _siteMapProvider.GetAllItems(siteId, true);

            var pages = abstractItems.Where(x => x.IsPage).Select(x => _mapper.Map<SiteTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = abstractItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var discriminators = _itemDifinitionProvider.GetAllItemDefinitions(siteId)
                .Select(x => _mapper.Map<DiscriminatorModel>(x))
                .ToList();

            var pageStructure = SiteMapStructureBuilder.GetPageStructure(pages, widgets, discriminators);
            var widgetStructure = SiteMapStructureBuilder.GetWidgetStructure(null, widgets, discriminators);

            return new ArchiveTreeModel
            {
                Pages = pageStructure,
                Widgets = widgetStructure
            };
        }

        public void PublishSiteMapItems(int siteId, int userId, List<int> itemIds)
        {
            if (itemIds == null || !itemIds.Any())
                throw new ArgumentNullException("itemIds");
            if (itemIds.Any(x => x <= 0))
                throw new ArgumentException("itemId <= 0");

            var items = _siteMapProvider.GetByIds(siteId, false, itemIds);
            if (!items.Any())
                throw new InvalidOperationException("Элемент не найден.");

            var status = _statusTypeProvider.GetStatus(siteId, QpContentItemStatus.Published);
            var contentId = _siteMapProvider.GetContentId(siteId);

            _qpDataProvider.Publish(siteId, contentId, userId, items, status.Id);
        }

        public void ReorderSiteMapItems(int siteId, int userId, int itemId, int relatedItemId, bool isInsertBefore, int step)
        {
            if (itemId <= 0)
                throw new ArgumentException("itemId <= 0");
            if (relatedItemId <= 0)
                throw new ArgumentException("relatedItemId <= 0");

            var items = _siteMapProvider.GetByIds(siteId, false, new[] { itemId, relatedItemId });

            if (!items.Any(x => x.Id == itemId) || !items.Any(x => x.Id == relatedItemId))
                throw new InvalidOperationException("itemId or relatedItemId doesn't exist");

            var item = items.First(x => x.Id == itemId);

            var list = _siteMapProvider.GetItems(siteId, false, item.ParentId.HasValue ? new[] { item.ParentId.Value } : new int[] { });

            var result = list.Select(x => _mapper.Map<SiteTreeModel>(x)).ToList();
            result.Remove(result.SingleOrDefault(x => x.Id == itemId));
            var relatedIndex = result.IndexOf(result.SingleOrDefault(x => x.Id == relatedItemId));
            result.Insert(isInsertBefore ? relatedIndex : (relatedIndex + 1 >= result.Count ? result.Count : relatedIndex + 1), _mapper.Map<SiteTreeModel>(item));

            for (var i = 0; i < result.Count; i++)
            {
                var i1 = i;
                var listItem = list.SingleOrDefault(x => x.Id == result[i1].Id);
                listItem.IndexOrder = i * step;
            }

            var contentId = _siteMapProvider.GetContentId(siteId);

            _qpDataProvider.Reorder(siteId, contentId, userId, list);
        }

        public void MoveSiteMapItem(int siteId, int userId, int itemId, int newParentId)
        {
            if (itemId <= 0)
                throw new ArgumentException("itemId <= 0");
            if (newParentId <= 0)
                throw new ArgumentException("newParentId <= 0");

            var items = _siteMapProvider.GetByIds(siteId, false, new[] { itemId, newParentId });

            if (!items.Any(x => x.Id == itemId) || !items.Any(x => x.Id == newParentId))
                throw new InvalidOperationException("itemId or newParentId doesn't exist");

            var contentId = _siteMapProvider.GetContentId(siteId);

            _qpDataProvider.Move(siteId, contentId, userId, itemId, newParentId);
        }

        public void RemoveSiteMapItems(
            int siteId, int userId, int itemId, 
            bool isDeleteAllVersions, bool isDeleteContentVersion, int? contentVersionId)
        {
            if (itemId <= 0)
                throw new ArgumentException("itemId <= 0");
            if (!isDeleteContentVersion && contentVersionId == null)
                throw new InvalidOperationException("Не указана версия для замены.");

            var item = _siteMapProvider.GetByIds(siteId, false, new[] { itemId })?.FirstOrDefault();
            if (item == null || item.Id == 0)
                throw new InvalidOperationException("Элемент не найден.");

            if (contentVersionId.HasValue)
            {
                var contentVersion = _siteMapProvider.GetByIds(siteId, false, new[] { contentVersionId.Value })?.FirstOrDefault();
                if (contentVersion == null || contentVersion.Id == 0)
                    throw new InvalidOperationException("Контентная версия не найдена.");
            }

            var rootPageId = _siteMapProvider.GetRootPage(siteId)?.Id;
            if (itemId == rootPageId)
                throw new InvalidOperationException("Нельзя удалить главную  страницу.");

            var itemsToArchive = new List<AbstractItemData>();
            AbstractItemData moveContentVersion = null;

            var allItems = _siteMapProvider.GetAllItems(siteId, false);
            var pages = allItems.Where(x => x.IsPage).Select(x => _mapper.Map<SiteTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = allItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var pageStructure = item.IsPage ? SiteMapStructureBuilder.GetPageStructure(pages, widgets, null, itemId) : new List<SiteTreeModel>();
            var widgetStructure = !item.IsPage ? SiteMapStructureBuilder.GetWidgetStructure(null, widgets, null, itemId) : new List<WidgetTreeModel>();

            void funcWidgets(List<WidgetTreeModel> items)
            {
                foreach (var i in items)
                {
                    itemsToArchive.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (i.HasChildren)
                        funcWidgets(i.Children);
                }
            }

            void func(List<SiteTreeModel> items)
            {
                foreach (var i in items)
                {
                    itemsToArchive.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (!isDeleteAllVersions & !isDeleteContentVersion)
                    {
                        moveContentVersion = allItems.FirstOrDefault(x => x.Id == contentVersionId);
                        if(moveContentVersion != null && moveContentVersion.VersionOfId != null)
                        {
                            moveContentVersion.Alias = item.Alias;
                            moveContentVersion.ParentId = item.ParentId;
                        }
                    }
                    else
                    {
                        if (i.HasContentVersion)
                            itemsToArchive.AddRange(allItems.Where(x => i.ContentVersions.Any(y => x.Id == y.Id)));
                    }

                    if (i.HasWidgets)
                        funcWidgets(i.Widgets);

                    if (i.HasChildren)
                        func(i.Children);
                }
            }

            if (isDeleteAllVersions)
            { 
                var structuralVersions = allItems.Where(x => x.ParentId == item.ParentId && x.Alias == item.Alias && x.Id != item.Id).ToList();
                if (structuralVersions.Any())
                    itemsToArchive.AddRange(structuralVersions);
            }

            func(pageStructure);
            funcWidgets(widgetStructure);

            var contentId = _siteMapProvider.GetContentId(siteId);

            if (itemsToArchive.Any())
                _qpDataProvider.Remove(siteId, contentId, userId, itemsToArchive);
            if (moveContentVersion != null)
                _qpDataProvider.MoveUpContentVersion(siteId, contentId, userId, moveContentVersion);
        }

        public void RestoreSiteMapItems(
            int siteId, int userId, int itemId, 
            bool isRestoreAllVersions, bool isRestoreChildren, bool isRestoreContentVersions, bool isRestoreWidgets)
        {
            if (itemId <= 0)
                throw new ArgumentException("itemId <= 0");

            var item = _siteMapProvider.GetByIds(siteId, true, new[] { itemId })?.FirstOrDefault();
            if (item == null || item.Id == 0)
                throw new InvalidOperationException("Элемент не найден.");

            if (item.VersionOfId != null || item.ParentId != null)
            {
                var parent = _siteMapProvider.GetByIds(siteId, false, new int[] { item.VersionOfId ?? item.ParentId.Value }).FirstOrDefault();
                if (parent == null || parent.Id == 0)
                    throw new InvalidOperationException("Нельзя восстановить элемент без элемента-предка.");
            }

            var itemsToRestore = new List<AbstractItemData>();

            var allItems = _siteMapProvider.GetAllItems(siteId, true);
            var pages = allItems.Where(x => x.IsPage).Select(x => _mapper.Map<SiteTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = allItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var pageStructure = item.IsPage ? SiteMapStructureBuilder.GetPageStructure(pages, widgets, null, itemId) : new List<SiteTreeModel>();
            var widgetStructure = !item.IsPage ? SiteMapStructureBuilder.GetWidgetStructure(null, widgets, null, itemId) : new List<WidgetTreeModel>();

            void funcWidgets(List<WidgetTreeModel> items)
            {
                foreach (var i in items)
                {
                    itemsToRestore.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (i.HasChildren)
                        funcWidgets(i.Children);
                }
            }

            void func(List<SiteTreeModel> items)
            {
                foreach (var i in items)
                {
                    itemsToRestore.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (isRestoreContentVersions && i.HasContentVersion)
                        itemsToRestore.AddRange(allItems.Where(x => i.ContentVersions.Any(y => x.Id == y.Id)));

                    if (isRestoreWidgets && i.HasWidgets)
                        funcWidgets(i.Widgets);

                    if (isRestoreChildren && i.HasChildren)
                        func(i.Children);
                }
            }

            if (isRestoreAllVersions && item.IsPage && item.VersionOfId == null)
            {
                var structuralVersions = allItems.Where(x => x.ParentId == item.ParentId && x.Alias == item.Alias && x.Id != item.Id).ToList();
                if (structuralVersions.Any())
                    itemsToRestore.AddRange(structuralVersions);
            }

            func(pageStructure);
            funcWidgets(widgetStructure);

            var contentId = _siteMapProvider.GetContentId(siteId);

            if (itemsToRestore.Any())
                _qpDataProvider.Restore(siteId, contentId, userId, itemsToRestore);
        }

        public void DeleteSiteMapItems(
            int siteId, int userId, int itemId,
            bool isDeleteAllVersions)
        {
            if (itemId <= 0)
                throw new ArgumentException("itemId <= 0");

            var item = _siteMapProvider.GetByIds(siteId, true, new[] { itemId })?.FirstOrDefault();
            if (item == null || item.Id == 0)
                throw new InvalidOperationException("Элемент не найден.");

            var rootPageId = _siteMapProvider.GetRootPage(siteId)?.Id;
            if (itemId == rootPageId)
                throw new InvalidOperationException("Нельзя удалить главную  страницу.");

            var itemsToDelete = new List<AbstractItemData>();

            var allItems = _siteMapProvider.GetAllItems(siteId, true);
            var pages = allItems.Where(x => x.IsPage).Select(x => _mapper.Map<SiteTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = allItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var pageStructure = item.IsPage ? SiteMapStructureBuilder.GetPageStructure(pages, widgets, null, itemId) : new List<SiteTreeModel>();
            var widgetStructure = !item.IsPage ? SiteMapStructureBuilder.GetWidgetStructure(null, widgets, null, itemId) : new List<WidgetTreeModel>();

            void funcWidgets(List<WidgetTreeModel> items)
            {
                foreach (var i in items)
                {
                    itemsToDelete.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (i.HasChildren)
                        funcWidgets(i.Children);
                }
            }

            void func(List<SiteTreeModel> items)
            {
                foreach (var i in items)
                {
                    itemsToDelete.AddRange(allItems.Where(x => x.Id == i.Id));

                    if (i.HasContentVersion)
                        itemsToDelete.AddRange(allItems.Where(x => i.ContentVersions.Any(y => x.Id == y.Id)));

                    if (i.HasWidgets)
                        funcWidgets(i.Widgets);

                    if (i.HasChildren)
                        func(i.Children);
                }
            }

            if (isDeleteAllVersions)
            {
                var structuralVersions = allItems.Where(x => x.ParentId == item.ParentId && x.Alias == item.Alias && x.Id != item.Id).ToList();
                if (structuralVersions.Any())
                    itemsToDelete.AddRange(structuralVersions);
            }

            func(pageStructure);
            funcWidgets(widgetStructure);

            var contentId = _siteMapProvider.GetContentId(siteId);

            if (itemsToDelete.Any())
                _qpDataProvider.Delete(siteId, contentId, userId, itemsToDelete);
        }

    }
}
