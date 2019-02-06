using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core
{
    public class RegionService : IRegionService
    {
        private readonly IDictionaryProvider _dictionaryProvider;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IMapper _mapper;

        public RegionService(IDictionaryProvider dictionaryProvider, ISettingsProvider settingsProvider, IMapper mapper)
        {
            _dictionaryProvider = dictionaryProvider;
            _settingsProvider = settingsProvider;
            _mapper = mapper;
        }

        public List<RegionModel> GetRegions(int siteId)
        {
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (!useRegion)
                return null;

            var regionData = _dictionaryProvider.GetAllRegions(siteId);
            var regions = regionData.Select(x => _mapper.Map<RegionModel>(x)).ToList();
            return regions;
        }

        public List<RegionModel> GetRegionStructure(int siteId)
        {
            var useRegion = _settingsProvider.HasRegion(siteId);
            if (!useRegion)
                return null;

            var regionData = _dictionaryProvider.GetAllRegions(siteId);
            var regions = regionData.Select(x => _mapper.Map<RegionModel>(x)).ToList();

            var regionStructure = RegionStructureBuilder.GetRegionStructure(regions);

            return regionStructure;
        }
    }
}
