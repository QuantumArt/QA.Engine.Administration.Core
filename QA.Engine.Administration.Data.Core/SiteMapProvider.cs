using Dapper;
using Microsoft.Extensions.Logging;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Core.Qp;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Microsoft.SqlServer.Server;
using QA.DotNetCore.Engine.Persistent.Dapper;

namespace QA.Engine.Administration.Data.Core
{
    public class SiteMapProvider : ISiteMapProvider
    {
        private readonly IUnitOfWork _uow;
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;
        private readonly ILogger<SiteMapProvider> _logger;

        public SiteMapProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer,
            ILogger<SiteMapProvider> logger)
        {
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
            _uow = uow;
            _logger = logger;
        }

        #region запросы с использованием NetName таблиц и столбцов

        #region get all abstract items

        private string GetAllAbstractItemsQuery(int siteId, bool isArchive)
        {
            string query = $@"
SELECT
    ai.content_item_id AS Id,
    ai.archive AS IsArchive,
    ai.|QPAbstractItem.Name| as Alias,
    ai.|QPAbstractItem.Title| as Title,
    ai.|QPAbstractItem.Parent| AS ParentId,
    ai.|QPAbstractItem.IsVisible| AS IsVisible,
    ai.|QPAbstractItem.ZoneName| AS ZoneName,
    ai.|QPAbstractItem.IndexOrder| AS IndexOrder,
    ai.|QPAbstractItem.ExtensionId| AS ExtensionId,
    ai.|QPAbstractItem.VersionOf| AS VersionOfId,
    ai.|QPAbstractItem.IsInSiteMap| AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.|QPDiscriminator.Name| as Discriminator,
    def.|QPDiscriminator.IsPage| as IsPage,
    def.|QPDiscriminator.Title| as DiscriminatorTitle,
    def.|QPDiscriminator.IconUrl| as IconUrl,
    CASE WHEN ai.STATUS_TYPE_ID IN (SELECT st.STATUS_TYPE_ID FROM STATUS_TYPE st WHERE st.STATUS_TYPE_NAME=N'Published') THEN 1 ELSE 0 END AS Published
FROM |QPAbstractItem| ai
INNER JOIN |QPDiscriminator| def on ai.|QPAbstractItem.Discriminator| = def.content_item_id
WHERE ai.archive={(isArchive ? "1" : "0")} 
  AND ai.visible=1
ORDER BY ai.|QPAbstractItem.Parent|, ai.|QPAbstractItem.IndexOrder|, ai.content_item_id
";
            return _netNameQueryAnalyzer.PrepareQuery(query, siteId, false, true);
        }

        #endregion

        #region get abstract items with parent

        private string GetAbstractItemsQuery(int siteId, bool isArchive, string parentExpression)
        {
            var query = $@"
SELECT
    ai.content_item_id AS Id,
    ai.archive AS IsArchive,
    ai.|QPAbstractItem.Name| as Alias,
    ai.|QPAbstractItem.Title| as Title,
    ai.|QPAbstractItem.Parent| AS ParentId,
    ai.|QPAbstractItem.IsVisible| AS IsVisible,
    ai.|QPAbstractItem.ZoneName| AS ZoneName,
    ai.|QPAbstractItem.IndexOrder| AS IndexOrder,
    ai.|QPAbstractItem.ExtensionId| AS ExtensionId,
    ai.|QPAbstractItem.VersionOf| AS VersionOfId,
    ai.|QPAbstractItem.IsInSiteMap| AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.|QPDiscriminator.Name| as Discriminator,
    def.|QPDiscriminator.IsPage| as IsPage,
    def.|QPDiscriminator.Title| as DiscriminatorTitle,
    def.|QPDiscriminator.IconUrl| as IconUrl,
    CASE WHEN ai.STATUS_TYPE_ID IN (SELECT st.STATUS_TYPE_ID FROM STATUS_TYPE st WHERE st.STATUS_TYPE_NAME=N'Published') THEN 1 ELSE 0 END AS Published
FROM |QPAbstractItem| ai
INNER JOIN |QPDiscriminator| def on ai.|QPAbstractItem.Discriminator| = def.content_item_id
WHERE ai.archive={(isArchive ? "1" : "0")} 
AND def.|QPDiscriminator.IsPage|=1 AND ai.|QPAbstractItem.VersionOf| is null 
    AND (
        ai.|QPAbstractItem.Parent| {parentExpression}
        OR EXISTS (SELECT 1 FROM |QPAbstractItem| ai1 
            WHERE ai.|QPAbstractItem.Parent| {parentExpression} AND ai1.content_item_id=ai.|QPAbstractItem.VersionOf|)
    )
ORDER BY ai.|QPAbstractItem.Parent|, ai.|QPAbstractItem.IndexOrder|, ai.content_item_id
";
            return _netNameQueryAnalyzer.PrepareQuery(query, siteId, false, true);
        }

