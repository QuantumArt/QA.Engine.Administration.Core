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

        public List<SiteTreeModel> GetSiteMapItems(int siteId, bool isStage, int? parentId)
        {
            var items = _siteMapProvider.GetItems(siteId, isStage, parentId.HasValue ? new[] { parentId.Value } : null)
                .Select(x => _mapper.Map<SiteTreeModel>(x))
                .ToList();
            var children = _siteMapProvider.GetItems(siteId, isStage, items.Select(x => x.Id))
                .Select(x => _mapper.Map<SiteTreeModel>(x))
                .ToList();
            var discriminators = _itemDifinitionProvider.GetAllItemDefinitions(siteId, isStage)
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

        public List<WidgetModel> GetWidgetItems(int siteId, bool isStage, int parentId)
        {
            var items = _widgetProvider.GetItems(siteId, isStage, new[] { parentId })
                .Select(x => _mapper.Map<WidgetModel>(x))
                .ToList();
            var children = _widgetProvider.GetItems(siteId, isStage, items.Select(x => x.Id))
                .Select(x => _mapper.Map<WidgetModel>(x))
                .ToList();
            var discriminators = _itemDifinitionProvider.GetAllItemDefinitions(siteId, isStage)
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

        public List<SiteTreeModel> GetSiteMapStructure(int siteId, bool isStage)
        {
            var abstractItems = _siteMapProvider.GetAllItems(siteId, isStage);

            var pages = abstractItems.Where(x => x.IsPage).Select(x => _mapper.Map<SiteTreeModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = abstractItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var discriminators = _itemDifinitionProvider.GetAllItemDefinitions(siteId, isStage)
                .Select(x => _mapper.Map<DiscriminatorModel>(x))
                .ToList();

            var pageStructure = GetPageStructure(pages, widgets, discriminators);

            return pageStructure;
        }

        public void PublishSiteMapItems(int siteId, bool isStage, int userId, List<int> itemIds)
        {
            if (itemIds == null || !itemIds.Any())
                throw new ArgumentNullException("itemIds");
            if (itemIds.Any(x => x <= 0))
                throw new ArgumentException("itemId <= 0");

            var items = _siteMapProvider.GetByIds(siteId, isStage, itemIds);
            if (!items.Any())
                throw new InvalidOperationException("Элемент не найден.");

            var status = _statusTypeProvider.GetStatus(siteId, isStage, QpContentItemStatus.Published);
            var contentId = _siteMapProvider.GetContentId(siteId);

            _qpDataProvider.Publish(siteId, contentId, userId, items, status.Id);
        }

        public void ReorderSiteMapItems(int siteId, bool isStage, int userId, int itemId, int relatedItemId, bool isInsertBefore, int step)
        {
            if (itemId <= 0)
                throw new ArgumentException("itemId <= 0");
            if (relatedItemId <= 0)
                throw new ArgumentException("relatedItemId <= 0");

            var items = _siteMapProvider.GetByIds(siteId, isStage, new[] { itemId, relatedItemId });

            if (!items.Any(x => x.Id == itemId) || !items.Any(x => x.Id == relatedItemId))
                throw new InvalidOperationException("itemId or relatedItemId doesn't exist");

            var item = items.First(x => x.Id == itemId);

            var list = _siteMapProvider.GetItems(siteId, isStage, item.ParentId.HasValue ? new[] { item.ParentId.Value } : new int[] { });

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

        public void MoveSiteMapItem(int siteId, bool isStage, int userId, int itemId, int newParentId)
        {
            if (itemId <= 0)
                throw new ArgumentException("itemId <= 0");
            if (newParentId <= 0)
                throw new ArgumentException("newParentId <= 0");

            var items = _siteMapProvider.GetByIds(siteId, isStage, new[] { itemId, newParentId });

            if (!items.Any(x => x.Id == itemId) || !items.Any(x => x.Id == newParentId))
                throw new InvalidOperationException("itemId or newParentId doesn't exist");

            var contentId = _siteMapProvider.GetContentId(siteId);

            _qpDataProvider.Move(siteId, contentId, userId, itemId, newParentId);
        }

        private List<SiteTreeModel> GetPageStructure(List<SiteTreeModel> pages, List<WidgetModel> widgets, List<DiscriminatorModel> discriminators)
        {
            var pageStructure = pages
                .Where(x => x.ParentId == null && x.IsPage && x.VersionOfId == null)
                .Select(x => _mapper.Map<SiteTreeModel>(x))
                .ToList();
            pageStructure.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
            pageStructure.ForEach(x => pages.Remove(x));

            var elements = pageStructure;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<SiteTreeModel>();
                foreach (var el in elements)
                {
                    el.Children = pages.Where(x => x.ParentId != null && x.ParentId == el.Id).Cast<SiteTreeModel>().ToList();
                    el.ContentVersion = pages.Where(x => x.ParentId == null && x.VersionOfId == el.Id).Cast<SiteTreeModel>().ToList();

                    if (el.HasChildren)
                    {
                        makeTree = true;
                        tmp.AddRange(el.Children);
                        el.Children.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
                        el.Children.ForEach(x => pages.Remove(x));
                    }
                    else if (el.HasContentVersion)
                    {
                        makeTree = true;
                        tmp.AddRange(el.ContentVersion);
                        el.ContentVersion.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
                        el.ContentVersion.ForEach(x => pages.Remove(x));
                    }
                    else
                        tmp.Add(el);

                    el.Widgets = GetWidgetStructure(el, widgets, discriminators);
                }
                elements = tmp;
            }

            return pageStructure;
        }

        private List<WidgetModel> GetWidgetStructure(SiteTreeModel page, List<WidgetModel> widgets, List<DiscriminatorModel> discriminators)
        {
            var widgetStructure = widgets
                .Where(x => x.ParentId == page.Id)
                .ToList();
            widgetStructure.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
            widgetStructure.ForEach(x => widgets.Remove(x));

            var elements = widgetStructure;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<WidgetModel>();
                foreach (var el in elements)
                {
                    el.Children = widgets.Where(x => !x.IsPage && x.ParentId == el.Id).ToList();

                    if (el.HasChildren)
                    {
                        makeTree = true;
                        tmp.AddRange(el.Children);
                        el.Children.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
                        el.Children.ForEach(x => widgets.Remove(x));
                    }
                    else
                        tmp.Add(el);
                }
                elements = tmp;
            }

            return widgetStructure;
        }
    }
}
