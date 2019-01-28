using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Models;

namespace QA.Engine.Administration.Services.Core.Mapper
{
    public class WidgetProfile : Profile
    {
        public WidgetProfile()
        {
            CreateMap<AbstractItemData, WidgetModel>()
                .ForMember(x => x.Children, opt => opt.Ignore())
                .ForMember(x => x.Discriminator, opt => opt.Ignore());
        }
    }
}