        #endregion

        #region get abstract items by ids

        private string GetCmdGetAbstractItemByIds(int siteId, bool isArchive, string idsExpression)
        {
            string query = $@"
SELECT
    ai.content_item_id AS Id,
    ai.archive AS IsArchive,
    ai.|QPAbstractItem.Name| as Alias,
    ai.|QPAbstractItem.Title| as Title,
    ai.|QPAbstractItem.Parent| AS ParentId,
    ai.|QPAbstractItem.IsVisible| AS IsVisible,
    ai.|QPAbstractItem.ZoneName| AS ZoneName,
    ai.|QPAbstractItem.IndexOrder| AS IndexOrder,
    ai.|QPAbstractItem.ExtensionId| AS ExtensionId,
    ai.|QPAbstractItem.VersionOf| AS VersionOfId,
    ai.|QPAbstractItem.IsInSiteMap| AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.|QPDiscriminator.Name| as Discriminator,
    def.|QPDiscriminator.IsPage| as IsPage,
    def.|QPDiscriminator.Title| as DiscriminatorTitle,
    def.|QPDiscriminator.IconUrl| as IconUrl,
    CASE WHEN ai.STATUS_TYPE_ID IN (SELECT st.STATUS_TYPE_ID FROM STATUS_TYPE st WHERE st.STATUS_TYPE_NAME=N'Published') THEN 1 ELSE 0 END AS Published
FROM |QPAbstractItem| ai
INNER JOIN |QPDiscriminator| def on ai.|QPAbstractItem.Discriminator| = def.content_item_id
WHERE ai.archive={(isArchive ? "1" : "0")} AND ai.content_item_id IN (SELECT Id FROM {idsExpression})
";
            return _netNameQueryAnalyzer.PrepareQuery(query, siteId, false, true);
        }

        #endregion

        #region get root page

        private const string CmdGetRootPage = @"
SELECT
    ai.content_item_id AS Id,
    ai.archive AS IsArchive,
    ai.|QPAbstractItem.Name| as Alias,
    ai.|QPAbstractItem.Title| as Title,
    ai.|QPAbstractItem.Parent| AS ParentId,
    ai.|QPAbstractItem.IsVisible| AS IsVisible,
    ai.|QPAbstractItem.ZoneName| AS ZoneName,
    ai.|QPAbstractItem.IndexOrder| AS IndexOrder,
    ai.|QPAbstractItem.ExtensionId| AS ExtensionId,
    ai.|QPAbstractItem.VersionOf| AS VersionOfId,
    ai.|QPAbstractItem.IsInSiteMap| AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.|QPDiscriminator.Name| as Discriminator,
    def.|QPDiscriminator.IsPage| as IsPage,
    def.|QPDiscriminator.Title| as DiscriminatorTitle,
    def.|QPDiscriminator.IconUrl| as IconUrl,
    CASE WHEN ai.STATUS_TYPE_ID IN (SELECT st.STATUS_TYPE_ID FROM STATUS_TYPE st WHERE st.STATUS_TYPE_NAME=N'Published') THEN 1 ELSE 0 END AS Published
FROM |QPAbstractItem| ai
INNER JOIN |QPDiscriminator| def on ai.|QPAbstractItem.Discriminator| = def.content_item_id
WHERE ai.archive=0 AND ai.|QPAbstractItem.Parent| IS NULL AND ai.|QPAbstractItem.VersionOf| IS NULL AND def.|QPDiscriminator.IsPage|=1
ORDER BY ai.content_item_id";

