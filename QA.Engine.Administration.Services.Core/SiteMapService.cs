using System;
using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Models;
using QA.Engine.Administration.Services.Core.Filters;
using System.Diagnostics;
using NLog;

namespace QA.Engine.Administration.Services.Core
{
    public class SiteMapService : ISiteMapService
    {
        private readonly ISiteMapProvider _siteMapProvider;
        private readonly IWidgetProvider _widgetProvider;
        private readonly IDictionaryProvider _dictionaryProvider;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IItemExtensionProvider _itemExtensionProvider;
        private readonly IMapper _mapper;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private const int START_PAGE_EXTENSION_ID = 547;

        public SiteMapService(
            ISiteMapProvider siteMapProvider, IWidgetProvider widgetProvider,
            IDictionaryProvider dictionaryProvider, ISettingsProvider settingsProvider,
            IItemExtensionProvider itemExtensionProvider, IMapper mapper)
        {
            _siteMapProvider = siteMapProvider;
            _widgetProvider = widgetProvider;
            _dictionaryProvider = dictionaryProvider;
            _settingsProvider = settingsProvider;
            _itemExtensionProvider = itemExtensionProvider;
            _mapper = mapper;
        }

        public List<PageModel> GetSiteMapItems(int siteId, bool isArchive, int? parentId, int[] regionIds = null,
            bool? useHierarchyRegionFilter = null)
        {
            Func<AbstractItemData, bool> regionFilter = _ => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }

            var parentFilter = parentId.HasValue ? new[] { parentId.Value } : null;
            var items = _siteMapProvider.GetItems(siteId, isArchive, parentFilter, useRegion)
                .Where(regionFilter)
                .Select(x => _mapper.Map<PageModel>(x))
                .ToList();

            var childFilter = items.Select(x => x.Id).ToArray();
            var children = _siteMapProvider.GetItems(siteId, isArchive, childFilter, useRegion)
                .Where(regionFilter)
                .Select(x => _mapper.Map<PageModel>(x))
                .ToList();

            foreach (var item in items)
                item.Children = children.Where(x => x.ParentId == item.Id).ToList();

            return items;
        }

        public List<WidgetModel> GetWidgetItems(int siteId, bool isArchive, int parentId, int[] regionIds = null,
            bool? useHierarchyRegionFilter = null)
        {
            Func<AbstractItemData, bool> regionFilter = _ => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }

            var parentFilter = new[] { parentId };
            var items = _widgetProvider.GetItems(siteId, isArchive, parentFilter)
                .Where(regionFilter)
                .Select(x => _mapper.Map<WidgetModel>(x))
                .ToList();

            var childFilter = items.Select(x => x.Id);
            var children = _widgetProvider.GetItems(siteId, isArchive, childFilter)
                .Where(regionFilter)
                .Select(x => _mapper.Map<WidgetModel>(x))
                .ToList();

            foreach (var item in items)
                item.Children = children.Where(x => x.ParentId == item.Id).ToList();

