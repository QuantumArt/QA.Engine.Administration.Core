using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Quantumart.QPublishing.Database;
using System;
using System.Linq;
using Microsoft.Extensions.Options;
using NLog;
using QP.ConfigurationService.Models;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public class WebAppQpHelper : IWebAppQpHelper
    {
        private readonly HttpContext _httpContext;
        private readonly QpHelper _qpHelper;
        private readonly Lazy<SerializableQpViewModelBase> _serializableQpViewModelBaseLazy;
        private readonly EnvironmentConfiguration _config;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public WebAppQpHelper(IHttpContextAccessor httpContextAccessor, QpHelper qpHelper, IOptions<EnvironmentConfiguration> options)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _qpHelper = qpHelper;
            _config = options.Value;
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
                    _logger.Warn(ex, "Error while receiving customer codes");
                }
            }

            if (result == null)
            {
                _logger.Warn("Cannot find customer code");
                return null;
            }

            if (!result.ConnectionString.Contains("Persist Security Info"))
            {
                result.ConnectionString += ";Persist Security Info=True";
            }

            return result;
        }


        public bool IsQpMode => _qpHelper.IsQpMode;

        public string HostId => _serializableQpViewModelBaseLazy.Value?.HostId ?? _qpHelper.HostId;

        public string CustomerCode => _serializableQpViewModelBaseLazy.Value?.CustomerCode ?? _qpHelper.CustomerCode;

        public string PassedCustomerCode => _qpHelper.CustomerCode;

        public EnvironmentConfiguration Config => _config;

        public string BackendSid => _serializableQpViewModelBaseLazy.Value?.BackendSid ?? _qpHelper.BackendSid;

        public string QpKey => _serializableQpViewModelBaseLazy.Value?.QpKey ?? _qpHelper.QpKey;

        public int SiteId
        {
            get
            {
                var result = _serializableQpViewModelBaseLazy.Value?.SiteId ?? _qpHelper.SiteId;
                return Int32.TryParse(result, out var intResult) ? intResult : 0;
            }
        }

        public int SavedSiteId => _httpContext.Session.GetInt32(QPSecurityChecker.SiteIdKey) ?? 0;

        public string SavedConnectionString => _httpContext.Session.GetString(QPSecurityChecker.ConnectionStringKey);

        public DatabaseType SavedDbType => (DatabaseType)(_httpContext.Session.GetInt32(QPSecurityChecker.DbTypeKey) ?? 0);

        public int UserId => _httpContext.Session.GetInt32(DBConnector.LastModifiedByKey) ?? 0;
    }
}
