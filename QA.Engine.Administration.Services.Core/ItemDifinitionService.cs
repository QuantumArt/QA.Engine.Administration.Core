using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core
{
    public class ItemDifinitionService : IItemDifinitionService
    {
        private readonly IItemDifinitionProvider _itemDifinitionProvider;
        private readonly IMapper _mapper;

        public ItemDifinitionService(IItemDifinitionProvider itemDifinitionProvider, IMapper mapper)
        {
            _itemDifinitionProvider = itemDifinitionProvider;
            _mapper = mapper;
        }

        public IEnumerable<DiscriminatorModel> GetAllItemDefinitions(int siteId, bool isStage)
        {
            return _itemDifinitionProvider.GetAllItemDefinitions(siteId, isStage)
                .Select(x => _mapper.Map<DiscriminatorModel>(x));
        }
    }
}
