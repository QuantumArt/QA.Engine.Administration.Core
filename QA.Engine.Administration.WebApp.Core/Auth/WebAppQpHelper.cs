using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public interface IWebAppQpHelper
    {
        /// <summary>
        /// Id бэкенда
        /// </summary>
        string BackendSid { get; }
        /// <summary>
        /// Код поставщика
        /// </summary>
        string CustomerCode { get; }
        /// <summary>
        /// Id хоста
        /// </summary>
        string HostId { get; }
        /// <summary>
        /// Признак запуска через Custom Action Qp
        /// </summary>
        bool IsQpMode { get; }
        /// <summary>
        /// Ключ
        /// </summary>
        string QpKey { get; }
        /// <summary>
        /// Идентификатор сайта
        /// </summary>
        int SiteId { get; }

        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Id пользователя
        /// </summary>
        int UserId { get; }
    }

    public class WebAppQpHelper : IWebAppQpHelper
    {
        HttpContext _httpContext;
        QpHelper _qpHelper;
        Lazy<SerializableQpViewModelBase> _serializableQpViewModelBaseLazy;

        public WebAppQpHelper(IHttpContextAccessor httpContextAccessor, QpHelper qpHelper)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _qpHelper = qpHelper;
            _serializableQpViewModelBaseLazy = new Lazy<SerializableQpViewModelBase>(() =>
            {
                var httpContext = _httpContext;

                var isAjaxRequest = false;
                if (httpContext.Request.Headers.TryGetValue("X-Requested-With", out StringValues xRequestedWith))
                    isAjaxRequest = xRequestedWith.Contains("XMLHttpRequest");

                var data = string.Empty;
                if (httpContext.Request.Headers.TryGetValue("Qp-Site-Params", out StringValues qpSiteParams))
                    data = string.Join("", qpSiteParams.ToArray());

                if (isAjaxRequest && httpContext.Request.Method.ToLower() == "post" && string.IsNullOrEmpty(data))
                    return SerializableQpViewModelBase.FromJsonString(httpContext.Request.Body);

                if (!string.IsNullOrEmpty(data))
                    return SerializableQpViewModelBase.FromJsonString(data);

                return null;
            });
        }

        public bool IsQpMode => _qpHelper.IsQpMode;

        public string HostId
        {
            get
            {
                var obj = _serializableQpViewModelBaseLazy.Value;
                return obj != null ? obj.HostId : _qpHelper.HostId;
            }
        }

        public string CustomerCode
        {
            get
            {
                var obj = _serializableQpViewModelBaseLazy.Value;
                return obj != null ? obj.CustomerCode : _qpHelper.CustomerCode;
            }
        }

        public string BackendSid
        {
            get
            {
                var obj = _serializableQpViewModelBaseLazy.Value;
                return obj != null ? obj.BackendSid : _qpHelper.BackendSid;
            }
        }

        public string QpKey
        {
            get
            {
                var obj = _serializableQpViewModelBaseLazy.Value;
                return obj != null ? obj.QpKey : _qpHelper.QpKey;
            }
        }

        public int SiteId
        {
            get
            {
                var siteConfiguration = SiteConfiguration.Get(_httpContext);
                if (siteConfiguration != null)
                    return siteConfiguration.SiteId;

                var obj = _serializableQpViewModelBaseLazy.Value;
                var siteId = obj != null ? obj.SiteId : _qpHelper.SiteId;
                if (!int.TryParse(siteId, out int result))
                    throw new Exception("Site Id should not be empty");

                return result;
            }
        }

        public string ConnectionString => SiteConfiguration.Get(_httpContext).ConnectionString;

        public int UserId => _httpContext.Session.GetInt32(DBConnector.LastModifiedByKey) ?? 0;
    }
}
