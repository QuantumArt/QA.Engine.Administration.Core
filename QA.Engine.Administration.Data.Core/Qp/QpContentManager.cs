using Microsoft.Extensions.Logging;
using QA.Engin.Administration.Common.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace QA.Engine.Administration.Data.Core.Qp
{
    /// <summary>
    /// Класс для работы с контентами QP
    /// </summary>
    public class QpContentManager : IQpContentManager
    {
        private readonly ContentDataQueryObject _query;
        private readonly IQpDbConnector _dbConnection;
        private readonly List<string> _includes;
        private readonly ILogger<IQpContentManager> _logger;

        /// <summary>
        /// Название поля ключа записей
        /// </summary>
        protected const string ContentItemIdFieldName = "CONTENT_ITEM_ID";
        protected const string ArchiveFieldName = "ARCHIVE";
        protected const string StatusTypeIdFieldName = "STATUS_TYPE_ID";

        #region Constructors

        /// <summary>
        /// Конструирует объект
        /// </summary>
        public QpContentManager(IQpDbConnector dbConnector, ILogger<IQpContentManager> logger)
        {
            _dbConnection = dbConnector;
            _logger = logger;

            _query = new ContentDataQueryObject(
                null, null, null, null, null, null,
                (long)0, (long)0,
                (byte)0,
                QpContentItemStatus.Published.GetDescription(),
                (byte)0, (byte)0, false, 0.0, false, false);

            _includes = new List<string>();
        }

        #endregion

        #region Prepare properties

        /// <summary>
        /// Устанавливает подключение к QP
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <returns></returns>
        public virtual IQpContentManager Connect()
        {
            _query.DbConnector = _dbConnection.DbConnector;
            return this;
        }

        /// <summary>
        /// Устанавливает подключение к QP
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <returns></returns>
        public virtual IQpContentManager Connection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");
            _query.DbConnector = new DBConnector(connectionString);
            return this;
        }

        /// <summary>
        /// Устанавливает имя сайта
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public virtual IQpContentManager SiteName(string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
                throw new ArgumentNullException("siteName");

            _query.SiteName = siteName;
            return this;
        }

        /// <summary>
        /// Устанавливает название контента
        /// </summary>
        /// <param name="contentName">Имя контента</param>
        /// <returns></returns>
        public virtual IQpContentManager ContentName(string contentName)
        {
            if (string.IsNullOrEmpty(contentName))
                throw new ArgumentNullException("contentName");

            _query.ContentName = contentName;
            return this;
        }

        /// <summary>
        /// Устанавливает список полей
        /// </summary>
        /// <param name="fields">Поля через запятую. * - игнорируется.</param>
        /// <returns></returns>
        public virtual IQpContentManager Fields(string fields)
        {
            if (string.IsNullOrEmpty(fields))
                throw new ArgumentNullException("fields");

            var fieldList = new List<string>();

            var fieldArray = fields.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var f in fieldArray)
            {
                if (!fieldList.Contains(f))
                {
                    fieldList.Add(f);
                }
            }

            _query.Fields = string.Join(",", fieldList);
            return this;
        }

        /// <summary>
        /// Устанавливает фильтр выборки
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual IQpContentManager Where(string where)
        {
            _query.Where = where;
            return this;
        }

        /// <summary>
        /// Устанавливает сортировку
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public virtual IQpContentManager OrderBy(string orderBy)
        {
            _query.OrderBy = orderBy;
            return this;
        }

        /// <summary>
        /// Устанавливает начальный индекс страницы
        /// </summary>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public virtual IQpContentManager StartIndex(long startIndex)
        {
            _query.StartRow = startIndex;
            return this;
        }

        /// <summary>
        /// Устанавливает размер страницы для выборки
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IQpContentManager PageSize(long pageSize)
        {
            _query.PageSize = pageSize;
            return this;
        }

        /// <summary>
        /// Устанаваливает использование расписания
        /// </summary>
        /// <param name="isUseSchedule"></param>
        /// <returns></returns>
        public virtual IQpContentManager IsUseSchedule(bool isUseSchedule)
        {
            _query.UseSchedule = (byte)(isUseSchedule ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Устанавливате статус записей для выборки. Несколько статусов необходимо передавать через запятую.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual IQpContentManager StatusName(QpContentItemStatus status)
        {
            _query.StatusName = status.GetDescription();
            return this;
        }

        /// <summary>
        /// Устанавливате статус записей для выборки. Несколько статусов необходимо передавать через запятую.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual IQpContentManager StatusName(string status)
        {
            _query.Parameters.Clear();
            _query.StatusName = status;
            return this;
        }

        public virtual IQpContentManager ContentId(int contentId)
        {
            _query.ContentId = contentId;
            return this;
        }

        /// <summary>
        /// Признак показа расщепленной версии записи
        /// </summary>
        /// <param name="isShowSplittedArticle"></param>
        /// <returns></returns>
        public virtual IQpContentManager IsShowSplittedArticle(bool isShowSplittedArticle)
        {
            _query.ShowSplittedArticle = (byte)(isShowSplittedArticle ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Устанавливает признак получения архивных записей
        /// </summary>
        /// <param name="isIncludeArchive"></param>
        /// <returns></returns>
        public virtual IQpContentManager IsIncludeArchive(bool isIncludeArchive)
        {
            _query.IncludeArchive = (byte)(isIncludeArchive ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Устанавливает признак кэширования
        /// </summary>
        /// <param name="isCacheResult">Признак кэширования</param>
        /// <returns></returns>
        public virtual IQpContentManager IsCacheResult(bool isCacheResult)
        {
            _query.CacheResult = isCacheResult;
            return this;
        }

        /// <summary>
        /// Устанаваливает признак сброса кэша
        /// </summary>
        /// <param name="isResetCache"></param>
        /// <returns></returns>
        public virtual IQpContentManager IsResetCache(bool isResetCache)
        {
            _query.WithReset = isResetCache;
            return this;
        }

        /// <summary>
        /// Устанавливает интервал кэширования
        /// </summary>
        /// <param name="cacheInterval"></param>
        /// <returns></returns>
        public virtual IQpContentManager CacheInterval(double cacheInterval)
        {
            _query.CacheInterval = cacheInterval;
            return this;
        }

        /// <summary>
        /// Устанавливает использование клиентской выборки
        /// </summary>
        /// <param name="isUseClientSelection"></param>
        /// <returns></returns>
        public virtual IQpContentManager IsUseClientSelection(bool isUseClientSelection)
        {
            _query.UseClientSelection = isUseClientSelection;
            return this;
        }

        /// <summary>
        /// Устанавливает список дополнительных контентов
        /// </summary>
        /// <param name="path">Имя поля</param>
        /// <returns></returns>
        public virtual IQpContentManager Include(string path)
        {
            // TODO: последовательность через точку
            if (_includes.Contains(path))
            {
                return this;
            }

            _includes.Add(path);
            Fields(path);

            return this;
        }

        #endregion

        /// <summary>
        /// Возвращает результат запроса
        /// </summary>
        /// <returns></returns>
        public virtual QpContentResult Get()
        {
            ValidateQuery();

            long totalRecords = 0;
            var result = new QpContentResult
            {
                PrimaryContent = _dbConnection.GetContentData(_query, out totalRecords),
                Query = _query,
                DbConnection = _dbConnection
            };

            result.AddContent(_query.ContentName, result.PrimaryContent);

            GetIncludes(result);

            return result;
        }

        /// <summary>
        /// Возвращает результат запроса
        /// </summary>
        /// <returns></returns>
        public virtual QpContentResult GetRealData()
        {
            ValidateQuery();

            if (string.IsNullOrEmpty(_query.Fields))
                throw new ArgumentNullException("Query.Fields");

            var command = new SqlCommand();
            command.CommandText = "SELECT " + _query.Fields +
                " FROM " + _query.ContentName +
                (string.IsNullOrEmpty(_query.Where) ? "" : (" WHERE " + _query.Where));

            var result = new QpContentResult
            {
                PrimaryContent = _dbConnection.GetRealData(command),
                Query = _query,
                DbConnection = _dbConnection
            };

            result.AddContent(_query.ContentName, result.PrimaryContent);

            return result;
        }

        /// <summary>
        /// Получает связанные контенты
        /// </summary>
        /// <param name="result">Результат</param>
        protected virtual void GetIncludes(QpContentResult result)
        {
            if (_includes.Count > 0)
            {
                var attributes = new QpMetadataManager(_dbConnection)
                    .GetContentAttributes(
                        _query.SiteName,
                        _query.ContentName);

                foreach (var path in _includes)
                {
                    var attr = attributes.Where(w => w.Name == path).SingleOrDefault();
                    string contentName = _dbConnection.GetContentName(attr.RelatedContentId.Value);

                    string values = string.Empty;

                    if (attr.LinkId == null)
                    {
                        foreach (DataRow row in result.PrimaryContent.Rows)
                        {
                            if (contentName == _query.ContentName & !string.IsNullOrEmpty(row[path].ToString()))
                            {
                                if (result.PrimaryContent.Select(ContentItemIdFieldName + " = " + row[path].ToString()).Any())
                                {
                                    continue;
                                }
                            }

                            if (!string.IsNullOrEmpty(values) && !values.EndsWith(", "))
                            {
                                values += ", ";
                            }

                            string val = row[path].ToString();
                            if (!string.IsNullOrEmpty(val))
                            {
                                values += val;
                            }
                        }
                    }
                    else
                    {
                        foreach (DataRow row in result.PrimaryContent.Rows)
                        {
                            if (!string.IsNullOrEmpty(values))
                            {
                                values += ", ";
                            }

                            string val = row[ContentItemIdFieldName].ToString();
                            if (!string.IsNullOrEmpty(val))
                            {
                                values += val;
                            }
                        }

                        if (!string.IsNullOrEmpty(values) && !string.IsNullOrWhiteSpace(values))
                        {
                            values = _dbConnection.GetContentItemLinkIDs(attr.Name, values);
                        }
                    }

                    values = values.Trim();
                    if (values.EndsWith(","))
                    {
                        values = values.Substring(0, values.Length - 1);
                    }

                    if (!string.IsNullOrEmpty(values))
                    {
                        long total = 0;

                        var query = new ContentDataQueryObject(
                            _dbConnection.DbConnector, _query.SiteName, contentName, "*",
                            string.Format(ContentItemIdFieldName + " in ({0})", values), null,
                            (long)0, (long)0,
                            (byte)0,
                            QpContentItemStatus.Published.GetDescription(),
                            (byte)0, (byte)0, false, 0.0, false, false);

                        var contentData = _dbConnection.GetContentData(query, out total);
                        var existsContentData = result.GetContent(contentName);

                        if (existsContentData != null)
                        {
                            //NOTE: this line doesn't coverage unit tests
                            existsContentData.Merge(
                                contentData);
                        }

                        result.AddContent(contentName, contentData);
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет правильность запроса
        /// </summary>
        protected virtual void ValidateQuery()
        {
            if (_dbConnection == null)
                throw new ArgumentNullException("DbConnection");
            if (_query.DbConnector == null)
                throw new ArgumentNullException("Query.DbConnector");
            if (_query.SiteName == null)
                throw new ArgumentNullException("Query.SiteName");
            if (_query.ContentName == null)
                throw new ArgumentNullException("Query.ContentName");
        }

        /// <summary>
        /// Отправляет элементы в архив
        /// </summary>
        public void Archive(int userId)
        {
            ValidateQuery();

            var values = new List<Dictionary<string, string>>();
            var dataTable = _dbConnection.GetContentData(_query, out var totalRecords);

            _logger.LogTrace($"archive. get content data. totalRecords: {totalRecords}, data rows: {SerializeData(dataTable.Rows)}");

            foreach (DataRow row in dataTable.Rows)
            {
                values.Add(new Dictionary<string, string>
                {
                    { ContentItemIdFieldName, row[ContentItemIdFieldName].ToString() },
                    { ArchiveFieldName, "1" }
                });
            }

            if (values.Any())
            {
                _logger.LogDebug($"archive. mass update. contentId: {_query.ContentId}, values: {SerializeData(values)}");
                _query.DbConnector.MassUpdate(_query.ContentId, values, userId);
            }
        }

        /// <summary>
        /// Восстанавливает элементы из архива
        /// </summary>
        public void Restore(int userId)
        {
            ValidateQuery();

            var values = new List<Dictionary<string, string>>();
            var dataTable = _dbConnection.GetContentData(_query, out var totalRecords);

            _logger.LogTrace($"restore. get content data. totalRecords: {totalRecords}, data rows: {SerializeData(dataTable.Rows)}");

            foreach (DataRow row in dataTable.Rows)
            {
                values.Add(new Dictionary<string, string>
                {
                    { ContentItemIdFieldName, row[ContentItemIdFieldName].ToString() },
                    { ArchiveFieldName, "0" }
                });
            }

            if (values.Any())
            {
                _logger.LogDebug($"restore. mass update. contentId: {_query.ContentId}, values: {SerializeData(values)}");
                _query.DbConnector.MassUpdate(_query.ContentId, values, userId);
            }
        }

        /// <summary>
        /// Удаляет элемент из БД
        /// </summary>
        public void Delete(int userId)
        {
            ValidateQuery();

            var values = new List<Dictionary<string, string>>();
            var dataTable = _dbConnection.GetContentData(_query, out var totalRecords);

            _logger.LogTrace($"delete. get content data. totalRecords: {totalRecords}, data rows: {SerializeData(dataTable.Rows)}");

            foreach (DataRow row in dataTable.Rows)
            {
                var id = int.Parse(row[ContentItemIdFieldName].ToString());
                _logger.LogDebug($"delete. id: {id}");
                _dbConnection.DeleteContentItem(id);
            }
        }

        /// <summary>
        /// Изменяет статус элементов
        /// </summary>
        public void ChangeStatus(int userId, int statusId)
        {
            ValidateQuery();

            var values = new List<Dictionary<string, string>>();
            var dataTable = _dbConnection.GetContentData(_query, out var totalRecords);

            _logger.LogTrace($"changeStatus. get content data. totalRecords: {totalRecords}, data rows: {SerializeData(dataTable.Rows)}");

            foreach (DataRow row in dataTable.Rows)
            {
                values.Add(new Dictionary<string, string>
                {
                    { ContentItemIdFieldName, row[ContentItemIdFieldName].ToString() },
                    { StatusTypeIdFieldName, statusId.ToString(CultureInfo.InvariantCulture) }
                });
            }

            if (values.Any())
            {
                _logger.LogDebug($"changeStatus. mass update. contentId: {_query.ContentId}, values: {SerializeData(values)}");
                _query.DbConnector.MassUpdate(_query.ContentId, values, userId);
            }
        }

        private static string SerializeData(object data) => Newtonsoft.Json.JsonConvert.SerializeObject(data);

    }
}
