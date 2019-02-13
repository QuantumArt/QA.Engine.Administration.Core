using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Layouts;
using NLog.Targets;
using NLog.Web;
using System.Linq;
using AutoMapper;
using QA.Engine.Administration.Logger.Core.Models;
using QA.Engine.Administration.Logger.Core.Converter;
using Microsoft.Extensions.Configuration;

namespace QA.Engine.Administration.Logger.Core
{
    public static class Extentions
    {
        /// <summary>
        /// Добавляем логирование в формате json
        /// </summary>
        /// <param name="services"></param>
        /// <param name="fields">Расширяем поля лога</param>
        /// <param name="appName">Имя приложения</param>
        public static void AddJsonLogger(this IServiceCollection services, IConfiguration configuration, LoggerConfig config = null)
        {
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                logging.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
                Configure(config, configuration);
            });
        }

        /// <summary>
        /// Добавляем логирование в формате json
        /// </summary>
        /// <param name="webHostBuilder"></param>
        /// <param name="fields">Расширяем поля лога</param>
        /// <param name="appName">Имя приложения</param>
        /// <returns></returns>
        public static IWebHostBuilder AddJsonLogger(this IWebHostBuilder webHostBuilder, LoggerConfig config = null)
        {
            return webHostBuilder
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

                })
                .UseNLog()
                .ConfigureServices((ctx, srv) =>
                {
                    Configure(config, ctx.Configuration);
                });
        }

        private static void Configure(LoggerConfig config, IConfiguration configuration)
        {
            config = config ?? new LoggerConfig();
            var jsonLoggin = configuration.GetSection("JsonLogging").Get<LoggerConfig>();
            config.LogLevel = jsonLoggin.LogLevel;
            config.IgnoreMicrosoftLogs = jsonLoggin.IgnoreMicrosoftLogs;
            config.IgnoreSystemNetLogs = jsonLoggin.IgnoreSystemNetLogs;
            config.UseFileLogger = jsonLoggin.UseFileLogger;
            config.LogFilePath = jsonLoggin.LogFilePath;
            config.ServiceName = configuration.GetValue<string>(nameof(config.ServiceName)) ?? config.ServiceName;
            config.ServiceVersion = configuration.GetValue<string>(nameof(config.ServiceVersion)) ?? config.ServiceVersion;

            var attributes = LoggerConfig.GetDefualtLayout();

            // если передавались поля для расширения лога, то добавляем их
            if (config.LogFields != null)
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<JsonField, JsonAttribute>().ConvertUsing<JsonAttributeTypeConverter>();
                    cfg.CreateMap<JsonLayoutField, JsonLayout>();
                });
                var jsonAttributes = config.LogFields.Select(x => Mapper.Map<JsonField, JsonAttribute>(x));
                attributes.AddRange(jsonAttributes);
            }

            // добавляем все поля лога в JsonLayout и записываем в конфиг
            var jsonLayout = new JsonLayout();
            attributes.ForEach(x => jsonLayout.Attributes.Add(x));

            var consoleTarget = new ConsoleTarget("all_log")
            {
                Layout = jsonLayout
            };

            var fileTarget = new FileTarget("all_log_file")
            {
                Layout = jsonLayout,
                Header = "Log was created at ${longdate}${newline}",
                Footer = "Log was archived at ${longdate}",
                FileName = $"{config.LogFilePath}\\current.log",
                ArchiveFileName = $"{config.LogFilePath}\\archive\\archive_${{shortdate}}.{{##}}.log"
            };

            var nullTarget = new NullTarget("blackHole");

            // создаем конфиг NLog
            var nlogConfig = new LoggingConfiguration();
            // добавление переменных для NLog
            nlogConfig.Variables.Add(nameof(config.ServiceName), config.ServiceName);
            nlogConfig.Variables.Add(nameof(config.ServiceVersion), config.ServiceVersion);

            if (config.IgnoreMicrosoftLogs)
                nlogConfig.AddRule(NLog.LogLevel.FromOrdinal((int)config.LogLevel), NLog.LogLevel.Off, nullTarget, "Microsoft.*", true);
            if (config.IgnoreSystemNetLogs)
                nlogConfig.AddRule(NLog.LogLevel.FromOrdinal((int)config.LogLevel), NLog.LogLevel.Off, nullTarget, "System.Net.*", true);
            nlogConfig.AddRule(NLog.LogLevel.FromOrdinal((int)config.LogLevel), NLog.LogLevel.Off, consoleTarget);
            if (config.UseFileLogger)
                nlogConfig.AddRule(NLog.LogLevel.FromOrdinal((int)config.LogLevel), NLog.LogLevel.Off, fileTarget);

            nlogConfig.AddTarget(nullTarget);
            nlogConfig.AddTarget(consoleTarget);
            if (config.UseFileLogger)
                nlogConfig.AddTarget(fileTarget);

            LogManager.Configuration = nlogConfig;
        }
    }
}