            return items;
        }

        public List<PageModel> GetSiteMapTree(int siteId, int[] regionIds = null, bool? useHierarchyRegionFilter = null)
        {
            var useRegion = _settingsProvider.HasRegion(siteId);
            _logger.ForDebugEvent().Message("GetSiteMapSubStructure")
                .Property("siteId", siteId)
                .Property("regionIds", regionIds != null ? string.Join(", ", regionIds) : "")
                .Property("useRegion", useRegion)
                .Property("useHierarchyRegionFilter", useHierarchyRegionFilter)
                .Log();

            Func<AbstractItemData, bool> regionFilter = _ => true;
            if (useRegion)
            {
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var abstractItems = _siteMapProvider.GetAllItems(siteId, false, useRegion)
                .Where(regionFilter)
                .ToList();
            stopwatch.Stop();
            _logger.ForDebugEvent().Message("GetSiteMapTree: loading all abstract items")
                .Property("elapsed", stopwatch.ElapsedMilliseconds)
                .Log();

            stopwatch.Reset();
            stopwatch.Start();

            var pages = abstractItems.Where(x => x.IsPage).Select(x => _mapper.Map<PageModel>(x))
                .OrderBy(x => x.IndexOrder).ToList();
            var widgets = abstractItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetModel>(x))
                .OrderBy(x => x.IndexOrder).ToList();

            var result = SiteMapStructureBuilder.GetPageTree(pages, widgets);

            stopwatch.Stop();
            _logger.ForDebugEvent().Message("GetSiteMapTree: convert abstract items to tree")
                .Property("elapsed", stopwatch.ElapsedMilliseconds)
                .Log();

            return result;
        }

        public PageModel GetSiteMapSubTree(int siteId, int itemId, int[] regionIds = null,
            bool? useHierarchyRegionFilter = null)
        {
            var useRegion = _settingsProvider.HasRegion(siteId);
            _logger.ForDebugEvent().Message("GetSiteMapSubTree")
                .Property("siteId", siteId)
                .Property("regionIds", regionIds != null ? string.Join(", ", regionIds) : "")
                .Property("useRegion", useRegion)
                .Property("useHierarchyRegionFilter", useHierarchyRegionFilter)
                .Log();
            Func<AbstractItemData, bool> regionFilter = _ => true;
            if (useRegion)
            {
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var ids = new[] { itemId };
            var abstractItems = _siteMapProvider.GetByIds(siteId, false, ids, useRegion, true)
                .Where(regionFilter)
                .ToList();

            stopwatch.Stop();
            _logger.ForDebugEvent().Message("GetSiteMapSubTree: loading abstract items")
                .Property("elapsed", stopwatch.ElapsedMilliseconds)
                .Log();

            stopwatch.Reset();
            stopwatch.Start();

            var pages = abstractItems.Where(x => x.IsPage).Select(x => _mapper.Map<PageModel>(x))
                .OrderBy(x => x.IndexOrder).ToList();
            var widgets = abstractItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetModel>(x))
                .OrderBy(x => x.IndexOrder).ToList();

            var result = SiteMapStructureBuilder.GetPageSubTree(itemId, pages, widgets);
            stopwatch.Stop();
            _logger.ForDebugEvent().Message("GetSiteMapSubTree: converting abstract items to tree")
                .Property("elapsed", stopwatch.ElapsedMilliseconds)
                .Log();

            return result.FirstOrDefault();
        }

        public List<ArchiveModel> GetArchiveTree(int siteId)
        {
            var useRegion = _settingsProvider.HasRegion(siteId);
            _logger.ForDebugEvent().Message("GetArchiveTree")
                .Property("siteId", siteId)
                .Property("useRegion", useRegion)
                .Log();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var abstractItems = _siteMapProvider.GetAllItems(siteId, true, useRegion);
            stopwatch.Stop();

            _logger.ForDebugEvent().Message("GetArchiveTree: loading all abstract items")
                .Property("elapsed", stopwatch.ElapsedMilliseconds)
                .Log();

            stopwatch.Reset();
            stopwatch.Start();

            var archives = _mapper.Map<List<ArchiveModel>>(abstractItems).OrderBy(x => x.IndexOrder).ToList();

            var result = SiteMapStructureBuilder.GetArchiveTree(archives);
            stopwatch.Stop();

            _logger.ForDebugEvent().Message("GetArchiveTree: converting archive abstract items to tree")
                .Property("elapsed", stopwatch.ElapsedMilliseconds)
                .Log();

            return result;
        }

        public ArchiveModel GetArchiveSubTree(int siteId, int itemId)
        {
            var useRegion = _settingsProvider.HasRegion(siteId);

            _logger.ForDebugEvent().Message("GetArchiveSubTree")
                .Property("siteId", siteId)
                .Property("itemId", itemId)
                .Property("useRegion", useRegion)
                .Log();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var itemIds = new[] { itemId };
            var abstractItems = _siteMapProvider.GetByIds(siteId, true, itemIds, useRegion, true);
            stopwatch.Stop();

            _logger.ForDebugEvent().Message("GetArchiveSubTree: loading archive abstract items")
                .Property("elapsed", stopwatch.ElapsedMilliseconds)
                .Log();

            stopwatch.Reset();
            stopwatch.Start();

            var archives = _mapper.Map<List<ArchiveModel>>(abstractItems).OrderBy(x => x.IndexOrder).ToList();

            var result = SiteMapStructureBuilder.GetArchiveSubTree(itemId, archives);
            stopwatch.Stop();

            _logger.ForDebugEvent().Message("GetArchiveSubTree: converting archive abstract items to tree")
                .Property("elapsed", stopwatch.ElapsedMilliseconds)
                .Log();

            return result.FirstOrDefault();
        }

        public List<ExtensionFieldModel> GetItemExtensionFields(int siteId, int id, int extensionId)
        {
            var fields = _itemExtensionProvider.GetItemExtensionFields(siteId, id, extensionId);
            var result = _mapper.Map<List<ExtensionFieldModel>>(fields);
            return result;
        }

        public string GetRelatedItemName(int siteId, int id, int attributeId)
        {
            var result = _itemExtensionProvider.GetRelatedItemName(siteId, id, attributeId);
            return result;
        }

        public Dictionary<int, string> GetManyToOneRelatedItemNames(int siteId, int id, int value, int attributeId)
        {
            var result = _itemExtensionProvider.GetManyToOneRelatedItemNames(siteId, id, value, attributeId);
            return result;
        }

        public string GetPathToPage(int siteId, int pageId)
        {
            _logger.ForDebugEvent().Message("GetPathToPage")
                .Property("siteId", siteId)
                .Property("pageId", pageId)
                .Log();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Stack<string> result = new();
            PageData pageData = _siteMapProvider.GetPageById(siteId, pageId);

            while (pageData is not null && pageData.ExtensionId != START_PAGE_EXTENSION_ID && pageData.ParentId is not null)
            {
                if (!string.IsNullOrEmpty(pageData.Alias))
                {
                    result.Push(pageData.Alias);
                }

                pageData = _siteMapProvider.GetPageById(siteId, pageData.ParentId.Value);
            }

            stopwatch.Stop();
            _logger.ForDebugEvent().Message("GetPathToPage: calculating page path")
                .Property("elapsed", stopwatch.ElapsedMilliseconds)
                .Log();

            return string.Join("/", result);
        }
    }
}