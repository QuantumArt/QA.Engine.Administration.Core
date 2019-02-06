using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core
{
    public class RegionStructureBuilder
    {
        public static List<RegionModel> GetRegionStructure(List<RegionModel> regions)
        {
            var regionStructure = regions
                .Where(x => x.ParentId == null)
                .ToList();
            if (!regionStructure.Any())
                regionStructure = regions.Where(x => !regions.Any(y => y.Id == x.ParentId)).ToList();

            regionStructure.ForEach(x => regions.Remove(x));

            var result = GetRegionTree(regionStructure, regions);

            return result;
        }

        private static List<RegionModel> GetRegionTree(List<RegionModel> regionStructure, List<RegionModel> regions)
        {
            var elements = regionStructure;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<RegionModel>();
                foreach (var el in elements)
                {
                    el.Children = regions.Where(x => x.ParentId != null && x.ParentId == el.Id).ToList();

                    if (el.HasChildren)
                    {
                        makeTree = true;
                        tmp.AddRange(el.Children);
                        el.Children.ForEach(x => regions.Remove(x));
                    }
                }
                elements = tmp;
            }

            return regionStructure;
        }
    }
}
