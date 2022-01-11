using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Engine.Administration.Common.Core;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.OnScreen;
using System;
using System.Data;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QP.ConfigurationService.Models;
using DatabaseType = QP.ConfigurationService.Models.DatabaseType;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public class QPSecurityChecker
    {
        private static readonly string AuthenticationKey = "QP_ManagePages_AuthenticationKey";
        public const string ConnectionStringKey = "QP_ManagePages_ConnectionString";
        private const string CustomerCodeKey = "QP_ManagePages_CustomerCode";
        public const string DbTypeKey = "QP_ManagePages_DbType";
        public static readonly string SiteIdKey = "QP_ManagePages_SiteId";
        public static readonly string UserLanguageKey = "QP_ManagePages_Language";

        protected static readonly string UserLanguageFieldName = "LANGUAGE_ID";

        private readonly HttpContext _httpContext;
        private readonly IWebAppQpHelper _webAppQpHelper;
        private readonly EnvironmentConfiguration _configuration;
        private readonly ILogger<QPSecurityChecker> _logger;
        private readonly IUnitOfWork _uow;
        public QPSecurityChecker(IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            IWebAppQpHelper webAppQpHelper,
            IOptions<EnvironmentConfiguration> options,
            ILogger<QPSecurityChecker> logger)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _webAppQpHelper = webAppQpHelper;
            _configuration = options.Value;
            _logger = logger;
            _uow = uow;
        }

        /// <summary>
        /// Проверяет, авторизован ли пользователь
        /// </summary>
        public bool CheckAuthorization()
        {
            if (_httpContext.Session == null)
            {
                _logger.LogError("Session is not enabled");
                return false;
            }

            if (_configuration.IgnoreQPSecurityChecker)
            {
                return true;
            }

            var savedUserId = GetSavedUserIdFromSession();
            if (savedUserId > 0)
            {
                return true;
            }

            if (_configuration.UseFake && _configuration.FakeData != null)
            {
                SaveFakeDataToSession();
                return true;
            }

            var dBConnector = GetDBConnector();
            if (dBConnector == null)
            {
                _logger.LogWarning($"Customer code not found: {_webAppQpHelper.CustomerCode}");
                return false;
            }

            var userId = GetAuthUserId(dBConnector);
            if (userId <= 0)
            {
                _logger.LogWarning($"Could not authenticate with backend SID: {_webAppQpHelper.BackendSid}");
                return false;
            }

            var langName = GetLangName(userId, dBConnector);
            SaveAuthDataToSession(userId, langName, dBConnector);

            return true;
        }

        private int GetAuthUserId(DBConnector dBConnector)
        {
            var qScreen = new QScreen(dBConnector);
            var userId = qScreen.AuthenticateForCustomTab(dBConnector, _webAppQpHelper.BackendSid);
            return userId;
        }

        private int GetSavedUserIdFromSession()
        {
            var result = 0;
            if (_httpContext.Session.Get(AuthenticationKey) != null)
            {
                var code = _webAppQpHelper.CustomerCode;
                if (string.IsNullOrEmpty(code) || _httpContext.Session.GetString(CustomerCodeKey) == code)
                {
                    result = _httpContext.Session.GetInt32(DBConnector.LastModifiedByKey) ?? 0;
                }
            }
            return result;
        }

        private void SaveAuthDataToSession(int userId, string langName, DBConnector dBConnector)
        {
            _httpContext.Session.SetInt32(DBConnector.LastModifiedByKey, userId);
            _httpContext.Session.SetString(UserLanguageKey, langName);
            _httpContext.Session.SetString(ConnectionStringKey, dBConnector.CustomConnectionString);
            _httpContext.Session.SetString(CustomerCodeKey, _webAppQpHelper.CustomerCode);
            _httpContext.Session.SetInt32(SiteIdKey, _webAppQpHelper.SiteId);
            _httpContext.Session.SetInt32(DbTypeKey, (int) dBConnector.DatabaseType);
            _httpContext.Session.Set(AuthenticationKey, BitConverter.GetBytes(true));
        }

        private string GetLangName(int userId, DBConnector dBConnector)
        {
            string langName = "";
            try
            {
                var userInfo = GetUserInfo(userId, dBConnector);
                if (userInfo != null && userInfo.Rows.Count > 0)
                {
                    var lang = userInfo.Rows[0][UserLanguageFieldName].ToString();
                    int.TryParse(lang, out var langId);
                    langName = ((QpLanguage) Enum.Parse(typeof(QpLanguage), langId.ToString())).GetDescription();
                    _httpContext.Session.SetString(UserLanguageKey, langName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                langName = QpLanguage.Default.GetDescription();
            }

            return langName;
        }

        private void SaveFakeDataToSession()
        {
            _httpContext.Session.SetInt32(AuthenticationKey, 1);
            _httpContext.Session.SetInt32(DBConnector.LastModifiedByKey, _configuration.FakeData.UserId);
            _httpContext.Session.SetString(UserLanguageKey, _configuration.FakeData.LangName);
        }

        private DBConnector GetDBConnector()
        {
            DBConnector result;

            if (_uow?.Connection != null)
            {
                result = new DBConnector(_uow.Connection);
            }
            else
            {
                var dbConfig = GetCurrentDbConfig();
                if (dbConfig == null)
                {
                    return null;
                }
                result = new DBConnector(dbConfig.ConnectionString, dbConfig.DbType);
            }

            return result;
        }

        private CustomerConfiguration GetCurrentDbConfig()
        {
            CustomerConfiguration dbConfig;
            var newCustomerCode = _webAppQpHelper.CustomerCode;
            var hasNewCustomerCode = !string.IsNullOrEmpty(newCustomerCode);
            var connectionString = hasNewCustomerCode ? null : _httpContext.Session.GetString(ConnectionStringKey);
            var customerCode = hasNewCustomerCode ? newCustomerCode : _httpContext.Session.GetString(CustomerCodeKey);
            var dbType = (DatabaseType) (_httpContext.Session.GetInt32(DbTypeKey) ?? 1);
            var isNewDbConfig = string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(customerCode);
            dbConfig = isNewDbConfig
                ? _webAppQpHelper.GetCurrentCustomerConfiguration()
                : new CustomerConfiguration()
                {
                    ConnectionString = connectionString, Name = customerCode, DbType = dbType
                };
            return dbConfig;
        }

        private static DataTable GetUserInfo(int user_id, DBConnector dBConnector)
        {
            string str = string.Concat("select * from users where user_id = ", user_id.ToString());
            return dBConnector.GetCachedData(str);
        }
    }
}
