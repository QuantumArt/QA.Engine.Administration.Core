﻿using System;
using System.Collections.Generic;
using System.Text;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface ISiteMapModifyService
    {
        void EditSiteMapItem(int siteId, int userId, int itemId, string title);
        void PublishSiteMapItems(int siteId, int userId, List<int> itemIds);
        void ReorderSiteMapItems(int siteId, int userId, int itemId, int relatedItemId, bool isInsertBefore, int step);
        void MoveSiteMapItem(int siteId, int userId, int itemId, int newParentId);
        void RemoveSiteMapItems(int siteId, int userId, int itemId, bool isDeleteAllVersions, bool isDeleteContentVersion, int? contentVersionId);
        void RestoreSiteMapItems(int siteId, int userId, int itemId, bool isRestoreAllVersions, bool isRestoreAllChildren, bool isRestoreContentVersions, bool isRestoreWidgets);
        void DeleteSiteMapItems(int siteId, int userId, int itemId, bool isDeleteAllVersions);
    }
}