        private string GetRegionsQuery(int siteId, bool isArchive = false)
        {
            string query = $@"
            SELECT 
            reg.CONTENT_ITEM_ID AS Id, 
                reg.|QPRegion.Alias| AS Alias, 
            reg.|QPRegion.ParentId| AS ParentId, 
            reg.|QPRegion.Title| AS Title
            FROM |QPRegion| reg
            WHERE reg.ARCHIVE = {(isArchive ? "1" : "0")}";
            return _netNameQueryAnalyzer.PrepareQuery(query, siteId, false, true);
        }

        private string GetRegionLinkIdQuery(int siteId)
        {
            string query =
                $@"SELECT {SqlQuerySyntaxHelper.Top(_uow.DatabaseType, "1")} |QPAbstractItem.Regions| FROM |QPAbstractItem| WHERE |QPAbstractItem.Regions|
                IS NOT NULL {SqlQuerySyntaxHelper.Limit(_uow.DatabaseType, "1")}";
            return _netNameQueryAnalyzer.PrepareQuery(query, siteId, false, true);
        }

        private string GetLinkItemIdQuery(int linkId)
        {
            return $@"SELECT item_id AS ItemId, linked_item_id AS LinkedItemId FROM link_{linkId}_united";
        }

        #endregion

        #endregion

        public List<AbstractItemData> GetAllItems(int siteId, bool isArchive, bool useRegion, IDbTransaction transaction = null)
        {
            _logger.LogDebug($"getAllItems. siteId: {siteId}, isArchive: {isArchive}, useRegion: {useRegion}");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            string query = GetAllAbstractItemsQuery(siteId, isArchive);
            var items = _uow.Connection.Query<AbstractItemData>(query, transaction).ToList();
            _logger.LogDebug($"getAllItems. count: {items.Count}. {stopwatch.ElapsedMilliseconds}ms");
            if (useRegion)
            {
                var regions = _uow.Connection.Query<RegionData>(GetRegionsQuery(siteId, isArchive), transaction).ToList();
                var regionLinkId = _uow.Connection.QueryFirst<int>(GetRegionLinkIdQuery(siteId), transaction);
                var links = _uow.Connection.Query<LinkItem>(GetLinkItemIdQuery(regionLinkId), transaction)
                    .ToList()
                    .GroupBy(k => k.ItemId)
                    .ToDictionary(k => k.Key, v => v.Select(x => x.LinkedItemId).ToList());

                foreach (var abstractItem in items)
                {
                    if (links.TryGetValue(abstractItem.Id, out List<int> regionIds))
                        abstractItem.RegionIds =
                            regions.Where(x => regionIds.Contains(x.Id)).Select(x => x.Id).ToList();
                    else
                        abstractItem.RegionIds = new List<int>();
                }
            }

            return items;
        }

