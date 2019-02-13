using System;
using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Models;
using QA.Engine.Administration.Services.Core.Filters;

namespace QA.Engine.Administration.Services.Core
{
    public class SiteMapService : ISiteMapService
    {
        private readonly ISiteMapProvider _siteMapProvider;
        private readonly IWidgetProvider _widgetProvider;
        private readonly IDictionaryProvider _dictionaryProvider;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IMapper _mapper;

        public SiteMapService(
            ISiteMapProvider siteMapProvider, IWidgetProvider widgetProvider, 
            IDictionaryProvider dictionaryProvider, ISettingsProvider settingsProvider, IMapper mapper)
        {
            _siteMapProvider = siteMapProvider;
            _widgetProvider = widgetProvider;
            _dictionaryProvider = dictionaryProvider;
            _settingsProvider = settingsProvider;
            _mapper = mapper;
        }

        public List<PageModel> GetSiteMapItems(int siteId, bool isArchive, int? parentId, int[] regionIds = null, bool? useHierarchyRegionFilter = null)
        {
            Func<AbstractItemData, bool> regionFilter = x => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
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
            Func<AbstractItemData, bool> regionFilter = x => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
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

        public List<PageModel> GetSiteMapStructure(int siteId, int[] regionIds = null, bool? useHierarchyRegionFilter = null)
        {
            Func<AbstractItemData, bool> regionFilter = x => true;
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (useRegion)
            {
                var rootPage = _siteMapProvider.GetRootPage(siteId);
                var regions = _dictionaryProvider.GetAllRegions(siteId);
                var filter = RegionFilterFactory.Create(rootPage, regions, useHierarchyRegionFilter ?? false);
                regionFilter = filter.GetFilter(regionIds);
            }

            var abstractItems = _siteMapProvider.GetAllItems(siteId, false, useRegion)
                .Where(regionFilter)
                .ToList();

            var pages = abstractItems.Where(x => x.IsPage).Select(x => _mapper.Map<PageModel>(x)).OrderBy(x => x.IndexOrder).ToList();
            var widgets = abstractItems.Where(x => !x.IsPage).Select(x => _mapper.Map<WidgetModel>(x)).OrderBy(x => x.IndexOrder).ToList();

            var pageStructure = SiteMapStructureBuilder.GetPageStructure(pages, widgets);

            return pageStructure;
        }

        public List<ArchiveModel> GetArchiveStructure(int siteId)
        {
            var useRegion = _settingsProvider.HasRegion(siteId);
            var abstractItems = _siteMapProvider.GetAllItems(siteId, true, useRegion);

            var archives = _mapper.Map<List<ArchiveModel>>(abstractItems).OrderBy(x => x.IndexOrder).ToList();

            var archiveStructure = SiteMapStructureBuilder.GetArchiveStructure(archives);

            return archiveStructure;
        }
    }
}
