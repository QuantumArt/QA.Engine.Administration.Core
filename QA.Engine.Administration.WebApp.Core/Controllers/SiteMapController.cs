using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Engin.Administration.Common.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using QA.Engine.Administration.WebApp.Core.Auth;
using QA.Engine.Administration.WebApp.Core.Models;
using System;
using System.Collections.Generic;

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
        public ApiResult<List<PageModel>> GetSiteMapTree([FromQuery]int[] regionIds = null)
        {
            _logger.LogTrace($"getSiteMapTree regionIds={string.Join(", ", regionIds)}, userId={_userId}");
            var siteMap = _siteMapService.GetSiteMapTree(_siteId, regionIds, _useHierarchyRegionFilter);
            return ApiResult<List<PageModel>>.Success(siteMap);
        }

        /// <summary>
        /// Возвращает дерево карты сайта от элемента
        /// </summary>
        /// <returns></returns>
        [HttpGet("getSiteMapSubTree")]
        public ApiResult<PageModel> GetSiteMapSubTree(int id, [FromQuery]int[] regionIds = null)
        {
            _logger.LogTrace($"getSiteMapSubTree id={id}, regionIds={string.Join(", ", regionIds)}, userId={_userId}");
            var siteMap = _siteMapService.GetSiteMapSubTree(_siteId, id, regionIds, _useHierarchyRegionFilter);
            return ApiResult<PageModel>.Success(siteMap);
        }

        /// <summary>
        /// Возвращает полное дерево архива
        /// </summary>
        /// <returns></returns>
        [HttpGet("getArchiveTree")]
        public ApiResult<List<ArchiveModel>> GetArchiveTree()
        {
            _logger.LogTrace($"getArchiveTree userId={_userId}");
            var archive = _siteMapService.GetArchiveTree(_siteId);
            return ApiResult<List<ArchiveModel>>.Success(archive);
        }

        /// <summary>
        /// Возвращает дерево архива от элемента
        /// </summary>
        /// <returns></returns>
        [HttpGet("getArchiveSubTree")]
        public ApiResult<ArchiveModel> GetArchiveSubTree(int id)
        {
            _logger.LogTrace($"getArchiveSubTree id={id}, userId={_userId}");
            var archive = _siteMapService.GetArchiveSubTree(_siteId, id);
            return ApiResult<ArchiveModel>.Success(archive);
        }

        /// <summary>
        /// Возвращает дочерние страницы родительского элемента (страницы)
        /// </summary>
        /// <param name="parentId">Id родительской страницы</param>
        /// <param name="isArchive">Признак возврата архивных элементов</param>
        /// <param name="regionIds">Id регионов</param>
        /// <returns></returns>
        [HttpGet("getPageTree")]
        public ApiResult<List<PageModel>> GetPageTree(bool isArchive, int? parentId, [FromQuery]int[] regionIds = null)
        {
            _logger.LogTrace($"getPageTree isArchive={isArchive}, parentId={parentId}, regionIds={string.Join(", ", regionIds)}, userId={_userId}");
            var siteMap = _siteMapService.GetSiteMapItems(_siteId, isArchive, parentId, regionIds, _useHierarchyRegionFilter);
            return ApiResult<List<PageModel>>.Success(siteMap);
        }

        /// <summary>
        /// Возвращает дочерние виджеты у родительского элемента (страницы или виджета)
        /// </summary>
        /// <param name="parentId">Id родительского элемента</param>
        /// <param name="isArchive">Признак возврата архивных элементов</param>
        /// <param name="regionIds">Id регионов</param>
        /// <returns></returns>
        [HttpGet("getWidgetTree")]
        public ApiResult<List<WidgetModel>> GetWidgetTree(bool isArchive, int parentId, [FromQuery]int[] regionIds = null)
        {
            _logger.LogTrace($"getWidgetTree isArchive={isArchive}, parentId={parentId}, regionIds={string.Join(", ", regionIds)}, userId={_userId}");
            var widgets = _siteMapService.GetWidgetItems(_siteId, isArchive, parentId, regionIds, _useHierarchyRegionFilter);
            return ApiResult<List<WidgetModel>>.Success(widgets);
        }

        /// <summary>
        /// Возвращает поля расширения для статьи контента
        /// </summary>
        /// <param name="id">Id статьи</param>
        /// <param name="extantionId">Id расширения</param>
        /// <returns></returns>
        [HttpGet("getExtantionFields")]
        public ApiResult<List<ExtensionFieldModel>> GetExtantionFields(int id, int extantionId)
        {
            var fields = _siteMapService.GetItemExtantionFields(_siteId, id, extantionId);
            return ApiResult<List<ExtensionFieldModel>>.Success(fields);
        }

        /// <summary>
        /// Опубликовать страницу
        /// </summary>
        /// <param name="itemIds">Id страниц</param>
        /// <returns></returns>
        [HttpPost("publish")]
        public ApiResult Publish([FromBody]List<int> itemIds)
        {
            _logger.LogTrace($"publish itemIds={itemIds}, userId={_userId}");
            _siteMapModifyService.PublishSiteMapItems(_siteId, _userId, itemIds);
            return ApiResult.Success();
        }

        /// <summary>
        /// Изменить порядок отображения страниц
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("reorder")]
        public ApiResult Reorder([FromBody]ReorderModel model)
        {
            _logger.LogTrace($"publish model={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
            _siteMapModifyService.ReorderSiteMapItems(_siteId, _userId, model.ItemId, model.RelatedItemId, model.IsInsertBefore, _step);
            return ApiResult.Success();
        }

        /// <summary>
        /// Переместить страницу к новому родительскому элементу
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("move")]
        public ApiResult Move([FromBody]MoveModel model)
        {
            _logger.LogTrace($"move model={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
            _siteMapModifyService.MoveSiteMapItem(_siteId, _userId, model.ItemId, model.NewParentId);
            return ApiResult.Success();
        }

        /// <summary>
        /// Редактировать
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("edit")]
        public ApiResult Edit([FromBody]EditModel model)
        {
            _logger.LogTrace($"publish edit={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
            _siteMapModifyService.EditSiteMapItem(_siteId, _userId, model);
            return ApiResult.Success();
        }

        /// <summary>
        /// Удаление элементов в архив
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("archive")]
        public ApiResult Archive([FromBody]RemoveModel model)
        {
            _logger.LogTrace($"remove model={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
            _siteMapModifyService.RemoveSiteMapItems(
                _siteId, _userId, model.ItemId,
                model.IsDeleteAllVersions ?? false,
                model.IsDeleteContentVersions ?? false,
                model.ContentVersionId);
            return ApiResult.Success();
        }

        /// <summary>
        /// Восстановление элементов
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("restore")]
        public ApiResult Restore([FromBody]RestoreModel model)
        {
            _logger.LogTrace($"restore model={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
            _siteMapModifyService.RestoreSiteMapItems(
                _siteId, _userId, model.ItemId,
                model.IsRestoreAllVersions ?? false,
                model.IsRestoreChildren ?? false,
                model.IsRestoreContentVersions ?? false,
                model.IsRestoreWidgets ?? false);
            return ApiResult.Success();
        }

        /// <summary>
        /// Удаление элементов
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        public ApiResult Delete([FromBody]DeleteModel model)
        {
            _logger.LogTrace($"delete model={Newtonsoft.Json.JsonConvert.SerializeObject(model)}, userId={_userId}");
            _siteMapModifyService.DeleteSiteMapItems(
                _siteId, _userId, model.ItemId,
                model.IsDeleteAllVersions ?? false);
            return ApiResult.Success();
        }

    }

    
}
