using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
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
        /// <param name="query">Запрос</param>
        /// <param name="totalRecords">Общее количество строк</param>
        /// <returns></returns>
        public DataTable GetContentData(ContentDataQueryObject query, out long totalRecords)
        {
            return DbConnector.GetContentData(query, out totalRecords);
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

        /// <summary>
        /// Вовзращает имя контента
        /// </summary>
        /// <param name="contentId">Идентификатор контента</param>
        /// <returns></returns>
        public string GetContentName(int contentId)
        {
            return DbConnector.GetContentName(contentId);
        }

        /// <summary>
        /// Возвращает идентификаторы связанных записей
        /// </summary>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="values">Ид'ы записей, для которой необходимо получить данные</param>
        /// <returns></returns>
        public string GetContentItemLinkIDs(string fieldName, string values)
        {
            return DbConnector.GetContentItemLinkIDs(fieldName, values);
        }
        
        public DbCommand CreateCommand(string text)
        {
            return DbConnector.CreateDbCommand(text);
        }
    }
}
