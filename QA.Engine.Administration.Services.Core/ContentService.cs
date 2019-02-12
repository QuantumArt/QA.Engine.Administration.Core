using AutoMapper;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using System;
using System.Linq;

namespace QA.Engine.Administration.Services.Core
{
    public class ContentService : IContentService
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IMapper _mapper;

        public ContentService(ISettingsProvider settingsProvider, IMapper mapper)
        {
            _settingsProvider = settingsProvider;
            _mapper = mapper;
        }

        public QpContentModel GetQpContent(int siteId, string contentName)
        {
            if (string.IsNullOrEmpty(contentName))
                throw new ArgumentNullException("contentName");
            var content = _settingsProvider.GetContent(siteId, contentName);
            var fields = _settingsProvider.GetFields(siteId, content.Id);
            var result = new QpContentModel { Id = content.Id };
            result.Fields = fields.Select(x => new QpFieldModel { FieldId = $"field_{x.Id}", Name = x.Name }).ToList();
            return result;
        }
    }
}
