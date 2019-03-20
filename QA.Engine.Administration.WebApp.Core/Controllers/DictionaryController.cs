using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Engin.Administration.Common.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using QA.Engine.Administration.WebApp.Core.Auth;
using QA.Engine.Administration.WebApp.Core.Models;

namespace QA.Engine.Administration.WebApp.Core.Controllers
{
    /// <summary>
    /// Api справочников
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [TypeScriptController]
    public class DictionaryController : ControllerBase
    {
        private readonly IItemDifinitionService _itemDifinitionService;
        private readonly IRegionService _regionService;
        private readonly ICultureService _cultureService;
        private readonly IContentService _contentService;
        private readonly ICustomActionService _customActionService;
        private readonly IMapper _mapper;
        private readonly ILogger<SiteMapController> _logger;
        private readonly IStringLocalizerFactory _stringLocalizerFactory;
        private readonly CustomAction _customActionConfig;
        private readonly int _siteId;
        private readonly int _userId;

        public DictionaryController(
            IItemDifinitionService itemDifinitionService, IRegionService regionService, ICultureService cultureService, IContentService contentService,
            IOptions<EnvironmentConfiguration> options, IWebAppQpHelper webAppQpHelper, ICustomActionService customActionService,
            IMapper mapper, ILogger<SiteMapController> logger, IStringLocalizerFactory stringLocalizerFactory)
        {
            _itemDifinitionService = itemDifinitionService;
            _regionService = regionService;
            _cultureService = cultureService;
            _contentService = contentService;
            _customActionService = customActionService;
            _mapper = mapper;
            _logger = logger;
            _stringLocalizerFactory = stringLocalizerFactory;

            _customActionConfig = options.Value?.CustomAction;

            _siteId = webAppQpHelper.SiteId;
            _userId = webAppQpHelper.UserId;
        }

        /// <summary>
        /// Тексты
        /// </summary>
        /// <returns></returns>
        [HttpGet("getTexts")]
        public ApiResult<Dictionary<string, string>> GetTexts()
        {
            var assemblyName = Assembly.GetAssembly(typeof(DictionaryController));
            var localizer = _stringLocalizerFactory.Create("Texts", assemblyName.FullName).WithCulture(Thread.CurrentThread.CurrentUICulture);
            var result = localizer.GetAllStrings().ToDictionary(x => x.Name, x => x.Value);
            return ApiResult<Dictionary<string, string>>.Success(result);
        }

        /// <summary>
        /// Возвращает типы контента
        /// </summary>
        /// <returns></returns>
        [HttpGet("getDiscriminators")]
        public ApiResult<List<DiscriminatorModel>> GetDiscriminators()
        {
            _logger.LogTrace($"getDefinitions userId={_userId}");
            var discriminators = _itemDifinitionService.GetAllItemDefinitions(_siteId);
            return ApiResult<List<DiscriminatorModel>>.Success(discriminators);
        }

        /// <summary>
        /// Возвращает регионы списком
        /// </summary>
        /// <returns></returns>
        [HttpGet("getFlatRegions")]
        public ApiResult<List<RegionModel>> GetFlatRegions()
        {
            _logger.LogTrace($"getFlatRegions userId={_userId}");
            var regions = _regionService.GetRegions(_siteId);
            return ApiResult<List<RegionModel>>.Success(regions);
        }

        /// <summary>
        /// Возвращает дерево регионов
        /// </summary>
        /// <returns></returns>
        [HttpGet("getRegionTree")]
        public ApiResult<List<RegionModel>> GetRegionTree()
        {
            _logger.LogTrace($"getRegionTree userId={_userId}");
            var regions = _regionService.GetRegionStructure(_siteId);
            return ApiResult<List<RegionModel>>.Success(regions);
        }

        /// <summary>
        /// Возвращает контент qp с полями
        /// </summary>
        /// <returns></returns>
        [HttpGet("getQpContent")]
        public ApiResult<QpContentModel> GetQpContent(string contentName)
        {
            _logger.LogTrace($"getRegionTree contentName={contentName}, userId={_userId}");
            var content = _contentService.GetQpContent(_siteId, contentName);
            return ApiResult<QpContentModel>.Success(content);
        }

        /// <summary>
        /// Возвращает культуры
        /// </summary>
        /// <returns></returns>
        [HttpGet("getCultures")]
        public ApiResult<List<CultureModel>> GetCultures()
        {
            _logger.LogTrace($"getCultures userId={_userId}");
            var cultures = _cultureService.GetCultures(_siteId);
            return ApiResult<List<CultureModel>>.Success(cultures);
        }

        /// <summary>
        /// Получить код custom action
        /// </summary>
        /// <returns></returns>
        [HttpGet("getCustomAction")]
        public ApiResult<CustomActionModel> GetCustomAction()
        {
            _logger.LogTrace($"getCustomActionCode alias={_customActionConfig?.Alias}, userId={_userId}");
            var customAction = _customActionService.GetCustomAction(_customActionConfig?.Alias);
            customAction.ItemIdParamName = _customActionConfig?.ItemIdParamName;
            customAction.CultureParamName = _customActionConfig?.CultureParamName;
            customAction.RegionParamName = _customActionConfig?.RegionParamName;
            return ApiResult<CustomActionModel>.Success(customAction);
        }
    }
}