        public List<AbstractItemData> GetItems(int siteId, 
            bool isArchive, 
            IEnumerable<int> parentIds, 
            bool useRegion, 
            IDbTransaction transaction = null)
        {
            _logger.LogDebug(
                $"getItems. siteId: {siteId}, isArchive: {isArchive}, useRegion: {useRegion}, parentIds: {SerializeData(parentIds)}");

            const int maxParentIdsPerRequest = 500;

            List<AbstractItemData> items;
            string query;
            if (parentIds == null || !parentIds.Any())
            {
                query = GetAbstractItemsQuery(siteId, isArchive, "IS NULL");
                items = _uow.Connection.Query<AbstractItemData>(query, transaction).ToList();
                if (useRegion)
                {
                    AddRegionInfo(siteId, isArchive, items);
                }

                _logger.LogDebug($"getItems. count: {items.Count}");
                return items;
            }

            if (parentIds.Count() > maxParentIdsPerRequest)
            {
                items = new List<AbstractItemData>();
                for (var i = 0; i < (float) parentIds.Count() / maxParentIdsPerRequest; i++)
                {
                    int[] r = parentIds.Skip(i * maxParentIdsPerRequest).Take(maxParentIdsPerRequest).ToArray();
                    items.AddRange(GetItems(siteId, isArchive, r, useRegion));
                }

                _logger.LogDebug($"getItems. count: {items.Count}");
                return items;
            }

            var idList = SqlQuerySyntaxHelper.IdList(_uow.DatabaseType, "@ParentIds", "parentIds");
            query = GetAbstractItemsQuery(siteId, isArchive, $"IN (SELECT Id FROM {idList})");

            if (_uow.DatabaseType == DatabaseType.SqlServer)
            {
                items = _uow.Connection.Query<AbstractItemData>(query,
                        new {ParentIds = SqlQuerySyntaxHelper.IdsToDataTable(parentIds).AsTableValuedParameter("Ids")},
                        transaction)
                    .ToList();
            }
            else
            {
                items = _uow.Connection.Query<AbstractItemData>(query, new {ParentIds = parentIds}, transaction).ToList();
            }

            if (useRegion)
            {
                AddRegionInfo(siteId, isArchive, items);
            }

            return items;
        }

        private void AddRegionInfo(int siteId, bool isArchive, List<AbstractItemData> items, IDbTransaction transaction = null)
        {
            var regions = _uow.Connection.Query<RegionData>(GetRegionsQuery(siteId, isArchive), transaction).ToList();
            var regionLinkId = _uow.Connection.QueryFirst<int>(GetRegionLinkIdQuery(siteId), transaction);
            var links = _uow.Connection.Query<LinkItem>(GetLinkItemIdQuery(regionLinkId), transaction)
                .ToList()
                .GroupBy(k => k.ItemId)
                .ToDictionary(k => k.Key, v => v.Select(x => x.LinkedItemId).ToList());

            foreach (var abstractItem in items)
            {
                abstractItem.RegionIds = links.TryGetValue(abstractItem.Id, out List<int> regionIds)
                    ? regions.Where(x => regionIds.Contains(x.Id)).Select(x => x.Id).ToList()
                    : Enumerable.Empty<int>().ToList();
            }
        }

        public List<AbstractItemData> GetByIds(int siteId, bool isArchive, IEnumerable<int> itemIds, IDbTransaction transaction = null)
        {
            _logger.LogDebug($"getByIds. siteId: {siteId}, isArchive: {isArchive}, itemIds: {SerializeData(itemIds)}");

            if (itemIds == null || !itemIds.Any()) throw new ArgumentNullException("itemIds");
            if (itemIds.Any(x => x <= 0)) throw new ArgumentException("itemId <= 0");

            var idList = SqlQuerySyntaxHelper.IdList(_uow.DatabaseType, "@ItemIds", "itemIds");
            var query = GetCmdGetAbstractItemByIds(siteId, isArchive, idList);

            List<AbstractItemData> result;
            
            if (_uow.DatabaseType == DatabaseType.SqlServer)
            {
                result = _uow.Connection.Query<AbstractItemData>(query,
                        new {ItemIds = SqlQuerySyntaxHelper.IdsToDataTable(itemIds).AsTableValuedParameter("Ids")},
                        transaction)
                    .ToList();
            }
            else
            {
                result = _uow.Connection.Query<AbstractItemData>(query, new {ItemIds = itemIds}, transaction).ToList();
            }
            
            _logger.LogDebug($"getByIds. count: {result.Count}, result: {SerializeData(result)}");
            return result;
        }

        public AbstractItemData GetRootPage(int siteId, IDbTransaction transaction = null)
        {
            _logger.LogDebug($"getRootPage. siteId: {siteId}");
            var query = _netNameQueryAnalyzer.PrepareQuery(CmdGetRootPage, siteId, false, true);
            var result = _uow.Connection.Query<AbstractItemData>(query, transaction).FirstOrDefault();
            _logger.LogDebug($"getRootPage. result: {SerializeData(result)}");
            return result;
        }

        private static string SerializeData(object data) => Newtonsoft.Json.JsonConvert.SerializeObject(data);
    }
}
