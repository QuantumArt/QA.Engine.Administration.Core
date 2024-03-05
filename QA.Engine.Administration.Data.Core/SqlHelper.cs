using QA.DotNetCore.Engine.Persistent.Dapper;
using QA.DotNetCore.Engine.Persistent.Interfaces;

namespace QA.Engine.Administration.Data.Core;

public class SqlHelper
{
    private readonly IUnitOfWork _uow;

    public SqlHelper(IUnitOfWork uow)
    {
        _uow = uow;
    }
    
    public string GetBool(bool value)
    {
        return SqlQuerySyntaxHelper.ToBoolSql(_uow.DatabaseType, value);
    }
}