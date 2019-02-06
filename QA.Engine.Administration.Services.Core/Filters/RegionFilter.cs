using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Filters
{
    internal abstract class RegionFilter
    {
        protected AbstractItemData RootPage;
        protected List<RegionData> Regions;

        public RegionFilter(AbstractItemData rootPage, List<RegionData> regions)
        {
            RootPage = rootPage;
            Regions = regions;
        }

        public abstract Func<AbstractItemData, bool> GetFilter(int[] regionIds);
    }
}
