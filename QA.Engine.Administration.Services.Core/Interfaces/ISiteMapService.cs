using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface ISiteMapService
    {
        List<PageModel> GetSiteMapItems(int siteId, bool isArchive, int? parentId, int[] regionIds = null, bool? useHierarchyRegionFilter = null);
        List<WidgetModel> GetWidgetItems(int siteId, bool isArchive, int parentId, int[] regionIds = null, bool? useHierarchyRegionFilter = null);
        List<PageModel> GetSiteMapStructure(int siteId, int[] regionIds = null, bool? useHierarchyRegionFilter = null);
        List<ArchiveModel> GetArchiveStructure(int siteId);
    }
}
