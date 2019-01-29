using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.WebApp.Core.Auth;
using QA.Engine.Administration.WebApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.WebApp.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteMapController : ControllerBase
    {
        private readonly ISiteMapService _siteMapService;
        private readonly IItemDifinitionService _itemDifinitionService;
        private readonly ILogger<SiteMapController> _logger;
        // public const int SITE_ID = 35; // beeline ge
        // public const int SITE_ID = 52; // demosite
        private readonly int _siteId;
        private readonly bool _isStage;
        private readonly int _step;
        private readonly int _userId;

        public SiteMapController(
            ISiteMapService siteMapService, IItemDifinitionService itemDifinitionService,
            IOptions<EnvironmentConfiguration> options, IWebAppQpHelper webAppQpHelper, ILogger<SiteMapController> logger)
        {
            _siteMapService = siteMapService;
            _itemDifinitionService = itemDifinitionService;
            _logger = logger;

            _siteId = webAppQpHelper.SiteId;
            _userId = webAppQpHelper.UserId;
            _isStage = options.Value.IsStage;
            _step = options.Value.IndexOrderStep;
        }

        [HttpGet("getAllItems")]
        public ApiResult<object[]> GetSiteMap()
        {
            var result = _siteMapService.GetSiteMapStructure(_siteId, _isStage).ToArray();
            return ApiResult<object[]>.Success(result);
        }

        [HttpGet("getTree")]
        public ApiResult<object[]> GetSiteTree(int? parentId)
        {
            var result = _siteMapService.GetSiteMapItems(_siteId, _isStage, parentId).ToArray();
            return ApiResult<object[]>.Success(result);
        }

        [HttpGet("getWidgets")]
        public ApiResult<object[]> GetWidgets(int parentId)
        {
            var result = _siteMapService.GetWidgetItems(_siteId, _isStage, parentId).ToArray();
            return ApiResult<object[]>.Success(result);
        }

        [HttpGet("getDefinitions")]
        public ApiResult<object[]> GetDescriminators()
        {
            var result = _itemDifinitionService.GetAllItemDefinitions(_siteId, _isStage).ToArray();
            return ApiResult<object[]>.Success(result);
        }

        [HttpPost("publish")]
        public ApiResult Publish([FromBody]List<int> itemIds)
        {
            try
            {
                _siteMapService.PublishSiteMapItems(_siteId, _isStage, _userId, itemIds);
                return ApiResult.Success();
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Publish error");
                return ApiResult.Fail(e);
            }
        }

        [HttpPost("reorder")]
        public ApiResult Reorder([FromBody]ReorderModel model)
        {
            try
            {
                _siteMapService.ReorderSiteMapItems(_siteId, _isStage, _userId, model.ItemId, model.RelatedItemId, model.IsInsertBefore, _step);
                return ApiResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Reorder error");
                return ApiResult.Fail(e);
            }
        }

        [HttpPost("move")]
        public ApiResult Move([FromBody]MoveModel model)
        {
            try
            {
                _siteMapService.MoveSiteMapItem(_siteId, _isStage, _userId, model.ItemId, model.NewParentId);
                return ApiResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Move error");
                return ApiResult.Fail(e);
            }
        }

    }

    
}
