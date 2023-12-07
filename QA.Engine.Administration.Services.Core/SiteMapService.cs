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
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

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
            Func<AbstractItemData, bool> regionFilter = x => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                _logger.Trace("Use region filter");
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }
            
            var iconUrl = _settingsProvider.GetIconUrl(siteId);

            var items = _siteMapProvider.GetItems(siteId, isArchive, parentId.HasValue ? new[] { parentId.Value } : null, useRegion)
                .Where(regionFilter)
                .Select(x => _mapper.Map<PageModel>(x))
                .ToList();
            // items.ForEach(x => x.IconUrl = $"{iconUrl}/{x.IconUrl}");
            var children = _siteMapProvider.GetItems(siteId, isArchive, items.Select(x => x.Id), useRegion)
                .Where(regionFilter)
                .Select(x => _mapper.Map<PageModel>(x))
                .ToList();
            // children.ForEach(x => x.IconUrl = $"{iconUrl}/{x.IconUrl}");

            foreach (var item in items)
                item.Children = children.Where(x => x.ParentId == item.Id).ToList();

            return items;
        }

        public List<WidgetModel> GetWidgetItems(int siteId, bool isArchive, int parentId, int[] regionIds = null, bool? useHierarchyRegionFilter = null)
        {
            Func<AbstractItemData, bool> regionFilter = x => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                _logger.Trace("Use region filter");
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }

            var iconUrl = _settingsProvider.GetIconUrl(siteId);

            var items = _widgetProvider.GetItems(siteId, isArchive, new[] { parentId })
                .Where(regionFilter)
                .Select(x => _mapper.Map<WidgetModel>(x))
                .ToList();
            // items.ForEach(x => x.IconUrl = $"{iconUrl}/{x.IconUrl}");
            var children = _widgetProvider.GetItems(siteId, isArchive, items.Select(x => x.Id))
                .Where(regionFilter)
                .Select(x => _mapper.Map<WidgetModel>(x))
                .ToList();
            // children.ForEach(x => x.IconUrl = $"{iconUrl}/{x.IconUrl}");

            foreach (var item in items)
                item.Children = children.Where(x => x.ParentId == item.Id).ToList();

            return items;
        }

        public List<PageModel> GetSiteMapTree(int siteId, int[] regionIds = null, bool? useHierarchyRegionFilter = null)
        {
            _logger.Trace($"GetSiteMapStructure siteId={siteId}, regionIds={string.Join(", ", regionIds)}, useHierarchyRegionFilter={useHierarchyRegionFilter}");
            Func<AbstractItemData, bool> regionFilter = x => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                _logger.Trace("Use region filter");
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }

            var iconUrl = _settingsProvider.GetIconUrl(siteId);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var abstractItems = _siteMapProvider.GetAllItems(siteId, false, useRegion)
                .Where(regionFilter)
                .ToList();
            // abstractItems.ForEach(x => x.IconUrl = $"{iconUrl}/{x.IconUrl}");
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
            _logger.Trace($"GetSiteMapSubStructure siteId={siteId}, regionIds={string.Join(", ", regionIds)}, useHierarchyRegionFilter={useHierarchyRegionFilter}");
            Func<AbstractItemData, bool> regionFilter = x => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                _logger.Trace("Use region filter");
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }

            var iconUrl = _settingsProvider.GetIconUrl(siteId);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var abstractItems = _siteMapProvider.GetAllItems(siteId, false, useRegion)
                .Where(regionFilter)
                .ToList();
            // abstractItems.ForEach(x => x.IconUrl = $"{iconUrl}/{x.IconUrl}");
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
            var iconUrl = _settingsProvider.GetIconUrl(siteId);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var abstractItems = _siteMapProvider.GetAllItems(siteId, true, useRegion);
            // abstractItems.ForEach(x => x.IconUrl = $"{iconUrl}/{x.IconUrl}");
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
            var iconUrl = _settingsProvider.GetIconUrl(siteId);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var abstractItems = _siteMapProvider.GetAllItems(siteId, true, useRegion);
            // abstractItems.ForEach(x => x.IconUrl = $"{iconUrl}/{x.IconUrl}");
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

        public List<ExtensionFieldModel> GetItemExtantionFields(int siteId, int id, int extensionId)
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
