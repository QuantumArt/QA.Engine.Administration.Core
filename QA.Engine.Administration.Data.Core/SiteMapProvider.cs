using Dapper;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using NLog;
using QA.DotNetCore.Engine.Persistent.Dapper;

namespace QA.Engine.Administration.Data.Core
{
    public class SiteMapProvider : ISiteMapProvider
    {
        private readonly IUnitOfWork _uow;
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public SiteMapProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer)
        {
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
            _uow = uow;
        }

        #region запросы с использованием NetName таблиц и столбцов

        #region get all abstract items

        private string GetAllAbstractItemsQuery(int siteId, bool isArchive)
        {
            string query = $@"
SELECT
    ai.content_item_id AS Id,
    ai.archive AS Archive,
    ai.visible As Visible,
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
ORDER BY ai.|QPAbstractItem.Parent|, ai.|QPAbstractItem.IndexOrder|, ai.content_item_id
";
            return _netNameQueryAnalyzer.PrepareQuery(query, siteId, false, true);
        }

        #endregion

        #region get abstract items with parent

        private string GetAbstractItemsQuery(int siteId, bool isArchive, string parentExpression, bool onlyPages)
        {
            var onlyPagesFilter = onlyPages
                ? "AND def.|QPDiscriminator.IsPage|=1 AND ai.|QPAbstractItem.VersionOf| is null"
                : "";
            var query = $@"
SELECT
    ai.content_item_id AS Id,
    ai.archive AS Archive,
    ai.visible As Visible,
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
{onlyPagesFilter} 
    AND (ai.|QPAbstractItem.Parent| {parentExpression} OR ai.|QPAbstractItem.VersionOf| {parentExpression})
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
    ai.archive AS Archive,
    ai.visible AS Visible,
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
    ai.archive AS Archive,
    ai.visible As Visible,
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

        public List<AbstractItemData> GetAllItems(int siteId, bool isArchive, bool useRegion,
            IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetAllItems")
                .Property("siteId", siteId)
                .Property("isArchive", isArchive)
                .Property("useRegion", useRegion)
                .Log();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            string query = GetAllAbstractItemsQuery(siteId, isArchive);
            var items = _uow.Connection.Query<AbstractItemData>(query, transaction).ToList();

            _logger.ForDebugEvent().Message("GetAllItems")
                .Property("count", items.Count)
                .Property("elapsed", stopwatch.ElapsedMilliseconds)
                .Log();

            if (useRegion)
            {
                var regions = _uow.Connection.Query<RegionData>(GetRegionsQuery(siteId, isArchive), transaction)
                    .ToList();
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
            bool loadChildren = false,
            IDbTransaction transaction = null)
        {
            var parentIdsArr = parentIds?.ToArray() ?? Array.Empty<int>();

            _logger.ForDebugEvent().Message("GetItems")
                .Property("siteId", siteId)
                .Property("isArchive", isArchive)
                .Property("useRegion", useRegion)
                .Property("loadChildren", loadChildren)
                .Property("parentIds", parentIdsArr)
                .Log();

            const int maxParentIdsPerRequest = 500;

            List<AbstractItemData> items;
            string query;
            if (!parentIdsArr.Any())
            {
                query = GetAbstractItemsQuery(siteId, isArchive, "IS NULL", !loadChildren);
                items = _uow.Connection.Query<AbstractItemData>(query, transaction).ToList();
                if (useRegion)
                {
                    AddRegionInfo(siteId, isArchive, items);
                }

                _logger.ForDebugEvent().Message("GetItems").Property("count", items.Count).Log();
                return items;
            }

            if (parentIdsArr.Length > maxParentIdsPerRequest)
            {
                _logger.ForDebugEvent().Message("GetItems: limit exceeded")
                    .Property("limit", parentIdsArr.Length)
                    .Property("Length", maxParentIdsPerRequest)
                    .Log();

                items = new List<AbstractItemData>();
                for (var i = 0; i < (float)parentIdsArr.Length / maxParentIdsPerRequest; i++)
                {
                    int[] r = parentIdsArr.Skip(i * maxParentIdsPerRequest).Take(maxParentIdsPerRequest).ToArray();
                    items.AddRange(GetItems(siteId, isArchive, r, useRegion, loadChildren));
                }

                _logger.ForDebugEvent().Message("GetItems").Property("count", items.Count).Log();
                return items;
            }

            var idList = SqlQuerySyntaxHelper.IdList(_uow.DatabaseType, "@ParentIds", "parentIds");
            query = GetAbstractItemsQuery(siteId, isArchive, $"IN (SELECT Id FROM {idList})", !loadChildren);

            if (_uow.DatabaseType == DatabaseType.SqlServer)
            {
                items = _uow.Connection.Query<AbstractItemData>(query,
                        new
                        {
                            ParentIds = SqlQuerySyntaxHelper.IdsToDataTable(parentIdsArr).AsTableValuedParameter("Ids")
                        },
                        transaction)
                    .ToList();
            }
            else
            {
                items = _uow.Connection.Query<AbstractItemData>(query, new { ParentIds = parentIdsArr }, transaction)
                    .ToList();
            }

            if (useRegion)
            {
                AddRegionInfo(siteId, isArchive, items);
            }

            if (items.Any() && loadChildren)
            {
                var ids = items.Select(n => n.Id).ToArray();
                var children = GetItems(siteId, isArchive, ids, useRegion, true);
                items.AddRange(children);
            }

            return items;
        }

        private void AddRegionInfo(int siteId, bool isArchive, List<AbstractItemData> items,
            IDbTransaction transaction = null)
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

        public List<AbstractItemData> GetByIds(
            int siteId,
            bool isArchive,
            IEnumerable<int> itemIds,
            bool useRegion = false,
            bool loadChildren = false,
            IDbTransaction transaction = null)
        {
            var itemIdsArr = itemIds?.ToArray() ?? Array.Empty<int>();
            if (!itemIdsArr.Any()) throw new ArgumentNullException(nameof(itemIds));
            if (itemIdsArr.Any(x => x <= 0)) throw new ArgumentException("itemId <= 0");

            _logger.ForDebugEvent().Message("GetByIds")
                .Property("siteId", siteId)
                .Property("isArchive", isArchive)
                .Property("loadChildren", loadChildren)
                .Property("itemIds", itemIdsArr)
                .Log();

            var idList = SqlQuerySyntaxHelper.IdList(_uow.DatabaseType, "@ItemIds", "itemIds");
            var query = GetCmdGetAbstractItemByIds(siteId, isArchive, idList);

            List<AbstractItemData> result;

            if (_uow.DatabaseType == DatabaseType.SqlServer)
            {
                result = _uow.Connection.Query<AbstractItemData>(query,
                        new { ItemIds = SqlQuerySyntaxHelper.IdsToDataTable(itemIdsArr).AsTableValuedParameter("Ids") },
                        transaction)
                    .ToList();
            }
            else
            {
                result = _uow.Connection.Query<AbstractItemData>(query, new { ItemIds = itemIdsArr }, transaction)
                    .ToList();
            }

            _logger.ForDebugEvent().Message("GetByIds")
                .Property("count", result.Count)
                .Property("result", result)
                .Log();

            if (result.Any() && loadChildren)
            {
                var children = GetItems(siteId, isArchive, itemIdsArr, useRegion, true);
                result.AddRange(children);
            }

            return result;
        }

        public AbstractItemData GetRootPage(int siteId, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetRootPage")
                .Property("siteId", siteId)
                .Log();
            var query = _netNameQueryAnalyzer.PrepareQuery(CmdGetRootPage, siteId, false, true);
            var result = _uow.Connection.Query<AbstractItemData>(query, transaction).FirstOrDefault();
            _logger.ForDebugEvent().Message("GetRootPage")
                .Property("result", result)
                .Log();
            return result;
        }
    }
}