using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using QA.Engine.Administration.Services.Core.Models;

namespace QA.Engine.Administration.Services.Core.Mapper
{
    public class ArchiveProfile: Profile
    {
        public ArchiveProfile()
        {
            CreateMap<AbstractItemData, ArchiveModel>()
                .ForMember(x => x.Children, opt => opt.Ignore());
        }
    }
}
