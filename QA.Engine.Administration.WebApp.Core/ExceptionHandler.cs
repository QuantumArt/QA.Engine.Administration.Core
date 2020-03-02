using Microsoft.AspNetCore.Http;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QA.Engine.Administration.WebApp.Core.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace QA.Engine.Administration.WebApp.Core
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public ExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var result = JsonConvert.SerializeObject(
                ApiResult.Fail(exception),
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            await response.WriteAsync(result);
        }
    }
}
