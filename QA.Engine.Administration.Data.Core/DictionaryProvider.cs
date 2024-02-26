using Dapper;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Common.Core;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NLog;

namespace QA.Engine.Administration.Data.Core
{
    public class DictionaryProvider : IDictionaryProvider
    {
        private readonly IUnitOfWork _uow;
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public DictionaryProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer)
        {
            _uow = uow;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
        }

        private string CmdGetAllItemDefinitions = @"
SELECT
    CONTENT_ITEM_ID as Id,
    ARCHIVE AS Archive,
    |QPDiscriminator.Name| as Discriminator,
    |QPDiscriminator.IsPage| as IsPage,
    |QPDiscriminator.Title| as Title,
    |QPDiscriminator.Description| as Description,
    |QPDiscriminator.IconUrl| as IconUrl,
    |QPDiscriminator.PreferredContentId| as PreferredContentId,
    |QPDiscriminator.IconClass| as IconClass,
    |QPDiscriminator.IconIntent| as IconIntent
FROM |QPDiscriminator|
";
        private const string CmdGetAllStatusTypes = @"
SELECT
    STATUS_TYPE_ID as Id,
    STATUS_TYPE_NAME as Name,
    WEIGHT as Weight,
    DESCRIPTION as Description
FROM STATUS_TYPE
WHERE SITE_ID=@SiteId
";
        private const string CmdGetStatusTypeById = @"
SELECT
    STATUS_TYPE_ID as Id,
    STATUS_TYPE_NAME as Name,
    WEIGHT as Weight,
    DESCRIPTION as Description
FROM STATUS_TYPE
WHERE SITE_ID=@SiteId AND STATUS_TYPE_NAME=@Status
";

        private string GetAllRegionsQuery()
        {
            return @"
SELECT 
    reg.CONTENT_ITEM_ID AS Id, 
    reg.|QPRegion.Alias| AS Alias, 
    reg.|QPRegion.ParentId| AS ParentId, 
    reg.|QPRegion.Title| AS Title
FROM |QPRegion| reg
WHERE reg.ARCHIVE = 0
";
        }

        private string CmdGetAllCultures()
        {
            return @"
SELECT 
    reg.CONTENT_ITEM_ID AS Id, 
    reg.|QPCulture.Title| AS Title, 
    reg.|QPCulture.Name| AS Name
FROM |QPCulture| reg
WHERE reg.ARCHIVE = 0
";
        }

        public List<ItemDefinitionData> GetAllItemDefinitions(int siteId, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetAllItemDefinitions").Property("siteId", siteId).Log();
            var query = _netNameQueryAnalyzer.PrepareQuery(CmdGetAllItemDefinitions, siteId, false);
            var result = _uow.Connection.Query<ItemDefinitionData>(query, transaction).ToList();
            _logger.ForDebugEvent().Message("GetAllItemDefinitions").Property("total", result.Count).Log();
            return result;
        }

        public List<StatusTypeData> GetAllStatusTypes(int siteId, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetAllStatusTypes").Property("siteId", siteId).Log();
            var result = _uow.Connection.Query<StatusTypeData>(CmdGetAllStatusTypes, 
                new { SiteId = siteId }, transaction).ToList();
            _logger.ForDebugEvent().Message("GetAllStatusTypes").Property("total", result.Count).Log();
            return result;
        }

        public StatusTypeData GetStatusType(int siteId, QpContentItemStatus status, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetStatusType")
                .Property("siteId", siteId)
                .Property("status", status)
                .Log();
            
            var result = _uow.Connection.QueryFirst<StatusTypeData>(CmdGetStatusTypeById, 
                new { SiteId = siteId, Status = status.GetDescription() }, transaction);

            _logger.ForDebugEvent().Message("GetStatusType")
                .Property("statusId", result.Id)
                .Property("statusName", result.Name)
                .Log();
            
            return result;
        }

        public List<RegionData> GetAllRegions(int siteId, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetAllRegions").Property("siteId", siteId).Log();
            var query = _netNameQueryAnalyzer.PrepareQuery(GetAllRegionsQuery(), siteId, true, true);
            var result = _uow.Connection.Query<RegionData>(query, transaction).ToList();
            _logger.ForDebugEvent().Message("GetAllRegions").Property("total", result.Count).Log();
            return result;
        }

        public List<CultureData> GetAllCultures(int siteId, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetAllCultures").Property("siteId", siteId).Log();
            var query = _netNameQueryAnalyzer.PrepareQuery(CmdGetAllCultures(), siteId, true, true);
            var result = _uow.Connection.Query<CultureData>(query, transaction).ToList();
            _logger.ForDebugEvent().Message("GetAllCultures").Property("total", result.Count).Log();
            return result;
        }
    }
}
