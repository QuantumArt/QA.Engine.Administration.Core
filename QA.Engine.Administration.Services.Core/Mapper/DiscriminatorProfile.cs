using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Models;

namespace QA.Engine.Administration.Services.Core.Mapper
{
    public class DiscriminatorProfile : Profile
    {
        public DiscriminatorProfile()
        {
            CreateMap<ItemDefinitionData, DiscriminatorModel>();
        }
    }
}
