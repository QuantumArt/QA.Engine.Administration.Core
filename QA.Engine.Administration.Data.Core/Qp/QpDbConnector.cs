using Quantumart.QPublishing.Database;
using System;
using System.Data;
using System.Data.Common;

namespace QA.Engine.Administration.Data.Core.Qp
{
    /// <summary>
    /// Реализация подключения к БД Qp
    /// </summary>
    public class QpDbConnector : IQpDbConnector
    {

        /// <summary>
        /// Подключения к БД Qp
        /// </summary>
        public DBConnector DbConnector { get; private set; }

        /// <summary>
        /// Конструирует объект
        /// </summary>
        /// <param name="connection">Подключение</param>
        public QpDbConnector(IDbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }
            DbConnector = new DBConnector(connection);
        }

        /// <summary>
        /// Возвращает данные контента
        /// </summary>
        /// <param name="command">Команда к БД</param>
        /// <returns></returns>
        public DataTable GetRealData(DbCommand command)
        {
            return DbConnector.GetRealData(command);
        }

        public DbCommand CreateCommand(string text)
        {
            return DbConnector.CreateDbCommand(text);
        }
    }
}
