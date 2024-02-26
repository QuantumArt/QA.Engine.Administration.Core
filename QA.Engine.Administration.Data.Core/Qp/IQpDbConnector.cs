using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using System.Data;
using System.Data.Common;

namespace QA.Engine.Administration.Data.Core.Qp
{
    /// <summary>
    /// Контракт подключения к БД Qp
    /// </summary>
    public interface IQpDbConnector
    {
        /// <summary>
        /// DbConnector
        /// </summary>
        DBConnector DbConnector { get; }

        /// <summary>
        /// Возвращает данные контента
        /// </summary>
        /// <param name="command">Команда к БД</param>
        /// <returns></returns>
        DataTable GetRealData(DbCommand command);

        DbCommand CreateCommand(string text);
    }
}
