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

            //services.AddSpaStaticFiles(configuration =>
            //{
            //    configuration.RootPath = "ClientApp";
            //});

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
            services.AddScoped<IItemDifinitionProvider, ItemDifinitionProvider>();
            services.AddScoped<IStatusTypeProvider, StatusTypeProvider>();
            services.AddScoped<IQpDataProvider, QpDataProvider>();

            services.AddScoped<IQpDbConnector, QpDbConnector>(sp => new QpDbConnector(GetConnectionString(sp)));
            services.AddScoped<IQpMetadataManager, QpMetadataManager>();
            services.AddScoped<IQpContentManager, QpContentManager>();

            services.AddScoped<ISiteMapService, SiteMapService>();
            services.AddScoped<IItemDifinitionService, ItemDifinitionService>();

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.SameSite = SameSiteMode.None;
            });

            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                //    HotModuleReplacement = true,
                //    ReactHotModuleReplacement = true,
                //    ConfigFile = "webpack.config.dev.js"
                //});
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            //app.UseSpaStaticFiles();

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

            //app.UseSpa(spa =>
            //{
            //    spa.Options.SourcePath = "ClientApp";
            //    if (env.IsDevelopment())
            //        spa.UseReactDevelopmentServer(npmScript: "start:dev");
            //});
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
