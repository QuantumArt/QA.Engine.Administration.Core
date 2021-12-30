using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Engine.Administration.WebApp.Core.Business.Models;
using QP.ConfigurationService.Models;
using ConnectionInfo = QA.Engine.Administration.WebApp.Core.Business.Models.ConnectionInfo;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public class WebAppQpHelper : IWebAppQpHelper
    {
        private HttpContext _httpContext;
        private QpHelper _qpHelper;
        private Lazy<SerializableQpViewModelBase> _serializableQpViewModelBaseLazy;
        private EnvironmentConfiguration _config;
        private ILogger<WebAppQpHelper> _logger;

        public WebAppQpHelper(IHttpContextAccessor httpContextAccessor, QpHelper qpHelper, IOptions<EnvironmentConfiguration> options, ILogger<WebAppQpHelper> logger)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _qpHelper = qpHelper;
            _config = options.Value;
            _logger = logger;
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

        public CustomerConfiguration GetCurrentCustomerConfiguration()
        {
            CustomerConfiguration result = null;

            if (!string.IsNullOrEmpty(CustomerCode))
            {
                DBConnector.ConfigServiceUrl = Config.ConfigurationServiceUrl;
                DBConnector.ConfigServiceToken = Config.ConfigurationServiceToken;
                try
                {
                    result = DBConnector.GetCustomerConfiguration(CustomerCode).Result;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error while receiving customer codes");
                }
            }

            if (result.DbType == DatabaseType.SqlServer && !result.ConnectionString.Contains("Persist Security Info"))
            {
                result.ConnectionString += ";Persist Security Info=True";
            }

            return result;
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

        public EnvironmentConfiguration Config => _config;

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
                var obj = _serializableQpViewModelBaseLazy.Value;
                var result = obj != null ? obj.SiteId : _qpHelper.SiteId;
                return Int32.TryParse(result, out var intResult) ? intResult : 0;
            }
        }

        public int SavedSiteId => _httpContext.Session.GetInt32(QPSecurityChecker.SiteIdKey) ?? 0;

        public string SavedConnectionString => _httpContext.Session.GetString(QPSecurityChecker.ConnectionStringKey);

        public DatabaseType SavedDbType => (DatabaseType)(_httpContext.Session.GetInt32(QPSecurityChecker.DbTypeKey) ?? 0);

        public int UserId => _httpContext.Session.GetInt32(DBConnector.LastModifiedByKey) ?? 0;
    }
}
