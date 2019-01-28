﻿using Dapper;
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
        private const string CmdGetAbstractItem = @"
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
    ai.[|QPAbstractItem.IsInSiteMap|] AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.[|QPDiscriminator.Name|] as Discriminator,
    def.[|QPDiscriminator.IsPage|] as IsPage,
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
";

        private const string CmdGetAbstractPageItem = @"
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
    ai.[|QPAbstractItem.IsInSiteMap|] AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.[|QPDiscriminator.Name|] as Discriminator,
    def.[|QPDiscriminator.IsPage|] as IsPage,
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE def.[|QPDiscriminator.IsPage|]=1 AND ai.[|QPAbstractItem.VersionOf|] is null 
    AND (
        ai.[|QPAbstractItem.Parent|] {0}
        OR EXISTS (SELECT 1 FROM [|QPAbstractItem|] ai1 
            WHERE ai.[|QPAbstractItem.Parent|] {0} AND ai1.content_item_id=ai.[|QPAbstractItem.VersionOf|])
    )
";
        private const string CmdGetAbstractItemById = @"
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
    ai.[|QPAbstractItem.IsInSiteMap|] AS IsInSiteMap,
    def.content_item_id AS DiscriminatorId,
    def.[|QPDiscriminator.Name|] as Discriminator,
    def.[|QPDiscriminator.IsPage|] as IsPage,
    CASE WHEN ai.[STATUS_TYPE_ID] = (SELECT TOP 1 st.STATUS_TYPE_ID FROM [STATUS_TYPE] st WHERE st.[STATUS_TYPE_NAME]=N'Published') THEN 1 ELSE 0 END AS Published
FROM [|QPAbstractItem|] ai
INNER JOIN [|QPDiscriminator|] def on ai.[|QPAbstractItem.Discriminator|] = def.content_item_id
WHERE ai.content_item_id IN @ItemIds
";

        public string AbstractItemNetName => "QPAbstractItem";

        public IEnumerable<AbstractItemData> GetAllItems(int siteId, bool isStage)
        {
            var query = _netNameQueryAnalyzer.PrepareQuery(CmdGetAbstractItem, siteId, isStage);
            return _connection.Query<AbstractItemData>(query).ToList();
        }

        public IEnumerable<AbstractItemData> GetItems(int siteId, bool isStage, IEnumerable<int> parentIds)
        {
            const int maxParentIdsPerRequest = 500;

            var query = _netNameQueryAnalyzer.PrepareQuery(CmdGetAbstractPageItem, siteId, isStage);

            if (parentIds == null || !parentIds.Any())
            {
                query = string.Format(query, "IS NULL");
                return _connection.Query<AbstractItemData>(query);
            }
            else
            {
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

        public IEnumerable<AbstractItemData> GetByIds(int siteId, bool isStage, IEnumerable<int> itemIds)
        {
            if (itemIds == null || !itemIds.Any())
                throw new ArgumentNullException("itemIds");
            if (itemIds.Any(x => x <= 0))
                throw new ArgumentException("itemId <= 0");

            var query = _netNameQueryAnalyzer.PrepareQuery(CmdGetAbstractItemById, siteId, isStage);
            return _connection.Query<AbstractItemData>(query, new { ItemIds = itemIds });
        }

        public int GetContentId(int siteId)
        {
            var content = _metaInfoRepository.GetContent(AbstractItemNetName, siteId);
            return content.ContentId;
        }

    }
}
