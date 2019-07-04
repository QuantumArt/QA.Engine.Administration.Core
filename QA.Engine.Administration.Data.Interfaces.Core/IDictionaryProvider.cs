using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IDictionaryProvider
    {
        List<ItemDefinitionData> GetAllItemDefinitions(int siteId, IDbTransaction transaction = null);
        List<StatusTypeData> GetAllStatusTypes(int siteId, IDbTransaction transaction = null);
        StatusTypeData GetStatusType(int siteId, QpContentItemStatus status, IDbTransaction transaction = null);
        List<RegionData> GetAllRegions(int siteId, IDbTransaction transaction = null);
        List<CultureData> GetAllCultures(int siteId, IDbTransaction transaction = null);
    }
}
