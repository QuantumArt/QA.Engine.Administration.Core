using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface ISiteMapProvider
    {
        List<AbstractItemData> GetAllItems(int siteId,
            bool isArchive,
            bool useRegion,
            IDbTransaction transaction = null);
        List<AbstractItemData> GetItems(int siteId,
            bool isArchive,
            IEnumerable<int> parentId,
            bool useRegion,
            bool loadChildren = false,
            IDbTransaction transaction = null);
        List<AbstractItemData> GetWidgetItems(int siteId,
            int parentId,
            string zoneName,
            IDbTransaction transaction = null);
        List<AbstractItemData> GetByIds(int siteId,
            bool isArchive,
            IEnumerable<int> itemIds,
            bool useRegion = false,
            bool loadChildren = false,
            IDbTransaction transaction = null);
        AbstractItemData GetRootPage(int siteId, IDbTransaction transaction = null);
    }
}
