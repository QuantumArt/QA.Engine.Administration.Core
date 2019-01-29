using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface ISiteMapService
    {
        List<SiteTreeModel> GetSiteMapItems(int siteId, bool isStage, int? parentId);
        List<WidgetModel> GetWidgetItems(int siteId, bool isStage, int parentId);
        List<SiteTreeModel> GetSiteMapStructure(int siteId, bool isStage);
        void PublishSiteMapItems(int siteId, bool isStage, int userId, List<int> itemIds);
        void ReorderSiteMapItems(int siteId, bool isStage, int userId, int itemId, int relatedItemId, bool isInsertBefore, int step);
        void MoveSiteMapItem(int siteId, bool isStage, int userId, int itemId, int newParentId);
    }
}
