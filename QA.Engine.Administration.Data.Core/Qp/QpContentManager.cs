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
        /// <summary>
        /// Название поля ключа записей
        /// </summary>
        protected const string ContentItemIdFieldName = "CONTENT_ITEM_ID";

        #region Properties

        /// <summary>
        /// Подключение к QP
        /// </summary>
        public IQpDbConnector DbConnection { get; protected set; }

        /// <summary>
        /// Элемент контента
        /// </summary>
        public IQpContentItem ContentItem { get; protected set; }

        /// <summary>
        /// Запрос
        /// </summary>
        public ContentDataQueryObject Query { get; protected set; }

        /// <summary>
        /// Список связанных контентов
        /// </summary>
        public List<string> Includes { get; protected set; }

        /// <summary>
        /// Список полей
        /// </summary>
        public List<string> FieldList { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Конструирует объект
        /// </summary>
        public QpContentManager(IQpDbConnector dbConnector, IQpContentItem contentItem)
        {
            DbConnection = dbConnector;
            ContentItem = contentItem;

            Query = new ContentDataQueryObject(
                null, null, null, null, null, null,
                (long)0, (long)0,
                (byte)0,
                QpContentItemStatus.Published.GetDescription(),
                (byte)0, (byte)0, false, 0.0, false, false);

            Includes = new List<string>();
        }

        #endregion

        #region Methods

        #region Prepare properties

        /// <summary>
        /// Устанавливает подключение к QP
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <returns></returns>
        public virtual IQpContentManager Connection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");
            Query.DbConnector = new DBConnector(connectionString);
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

            Query.SiteName = siteName;
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

            Query.ContentName = contentName;
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

            if (FieldList == null)
            {
                FieldList = new List<string>();
            }

            var fieldArray = fields.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var f in fieldArray)
            {
                if (!FieldList.Contains(f))
                {
                    FieldList.Add(f);
                }
            }

            Query.Fields = string.Join(",", FieldList);
            return this;
        }

        /// <summary>
        /// Устанавливает фильтр выборки
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual IQpContentManager Where(string where)
        {
            Query.Where = where;
            return this;
        }

        /// <summary>
        /// Устанавливает сортировку
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public virtual IQpContentManager OrderBy(string orderBy)
        {
            Query.OrderBy = orderBy;
            return this;
        }

        /// <summary>
        /// Устанавливает начальный индекс страницы
        /// </summary>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public virtual IQpContentManager StartIndex(long startIndex)
        {
            Query.StartRow = startIndex;
            return this;
        }

        /// <summary>
        /// Устанавливает размер страницы для выборки
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IQpContentManager PageSize(long pageSize)
        {
            Query.PageSize = pageSize;
            return this;
        }

        /// <summary>
        /// Устанаваливает использование расписания
        /// </summary>
        /// <param name="isUseSchedule"></param>
        /// <returns></returns>
        public virtual IQpContentManager IsUseSchedule(bool isUseSchedule)
        {
            Query.UseSchedule = (byte)(isUseSchedule ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Устанавливате статус записей для выборки. Несколько статусов необходимо передавать через запятую.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual IQpContentManager StatusName(QpContentItemStatus status)
        {
            Query.StatusName = status.GetDescription();
            return this;
        }

        /// <summary>
        /// Устанавливате статус записей для выборки. Несколько статусов необходимо передавать через запятую.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual IQpContentManager StatusName(string status)
        {
            Query.Parameters.Clear();
            Query.StatusName = status;
            return this;
        }

        public virtual IQpContentManager ContentId(int contentId)
        {
            Query.ContentId = contentId;
            return this;
        }

        /// <summary>
        /// Признак показа расщепленной версии записи
        /// </summary>
        /// <param name="isShowSplittedArticle"></param>
        /// <returns></returns>
        public virtual IQpContentManager IsShowSplittedArticle(bool isShowSplittedArticle)
        {
            Query.ShowSplittedArticle = (byte)(isShowSplittedArticle ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Устанавливает признак получения архивных записей
        /// </summary>
        /// <param name="isIncludeArchive"></param>
        /// <returns></returns>
        public virtual IQpContentManager IsIncludeArchive(bool isIncludeArchive)
        {
            Query.IncludeArchive = (byte)(isIncludeArchive ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Устанавливает признак кэширования
        /// </summary>
        /// <param name="isCacheResult">Признак кэширования</param>
        /// <returns></returns>
        public virtual IQpContentManager IsCacheResult(bool isCacheResult)
        {
            Query.CacheResult = isCacheResult;
            return this;
        }

        /// <summary>
        /// Устанаваливает признак сброса кэша
        /// </summary>
        /// <param name="isResetCache"></param>
        /// <returns></returns>
        public virtual IQpContentManager IsResetCache(bool isResetCache)
        {
            Query.WithReset = isResetCache;
            return this;
        }

        /// <summary>
        /// Устанавливает интервал кэширования
        /// </summary>
        /// <param name="cacheInterval"></param>
        /// <returns></returns>
        public virtual IQpContentManager CacheInterval(double cacheInterval)
        {
            Query.CacheInterval = cacheInterval;
            return this;
        }

        /// <summary>
        /// Устанавливает использование клиентской выборки
        /// </summary>
        /// <param name="isUseClientSelection"></param>
        /// <returns></returns>
        public virtual IQpContentManager IsUseClientSelection(bool isUseClientSelection)
        {
            Query.UseClientSelection = isUseClientSelection;
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
            if (Includes.Contains(path))
            {
                return this;
            }

            Includes.Add(path);
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
                PrimaryContent = DbConnection.GetContentData(Query, out totalRecords),
                Query = Query,
                DbConnection = DbConnection
            };

            result.AddContent(Query.ContentName, result.PrimaryContent);

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

            if (string.IsNullOrEmpty(Query.Fields))
                throw new ArgumentNullException("Query.Fields");

            var command = new SqlCommand();
            command.CommandText = "SELECT " + Query.Fields +
                " FROM " + Query.ContentName +
                (string.IsNullOrEmpty(Query.Where) ? "" : (" WHERE " + Query.Where));

            var result = new QpContentResult
            {
                PrimaryContent = DbConnection.GetRealData(command),
                Query = Query,
                DbConnection = DbConnection
            };

            result.AddContent(Query.ContentName, result.PrimaryContent);

            return result;
        }

        /// <summary>
        /// Получает связанные контенты
        /// </summary>
        /// <param name="result">Результат</param>
        protected virtual void GetIncludes(QpContentResult result)
        {
            if (Includes.Count > 0)
            {
                var attributes = new QpMetadataManager(DbConnection)
                    .GetContentAttributes(
                        Query.SiteName,
                        Query.ContentName);

                foreach (var path in Includes)
                {
                    var attr = attributes.Where(w => w.Name == path).SingleOrDefault();
                    string contentName = DbConnection.GetContentName(attr.RelatedContentId.Value);

                    string values = string.Empty;

                    if (attr.LinkId == null)
                    {
                        foreach (DataRow row in result.PrimaryContent.Rows)
                        {
                            if (contentName == Query.ContentName & !string.IsNullOrEmpty(row[path].ToString()))
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
                            values = DbConnection.GetContentItemLinkIDs(attr.Name, values);
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
                            DbConnection.DbConnector, Query.SiteName, contentName, "*",
                            string.Format(ContentItemIdFieldName + " in ({0})", values), null,
                            (long)0, (long)0,
                            (byte)0,
                            QpContentItemStatus.Published.GetDescription(),
                            (byte)0, (byte)0, false, 0.0, false, false);

                        var contentData = DbConnection.GetContentData(query, out total);
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
            if (DbConnection == null)
                throw new ArgumentNullException("DbConnection");
            if (ContentItem == null)
                throw new ArgumentNullException("ContentItem");
            if (Query.DbConnector == null)
                throw new ArgumentNullException("Query.DbConnector");
            if (Query.SiteName == null)
                throw new ArgumentNullException("Query.SiteName");
            if (Query.ContentName == null)
                throw new ArgumentNullException("Query.ContentName");
        }

        /// <summary>
        /// Отправляет элементы в архив
        /// </summary>
        public void Archive()
        {
            ValidateQuery();

            var items = DbConnection.GetContentData(Query, out var totalRecords);
            foreach (DataRow row in items.Rows)
            {
                var item = ContentItem.Read(
                    int.Parse(row["CONTENT_ITEM_ID"].ToString()),
                    DbConnection);
                item.Archive = true;
                item.Save();
            }
        }

        /// <summary>
        /// Восстанавливает элементы из архива
        /// </summary>
        public void Restore()
        {
            ValidateQuery();

            var items = DbConnection.GetContentData(Query, out var totalRecords);
            foreach (DataRow row in items.Rows)
            {
                var item = ContentItem.Read(
                    int.Parse(row["CONTENT_ITEM_ID"].ToString()),
                    DbConnection);
                item.Archive = false;
                item.Save();
            }
        }

        /// <summary>
        /// Отправляет элементы в архив
        /// </summary>
        public void Delete()
        {
            ValidateQuery();

            var items = DbConnection.GetContentData(Query, out var totalRecords);
            foreach (DataRow row in items.Rows)
            {
                var item = ContentItem.Read(
                    int.Parse(row["CONTENT_ITEM_ID"].ToString()),
                    DbConnection);
                DbConnection.DeleteContentItem(item.Id);
            }
        }

        /// <summary>
        /// ChangeStatus
        /// </summary>
        /// <param name="status"></param>
        public void ChangeStatus(int statusId)
        {
            ValidateQuery();

            var values = new List<Dictionary<string, string>>();
            var items = DbConnection.GetContentData(Query, out var totalRecords);
            foreach (DataRow row in items.Rows)
                values.Add(new Dictionary<string, string> {
                    { "CONTENT_ITEM_ID", row["CONTENT_ITEM_ID"].ToString() },
                    { "STATUS_TYPE_ID", statusId.ToString(CultureInfo.InvariantCulture) }
                });
            Query.DbConnector.MassUpdate(Query.ContentId, values, 1);
        }

        #endregion

    }
}
