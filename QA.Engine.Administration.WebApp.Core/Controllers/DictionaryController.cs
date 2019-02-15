using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Engine.Administration.Services.Core.Annotations;
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
        private readonly IContentService _contentService;
        private readonly IMapper _mapper;
        private readonly ILogger<SiteMapController> _logger;
        private readonly int _siteId;
        private readonly int _userId;

        public DictionaryController(
            IItemDifinitionService itemDifinitionService, IRegionService regionService, IContentService contentService,
            IOptions<EnvironmentConfiguration> options, IWebAppQpHelper webAppQpHelper, IMapper mapper, ILogger<SiteMapController> logger)
        {
            _itemDifinitionService = itemDifinitionService;
            _regionService = regionService;
            _contentService = contentService;
            _mapper = mapper;
            _logger = logger;

            _siteId = webAppQpHelper.SiteId;
            _userId = webAppQpHelper.UserId;
        }

        /// <summary>
        /// Возвращает типы контента
        /// </summary>
        /// <returns></returns>
        [HttpGet("getDefinitions")]
        public ApiResult<List<DiscriminatorModel>> GetDescriminators()
        {
            try
            {
                _logger.LogTrace($"getDefinitions userId={_userId}");
                var discriminators = _itemDifinitionService.GetAllItemDefinitions(_siteId);
                return ApiResult<List<DiscriminatorModel>>.Success(discriminators);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetDescriminators error");
                return ApiResult<List<DiscriminatorModel>>.Fail(e);
            }
        }

        /// <summary>
        /// Возвращает регионы списком
        /// </summary>
        /// <returns></returns>
        [HttpGet("getFlatRegions")]
        public ApiResult<List<RegionModel>> GetFlatRegions()
        {
            try
            {
                _logger.LogTrace($"getFlatRegions userId={_userId}");
                var regions = _regionService.GetRegions(_siteId);
                return ApiResult<List<RegionModel>>.Success(regions);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetFlatRegions error");
                return ApiResult<List<RegionModel>>.Fail(e);
            }
        }

        /// <summary>
        /// Возвращает дерево регионов
        /// </summary>
        /// <returns></returns>
        [HttpGet("getRegionTree")]
        public ApiResult<List<RegionModel>> GetRegionTree()
        {
            try
            {
                _logger.LogTrace($"getRegionTree userId={_userId}");
                var regions = _regionService.GetRegionStructure(_siteId);
                return ApiResult<List<RegionModel>>.Success(regions);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetRegionTree error");
                return ApiResult<List<RegionModel>>.Fail(e);
            }
        }

        /// <summary>
        /// Возвращает контент qp с полями
        /// </summary>
        /// <returns></returns>
        [HttpGet("getQpContent")]
        public ApiResult<QpContentModel> GetQpContent(string contentName)
        {
            try
            {
                _logger.LogTrace($"getRegionTree contentName={contentName}, userId={_userId}");
                var content = _contentService.GetQpContent(_siteId, contentName);
                return ApiResult<QpContentModel>.Success(content);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetQpContent error");
                return ApiResult<QpContentModel>.Fail(e);
            }
        }
    }
}
