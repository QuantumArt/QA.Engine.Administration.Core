using Dapper;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace QA.Engine.Administration.Data.Core
{
    public class StatusTypeProvider : IStatusTypeProvider
    {
        private readonly IDbConnection _connection;
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;

        public StatusTypeProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer)
        {
            _connection = uow.Connection;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
        }

        private const string CmdGetAll = @"
SELECT
    STATUS_TYPE_ID as Id,
    STATUS_TYPE_NAME as Name,
    WEIGHT as Weight,
    DESCRIPTION as Description
FROM [STATUS_TYPE]
WHERE [SITE_ID]=@SiteId
";
        private const string CmdGetById = @"
SELECT
    STATUS_TYPE_ID as Id,
    STATUS_TYPE_NAME as Name,
    WEIGHT as Weight,
    DESCRIPTION as Description
FROM [STATUS_TYPE]
WHERE [SITE_ID]=@SiteId AND [STATUS_TYPE_NAME]=@Status
";

        public IEnumerable<StatusTypeData> GetAll(int siteId)
        {
            return _connection.Query<StatusTypeData>(CmdGetAll, new { SiteId = siteId });
        }

        public StatusTypeData GetStatus(int siteId, QpContentItemStatus status)
        {
            return _connection.QueryFirst<StatusTypeData>(CmdGetById, new { SiteId = siteId, Status = status.GetDescription() });
        }
    }
}
