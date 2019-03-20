using AutoMapper;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.AspNetCore.SpaServices.Webpack;
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
using QA.Engin.Administration.Common.Core;

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

            services
                .AddMvc(opt =>
                {
                    opt.Filters.Add(typeof(QpAutorizationFilter));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc(SWAGGER_VERSION, new Info
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
            services.AddScoped<IUnitOfWork, UnitOfWork>(sp => new UnitOfWork(GetConnectionString(sp)));
            services.AddScoped<IAbstractItemRepository, AbstractItemRepository>();
            services.AddScoped<IItemDefinitionRepository, ItemDefinitionRepository>();
            services.AddScoped<IMetaInfoRepository, MetaInfoRepository>();

            services.AddScoped<ISiteMapProvider, SiteMapProvider>();
            services.AddScoped<IWidgetProvider, WidgetProvider>();
            services.AddScoped<IDictionaryProvider, DictionaryProvider>();
            services.AddScoped<IQpDataProvider, QpDataProvider>();
            services.AddScoped<ISettingsProvider, SettingsProvider>();
            services.AddScoped<IItemExtensionProvider, ItemExtensionProvider>();

            services.AddScoped<IQpDbConnector, QpDbConnector>(sp => new QpDbConnector(GetConnectionString(sp)));
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

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.SameSite = SameSiteMode.None;
            });

            services.AddAuthorization();

            services.AddLocalization(x => x.ResourcesPath = "Resources");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //app.UseHsts();
            app.UseMiddleware<ExceptionHandler>();

            app.UseStaticFiles();

            app.UseSession();
            //app.UseAuthentication();

            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", $"{SWAGGER_TITLE} {SWAGGER_VERSION}");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapSpaFallbackRoute(
                   name: "spa-fallback",
                   defaults: new { controller = "Home", action = "Index" });
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
            });

            var supportedCultures = new[]
            {
                new CultureInfo(QpLanguage.Russian.GetDescription()),
                new CultureInfo(QpLanguage.English.GetDescription()),
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(QpLanguage.Default.GetDescription()),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });
        }

        private string GetConnectionString(IServiceProvider sp)
        {
            var config = Configuration.Get<EnvironmentConfiguration>();
            var connectionString = config.UseFake
                ? Configuration.GetConnectionString("QpConnection")
                : sp.GetService<IWebAppQpHelper>()?.ConnectionString;
            return connectionString;
        }
    }
}
