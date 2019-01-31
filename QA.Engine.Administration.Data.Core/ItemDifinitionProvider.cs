using Dapper;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QA.Engine.Administration.Data.Core
{
    public class ItemDifinitionProvider : IItemDifinitionProvider
    {
        private readonly IDbConnection _connection;
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;
        private readonly IMetaInfoRepository _metaInfoRepository;

        public ItemDifinitionProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer, IMetaInfoRepository metaInfoRepository)
        {
            _connection = uow.Connection;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
            _metaInfoRepository = metaInfoRepository;
        }

        private const string CmdGetAll = @"
SELECT
    CONTENT_ITEM_ID as Id,
    ARCHIVE AS IsArchive,
    [|QPDiscriminator.Name|] as Discriminator,
    [|QPDiscriminator.TypeName|] as TypeName,
    [|QPDiscriminator.IsPage|] as IsPage,
    [|QPDiscriminator.Title|] as Title,
    [|QPDiscriminator.Description|] as Description,
    [|QPDiscriminator.IconUrl|] as IconUrl,
    [|QPDiscriminator.PreferredContentId|] as PreferredContentId
FROM [|QPDiscriminator|]
";

        public string ItemDefinitionNetName => "QPDiscriminator";

        public IEnumerable<ItemDefinitionData> GetAllItemDefinitions(int siteId)
        {
            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAll, siteId);
            return _connection.Query<ItemDefinitionData>(query).ToList();
        }
    }
}
