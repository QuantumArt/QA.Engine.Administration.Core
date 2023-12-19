using Dapper;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NLog;
using QA.DotNetCore.Engine.Persistent.Dapper;

namespace QA.Engine.Administration.Data.Core
{
    public class WidgetProvider : IWidgetProvider
    {
        private readonly IUnitOfWork _uow;
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public WidgetProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer)
        {
            _uow = uow;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
        }

        //запрос с использованием NetName таблиц и столбцов
        private string CmdGetAbstractWidgetItem(string parentExpression)
        {
            return $@"
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
    def.content_item_id AS DiscriminatorId,
    def.|QPDiscriminator.Name| as Discriminator,
    def.|QPDiscriminator.IsPage| as IsPage,
    CASE WHEN ai.STATUS_TYPE_ID IN (SELECT st.STATUS_TYPE_ID FROM STATUS_TYPE st WHERE st.STATUS_TYPE_NAME=N'Published') THEN 1 ELSE 0 END AS Published
FROM |QPAbstractItem| ai
INNER JOIN |QPDiscriminator| def on ai.|QPAbstractItem.Discriminator| = def.content_item_id
WHERE ai.archive={SqlQuerySyntaxHelper.ToBoolSql(_uow.DatabaseType, false)} AND def.|QPDiscriminator.IsPage|={SqlQuerySyntaxHelper.ToBoolSql(_uow.DatabaseType, false)} AND ai.|QPAbstractItem.Parent| {parentExpression}
";
        }

        public List<AbstractItemData> GetItems(int siteId, bool isArchive, IEnumerable<int> parentIds, IDbTransaction transaction = null)
        {
            if (parentIds == null)
                throw new ArgumentNullException(nameof(parentIds), "need not null and not empty parent ids");

            var parentIdsArr = parentIds.ToArray();
            _logger.ForDebugEvent().Message("GetItems")
                .Property("siteId", siteId)
                .Property("isArchive", isArchive)
                .Property("parentIds", parentIdsArr)
                .Log();

            if (!parentIdsArr.Any()) return new List<AbstractItemData>();

            const int maxParentIdsPerRequest = 500;

            var idList = SqlQuerySyntaxHelper.IdList(_uow.DatabaseType, "@ParentIds", "parentIds");
            var cmd = CmdGetAbstractWidgetItem($"IN (SELECT Id FROM {idList})");
            var query = _netNameQueryAnalyzer.PrepareQuery(cmd, siteId, false, true);
            var result = new List<AbstractItemData>();
            if (parentIdsArr.Length > maxParentIdsPerRequest)
            {
                for (var i = 0; i < (float) parentIdsArr.Length / maxParentIdsPerRequest; i++)
                {
                    var r = parentIdsArr.Skip(i * maxParentIdsPerRequest).Take(maxParentIdsPerRequest).ToArray();
                    result.AddRange(GetItems(siteId, isArchive, r));
                }
            }
            else
            {
                var sql = string.Format(query, SqlQuerySyntaxHelper.ToBoolSql(_uow.DatabaseType, isArchive));
                var items = _uow.Connection.Query<AbstractItemData>(sql, new
                {
                    Archive = isArchive,
                    ParentIds = parentIdsArr
                }, transaction);
                result.AddRange(items);
            }
            _logger.ForDebugEvent().Message("GetItems").Property("count", result.Count).Log();
            return result;
        }

    }
}
