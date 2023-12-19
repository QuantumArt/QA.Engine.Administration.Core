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
        /// <param name="query">Запрос</param>
        /// <param name="totalRecords">Общее количество строк</param>
        /// <returns></returns>
        DataTable GetContentData(ContentDataQueryObject query, out long totalRecords);

        /// <summary>
        /// Возвращает данные контента
        /// </summary>
        /// <param name="command">Команда к БД</param>
        /// <returns></returns>
        DataTable GetRealData(DbCommand command);

        /// <summary>
        /// Вовзращает имя контента
        /// </summary>
        /// <param name="contentId">Идентификатор контента</param>
        /// <returns></returns>
        string GetContentName(int contentId);

        /// <summary>
        /// Возвращает идентификаторы связанных записей
        /// </summary>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="values">Ид'ы записей, для которой необходимо получить данные</param>
        /// <returns></returns>
        string GetContentItemLinkIDs(string fieldName, string values);

        DbCommand CreateCommand(string text);
    }
}
