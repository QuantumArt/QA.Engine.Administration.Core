using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface ISiteMapProvider
    {
        IEnumerable<AbstractItemData> GetAllItems(int siteId, bool isStage);
        IEnumerable<AbstractItemData> GetItems(int siteId, bool isStage, IEnumerable<int> parentId);
        IEnumerable<AbstractItemData> GetByIds(int siteId, bool isStage, IEnumerable<int> itemIds);
        int GetContentId(int siteId);
        AbstractItemData GetRootPage(int siteId, bool isStage);
    }
}
