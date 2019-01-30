using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using QA.Engine.Administration.WebApp.Core.Auth;
using QA.Engine.Administration.WebApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.WebApp.Core.Controllers
{
    /// <summary>
    /// Api карты сайта
    /// </summary>
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

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="siteMapService"></param>
        /// <param name="itemDifinitionService"></param>
        /// <param name="options"></param>
        /// <param name="webAppQpHelper"></param>
        /// <param name="logger"></param>
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

        /// <summary>
        /// Возвращает полное дерево карты сайта
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAllItems")]
        public ApiResult<SiteTreeModel[]> GetSiteMap()
        {
            var result = _siteMapService.GetSiteMapStructure(_siteId, _isStage).ToArray();
            return ApiResult<SiteTreeModel[]>.Success(result);
        }

        /// <summary>
        /// Возвращает дочерние страницы родительского элемента (страницы)
        /// </summary>
        /// <param name="parentId">Id родительской страницы</param>
        /// <returns></returns>
        [HttpGet("getTree")]
        public ApiResult<SiteTreeModel[]> GetSiteTree(int? parentId)
        {
            var result = _siteMapService.GetSiteMapItems(_siteId, _isStage, parentId).ToArray();
            return ApiResult<SiteTreeModel[]>.Success(result);
        }

        /// <summary>
        /// Возвращает дочерние виджеты у родительского элемента (страницы или виджета)
        /// </summary>
        /// <param name="parentId">Id родительского элемента</param>
        /// <returns></returns>
        [HttpGet("getWidgets")]
        public ApiResult<WidgetModel[]> GetWidgets(int parentId)
        {
            var result = _siteMapService.GetWidgetItems(_siteId, _isStage, parentId).ToArray();
            return ApiResult<WidgetModel[]>.Success(result);
        }

        /// <summary>
        /// Возвращает типы контента
        /// </summary>
        /// <returns></returns>
        [HttpGet("getDefinitions")]
        public ApiResult<DiscriminatorModel[]> GetDescriminators()
        {
            var result = _itemDifinitionService.GetAllItemDefinitions(_siteId, _isStage).ToArray();
            return ApiResult<DiscriminatorModel[]>.Success(result);
        }

        /// <summary>
        /// Опубликовать страницу
        /// </summary>
        /// <param name="itemIds">Id страниц</param>
        /// <returns></returns>
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

        /// <summary>
        /// Изменить порядок отображения страниц
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Переместить страницу к новому родительскому элементу
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Удаление элементов
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("remove")]
        public ApiResult Remove([FromBody]RemoveModel model)
        {
            try
            {
                _siteMapService.RemoveSiteMapItems(_siteId, _isStage, _userId, model.ItemId, model.IsDeleteAllVersions ?? false, model.IsDeleteContentVersions ?? false, null);
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
