using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using System;

namespace QA.Engine.Administration.Services.Core
{
    public class CustomActionService: ICustomActionService
    {
        private readonly ISettingsProvider _settingsProvider;

        public CustomActionService(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public CustomActionModel GetCustomAction(string alias)
        {
            var customAction = _settingsProvider.GetCustomAction(alias);
            return customAction == null ? null : new CustomActionModel
            {
                Id = customAction.Id,
                Code = customAction.Code
            };
        }
    }
}
