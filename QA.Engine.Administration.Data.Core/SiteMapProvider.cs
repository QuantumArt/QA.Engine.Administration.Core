using Dapper;
using Microsoft.Extensions.Logging;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Core.Qp;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QA.Engine.Administration.Data.Core
{
    public class SiteMapProvider: ISiteMapProvider
    {
        private readonly IDbConnection _connection;
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;
        private readonly IMetaInfoRepository _metaInfoRepository;
        private readonly ILogger<SiteMapProvider> _logger;

        public SiteMapProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer, IMetaInfoRepository metaInfoRepository, ILogger<SiteMapProvider> logger)
        {
            _connection = uow.Connection;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
            _metaInfoRepository = metaInfoRepository;
            _logger = logger;
        }

        #region запросы с использованием NetName таблиц и столбцов

        #region get all abstract items
        private const string CmdGetAllAbstractItems = @"
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
    ai.[|QPAbstractItem.IsInSiteMap|] AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.[|QPDiscriminator.Name|] as Discriminator,
    def.[|QPDiscriminator.IsPage|] as IsPage,
    def.[|QPDiscriminator.Title|] as DiscriminatorTitle,
    def.[|QPDiscriminator.IconUrl|] as IconUrl,
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE ai.archive=@Archive AND ai.visible=1
ORDER BY ai.[|QPAbstractItem.Parent|], ai.[|QPAbstractItem.IndexOrder|], ai.content_item_id
";

        private const string CmdGetAllAbstractItemsWithRegions = @"
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
    ai.[|QPAbstractItem.IsInSiteMap|] AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.[|QPDiscriminator.Name|] as Discriminator,
    def.[|QPDiscriminator.IsPage|] as IsPage,
    def.[|QPDiscriminator.Title|] as DiscriminatorTitle,
    def.[|QPDiscriminator.IconUrl|] as IconUrl,
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def ON ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE ai.archive=@Archive AND ai.visible=1
ORDER BY ai.[|QPAbstractItem.Parent|], ai.[|QPAbstractItem.IndexOrder|], ai.content_item_id

SELECT 
    reg.CONTENT_ITEM_ID AS Id, 
    reg.[|QPRegion.Alias|] AS Alias, 
    reg.[|QPRegion.ParentId|] AS ParentId, 
    reg.[|QPRegion.Title|] AS Title
FROM [|QPRegion|] reg
WHERE reg.ARCHIVE = 0

DECLARE @linkId VARCHAR(4) = CAST((SELECT TOP 1 [|QPAbstractItem.Regions|] FROM [|QPAbstractItem|] WHERE [|QPAbstractItem.Regions|] IS NOT NULL) AS VARCHAR(4))
DECLARE @query NVARCHAR(MAX) = N'SELECT item_id AS ItemId, linked_item_id AS LinkedItemId FROM link_' + @linkId + '_united'
exec dbo.SP_EXECUTESQL @query

";
        #endregion

        #region get abstract items with parent
        private const string CmdGetAbstractItems = @"
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
    ai.[|QPAbstractItem.IsInSiteMap|] AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.[|QPDiscriminator.Name|] as Discriminator,
    def.[|QPDiscriminator.IsPage|] as IsPage,
    def.[|QPDiscriminator.Title|] as DiscriminatorTitle,
    def.[|QPDiscriminator.IconUrl|] as IconUrl,
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE ai.archive=@Archive AND def.[|QPDiscriminator.IsPage|]=1 AND ai.[|QPAbstractItem.VersionOf|] is null 
    AND (
        ai.[|QPAbstractItem.Parent|] {0}
        OR EXISTS (SELECT 1 FROM [|QPAbstractItem|] ai1 
            WHERE ai.[|QPAbstractItem.Parent|] {0} AND ai1.content_item_id=ai.[|QPAbstractItem.VersionOf|])
    )
ORDER BY ai.[|QPAbstractItem.Parent|], ai.[|QPAbstractItem.IndexOrder|], ai.content_item_id
";

        private const string CmdGetAbstractItemsWithRegions = @"
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
    ai.[|QPAbstractItem.IsInSiteMap|] AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.[|QPDiscriminator.Name|] as Discriminator,
    def.[|QPDiscriminator.IsPage|] as IsPage,
    def.[|QPDiscriminator.Title|] as DiscriminatorTitle,
    def.[|QPDiscriminator.IconUrl|] as IconUrl,
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def ON ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE ai.archive=@Archive AND def.[|QPDiscriminator.IsPage|]=1 AND ai.[|QPAbstractItem.VersionOf|] is null 
    AND (
        ai.[|QPAbstractItem.Parent|] {0}
        OR EXISTS (SELECT 1 FROM [|QPAbstractItem|] ai1 
            WHERE ai.[|QPAbstractItem.Parent|] {0} AND ai1.content_item_id=ai.[|QPAbstractItem.VersionOf|])
    )
ORDER BY ai.[|QPAbstractItem.Parent|], ai.[|QPAbstractItem.IndexOrder|], ai.content_item_id

SELECT 
    reg.CONTENT_ITEM_ID AS Id, 
    reg.[|QPRegion.Alias|] AS Alias, 
    reg.[|QPRegion.ParentId|] AS ParentId, 
    reg.[|QPRegion.Title|] AS Title
FROM [|QPRegion|] reg
WHERE reg.ARCHIVE = 0

DECLARE @linkId VARCHAR(4) = CAST((SELECT TOP 1 [|QPAbstractItem.Regions|] FROM [|QPAbstractItem|] WHERE [|QPAbstractItem.Regions|] IS NOT NULL) AS VARCHAR(4))
DECLARE @query NVARCHAR(MAX) = N'SELECT item_id AS ItemId, linked_item_id AS LinkedItemId FROM link_' + @linkId + '_united'
exec dbo.SP_EXECUTESQL @query

";
        #endregion

        #region get abstract items by ids
        private const string CmdGetAbstractItemByIds = @"
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
    ai.[|QPAbstractItem.IsInSiteMap|] AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.[|QPDiscriminator.Name|] as Discriminator,
    def.[|QPDiscriminator.IsPage|] as IsPage,
    def.[|QPDiscriminator.Title|] as DiscriminatorTitle,
    def.[|QPDiscriminator.IconUrl|] as IconUrl,
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE ai.archive=@Archive AND ai.content_item_id IN @ItemIds
";
        #endregion

        #region get root page
        private const string CmdGetRootPage = @"
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
    ai.[|QPAbstractItem.IsInSiteMap|] AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.[|QPDiscriminator.Name|] as Discriminator,
    def.[|QPDiscriminator.IsPage|] as IsPage,
    def.[|QPDiscriminator.Title|] as DiscriminatorTitle,
    def.[|QPDiscriminator.IconUrl|] as IconUrl,
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE ai.archive=0 AND ai.[|QPAbstractItem.Parent|] IS NULL AND ai.[|QPAbstractItem.VersionOf|] IS NULL AND def.[|QPDiscriminator.IsPage|]=1
ORDER BY ai.content_item_id";
        #endregion

        #endregion

        public List<AbstractItemData> GetAllItems(int siteId, bool isArchive, bool useRegion)
        {
            _logger.LogDebug($"getAllItems. siteId: {siteId}, isArchive: {isArchive}, useRegion: {useRegion}");
            string query;
            if (!useRegion)
            {
                query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAllAbstractItems, siteId);
                var result = _connection.Query<AbstractItemData>(query, new { Archive = isArchive }).ToList();
                _logger.LogDebug($"getAllItems. count: {result.Count()}");
                return result;
            }

            query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAllAbstractItemsWithRegions, siteId);
            var resultWithRegions = GetWithRegions(_connection, query, isArchive);
            _logger.LogDebug($"getAllItems. count: {resultWithRegions.Count()}");
            return resultWithRegions;
        }

        public List<AbstractItemData> GetItems(int siteId, bool isArchive, IEnumerable<int> parentIds, bool useRegion)
        {
            _logger.LogDebug($"getItems. siteId: {siteId}, isArchive: {isArchive}, useRegion: {useRegion}, parentIds: {SerializeData(parentIds)}");

            const int maxParentIdsPerRequest = 500;

            string query = useRegion 
                ? _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAbstractItemsWithRegions, siteId)
                : _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAbstractItems, siteId);

            if (parentIds == null || !parentIds.Any())
            {
                query = string.Format(query, "IS NULL");

                if (!useRegion)
                    return _connection.Query<AbstractItemData>(query, new { Archive = isArchive }).ToList();
                var resultWithRegions = GetWithRegions(_connection, query, isArchive);
                _logger.LogDebug($"getItems. count: {resultWithRegions.Count()}");
                return resultWithRegions;
            }
            else
            {
                if (parentIds.Count() > maxParentIdsPerRequest)
                {
                    var result = new List<AbstractItemData>();
                    for (var i = 0; i < (float)parentIds.Count() / maxParentIdsPerRequest; i++)
                    {
                        int[] r = parentIds.Skip(i * maxParentIdsPerRequest).Take(maxParentIdsPerRequest).ToArray();
                        result.AddRange(GetItems(siteId, isArchive, r, useRegion));
                    }
                    _logger.LogDebug($"getItems. count: {result.Count()}");
                    return result;
                }
                else
                {
                    query = string.Format(query, "IN @ParentIds");
                    if (!useRegion)
                        return _connection.Query<AbstractItemData>(query, new { Archive = isArchive, ParentIds = parentIds }).ToList();
                    var resultWithRegions = GetWithRegions(_connection, query, isArchive, parentIds);
                    _logger.LogDebug($"getItems. count: {resultWithRegions.Count()}");
                    return resultWithRegions;
                }
            }
        }

        public List<AbstractItemData> GetByIds(int siteId, bool isArchive, IEnumerable<int> itemIds)
        {
            _logger.LogDebug($"getByIds. siteId: {siteId}, isArchive: {isArchive}, itemIds: {SerializeData(itemIds)}");

            if (itemIds == null || !itemIds.Any())
                throw new ArgumentNullException("itemIds");
            if (itemIds.Any(x => x <= 0))
                throw new ArgumentException("itemId <= 0");

            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAbstractItemByIds, siteId);
            var result = _connection.Query<AbstractItemData>(query, new { Archive = isArchive, ItemIds = itemIds }).ToList();
            _logger.LogDebug($"getByIds. count: {result.Count()}, result: {SerializeData(result)}");
            return result;
        }

        public AbstractItemData GetRootPage(int siteId)
        {
            _logger.LogDebug($"getRootPage. siteId: {siteId}");
            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetRootPage, siteId);
            var result = _connection.Query<AbstractItemData>(query).FirstOrDefault();
            _logger.LogDebug($"getRootPage. result: {SerializeData(result)}");
            return result;
        }

        private static List<AbstractItemData> GetWithRegions(IDbConnection connection, string query, bool isArchive, IEnumerable<int> parentIds = null)
        {
            using (var multi = connection.QueryMultiple(query, new { Archive = isArchive, ParentIds = parentIds }))
            {
                var abstractItems = multi.Read<AbstractItemData>().ToList();
                var regions = multi.Read<RegionData>().ToList();
                var links = multi.Read<LinkItem>()
                    .GroupBy(k => k.ItemId)
                    .ToDictionary(
                        k => k.Key,
                        v => v.Select(x => x.LinkedItemId).ToList());

                foreach (var abstractItem in abstractItems)
                {
                    if (links.TryGetValue(abstractItem.Id, out List<int> regionIds))
                        abstractItem.RegionIds = regions.Where(x => regionIds.Contains(x.Id)).Select(x => x.Id).ToList();
                    else
                        abstractItem.RegionIds = new List<int>();
                }

                return abstractItems;
            }
        }

        private static string SerializeData(object data) => Newtonsoft.Json.JsonConvert.SerializeObject(data);
    }
}
