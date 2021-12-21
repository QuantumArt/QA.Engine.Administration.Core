using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using QA.Engine.Administration.Common.Core;
using QA.Engine.Administration.WebApp.Core.Models;
using System;
using System.Globalization;
using System.Threading;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public class QpAutorizationFilter : IAuthorizationFilter
    {
        private readonly IWebAppQpHelper _webAppQpHelper;
        private readonly EnvironmentConfiguration _configuration;
        private readonly QPSecurityChecker _securityChecker;

        public QpAutorizationFilter(IWebAppQpHelper webAppQpHelper, IOptions<EnvironmentConfiguration> options, QPSecurityChecker securityChecker)
        {
            _webAppQpHelper = webAppQpHelper;
            _configuration = options.Value;
            _securityChecker = securityChecker;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var isAuthorized = false;

            if (!string.IsNullOrWhiteSpace(_webAppQpHelper.CustomerCode))
            {
                SiteConfiguration.Set(httpContext, _webAppQpHelper.CustomerCode, _webAppQpHelper.SiteId, _configuration.UseFake);

                isAuthorized = _securityChecker.CheckAuthorization();

                var ci = new CultureInfo(httpContext.Session.GetString(QPSecurityChecker.UserLanguageKey) ?? QpLanguage.Default.GetDescription());
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }

            if (!isAuthorized)
                context.Result = new UnauthorizedObjectResult(ApiResult.Fail(new Exception("Unauthorized")));
        }

    }
}
