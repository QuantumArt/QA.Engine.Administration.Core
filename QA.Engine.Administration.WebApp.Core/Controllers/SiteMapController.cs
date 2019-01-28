using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        // public const int SITE_ID = 35; // beeline ge
        // public const int SITE_ID = 52; // demosite
        private readonly int _siteId;
        private readonly bool _isStage;

        public SiteMapController(ISiteMapService siteMapService, IItemDifinitionService itemDifinitionService, IHttpContextAccessor httpContextAccessor)
        {
            _siteMapService = siteMapService;
            _itemDifinitionService = itemDifinitionService;
            var siteConfiguration = SiteConfiguration.Get(httpContextAccessor.HttpContext);
            _siteId = siteConfiguration.SiteId;
            _isStage = siteConfiguration.IsStage;
        }

        [HttpGet("getAllItems")]
        public ApiResult<object[]> GetSiteMap()
        {
            return ApiResult<object[]>.Success(_siteMapService.GetSiteMapStructure(_siteId, _isStage).ToArray());
        }

        [HttpGet("getTree")]
        public ApiResult<object[]> GetSiteTree(int? parentId)
        {
            return ApiResult<object[]>.Success(_siteMapService.GetSiteMapItems(_siteId, _isStage, parentId).ToArray());
        }

        [HttpGet("getWidgets")]
        public ApiResult<object[]> GetWidgets(int parentId)
        {
            return ApiResult<object[]>.Success(_siteMapService.GetWidgetItems(_siteId, _isStage, parentId).ToArray());
        }

        [HttpGet("getDefinitions")]
        public ApiResult<object[]> GetDescriminators()
        {
            return ApiResult<object[]>.Success(_itemDifinitionService.GetAllItemDefinitions(_siteId, _isStage).ToArray());
        }

        [HttpPost("publish")]
        public ApiResult<object> Publish([FromQuery]List<int> itemIds)
        {
            return ApiResult<object>.Success(_siteMapService.PublishSiteMapItems(_siteId, _isStage, itemIds));
        }

    }

    
}
