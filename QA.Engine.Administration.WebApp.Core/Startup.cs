using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using QA.DotNetCore.Engine.Persistent.Configuration;
using QA.DotNetCore.Engine.QpData.Persistent.Dapper;
using QA.Engine.Administration.Common.Core;
using QA.Engine.Administration.Data.Core;
using QA.Engine.Administration.Data.Core.Qp;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Services.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Mapper.Extensions;
using QA.Engine.Administration.WebApp.Core.Auth;
using QP.ConfigurationService.Models;
using System;
using System.Globalization;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using Quantumart.QP8.Configuration;
using DatabaseType = QP.ConfigurationService.Models.DatabaseType;

namespace QA.Engine.Administration.WebApp.Core
{
    public class Startup
    {
        private const string SWAGGER_VERSION = "v1";
        private const string SWAGGER_TITLE = "Administration Web App";

        public Startup(IConfiguration configuration, ILoggerFactory factory)
        {
            Configuration = configuration;
            LoggerFactory = factory;
        }

        public IConfiguration Configuration { get; }

        public ILoggerFactory LoggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.Configure<EnvironmentConfiguration>(Configuration);
            EnvironmentConfiguration config = Configuration.Get<EnvironmentConfiguration>();

            _ = services
                .AddMvc()
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            _ = services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc(SWAGGER_VERSION, new OpenApiInfo()
                {
                    Title = SWAGGER_TITLE,
                    Version = SWAGGER_VERSION
                });
                string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                o.IncludeXmlComments(xmlPath);
            });

            _ = services.RegisterMappings();

            _ = services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _ = services.AddScoped<QpHelper>();
            _ = services.AddScoped<QPSecurityChecker>();
            _ = services.AddScoped<IWebAppQpHelper, WebAppQpHelper>();

            _ = services.AddScoped(sp =>
            {
                if (config.UseFake && config.FakeData != null)
                {
                    return new CustomerConfiguration
                    {
                        ConnectionString = config.FakeData.ConnectionString,
                        DbType = Enum.Parse<DatabaseType>(config.FakeData.DatabaseType)
                    };
                }

                IWebAppQpHelper qpHelper = sp.GetService<IWebAppQpHelper>();
                if (!string.IsNullOrEmpty(qpHelper.SavedConnectionString) && string.IsNullOrEmpty(qpHelper.PassedCustomerCode))
                {
                    return new CustomerConfiguration
                    {
                        ConnectionString = qpHelper.SavedConnectionString,
                        DbType = qpHelper.SavedDbType
                    };
                }

                return qpHelper.GetCurrentCustomerConfiguration();
            });

            _ = services.AddScoped<IUnitOfWork, UnitOfWork>(sp =>
            {
                CustomerConfiguration dbConfig = sp.GetRequiredService<CustomerConfiguration>();
                return dbConfig != null ? new UnitOfWork(dbConfig.ConnectionString, dbConfig.DbType.ToString()) : null;
            });

            services.TryAddSiteStructureRepositories();

            _ = services.AddScoped<ISiteMapProvider, SiteMapProvider>();
            _ = services.AddScoped<IWidgetProvider, WidgetProvider>();
            _ = services.AddScoped<IDictionaryProvider, DictionaryProvider>();
            _ = services.AddScoped<IQpDataProvider, QpDataProvider>();
            _ = services.AddScoped<ISettingsProvider, SettingsProvider>();
            _ = services.AddScoped<IItemExtensionProvider, ItemExtensionProvider>();
            _ = services.AddScoped<SqlHelper>();

            _ = services.AddScoped<IQpDbConnector, QpDbConnector>(sp =>
            {
                IUnitOfWork uow = sp.GetService<IUnitOfWork>();
                return new QpDbConnector(uow.Connection);
            });
            _ = services.AddScoped<IQpMetadataManager, QpMetadataManager>();

            _ = services.AddScoped<ISiteMapService, SiteMapService>();
            _ = services.AddScoped<ISiteMapModifyService, SiteMapModifyService>();
            _ = services.AddScoped<IItemDifinitionService, ItemDifinitionService>();
            _ = services.AddScoped<IRegionService, RegionService>();
            _ = services.AddScoped<ICultureService, CultureService>();
            _ = services.AddScoped<IContentService, ContentService>();
            _ = services.AddScoped<ICustomActionService, CustomActionService>();

            _ = services.AddDistributedMemoryCache();

            _ = services.Configure<S3Options>(Configuration.GetSection("S3"));
            _ = services.Configure<CookiePolicyOptions>(options =>
            {
                options.Secure = CookieSecurePolicy.SameAsRequest;
                options.MinimumSameSitePolicy = config.UseSameSiteNone ? SameSiteMode.None : SameSiteMode.Lax;
            });

            _ = services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = config.SessionTimeout;
                options.Cookie.SameSite = config.UseSameSiteNone ? SameSiteMode.None : SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.IsEssential = true;
            });

            _ = services.AddLocalization(x => x.ResourcesPath = "Resources");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            _ = app.UseMiddleware<ExceptionHandler>();
            _ = app.UseStaticFiles();
            _ = app.UseRouting();
            _ = app.UseSession();
            _ = app.UseMiddleware<QpAuthorizationMiddleware>();
            _ = app.UseSwagger();
            _ = app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", $"{SWAGGER_TITLE} {SWAGGER_VERSION}");
            });


            _ = app.Use(async (context, next) =>
            {
                CultureInfo ci = new(context.Session.GetString(QPSecurityChecker.UserLanguageKey) ??
                                         QpLanguage.Default.GetDescription());
                CultureInfo.CurrentCulture = ci;
                CultureInfo.CurrentUICulture = ci;
                await next.Invoke();
            });

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                _ = endpoints.MapFallbackToController("Index", "Home");
            });

            LogStart(app, factory);
        }

        private void LogStart(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            IConfiguration config = app.ApplicationServices.GetRequiredService<IConfiguration>();
            string name = config["ServiceName"];
            ILogger logger = loggerFactory.CreateLogger(GetType());
            logger.LogInformation("{appName} started", name);
        }
    }
}
