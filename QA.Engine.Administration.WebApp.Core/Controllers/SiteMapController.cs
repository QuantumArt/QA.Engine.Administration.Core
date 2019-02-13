using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using QA.Engine.Administration.WebApp.Core.Annotations;
using QA.Engine.Administration.WebApp.Core.Auth;
using QA.Engine.Administration.WebApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.WebApp.Core.Controllers
{
    /// <summary>
    /// Api получения карты сайта
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [TypeScriptController]
    public class SiteMapController : ControllerBase
    {
        private readonly ISiteMapService _siteMapService;
        private readonly ISiteMapModifyService _siteMapModifyService;
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
            ISiteMapService siteMapService, ISiteMapModifyService siteMapModifyService,
            IOptions<EnvironmentConfiguration> options, IWebAppQpHelper webAppQpHelper, IMapper mapper, ILogger<SiteMapController> logger)
        {
            _siteMapService = siteMapService;
            _siteMapModifyService = siteMapModifyService;
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
        [HttpGet("getSiteMapTree")]
        public ApiResult<List<PageViewModel>> GetSiteMapTree([FromQuery]int[] regionIds = null)
        {
            try
            {
                _logger.LogTrace($"getSiteMapTree regionIds={string.Join(", ", regionIds)}, userId={_userId}");
                var siteMap = _siteMapService.GetSiteMapStructure(_siteId, regionIds, _useHierarchyRegionFilter);
                var result = _mapper.Map<List<PageViewModel>>(siteMap);
                return ApiResult<List<PageViewModel>>.Success(result);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "GetSiteMapTree error");
                return ApiResult<List<PageViewModel>>.Fail(e);
            }
        }

        /// <summary>
        /// Возвращает полную структуру архива
        /// </summary>
        /// <returns></returns>
        [HttpGet("getArchiveTree")]
        public ApiResult<List<ArchiveViewModel>> GetArchiveTree()
        {
            try
            {
                _logger.LogTrace($"getArchiveTree userId={_userId}");
                var archive = _siteMapService.GetArchiveStructure(_siteId);
                var result = _mapper.Map<List<ArchiveViewModel>>(archive);
                return ApiResult<List<ArchiveViewModel>>.Success(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetSiteMapTree error");
                return ApiResult<List<ArchiveViewModel>>.Fail(e);
            }
        }

        /// <summary>
        /// Возвращает дочерние страницы родительского элемента (страницы)
        /// </summary>
        /// <param name="parentId">Id родительской страницы</param>
        /// <param name="isArchive">Признак возврата архивных элементов</param>
        /// <param name="regionIds">Id регионов</param>
        /// <returns></returns>
        [HttpGet("getPageTree")]
        public ApiResult<List<PageViewModel>> GetPageTree(bool isArchive, int? parentId, [FromQuery]int[] regionIds = null)
        {
            try
            {
                _logger.LogTrace($"getPageTree isArchive={isArchive}, parentId={parentId}, regionIds={string.Join(", ", regionIds)}, userId={_userId}");
                var siteMap = _siteMapService.GetSiteMapItems(_siteId, isArchive, parentId, regionIds, _useHierarchyRegionFilter);
                var result = _mapper.Map<List<PageViewModel>>(siteMap);
                return ApiResult<List<PageViewModel>>.Success(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetSiteMapTree error");
                return ApiResult<List<PageViewModel>>.Fail(e);
            }
        }

        /// <summary>
        /// Возвращает дочерние виджеты у родительского элемента (страницы или виджета)
        /// </summary>
        /// <param name="parentId">Id родительского элемента</param>
        /// <param name="isArchive">Признак возврата архивных элементов</param>
        /// <param name="regionIds">Id регионов</param>
        /// <returns></returns>
        [HttpGet("getWidgetTree")]
        public ApiResult<List<WidgetViewModel>> GetWidgetTree(bool isArchive, int parentId, [FromQuery]int[] regionIds = null)
        {
            try
            {
                _logger.LogTrace($"getWidgetTree isArchive={isArchive}, parentId={parentId}, regionIds={string.Join(", ", regionIds)}, userId={_userId}");
                var widgets = _siteMapService.GetWidgetItems(_siteId, isArchive, parentId, regionIds, _useHierarchyRegionFilter);
                var result = _mapper.Map<List<WidgetViewModel>>(widgets);
                return ApiResult<List<WidgetViewModel>>.Success(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetSiteMapTree error");
                return ApiResult<List<WidgetViewModel>>.Fail(e);
            }
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
                _logger.LogTrace($"publish itemIds={itemIds}, userId={_userId}");
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
        public ApiResult Reorder([FromBody]ReorderModel model)
        {
            try
            {
                _logger.LogTrace($"publish model={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
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
        public ApiResult Move([FromBody]MoveModel model)
        {
            try
            {
                _logger.LogTrace($"move model={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
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
        public ApiResult Edit([FromBody]EditModel model)
        {
            try
            {
                _logger.LogTrace($"publish edit={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
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
        public ApiResult Remove([FromBody]RemoveModel model)
        {
            try
            {
                _logger.LogTrace($"remove model={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
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
        public ApiResult Restore([FromBody]RestoreModel model)
        {
            try
            {
                _logger.LogTrace($"restore model={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
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
        public ApiResult Delete([FromBody]DeleteModel model)
        {
            try
            {
                _logger.LogTrace($"delete model={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
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
