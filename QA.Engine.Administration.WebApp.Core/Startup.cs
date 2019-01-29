using AutoMapper;
using System;
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

namespace QA.Engine.Administration.WebApp.Core
{
    public class Startup
    {
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
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseStaticFiles();
            //app.UseSpaStaticFiles();

            app.UseSession();
            //app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //app.UseSpa(spa =>
            //{
            //    spa.Options.SourcePath = "ClientApp";
            //    if (env.IsDevelopment())
            //        spa.UseReactDevelopmentServer(npmScript: "start");
            //});
        }

        private string GetConnectionString(IServiceProvider sp)
        {
            var config = Configuration.Get<EnvironmentConfiguration>();
            var connectionString = config.IgnoreAuth
                ? Configuration.GetConnectionString("QpConnection")
                : sp.GetService<IWebAppQpHelper>()?.ConnectionString;
            return connectionString;
        }
    }
}
