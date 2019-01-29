﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.OnScreen;
using System;
using System.Data;
using System.Security.Claims;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public class QPSecurityChecker
    {
        protected static readonly string AuthenticationKey = "QP_Beeline_AuthenticationKey";
        protected static readonly string UserLanguageFieldName = "LANGUAGE_ID";
        public static readonly string UserLanguageKey = "QP_User_Language";
        public static readonly string UserId = "QP_User_Id";

        private readonly HttpContext _httpContext;
        private readonly IWebAppQpHelper _webAppQpHelper;
        private readonly EnvironmentConfiguration _configuration;
        private readonly ILogger<QPSecurityChecker> _logger;

        public QPSecurityChecker(IHttpContextAccessor httpContextAccessor, IWebAppQpHelper webAppQpHelper, IOptions<EnvironmentConfiguration> options, ILogger<QPSecurityChecker> logger)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _webAppQpHelper = webAppQpHelper;
            _configuration = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Проверяет, авторизован ли пользователь
        /// </summary>
        public bool CheckAuthorization()
        {
            if (_httpContext.Session == null)
                return false;

            if (_configuration.IgnoreQPSecurityChecker)
                return true;

            if (_httpContext.Session.Get(AuthenticationKey) != null)
            {
                var uid = _httpContext.Session.GetInt32(DBConnector.LastModifiedByKey);
                if (uid > 0)
                    return true;
            }

            var connectionString = _webAppQpHelper.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                return false;
            var dBConnector = new DBConnector(connectionString);

            var qScreen = new QScreen(dBConnector);
            var userId = qScreen.AuthenticateForCustomTab(dBConnector, _webAppQpHelper.BackendSid);
            var result = userId > 0;

            if (result)
            {
                try
                {
                    var userInfo = GetUserInfo(userId, dBConnector);

                    if (userInfo != null && userInfo.Rows.Count > 0)
                    {
                        var lang = userInfo.Rows[0][UserLanguageFieldName].ToString();
                        int.TryParse(lang, out int langId);
                        string langName = ((QpLanguage)Enum.Parse(typeof(QpLanguage), langId.ToString())).GetDescription();
                        _httpContext.Session.SetString(UserLanguageKey, langName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    _httpContext.Session.SetString(UserLanguageKey, QpLanguage.Default.GetDescription());
                }

                _httpContext.Session.Set(AuthenticationKey, BitConverter.GetBytes(result));
                _httpContext.Session.SetInt32(DBConnector.LastModifiedByKey, userId);
                //_httpContext.Items[DBConnector.LastModifiedByKey] = userId;
            }

            return result;
        }

        private static DataTable GetUserInfo(int user_id, DBConnector dBConnector)
        {
            string str = string.Concat("select * from users where user_id = ", user_id.ToString());
            DataTable cachedData = dBConnector.GetCachedData(str);
            dBConnector = null;
            return cachedData;
        }
    }
}
