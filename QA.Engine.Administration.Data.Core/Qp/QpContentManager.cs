﻿using QA.Engine.Administration.Common.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using System;
using System.Collections.Generic;
using System.Data;
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


        /// <summary>
        /// Название поля ключа записей
        /// </summary>
        private const string ContentItemIdFieldName = "CONTENT_ITEM_ID";

        #region Constructors

        /// <summary>
        /// Конструирует объект
        /// </summary>
        public QpContentManager(IQpDbConnector dbConnector)
        {
            _dbConnection = dbConnector;

            _query = new ContentDataQueryObject(
                null, null, null, null, null, null,
                0, 0,
                0,
                QpContentItemStatus.Published.GetDescription(),
                0, 0, false, 0.0, false, false)
            {
                GetCount = false
            };

            _includes = new List<string>();
        }

        #endregion

        #region Prepare properties

        /// <summary>
        /// Устанавливает подключение к QP
        /// </summary>
        /// <returns></returns>
        public virtual IQpContentManager Connect()
        {
            _query.DbConnector = _dbConnection.DbConnector;
            return this;
        }

        /// <summary>
        /// Устанавливает подключение к QP
        /// </summary>
        /// <param name="connection">подключение</param>
        /// <returns></returns>
        public virtual IQpContentManager Connection(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            _query.DbConnector = new DBConnector(connection);
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
                throw new ArgumentNullException(nameof(siteName));

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
                throw new ArgumentNullException(nameof(contentName));

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
                throw new ArgumentNullException(nameof(fields));

            var fieldList = new List<string>();

            var fieldArray = fields.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

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
        /// <param name="showSplittedArticle"></param>
        /// <returns></returns>
        public virtual IQpContentManager ShowSplittedArticle(bool showSplittedArticle)
        {
            _query.ShowSplittedArticle = (byte)(showSplittedArticle ? 1 : 0);
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

            var result = new QpContentResult
            {
                PrimaryContent = _dbConnection.GetContentData(_query, out _),
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
                throw new InvalidOperationException("_query.Fields is null");

            string query = "SELECT " + _query.Fields +
                           " FROM " + _query.ContentName +
                           (string.IsNullOrEmpty(_query.Where) ? "" : " WHERE " + _query.Where);
            var command = _dbConnection.CreateCommand(query);

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
                    var attr = attributes.SingleOrDefault(w => w.Name == path);
                    if (attr?.RelatedContentId == null)
                    {
                        continue;
                    }
                    
                    string contentName = _dbConnection.GetContentName(attr.RelatedContentId.Value);
                    string values = string.Empty;

                    if (attr.LinkId == null)
                    {
                        foreach (DataRow row in result.PrimaryContent.Rows)
                        {
                            if (contentName == _query.ContentName & !string.IsNullOrEmpty(row[path].ToString()))
                            {
                                if (result.PrimaryContent.Select(ContentItemIdFieldName + " = " + row[path]).Any())
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
                        var query = new ContentDataQueryObject(
                            _dbConnection.DbConnector, _query.SiteName, contentName, "*",
                            ContentItemIdFieldName + $" in ({values})", null,
                            0, 0,
                            0,
                            QpContentItemStatus.Published.GetDescription(),
                            0, 0, false, 0.0, false, false);

                        var contentData = _dbConnection.GetContentData(query, out _);
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
                throw new InvalidOperationException("DbConnection is null");
            if (_query.DbConnector == null)
                throw new InvalidOperationException("Query.DbConnector is null");
            if (_query.SiteName == null)
                throw new InvalidOperationException("Query.SiteName is null");
            if (_query.ContentName == null)
                throw new InvalidOperationException("Query.ContentName is null");
        }
    }
}
