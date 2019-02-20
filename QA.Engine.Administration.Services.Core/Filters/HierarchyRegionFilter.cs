using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core.Filters
{
    internal class HierarchyRegionFilter : RegionFilter
    {
        public HierarchyRegionFilter(AbstractItemData rootPage, List<RegionData> regions) 
            : base (rootPage, regions)
        {
        }

        public override Func<AbstractItemData, bool> GetFilter(int[] regionIds)
        {
            if (regionIds == null || !regionIds.Any())
                return x => true;

            var filteredRegions = GetRegionIds(regionIds, Regions);
            return x => x.Id == RootPage.Id || x.RegionIds.Any(y => filteredRegions.Contains(y)) || !x.RegionIds.Any();
        }

        private static List<int> GetRegionIds(int[] regionIds, List<RegionData> regions)
        {
            var result = new List<int>();
            var elements = regions.Where(x => regionIds.Contains(x.Id));
            var @continue = true;
            while (@continue)
            {
                @continue = false;
                var tmp = new List<RegionData>();
                foreach (var el in elements)
                {
                    tmp.AddRange(regions.Where(x => x.ParentId != null && x.ParentId == el.Id));

                    if (tmp.Any())
                        @continue = true;
                }
                result.AddRange(tmp.Select(x => x.Id));
                elements = tmp;
            }

            return result;
        }
    }
}
