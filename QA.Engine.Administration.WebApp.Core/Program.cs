using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QA.Engine.Administration.Logger.Core;
using QA.Engine.Administration.Logger.Core.Models;

namespace QA.Engine.Administration.WebApp.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .AddJsonLogger(new LoggerConfig
                {
                    LogFields = new[] {
                            new JsonField("ip", "${aspnet-request-ip}")
                        }
                })
                .SuppressStatusMessages(true);
    }
}
