using System;
using System.Data;

namespace QA.Engine.Administration.Data.Core.Qp
{
    public class QpMetadataManager : IQpMetadataManager
    {
        private readonly IQpDbConnector _qpDbConnector;

        #region Constructors

        public QpMetadataManager(IQpDbConnector qpDbConnector)
        {
            _qpDbConnector = qpDbConnector;
        }

        #endregion

        /// <summary>
        /// Возвращает результат запроса
        /// </summary>
        /// <returns></returns>
        public virtual DataTable GetRealData(string tableName, string fields, string where = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));
            if (string.IsNullOrWhiteSpace(fields))
                throw new ArgumentNullException(nameof(fields));

            var filterFields = string.Join(",", fields.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries));
            var localWhere = string.IsNullOrWhiteSpace(where) ? "1 = 1" : where;
            var command = _qpDbConnector.CreateCommand($"SELECT {filterFields} FROM {tableName} WHERE {localWhere}");
            return _qpDbConnector.GetRealData(command);
        }
    }
}
