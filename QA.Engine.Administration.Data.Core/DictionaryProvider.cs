using Dapper;
using Microsoft.Extensions.Logging;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engin.Administration.Common.Core;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QA.Engine.Administration.Data.Core
{
    public class DictionaryProvider : IDictionaryProvider
    {
        private readonly IDbConnection _connection;
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;
        private readonly IMetaInfoRepository _metaInfoRepository;
        private readonly ILogger<DictionaryProvider> _logger;

        public DictionaryProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer, IMetaInfoRepository metaInfoRepository, ILogger<DictionaryProvider> logger)
        {
            _connection = uow.Connection;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
            _metaInfoRepository = metaInfoRepository;
            _logger = logger;
        }

        private const string CmdGetAllItemDefinitions = @"
SELECT
    CONTENT_ITEM_ID as Id,
    ARCHIVE AS IsArchive,
    [|QPDiscriminator.Name|] as Discriminator,
    [|QPDiscriminator.IsPage|] as IsPage,
    [|QPDiscriminator.Title|] as Title,
    [|QPDiscriminator.Description|] as Description,
    [|QPDiscriminator.IconUrl|] as IconUrl,
    [|QPDiscriminator.PreferredContentId|] as PreferredContentId,
    [|QPDiscriminator.IconClass|] as IconClass,
    [|QPDiscriminator.IconIntent|] as IconIntent
FROM [|QPDiscriminator|]
";
        private const string CmdGetAllStatusTypes = @"
SELECT
    STATUS_TYPE_ID as Id,
    STATUS_TYPE_NAME as Name,
    WEIGHT as Weight,
    DESCRIPTION as Description
FROM [STATUS_TYPE]
WHERE [SITE_ID]=@SiteId
";
        private const string CmdGetStatusTypeById = @"
SELECT
    STATUS_TYPE_ID as Id,
    STATUS_TYPE_NAME as Name,
    WEIGHT as Weight,
    DESCRIPTION as Description
FROM [STATUS_TYPE]
WHERE [SITE_ID]=@SiteId AND [STATUS_TYPE_NAME]=@Status
";
        private const string CmdGetAllRegions = @"
SELECT 
    reg.CONTENT_ITEM_ID AS Id, 
    reg.[|QPRegion.Alias|] AS Alias, 
    reg.[|QPRegion.ParentId|] AS ParentId, 
    reg.[|QPRegion.Title|] AS Title
FROM [|QPRegion|] reg
WHERE reg.ARCHIVE = 0
";
        private const string CmdGetAllCultures = @"
SELECT 
    reg.CONTENT_ITEM_ID AS Id, 
    reg.[|QPCulture.Title|] AS Title, 
    reg.[|QPCulture.Name|] AS Name
FROM [|QPCulture|] reg
WHERE reg.ARCHIVE = 0
";

        public List<ItemDefinitionData> GetAllItemDefinitions(int siteId)
        {
            _logger.LogDebug($"getAllItemDefinitions. siteId: {siteId}");
            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAllItemDefinitions, siteId);
            var result = _connection.Query<ItemDefinitionData>(query).ToList();
            _logger.LogDebug($"getAllItemDefinitions. total: {result.Count()}");
            return result;
        }

        public List<StatusTypeData> GetAllStatusTypes(int siteId)
        {
            _logger.LogDebug($"getAllStatusTypes. siteId: {siteId}");
            var result = _connection.Query<StatusTypeData>(CmdGetAllStatusTypes, new { SiteId = siteId }).ToList();
            _logger.LogDebug($"getAllStatusTypes. total: {result.Count()}");
            return result;
        }

        public StatusTypeData GetStatusType(int siteId, QpContentItemStatus status)
        {
            _logger.LogDebug($"getStatusType. siteId: {siteId}, status: {status}");
            var result = _connection.QueryFirst<StatusTypeData>(CmdGetStatusTypeById, new { SiteId = siteId, Status = status.GetDescription() });
            _logger.LogDebug($"getStatusType. statusId: {result.Id}, statusName: {result.Name}");
            return result;
        }

        public List<RegionData> GetAllRegions(int siteId)
        {
            _logger.LogDebug($"getAllRegions. siteId: {siteId}");
            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAllRegions, siteId);
            var result = _connection.Query<RegionData>(query).ToList();
            _logger.LogDebug($"getAllRegions. total: {result.Count()}");
            return result;
        }

        public List<CultureData> GetAllCultures(int siteId)
        {
            _logger.LogDebug($"GetAllCultures. siteId: {siteId}");
            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAllCultures, siteId);
            var result = _connection.Query<CultureData>(query).ToList();
            _logger.LogDebug($"GetAllCultures. total: {result.Count()}");
            return result;
        }
    }
}
