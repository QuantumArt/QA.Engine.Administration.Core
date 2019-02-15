using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using System;
using System.Data;
using System.Data.SqlClient;

namespace QA.Engine.Administration.Data.Core.Qp
{
    /// <summary>
    /// Реализация подключения к БД Qp
    /// </summary>
    public class QpDbConnector : IQpDbConnector
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;

        /// <summary>
        /// Подключения к БД Qp
        /// </summary>
        public DBConnector DbConnector { get; private set; }

        /// <summary>
        /// Конструирует объект
        /// </summary>
        /// <param name="connectionString">Название подключения</param>
        public QpDbConnector(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            _connection = new SqlConnection(connectionString);
            DbConnector = new DBConnector(_connection);
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
        public DataTable GetRealData(SqlCommand command)
        {
            return DbConnector.GetRealData(command);
        }

        /// <summary>
        /// Строка подключения
        /// </summary>
        public string InstanceConnectionString
        {
            get
            {
                return DbConnector.InstanceConnectionString;
            }
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
        /// <param name="itemId">Ид. записи, для которой необходимо получить данные</param>
        /// <returns></returns>
        public string GetContentItemLinkIDs(string fieldName, int itemId)
        {
            return DbConnector.GetContentItemLinkIDs(fieldName, itemId);
        }

        /// <summary>
        /// Удаляет элемент из БД
        /// </summary>
        /// <param name="contentItemId"></param>
        public void DeleteContentItem(int contentItemId)
        {
            DbConnector.DeleteContentItem(contentItemId);
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

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_transaction != null)
                _transaction.Rollback();
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            _transaction = _connection.BeginTransaction(isolationLevel);
            DbConnector.ExternalTransaction = _transaction;
        }

        public void CommitTransaction()
        {
            if (_transaction == null)
                return;
            _transaction.Commit();
            _transaction = null;
        }

        public void RollbackTransaction()
        {
            if (_transaction == null)
                return;
            _transaction.Rollback();
            _transaction = null;
        }
    }
}
