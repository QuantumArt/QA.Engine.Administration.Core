using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface IRegionService
    {
        List<RegionModel> GetRegions(int siteId);
        List<RegionModel> GetRegionStructure(int siteId);
    }
}
