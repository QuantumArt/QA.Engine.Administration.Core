using Dapper;
using QA.DotNetCore.Engine.Persistent.Interfaces;
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

        public DictionaryProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer, IMetaInfoRepository metaInfoRepository)
        {
            _connection = uow.Connection;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
            _metaInfoRepository = metaInfoRepository;
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
    [|QPDiscriminator.PreferredContentId|] as PreferredContentId
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

        public List<ItemDefinitionData> GetAllItemDefinitions(int siteId)
        {
            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAllItemDefinitions, siteId);
            return _connection.Query<ItemDefinitionData>(query).ToList();
        }

        public List<StatusTypeData> GetAllStatusTypes(int siteId)
        {
            return _connection.Query<StatusTypeData>(CmdGetAllStatusTypes, new { SiteId = siteId }).ToList();
        }

        public StatusTypeData GetStatusType(int siteId, QpContentItemStatus status)
        {
            return _connection.QueryFirst<StatusTypeData>(CmdGetStatusTypeById, new { SiteId = siteId, Status = status.GetDescription() });
        }

        public List<RegionData> GetAllRegions(int siteId)
        {
            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAllRegions, siteId);
            return _connection.Query<RegionData>(query).ToList();
        }
    }
}
