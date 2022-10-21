using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using QA.DotNetCore.Caching;
using QA.DotNetCore.Caching.Interfaces;
using QA.DotNetCore.Engine.CacheTags;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.DotNetCore.Engine.QpData.Persistent.Dapper;
using QA.DotNetCore.Engine.QpData.Settings;

namespace QA.Engine.Administration.WebApp.Core
{
    public static class SiteStructureServiceCollectionExtensions
    {
        public static void TryAddSiteStructureRepositories(this IServiceCollection services)
        {
            services.TryAddSingleton((provider) => provider.GetRequiredService<IOptions<QpSiteStructureCacheSettings>>().Value);
            services.TryAddMemoryCacheServices();
            services.TryAddScoped<IMetaInfoRepository, MetaInfoRepository>();
            services.TryAddScoped<INetNameQueryAnalyzer, NetNameQueryAnalyzer>();
            services.TryAddScoped<IQpContentCacheTagNamingProvider, DefaultQpContentCacheTagNamingProvider>();
            services.TryAddScoped<IAbstractItemRepository, AbstractItemRepository>();
        }

        public static void TryAddMemoryCacheServices(this IServiceCollection services)
        {
            _ = services.AddMemoryCache();
            services.TryAddSingleton<ICacheInvalidator, VersionedCacheCoreProvider>();
            services.TryAddSingleton<ICacheProvider, VersionedCacheCoreProvider>();
            services.TryAddSingleton<IMemoryCacheProvider, VersionedCacheCoreProvider>();
            services.TryAddSingleton<IDistributedMemoryCacheProvider, VersionedCacheCoreProvider>();
        }
    }
}
