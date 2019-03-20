using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core
{
    public class CultureService : ICultureService
    {
        private readonly IDictionaryProvider _dictionaryProvider;
        private readonly IMapper _mapper;

        public CultureService(IDictionaryProvider dictionaryProvider, IMapper mapper)
        {
            _dictionaryProvider = dictionaryProvider;
            _mapper = mapper;
        }

        public List<CultureModel> GetCultures(int siteId)
        {
            var cultureData = _dictionaryProvider.GetAllCultures(siteId);
            var cultures = cultureData.Select(x => _mapper.Map<CultureModel>(x)).ToList();
            return cultures;
        }
    }
}
