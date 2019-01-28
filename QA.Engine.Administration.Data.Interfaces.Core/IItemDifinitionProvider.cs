using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IItemDifinitionProvider
    {
        IEnumerable<ItemDefinitionData> GetAllItemDefinitions(int siteId, bool isStage);
    }
}
