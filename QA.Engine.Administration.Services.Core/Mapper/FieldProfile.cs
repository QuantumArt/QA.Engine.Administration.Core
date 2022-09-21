using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Models;

namespace QA.Engine.Administration.Services.Core.Mapper
{
    public class FieldProfile : Profile
    {
        public FieldProfile()
        {
            _ = CreateMap<FieldAttributeData, ExtensionFieldModel>();
        }
    }
}
