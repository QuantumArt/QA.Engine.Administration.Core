using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Data.Core.Qp
{
    /// <summary>
    /// Менеджер метаданных QP
    /// </summary>
    public class QpMetadataManager : IQpMetadataManager
    {
        private readonly DBConnector _dbConnection;

        #region Constructors

        /// <summary>
        /// Конструирует объект
        /// </summary>
        /// <param name="dbConnector">Объект dbConnector</param>
        public QpMetadataManager(IQpDbConnector dbConnector)
        {
            _dbConnection = dbConnector.DbConnector;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Возвращает список атрибутов контента
        /// </summary>
        /// <param name="siteName">Имя сайта</param>
        /// <param name="contentName">Имя контента</param>
        /// <returns></returns>
        public virtual List<ContentAttribute> GetContentAttributes(
            string siteName,
            string contentName)
        {
            if (string.IsNullOrEmpty(siteName))
                throw new ArgumentNullException(nameof(siteName));
            if (string.IsNullOrEmpty(contentName))
                throw new ArgumentNullException(nameof(contentName));

            return GetContentAttributes(GetContentId(
                siteName, contentName));
        }

        /// <summary>
        /// Возвращает список атрибутов контента
        /// </summary>
        /// <param name="contentId">Идентификатор контента</param>
        /// <returns></returns>
        public virtual List<ContentAttribute> GetContentAttributes(
            int contentId)
        {
            if (contentId <= 0)
                throw new ArgumentException("contentId <= 0");

            return _dbConnection.GetContentAttributeObjects(contentId).ToList();
        }

        /// <summary>
        /// Возвращает атрибут контента
        /// </summary>
        /// <param name="siteName">Имя сайта</param>
        /// <param name="contentName">Имя контента</param>
        /// <param name="fieldName">Имя поля</param>
        /// <returns></returns>
        public virtual ContentAttribute GetContentAttribute(
            string siteName,
            string contentName,
            string fieldName)
        {
            if (string.IsNullOrEmpty(siteName))
                throw new ArgumentNullException(nameof(siteName));
            if (string.IsNullOrEmpty(contentName))
                throw new ArgumentNullException(nameof(contentName));
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));

            int fieldId = _dbConnection.GetAttributeIdByNetName(
                GetContentId(
                    siteName,
                    contentName), fieldName);

            return _dbConnection.GetContentAttributeObject(fieldId);
        }

        /// <summary>
        /// Возвращает идентификатор контента
        /// </summary>
        /// <param name="siteName">Имя сайта</param>
        /// <param name="contentName">Имя контента</param>
        /// <returns></returns>
        public virtual int GetContentId(
            string siteName,
            string contentName)
        {
            if (string.IsNullOrEmpty(siteName))
                throw new ArgumentNullException(nameof(siteName));
            if (string.IsNullOrEmpty(contentName))
                throw new ArgumentNullException(nameof(contentName));

            int contentId = _dbConnection.GetContentId(
                GetSiteId(siteName), contentName);

            return contentId;
        }

        /// <summary>
        /// Возвращает имя контента
        /// </summary>
        /// <param name="contentId">Идентификатор контента</param>
        /// <returns></returns>
        public virtual string GetContentName(
            int contentId)
        {
            if (contentId <= 0)
                throw new ArgumentException("contentId <= 0");

            string contentName = _dbConnection.GetContentName(contentId);

            return contentName;
        }

        /// <summary>
        /// Возвращает идентификатор сайта
        /// </summary>
        /// <param name="siteName">Название сайта</param>
        /// <returns></returns>
        public virtual int GetSiteId(string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
                throw new ArgumentNullException(nameof(siteName));

            int siteId = _dbConnection.GetSiteId(siteName);
            return siteId;
        }

        /// <summary>
        /// Возвращает имя сайта
        /// </summary>
        /// <param name="siteId">Ид. сайта</param>
        /// <returns></returns>
        public virtual string GetSiteName(int siteId)
        {
            if (siteId <= 0)
                throw new ArgumentException("siteId <= 0");

            string siteName = _dbConnection.GetSiteName(siteId);
            return siteName;
        }

        #endregion
    }
}
