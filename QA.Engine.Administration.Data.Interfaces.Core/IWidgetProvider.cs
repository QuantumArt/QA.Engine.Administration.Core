using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IWidgetProvider
    {
        List<AbstractItemData> GetItems(int siteId, bool isArchive, IEnumerable<int> parentId);
    }
}
