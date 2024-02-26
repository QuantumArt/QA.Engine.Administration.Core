using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace QA.Engine.Administration.Services.Core.Mapper.Extensions
{
    public static class MapperServiceExtension
    {
        public static IServiceCollection RegisterMappings(this IServiceCollection services)
        {
            _ = services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ArchiveProfile>();
                cfg.AddProfile<DiscriminatorProfile>();
                cfg.AddProfile<PageProfile>();
                cfg.AddProfile<RegionProfile>();
                cfg.AddProfile<WidgetProfile>();
                cfg.AddProfile<EditProfile>();
                cfg.AddProfile<CultureProfile>();
                cfg.AddProfile<FieldProfile>();
            });

            return services;
        }
    }
}
