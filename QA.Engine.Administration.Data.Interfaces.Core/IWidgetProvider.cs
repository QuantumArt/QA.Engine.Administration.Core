using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IWidgetProvider
    {
        IEnumerable<AbstractItemData> GetItems(int siteId, bool isStage, IEnumerable<int> parentId);
    }
}
