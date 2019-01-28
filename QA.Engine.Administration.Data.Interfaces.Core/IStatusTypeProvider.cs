using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IStatusTypeProvider
    {
        IEnumerable<StatusTypeData> GetAll(int siteId, bool isStage);
        StatusTypeData GetStatus(int siteId, bool isStage, QpContentItemStatus status);
    }
}
