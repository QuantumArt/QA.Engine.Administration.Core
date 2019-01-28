using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Engine.Administration.WebApp.Core.Models
{
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
                Error = e.ToString()
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
    }
}
