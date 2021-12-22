using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public class QpAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public QpAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, QPSecurityChecker securityChecker)
        {

            var isAuthorized = securityChecker.CheckAuthorization();

            if (!isAuthorized)
            {
                context.Response.StatusCode = 403;
                context.Response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await context.Response.WriteAsync("Unauthorized");
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
