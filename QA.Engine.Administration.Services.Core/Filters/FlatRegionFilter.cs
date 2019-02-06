using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core.Filters
{
    internal class FlatRegionFilter : RegionFilter
    {
        public FlatRegionFilter(AbstractItemData rootPage, List<RegionData> regions)
            : base(rootPage, regions)
        {
        }

        public override Func<AbstractItemData, bool> GetFilter(int[] regionIds)
        {
            if (regionIds == null || !regionIds.Any())
                return x => true;

            return x => x.Id == RootPage.Id || x.Regions.Any(y => regionIds.Contains(y.Id)) || !x.Regions.Any();
        }
    }
}
