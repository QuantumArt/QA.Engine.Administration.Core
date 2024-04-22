using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface ISiteMapService
    {
        List<PageModel> GetSiteMapItems(int siteId, bool isArchive, int? parentId, int[] regionIds = null, bool? useHierarchyRegionFilter = null);
        List<WidgetModel> GetWidgetItems(int siteId, bool isArchive, int parentId, int[] regionIds = null, bool? useHierarchyRegionFilter = null);

        List<PageModel> GetSiteMapTree(int siteId, int[] regionIds = null, bool? useHierarchyRegionFilter = null);
        PageModel GetSiteMapSubTree(int siteId, int itemId, int[] regionIds = null, bool? useHierarchyRegionFilter = null);

        List<ArchiveModel> GetArchiveTree(int siteId);
        ArchiveModel GetArchiveSubTree(int siteId, int itemId);

        List<ExtensionFieldModel> GetItemExtensionFields(int siteId, int id, int extensionId);
        string GetRelatedItemName(int siteId, int id, int attributeId);
        Dictionary<int, string> GetManyToOneRelatedItemNames(int siteId, int id, int value, int attributeId);
        string GetPathToPage(int siteId, int pageId);
    }
}
