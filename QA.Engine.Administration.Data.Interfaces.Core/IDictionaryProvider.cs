using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IDictionaryProvider
    {
        List<ItemDefinitionData> GetAllItemDefinitions(int siteId);
        List<StatusTypeData> GetAllStatusTypes(int siteId);
        StatusTypeData GetStatusType(int siteId, QpContentItemStatus status);
        List<RegionData> GetAllRegions(int siteId);
        List<CultureData> GetAllCultures(int siteId);
    }
}
