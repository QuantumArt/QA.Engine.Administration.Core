using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using System.Data;
using System.Data.SqlClient;

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
        DataTable GetRealData(SqlCommand command);

        /// <summary>
        /// Строка подключения
        /// </summary>
        string InstanceConnectionString { get; }

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
        /// <param name="itemId">Ид. записи, для которой необходимо получить данные</param>
        /// <returns></returns>
        string GetContentItemLinkIDs(string fieldName, int itemId);

        /// <summary>
        /// Удаляет элемент из БД
        /// </summary>
        /// <param name="contentItemId"></param>
        void DeleteContentItem(int contentItemId);

        /// <summary>
        /// Возвращает идентификаторы связанных записей
        /// </summary>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="values">Ид'ы записей, для которой необходимо получить данные</param>
        /// <returns></returns>
        string GetContentItemLinkIDs(string fieldName, string values);

        void BeginTransaction(IsolationLevel isolationLevel);
        void CommitTransaction();
        void RollbackTransaction();
    }
}
