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
            IDbTransaction transaction = null);
        List<AbstractItemData> GetByIds(int siteId, 
            bool isArchive, 
            IEnumerable<int> itemIds, 
            IDbTransaction transaction = null);
        AbstractItemData GetRootPage(int siteId, IDbTransaction transaction = null);
    }
}
