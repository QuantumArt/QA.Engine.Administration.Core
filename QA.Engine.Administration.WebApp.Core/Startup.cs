using AutoMapper;
using System;
using System.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QA.Engine.Administration.Services.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.WebApp.Core.Auth;
using Microsoft.AspNetCore.Http;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.DotNetCore.Engine.QpData.Persistent.Dapper;
using QA.Engine.Administration.Data.Core;
using QA.Engine.Administration.Data.Core.Qp;
using Swashbuckle.AspNetCore.Swagger;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Npgsql;
using QA.Engine.Administration.Common.Core;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;
using DatabaseType = QP.ConfigurationService.Models.DatabaseType;

namespace QA.Engine.Administration.WebApp.Core
{
    public class Startup
    {
        const string SWAGGER_VERSION = "v1";
        const string SWAGGER_TITLE = "Administration Web App";

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
            services.Configure<EnvironmentConfiguration>(Configuration);
            var config = Configuration.Get<EnvironmentConfiguration>();

            services
                .AddMvc()
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc(SWAGGER_VERSION, new OpenApiInfo()
                {
                    Title = SWAGGER_TITLE,
                    Version = SWAGGER_VERSION
                });
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                o.IncludeXmlComments(xmlPath);
            });

            services.AddAutoMapper();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<QpHelper>();
            services.AddScoped<QPSecurityChecker>();
            services.AddScoped<IWebAppQpHelper, WebAppQpHelper>();

            services.AddScoped<INetNameQueryAnalyzer, NetNameQueryAnalyzer>();
            services.AddScoped<IUnitOfWork, UnitOfWork>(sp => {
                if (config.UseFake && config.FakeData != null)
                {
                    return new UnitOfWork(config.FakeData.ConnectionString, config.FakeData.DatabaseType);
                }
                var qpHelper = sp.GetService<IWebAppQpHelper>();
                if (!string.IsNullOrEmpty(qpHelper.SavedConnectionString))
                {
                    return new UnitOfWork(qpHelper.SavedConnectionString, qpHelper.SavedDbType.ToString());
                }
                var dbConfig = qpHelper.GetCurrentCustomerConfiguration();
                return dbConfig != null ? new UnitOfWork(dbConfig.ConnectionString, dbConfig.DbType.ToString()) : null;
            });

            services.AddScoped<IAbstractItemRepository, AbstractItemRepository>();
            services.AddScoped<IItemDefinitionRepository, ItemDefinitionRepository>();
            services.AddScoped<IMetaInfoRepository, MetaInfoRepository>();

            services.AddScoped<ISiteMapProvider, SiteMapProvider>();
            services.AddScoped<IWidgetProvider, WidgetProvider>();
            services.AddScoped<IDictionaryProvider, DictionaryProvider>();
            services.AddScoped<IQpDataProvider, QpDataProvider>();
            services.AddScoped<ISettingsProvider, SettingsProvider>();
            services.AddScoped<IItemExtensionProvider, ItemExtensionProvider>();

            services.AddScoped<IQpDbConnector, QpDbConnector>(sp =>
            {
                var uow = sp.GetService<IUnitOfWork>();
                return new QpDbConnector(uow.Connection);
            });
            services.AddScoped<IQpMetadataManager, QpMetadataManager>();
            services.AddScoped<IQpContentManager, QpContentManager>();

            services.AddScoped<ISiteMapService, SiteMapService>();
            services.AddScoped<ISiteMapModifyService, SiteMapModifyService>();
            services.AddScoped<IItemDifinitionService, ItemDifinitionService>();
            services.AddScoped<IRegionService, RegionService>();
            services.AddScoped<ICultureService, CultureService>();
            services.AddScoped<IContentService, ContentService>();
            services.AddScoped<ICustomActionService, CustomActionService>();

            services.AddDistributedMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.Secure = CookieSecurePolicy.SameAsRequest;
                options.MinimumSameSitePolicy = config.UseSameSiteNone ? SameSiteMode.None : SameSiteMode.Lax;
            });

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.SameSite = config.UseSameSiteNone ? SameSiteMode.None : SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.IsEssential = true;
            });

            services.AddLocalization(x => x.ResourcesPath = "Resources");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));
            app.UseMiddleware<ExceptionHandler>();

            app.UseRouting();

            app.UseSession();

            app.UseMiddleware<QpAuthorizationMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", $"{SWAGGER_TITLE} {SWAGGER_VERSION}");
            });

            app.UseStaticFiles();


            app.Use( async (context, next) =>
            {
                var ci = new CultureInfo(context.Session.GetString(QPSecurityChecker.UserLanguageKey) ??
                                         QpLanguage.Default.GetDescription());
                CultureInfo.CurrentCulture = ci;
                CultureInfo.CurrentUICulture = ci;
                await next.Invoke();
            });

            app.UseEndpoints(endpoints  =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapFallbackToController("Index", "Home");
            });

            LogStart(app, factory);
        }

        private void LogStart(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var name = config["ServiceName"];
            var logger = loggerFactory.CreateLogger(GetType());
            logger.LogInformation("{appName} started", name);
        }

    }
}
