using System.Data;

namespace QA.Engine.Administration.Data.Core.Qp
{
    /// <summary>
    /// Контракт для работы с БД QP
    /// </summary>
    public interface IQpMetadataManager
    {
        DataTable GetRealData(string tableName, string fields, string where);
   }
}
