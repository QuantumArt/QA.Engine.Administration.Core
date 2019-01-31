using Dapper;
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

        public SiteMapProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer, IMetaInfoRepository metaInfoRepository)
        {
            _connection = uow.Connection;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
            _metaInfoRepository = metaInfoRepository;
        }

        //запрос с использованием NetName таблиц и столбцов
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
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE ai.archive=@Archive AND ai.visible=1
ORDER BY ai.[|QPAbstractItem.Parent|], ai.[|QPAbstractItem.IndexOrder|], ai.content_item_id
";

        private const string CmdGetAbstractPageItems = @"
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
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE ai.archive=@Archive AND ai.content_item_id IN @ItemIds
";
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
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE ai.archive=0 AND ai.[|QPAbstractItem.Parent|] IS NULL AND ai.[|QPAbstractItem.VersionOf|] IS NULL AND def.[|QPDiscriminator.IsPage|]=1
ORDER BY ai.content_item_id";

        public string AbstractItemNetName => "QPAbstractItem";

        public IEnumerable<AbstractItemData> GetAllItems(int siteId, bool isArchive)
        {
            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAbstractItems, siteId);
            return _connection.Query<AbstractItemData>(query, new { Archive = isArchive });
        }

        public IEnumerable<AbstractItemData> GetItems(int siteId, bool isArchive, IEnumerable<int> parentIds)
        {
            const int maxParentIdsPerRequest = 500;

            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAbstractPageItems, siteId);

            if (parentIds == null || !parentIds.Any())
            {
                query = string.Format(query, "IS NULL");
                return _connection.Query<AbstractItemData>(query, new { Archive = isArchive });
            }
            else
            {
                if (parentIds.Count() > maxParentIdsPerRequest)
                {
                    var result = new List<AbstractItemData>();
                    for (var i = 0; i < (float)parentIds.Count() / maxParentIdsPerRequest; i++)
                    {
                        int[] r = parentIds.Skip(i * maxParentIdsPerRequest).Take(maxParentIdsPerRequest).ToArray();
                        result.AddRange(GetItems(siteId, isArchive, r));
                    }
                    return result;
                }
                else
                {
                    query = string.Format(query, "IN @ParentIds");
                    return _connection.Query<AbstractItemData>(query, new { Archive = isArchive, ParentIds = parentIds });
                }
            }
        }

        public IEnumerable<AbstractItemData> GetByIds(int siteId, bool isArchive, IEnumerable<int> itemIds)
        {
            if (itemIds == null || !itemIds.Any())
                throw new ArgumentNullException("itemIds");
            if (itemIds.Any(x => x <= 0))
                throw new ArgumentException("itemId <= 0");

            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetAbstractItemByIds, siteId);
            return _connection.Query<AbstractItemData>(query, new { Archive = isArchive, ItemIds = itemIds });
        }

        public int GetContentId(int siteId)
        {
            var content = _metaInfoRepository.GetContent(AbstractItemNetName, siteId);
            return content.ContentId;
        }

        public AbstractItemData GetRootPage(int siteId)
        {
            var query = _netNameQueryAnalyzer.PrepareQueryExtabtion(_metaInfoRepository, CmdGetRootPage, siteId);
            return _connection.Query<AbstractItemData>(query).FirstOrDefault();
        }

    }
}
