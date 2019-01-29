using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
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

            if (string.IsNullOrWhiteSpace(_webAppQpHelper.CustomerCode))
                throw new Exception("Customer code should not be empty");

            SiteConfiguration.Set(httpContext, _webAppQpHelper.CustomerCode, _webAppQpHelper.SiteId);

            var isAuthorize = _securityChecker.CheckAuthorization();

            var ci = new CultureInfo(httpContext.Session.GetString(QPSecurityChecker.UserLanguageKey) ?? QpLanguage.Default.GetDescription());
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            if (!isAuthorize)
                context.Result = new UnauthorizedResult();
        }

    }
}
