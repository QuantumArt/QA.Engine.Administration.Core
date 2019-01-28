using Dapper;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QA.Engine.Administration.Data.Core
{
    public class WidgetProvider: IWidgetProvider
    {
        private readonly IDbConnection _connection;
        private readonly IAbstractItemRepository _abstractItemRepository;
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;

        public WidgetProvider(IUnitOfWork uow, IAbstractItemRepository abstractItemRepository, INetNameQueryAnalyzer netNameQueryAnalyzer)
        {
            _connection = uow.Connection;
            _abstractItemRepository = abstractItemRepository;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
        }

        //запрос с использованием NetName таблиц и столбцов
        private const string CmdGetAbstractWidgetItem = @"
SELECT
    ai.content_item_id AS Id,
    ai.[|QPAbstractItem.Name|] as Alias,
    ai.[|QPAbstractItem.Title|] as Title,
    ai.[|QPAbstractItem.Parent|] AS ParentId,
    ai.[|QPAbstractItem.IsVisible|] AS IsVisible,
    ai.[|QPAbstractItem.ZoneName|] AS ZoneName,
    ai.[|QPAbstractItem.IndexOrder|] AS IndexOrder,
    ai.[|QPAbstractItem.ExtensionId|] AS ExtensionId,
    ai.[|QPAbstractItem.VersionOf|] AS VersionOfId,
    def.content_item_id AS DiscriminatorId,
    def.[|QPDiscriminator.Name|] as Discriminator,
    def.[|QPDiscriminator.IsPage|] as IsPage,
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE def.[|QPDiscriminator.IsPage|]=0 AND ai.[|QPAbstractItem.Parent|] IN @ParentIds
";

        public IEnumerable<AbstractItemData> GetItems(int siteId, bool isStage, IEnumerable<int> parentIds)
        {
            const int maxParentIdsPerRequest = 500;

            var query = _netNameQueryAnalyzer.PrepareQuery(CmdGetAbstractWidgetItem, siteId, isStage);

            if (parentIds == null)
                throw new ArgumentNullException("parentIds", "need not null and not empty parent ids");
            if (!parentIds.Any())
                return Array.Empty<AbstractItemData>();

            if (parentIds.Count() > maxParentIdsPerRequest)
            {
                var result = new List<AbstractItemData>();
                for (var i = 0; i < (float)parentIds.Count() / maxParentIdsPerRequest; i++)
                {
                    int[] r = parentIds.Skip(i * maxParentIdsPerRequest).Take(maxParentIdsPerRequest).ToArray();
                    result.AddRange(GetItems(siteId, isStage, r));
                }
                return result;
            }
            else
            {
                query = string.Format(query, "IN @ParentIds");
                return _connection.Query<AbstractItemData>(query, new { ParentIds = parentIds });
            }
        }

    }
}
