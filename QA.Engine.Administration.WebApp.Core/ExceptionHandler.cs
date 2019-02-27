using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

        public ExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ILogger<ExceptionHandler> logger)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred.");
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
