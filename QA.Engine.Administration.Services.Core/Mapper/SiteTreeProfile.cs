using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Models;

namespace QA.Engine.Administration.Services.Core.Mapper
{
    public class SiteTreeProfile : Profile
    {
        public SiteTreeProfile()
        {
            CreateMap<AbstractItemData, SiteTreeModel>()
                .ForMember(x => x.Children, opt => opt.Ignore())
                .ForMember(x => x.Widgets, opt => opt.Ignore())
                .ForMember(x => x.ContentVersion, opt => opt.Ignore())
                .ForMember(x => x.Discriminator, opt => opt.Ignore());
        }
    }
}
