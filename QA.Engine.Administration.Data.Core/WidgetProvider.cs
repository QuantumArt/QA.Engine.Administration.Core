using Dapper;
using Microsoft.Extensions.Logging;
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
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;
        private readonly IMetaInfoRepository _metaInfoRepository;
        private readonly ILogger<WidgetProvider> _logger;

        public WidgetProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer, IMetaInfoRepository metaInfoRepository, ILogger<WidgetProvider> logger)
        {
            _connection = uow.Connection;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
            _metaInfoRepository = metaInfoRepository;
            _logger = logger;
        }

        //запрос с использованием NetName таблиц и столбцов
        private const string CmdGetAbstractWidgetItem = @"
SELECT
    ai.content_item_id AS Id,
    ai.archive AS IsArchive,
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
WHERE ai.archive=0 AND def.[|QPDiscriminator.IsPage|]=0 AND ai.[|QPAbstractItem.Parent|] IN @ParentIds
";

        public List<AbstractItemData> GetItems(int siteId, bool isArchive, IEnumerable<int> parentIds)
        {
            _logger.LogDebug($"getItems. siteId: {siteId}, isArchive: {isArchive}, parentIds: {SerializeData(parentIds)}");

            const int maxParentIdsPerRequest = 500;

            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAbstractWidgetItem, siteId);

            if (parentIds == null)
                throw new ArgumentNullException("parentIds", "need not null and not empty parent ids");
            if (!parentIds.Any())
                return new List<AbstractItemData>();

            if (parentIds.Count() > maxParentIdsPerRequest)
            {
                var result = new List<AbstractItemData>();
                for (var i = 0; i < (float)parentIds.Count() / maxParentIdsPerRequest; i++)
                {
                    int[] r = parentIds.Skip(i * maxParentIdsPerRequest).Take(maxParentIdsPerRequest).ToArray();
                    result.AddRange(GetItems(siteId, isArchive, r));
                }
                _logger.LogDebug($"getItems. count: {result.Count()}");
                return result;
            }
            else
            {
                var result = _connection.Query<AbstractItemData>(query, new { Archive = isArchive, ParentIds = parentIds }).ToList();
                _logger.LogDebug($"getItems. count: {result.Count()}");
                return result;
            }
        }

        private static string SerializeData(object data) => Newtonsoft.Json.JsonConvert.SerializeObject(data);

    }
}
