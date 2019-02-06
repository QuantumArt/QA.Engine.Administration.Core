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
        private readonly IDictionaryProvider _dictionaryProvider;
        private readonly IMapper _mapper;

        public ItemDifinitionService(IDictionaryProvider dictionaryProvider, IMapper mapper)
        {
            _dictionaryProvider = dictionaryProvider;
            _mapper = mapper;
        }

        public List<DiscriminatorModel> GetAllItemDefinitions(int siteId)
        {
            return _dictionaryProvider.GetAllItemDefinitions(siteId)
                .Select(x => _mapper.Map<DiscriminatorModel>(x))
                .ToList();
        }
    }
}
