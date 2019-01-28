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

        public ItemDifinitionProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer)
        {
            _connection = uow.Connection;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
        }

        private const string CmdGetAll = @"
SELECT
    CONTENT_ITEM_ID as Id,
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

        public IEnumerable<ItemDefinitionData> GetAllItemDefinitions(int siteId, bool isStage)
        {
            var query = _netNameQueryAnalyzer.PrepareQuery(CmdGetAll, siteId, isStage);
            return _connection.Query<ItemDefinitionData>(query).ToList();
        }
    }
}
