using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface ISiteMapProvider
    {
        List<AbstractItemData> GetAllItems(int siteId, bool isArchive, bool useRegion);
        List<AbstractItemData> GetItems(int siteId, bool isArchive, IEnumerable<int> parentId, bool useRegion);
        List<AbstractItemData> GetByIds(int siteId, bool isArchive, IEnumerable<int> itemIds);
        AbstractItemData GetRootPage(int siteId);
    }
}
