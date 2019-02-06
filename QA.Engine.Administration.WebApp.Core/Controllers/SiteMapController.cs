using AutoMapper;
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
        private readonly ISiteMapModifyService _siteMapModifyService;
        private readonly IItemDifinitionService _itemDifinitionService;
        private readonly IMapper _mapper;
        private readonly ILogger<SiteMapController> _logger;
        
        private readonly int _siteId;
        private readonly int _step;
        private readonly int _userId;
        private readonly bool _useHierarchyRegionFilter;

        /// <summary>
        /// Конструктор
        /// </summary>
        public SiteMapController(
            ISiteMapService siteMapService, ISiteMapModifyService siteMapModifyService, IItemDifinitionService itemDifinitionService,
            IOptions<EnvironmentConfiguration> options, IWebAppQpHelper webAppQpHelper, IMapper mapper, ILogger<SiteMapController> logger)
        {
            _siteMapService = siteMapService;
            _siteMapModifyService = siteMapModifyService;
            _itemDifinitionService = itemDifinitionService;
            _mapper = mapper;
            _logger = logger;

            _siteId = webAppQpHelper.SiteId;
            _userId = webAppQpHelper.UserId;
            _step = options.Value.IndexOrderStep;
            _useHierarchyRegionFilter = options.Value?.UseHierarchyRegionFilter ?? false;
        }

        /// <summary>
        /// Возвращает полное дерево карты сайта
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAllItems")]
        public ApiResult<List<PageViewModel>> GetSiteMap([FromQuery]int[] regionIds = null)
        {
            var siteMap = _siteMapService.GetSiteMapStructure(_siteId, regionIds, _useHierarchyRegionFilter);
            var result = _mapper.Map<List<PageViewModel>>(siteMap);
            return ApiResult<List<PageViewModel>>.Success(result);
        }

        /// <summary>
        /// Возвращает полную структуру архива
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAllArchiveItems")]
        public ApiResult<ArchiveViewModel> GetAllArchiveItems()
        {
            var archive = _siteMapService.GetArchiveStructure(_siteId);
            var result = _mapper.Map<ArchiveViewModel>(archive);
            return ApiResult<ArchiveViewModel>.Success(result);
        }

        /// <summary>
        /// Возвращает дочерние страницы родительского элемента (страницы)
        /// </summary>
        /// <param name="parentId">Id родительской страницы</param>
        /// <param name="isArchive">Признак возврата архивных элементов</param>
        /// <param name="regionIds">Id регионов</param>
        /// <returns></returns>
        [HttpGet("getPageTree")]
        public ApiResult<List<PageViewModel>> GetPageTree(int? parentId, bool? isArchive, [FromQuery]int[] regionIds = null)
        {
            var siteMap = _siteMapService.GetSiteMapItems(_siteId, isArchive ?? false, parentId, regionIds, _useHierarchyRegionFilter);
            var result = _mapper.Map<List<PageViewModel>>(siteMap);
            return ApiResult<List<PageViewModel>>.Success(result);
        }

        /// <summary>
        /// Возвращает дочерние виджеты у родительского элемента (страницы или виджета)
        /// </summary>
        /// <param name="parentId">Id родительского элемента</param>
        /// <param name="isArchive">Признак возврата архивных элементов</param>
        /// <param name="regionIds">Id регионов</param>
        /// <returns></returns>
        [HttpGet("getWidgetTree")]
        public ApiResult<List<WidgetViewModel>> GetWidgetTree(int parentId, bool? isArchive, [FromQuery]int[] regionIds = null)
        {
            var widgets = _siteMapService.GetWidgetItems(_siteId, isArchive ?? false, parentId, regionIds, _useHierarchyRegionFilter);
            var result = _mapper.Map<List<WidgetViewModel>>(widgets);
            return ApiResult<List<WidgetViewModel>>.Success(result);
        }

        /// <summary>
        /// Возвращает типы контента
        /// </summary>
        /// <returns></returns>
        [HttpGet("getDefinitions")]
        public ApiResult<List<DiscriminatorViewModel>> GetDescriminators()
        {
            var discriminators = _itemDifinitionService.GetAllItemDefinitions(_siteId).ToArray();
            var result = _mapper.Map<List<DiscriminatorViewModel>>(discriminators);
            return ApiResult<List<DiscriminatorViewModel>>.Success(result);
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
                _siteMapModifyService.PublishSiteMapItems(_siteId, _userId, itemIds);
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
        public ApiResult Reorder([FromBody]ReorderRequestModel model)
        {
            try
            {
                _siteMapModifyService.ReorderSiteMapItems(_siteId, _userId, model.ItemId, model.RelatedItemId, model.IsInsertBefore, _step);
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
        public ApiResult Move([FromBody]MoveRequestModel model)
        {
            try
            {
                _siteMapModifyService.MoveSiteMapItem(_siteId, _userId, model.ItemId, model.NewParentId);
                return ApiResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Move error");
                return ApiResult.Fail(e);
            }
        }

        /// <summary>
        /// Редактировать
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("edit")]
        public ApiResult Edit([FromBody]EditRequestModel model)
        {
            try
            {
                _siteMapModifyService.EditSiteMapItem(_siteId, _userId, model.ItemId, model.Title);
                return ApiResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Edit error");
                return ApiResult.Fail(e);
            }
        }

        /// <summary>
        /// Удаление элементов в архив
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("remove")]
        public ApiResult Remove([FromBody]RemoveRequestModel model)
        {
            try
            {
                _siteMapModifyService.RemoveSiteMapItems(
                    _siteId, _userId, model.ItemId, 
                    model.IsDeleteAllVersions ?? false, 
                    model.IsDeleteContentVersions ?? false, 
                    model.ContentVersionId);
                return ApiResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Remove error");
                return ApiResult.Fail(e);
            }
        }

        /// <summary>
        /// Восстановление элементов
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("restore")]
        public ApiResult Restore([FromBody]RestoreRequestModel model)
        {
            try
            {
                _siteMapModifyService.RestoreSiteMapItems(
                    _siteId, _userId, model.ItemId, 
                    model.IsRestoreAllVersions ?? false, 
                    model.IsRestoreChildren?? false, 
                    model.IsRestoreContentVersions ?? false, 
                    model.IsRestoreWidgets ?? false);
                return ApiResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Restore error");
                return ApiResult.Fail(e);
            }
        }

        /// <summary>
        /// Удаление элементов
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        public ApiResult Delete([FromBody]DeleteRequestModel model)
        {
            try
            {
                _siteMapModifyService.DeleteSiteMapItems(
                    _siteId, _userId, model.ItemId,
                    model.IsDeleteAllVersions ?? false);
                return ApiResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Delete error");
                return ApiResult.Fail(e);
            }
        }

    }

    
}
