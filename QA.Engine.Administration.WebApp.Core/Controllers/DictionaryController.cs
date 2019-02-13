using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.WebApp.Core.Annotations;
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
        public ApiResult<List<DiscriminatorViewModel>> GetDescriminators()
        {
            try
            {
                _logger.LogTrace($"getDefinitions userId={_userId}");
                var discriminators = _itemDifinitionService.GetAllItemDefinitions(_siteId).ToArray();
                var result = _mapper.Map<List<DiscriminatorViewModel>>(discriminators);
                return ApiResult<List<DiscriminatorViewModel>>.Success(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetDescriminators error");
                return ApiResult<List<DiscriminatorViewModel>>.Fail(e);
            }
        }

        /// <summary>
        /// Возвращает регионы списком
        /// </summary>
        /// <returns></returns>
        [HttpGet("getFlatRegions")]
        public ApiResult<List<RegionViewModel>> GetFlatRegions()
        {
            try
            {
                _logger.LogTrace($"getFlatRegions userId={_userId}");
                var discriminators = _regionService.GetRegions(_siteId).ToArray();
                var result = _mapper.Map<List<RegionViewModel>>(discriminators);
                return ApiResult<List<RegionViewModel>>.Success(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetFlatRegions error");
                return ApiResult<List<RegionViewModel>>.Fail(e);
            }
        }

        /// <summary>
        /// Возвращает дерево регионов
        /// </summary>
        /// <returns></returns>
        [HttpGet("getRegionTree")]
        public ApiResult<List<RegionViewModel>> GetRegionTree()
        {
            try
            {
                _logger.LogTrace($"getRegionTree userId={_userId}");
                var discriminators = _regionService.GetRegionStructure(_siteId).ToArray();
                var result = _mapper.Map<List<RegionViewModel>>(discriminators);
                return ApiResult<List<RegionViewModel>>.Success(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetRegionTree error");
                return ApiResult<List<RegionViewModel>>.Fail(e);
            }
        }

        /// <summary>
        /// Возвращает контент qp с полями
        /// </summary>
        /// <returns></returns>
        [HttpGet("getQpContent")]
        public ApiResult<QpContentViewModel> GetQpContent(string contentName)
        {
            try
            {
                _logger.LogTrace($"getRegionTree contentName={contentName}, userId={_userId}");
                var content = _contentService.GetQpContent(_siteId, contentName);
                var result = _mapper.Map<QpContentViewModel>(content);
                return ApiResult<QpContentViewModel>.Success(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetQpContent error");
                return ApiResult<QpContentViewModel>.Fail(e);
            }
        }
    }
}
