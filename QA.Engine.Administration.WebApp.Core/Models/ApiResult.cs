using QA.Engine.Administration.Services.Core.Annotations;
using System;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    public class ApiResult
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }

        public static ApiResult Fail(Exception e)
        {
            return new ApiResult
            {
                IsSuccess = false,
                Error = e.Message
            };
        }

        public static ApiResult Success()
        {
            return new ApiResult
            {
                IsSuccess = true,
                Error = null
            };
        }
    }

    [TypeScriptType]
    public class ApiResult<T>
    {
        public T Data { get; set; }
        public bool IsSuccess { get; set; }
        public string Error { get; set; }

        public static ApiResult<T> Fail(Exception e)
        {
            return new ApiResult<T>
            {
                IsSuccess = false,
                Data = default(T),
                Error = e.Message
            };
        }

        public static ApiResult<T> Success(T data)
        {
            return new ApiResult<T>
            {
                IsSuccess = true,
                Data = data,
                Error = null
            };
        }

        public static ApiResult<T> Success()
        {
            return new ApiResult<T>
            {
                IsSuccess = true,
                Data = default(T),
                Error = null
            };
        }
    }
}
