using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    /// <summary>
    /// Методы для взаимойствия c Qp
    /// </summary>
    public class QpHelper
    {
        private readonly HttpContext _httpContext;
        private readonly EnvironmentConfiguration _configuration;

        public QpHelper(IHttpContextAccessor httpContextAccessor, IOptions<EnvironmentConfiguration> configuration)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration.Value;
        }

        protected string QpCustomerCodeParamName { get { return _configuration.CustomerCodeParamName; } }
        protected string QpSiteIdParamName { get { return _configuration.SiteIdParamName; } }
        protected string QpBackendSidParamName { get { return _configuration.BackendSidParamName; } }
        protected string QpHostIdParamName { get { return _configuration.HostIdParamName ?? "hostUID"; } }

        private const string CustomerCodeParamName = "CustomerCode";
        private const string SiteIdParamName = "SiteId";
        private const string BackendSidParamName = "BackendSid";
        private const string HostIdParamName = "HostId";

        private string GetParam(string name)
        {
            string result;
            if (string.IsNullOrEmpty(_httpContext.Request.Headers[name]))
                result = _httpContext.Request.Query[name];
            else
                result = _httpContext.Request.Headers[name];
            return result;
        }

        /// <summary>
        /// Код поставщика
        /// </summary>
        public string CustomerCode
        {
            get
            {
                    var param = GetParam(QpCustomerCodeParamName);
                    return string.IsNullOrEmpty(param)
                        ? GetParam(CustomerCodeParamName)
                        : param;
            }
        }

        /// <summary>
        /// Идентификатор сайта
        /// </summary>
        public string SiteId
        {
            get
            {
                var param = GetParam(QpSiteIdParamName);
                return string.IsNullOrEmpty(param)
                    ? GetParam(SiteIdParamName)
                    : param;
            }
        }

        /// <summary>
        /// Id бэкенда
        /// </summary>
        public string BackendSid
        {
            get
            {
                var param = GetParam(QpBackendSidParamName);
                return string.IsNullOrEmpty(param) 
                    ? GetParam(BackendSidParamName) 
                    : param;
            }
        }

        /// <summary>
        /// Id хоста
        /// </summary>
        public string HostId
        {
            get
            {
                var param = GetParam(QpHostIdParamName);
                return string.IsNullOrEmpty(param) 
                    ? GetParam(HostIdParamName) 
                    : param;
            }
        }

        /// <summary>
        /// Ключ
        /// </summary>
        public string QpKey
        {
            get
            {
                return (CustomerCode ?? string.Empty) + "_" + (SiteId ?? string.Empty);
            }
        }

        /// <summary>
        /// Признак запуска через Custom Action Qp
        /// </summary>
        public bool IsQpMode
        {
            get
            {
                return !string.IsNullOrEmpty(GetParam(QpHostIdParamName)) || string.IsNullOrEmpty(GetParam(HostIdParamName));
            }
        }
    }
}
