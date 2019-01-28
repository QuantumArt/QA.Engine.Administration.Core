using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
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
        string SiteId { get; }

        /// <summary>
        /// ConnectionString
        /// </summary>
        string ConnectionString { get; }
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

                if (isAjaxRequest && httpContext.Request.Method.ToLower() == "post")
                    return SerializableQpViewModelBase.FromJsonString(httpContext.Request.Body);

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

        public string SiteId
        {
            get
            {
                var obj = _serializableQpViewModelBaseLazy.Value;
                return obj != null ? obj.SiteId : _qpHelper.SiteId;
            }
        }

        public string ConnectionString => SiteConfiguration.Get(_httpContext).ConnectionString;
    }
}
