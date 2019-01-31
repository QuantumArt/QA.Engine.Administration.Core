using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface ISiteMapProvider
    {
        IEnumerable<AbstractItemData> GetAllItems(int siteId, bool isArchive);
        IEnumerable<AbstractItemData> GetItems(int siteId, bool isArchive, IEnumerable<int> parentId);
        IEnumerable<AbstractItemData> GetByIds(int siteId, bool isArchive, IEnumerable<int> itemIds);
        int GetContentId(int siteId);
        AbstractItemData GetRootPage(int siteId);
    }
}
