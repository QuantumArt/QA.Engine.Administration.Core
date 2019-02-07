using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Models;

namespace QA.Engine.Administration.Services.Core.Mapper
{
    public class PageProfile : Profile
    {
        public PageProfile()
        {
            CreateMap<AbstractItemData, PageModel>()
                .ForMember(x => x.Children, opt => opt.Ignore())
                .ForMember(x => x.Widgets, opt => opt.Ignore())
                .ForMember(x => x.ContentVersions, opt => opt.Ignore());
        }
    }
}
