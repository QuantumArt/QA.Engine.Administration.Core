using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NLog;
using QA.Engine.Administration.Common.Core;
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
        private readonly IItemDifinitionService _itemDefinitionService;
        private readonly IRegionService _regionService;
        private readonly ICultureService _cultureService;
        private readonly IContentService _contentService;
        private readonly ICustomActionService _customActionService;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IStringLocalizerFactory _stringLocalizerFactory;
        private readonly CustomAction _customActionConfig;
        private readonly string _rootPageDiscriminator;
        private readonly int _siteId;
        private readonly int _userId;

        public DictionaryController(
            IItemDifinitionService itemDefinitionService, IRegionService regionService, ICultureService cultureService, IContentService contentService,
            IOptions<EnvironmentConfiguration> options, IWebAppQpHelper webAppQpHelper, ICustomActionService customActionService, IStringLocalizerFactory stringLocalizerFactory)
        {
            _itemDefinitionService = itemDefinitionService;
            _regionService = regionService;
            _cultureService = cultureService;
            _contentService = contentService;
            _customActionService = customActionService;
            _stringLocalizerFactory = stringLocalizerFactory;

            _customActionConfig = options.Value?.CustomAction;
            _rootPageDiscriminator = options.Value?.StartPageDiscriminator;

            _siteId = webAppQpHelper.SavedSiteId;
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
            var localizer = _stringLocalizerFactory.Create("Texts", assemblyName.FullName);
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
            _logger.ForDebugEvent().Message("GET /getDiscriminators").Log();
            var discriminators = _itemDefinitionService.GetAllItemDefinitions(_siteId);
            return ApiResult<List<DiscriminatorModel>>.Success(discriminators);
        }

        /// <summary>
        /// Дискриминатор стартовой страницы
        /// </summary>
        /// <returns></returns>
        [HttpGet("getRootPageDiscriminator")]
        public ApiResult<string> GetRootPageDiscriminator()
        {
            _logger.ForDebugEvent().Message("GET /getRootPageDiscriminator").Log();
            return ApiResult<string>.Success(_rootPageDiscriminator);
        }

        /// <summary>
        /// Возвращает регионы списком
        /// </summary>
        /// <returns></returns>
        [HttpGet("getFlatRegions")]
        public ApiResult<List<RegionModel>> GetFlatRegions()
        {
            _logger.ForDebugEvent().Message("GET /getFlatRegions").Log();
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
            _logger.ForDebugEvent().Message("GET /getRegionTree").Log();
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
            _logger.ForDebugEvent().Message("GET /getQpContent").Property("contentName", contentName).Log();
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
            _logger.ForDebugEvent().Message("GET /getCultures").Property("userId", _userId).Log();
            var cultures = _cultureService.GetCultures(_siteId);
            return ApiResult<List<CultureModel>>.Success(cultures);
        }

        /// <summary>
        /// Получить код custom action
        /// </summary>
        /// <returns></returns>
        [HttpGet("getCustomAction")]
        public ApiResult<CustomActionModel> GetCustomAction(string alias)
        {
            _logger.ForDebugEvent().Message("GET /getCustomAction")
                .Property("alias", alias)
                .Property("configAlias", _customActionConfig?.Alias)
                .Log();
            alias = !string.IsNullOrWhiteSpace(alias) ? alias : _customActionConfig?.Alias;
            var customAction = _customActionService.GetCustomAction(alias);
            if (customAction == null)
                return ApiResult<CustomActionModel>.Success(null);
            customAction.ItemIdParamName = _customActionConfig?.ItemIdParamName;
            customAction.CultureParamName = _customActionConfig?.CultureParamName;
            customAction.RegionParamName = _customActionConfig?.RegionParamName;
            return ApiResult<CustomActionModel>.Success(customAction);
        }
    }
}
