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

        public List<PageModel> GetSiteMapItems(int siteId, bool isArchive, int? parentId, int[] regionIds = null, bool? useHierarchyRegionFilter = null)
        {
            Func<AbstractItemData, bool> regionFilter = _ => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                _logger.Trace("Use region filter");
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }
            
            var items = _siteMapProvider.GetItems(siteId, isArchive, parentId.HasValue ? new[] { parentId.Value } : null, useRegion)
                .Where(regionFilter)
                .Select(x => _mapper.Map<PageModel>(x))
                .ToList();
            var children = _siteMapProvider.GetItems(siteId, isArchive, items.Select(x => x.Id), useRegion)
                .Where(regionFilter)
                .Select(x => _mapper.Map<PageModel>(x))
                .ToList();
            
            foreach (var item in items)
                item.Children = children.Where(x => x.ParentId == item.Id).ToList();

            return items;
        }

        public List<WidgetModel> GetWidgetItems(int siteId, bool isArchive, int parentId, int[] regionIds = null, bool? useHierarchyRegionFilter = null)
        {
            Func<AbstractItemData, bool> regionFilter = _ => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                _logger.Trace("Use region filter");
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }

            var items = _widgetProvider.GetItems(siteId, isArchive, new[] { parentId })
                .Where(regionFilter)
                .Select(x => _mapper.Map<WidgetModel>(x))
                .ToList();
            var children = _widgetProvider.GetItems(siteId, isArchive, items.Select(x => x.Id))
                .Where(regionFilter)
                .Select(x => _mapper.Map<WidgetModel>(x))
                .ToList();

            foreach (var item in items)
                item.Children = children.Where(x => x.ParentId == item.Id).ToList();

            return items;
        }

        public List<PageModel> GetSiteMapTree(int siteId, int[] regionIds = null, bool? useHierarchyRegionFilter = null)
        {
            _logger.ForTraceEvent().Message("GetSiteMapSubStructure")
                .Property("siteId", siteId)
                .Property("regionIds", regionIds != null ? string.Join(", ", regionIds) : "")
                .Property("useHierarchyRegionFilter", useHierarchyRegionFilter)
                .Log();
            
            Func<AbstractItemData, bool> regionFilter = _ => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                _logger.Trace("Use region filter");
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
            _logger.Trace($"get all abstract items {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Reset();
            stopwatch.Start();

            var pages = abstractItems.Where(x => x.IsPage).Select(x => _mapper.Map<PageModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = abstractItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetModel>(x)).OrderBy(x => x.IndexOrder).ToList();

            var result = SiteMapStructureBuilder.GetPageTree(pages, widgets);
            stopwatch.Stop();
            _logger.Trace($"convert abstract items to tree {stopwatch.ElapsedMilliseconds}ms");

            return result;
        }

        public PageModel GetSiteMapSubTree(int siteId, int itemId, int[] regionIds = null, bool? useHierarchyRegionFilter = null)
        {
            _logger.ForTraceEvent().Message("GetSiteMapSubStructure")
                .Property("siteId", siteId)
                .Property("regionIds", regionIds != null ? string.Join(", ", regionIds) : "")
                .Property("useHierarchyRegionFilter", useHierarchyRegionFilter)
                .Log();
            Func<AbstractItemData, bool> regionFilter = _ => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                _logger.Trace("Use region filter");
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
            _logger.Trace($"get all abstract items {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Reset();
            stopwatch.Start();

            var pages = abstractItems.Where(x => x.IsPage).Select(x => _mapper.Map<PageModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = abstractItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetModel>(x)).OrderBy(x => x.IndexOrder).ToList();

            var result = SiteMapStructureBuilder.GetPageSubTree(itemId, pages, widgets);
            stopwatch.Stop();
            _logger.Trace($"convert abstract items to tree {stopwatch.ElapsedMilliseconds}ms");

            return result.FirstOrDefault();
        }

        public List<ArchiveModel> GetArchiveTree(int siteId)
        {
            _logger.Trace($"GetArchiveStructure siteId={siteId}");
            var useRegion = _settingsProvider.HasRegion(siteId);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var abstractItems = _siteMapProvider.GetAllItems(siteId, true, useRegion);
            stopwatch.Stop();
            _logger.Trace($"get all archive abstract items {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Reset();
            stopwatch.Start();

            var archives = _mapper.Map<List<ArchiveModel>>(abstractItems).OrderBy(x => x.IndexOrder).ToList();

            var result = SiteMapStructureBuilder.GetArchiveTree(archives);
            stopwatch.Stop();
            _logger.Trace($"convert abstract items to tree {stopwatch.ElapsedMilliseconds}ms");

            return result;
        }

        public ArchiveModel GetArchiveSubTree(int siteId, int itemId)
        {
            _logger.Trace($"GetArchiveSubStructure siteId={siteId}, itemId={itemId}");
            var useRegion = _settingsProvider.HasRegion(siteId);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var abstractItems = _siteMapProvider.GetAllItems(siteId, true, useRegion);
            stopwatch.Stop();
            _logger.Trace($"get all archive abstract items {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Reset();
            stopwatch.Start();

            var archives = _mapper.Map<List<ArchiveModel>>(abstractItems).OrderBy(x => x.IndexOrder).ToList();

            var result = SiteMapStructureBuilder.GetArchiveSubTree(itemId, archives);
            stopwatch.Stop();
            _logger.Trace($"convert archive abstract items to tree {stopwatch.ElapsedMilliseconds}ms");

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
    }
}
