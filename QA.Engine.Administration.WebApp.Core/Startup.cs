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

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<EnvironmentConfiguration>(Configuration);
            var config = Configuration.Get<EnvironmentConfiguration>();

            services
                .AddMvc(opt => { opt.Filters.Add(typeof(QpAutorizationFilter)); })
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
                if (config.UseFake)
                {
                    return new UnitOfWork(Configuration.GetConnectionString("QpConnection"),
                        config.DatabaseType);
                }
                var qpHelper = sp.GetService<IWebAppQpHelper>();
                DBConnector.ConfigServiceUrl = config.ConfigurationServiceUrl;
                DBConnector.ConfigServiceToken = config.ConfigurationServiceToken;
                try
                {
                    CustomerConfiguration dbConfig = DBConnector.GetCustomerConfiguration(qpHelper.CustomerCode).Result;
                    return new UnitOfWork(dbConfig.ConnectionString, dbConfig.DbType.ToString());
                }
                catch (Exception)
                {
                    return null;
                }

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

            services.AddAuthorization();

            services.AddLocalization(x => x.ResourcesPath = "Resources");


            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));
            //app.UseHsts();
            app.UseMiddleware<ExceptionHandler>();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", $"{SWAGGER_TITLE} {SWAGGER_VERSION}");
            });

            app.UseEndpoints(endpoints  =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapFallbackToController("Index", "Home");
            });

            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
            });

            var supportedCultures = new[]
            {
                new CultureInfo(QpLanguage.Russian.GetDescription()),
                new CultureInfo(QpLanguage.English.GetDescription())
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(QpLanguage.Default.GetDescription()),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
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
