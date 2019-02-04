using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface ISiteMapService
    {
        List<SiteTreeModel> GetSiteMapItems(int siteId, bool isArchive, int? parentId);
        List<WidgetTreeModel> GetWidgetItems(int siteId, bool isArchive, int parentId);
        List<SiteTreeModel> GetSiteMapStructure(int siteId);
        ArchiveTreeModel GetArchiveStructure(int siteId);
        void EditSiteMapItem(int siteId, int userId, int itemId, string title);
        void PublishSiteMapItems(int siteId, int userId, List<int> itemIds);
        void ReorderSiteMapItems(int siteId, int userId, int itemId, int relatedItemId, bool isInsertBefore, int step);
        void MoveSiteMapItem(int siteId, int userId, int itemId, int newParentId);
        void RemoveSiteMapItems(int siteId, int userId, int itemId, bool isDeleteAllVersions, bool isDeleteContentVersion, int? contentVersionId);
        void RestoreSiteMapItems(int siteId, int userId, int itemId, bool isRestoreAllVersions, bool isRestoreAllChildren, bool isRestoreContentVersions, bool isRestoreWidgets);
        void DeleteSiteMapItems(int siteId, int userId, int itemId, bool isDeleteAllVersions);
    }
}
