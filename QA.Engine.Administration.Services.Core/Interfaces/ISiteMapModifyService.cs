using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface ISiteMapModifyService
    {
        void EditSiteMapItem(int siteId, int userId, EditModel editModel);
        void PublishSiteMapItems(int siteId, int userId, List<int> itemIds);
        void ReorderSiteMapItems(int siteId, int userId, int itemId, int relatedItemId, bool isInsertBefore, int step);
        void MoveSiteMapItem(int siteId, int userId, int itemId, int newParentId);
        void ArchiveSiteMapItems(int siteId, int userId, int itemId, bool isDeleteAllVersions, bool isDeleteContentVersion, int? contentVersionId);
        void RestoreSiteMapItems(int siteId, int userId, int itemId, bool isRestoreAllVersions, bool isRestoreAllChildren, bool isRestoreContentVersions, bool isRestoreWidgets);
        void DeleteSiteMapItems(int siteId, int userId, int itemId, bool isDeleteAllVersions);
    }
}
