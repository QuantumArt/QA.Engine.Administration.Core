using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Models;

namespace QA.Engine.Administration.Services.Core.Mapper
{
    public class RegionProfile : Profile
    {
        public RegionProfile()
        {
            CreateMap<RegionData, RegionModel>()
                .ForMember(x => x.Children, opt => opt.Ignore());
        }
    }
}
