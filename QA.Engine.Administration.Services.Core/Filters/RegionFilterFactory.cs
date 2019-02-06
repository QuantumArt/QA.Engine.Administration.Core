using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QA.Engine.Administration.Services.Core.Filters
{
    internal class RegionFilterFactory
    {
        internal static RegionFilter Create(AbstractItemData rootPage, List<RegionData> regions, bool useHierarchyRegionsFilter)
        {
            return useHierarchyRegionsFilter ? new HierarchyRegionFilter(rootPage, regions) : new FlatRegionFilter(rootPage, regions) as RegionFilter;
        }
    }
}
