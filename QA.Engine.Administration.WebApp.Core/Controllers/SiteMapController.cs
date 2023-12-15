using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.Engine.Administration.Common.Core;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.Services.Core.Models;
using QA.Engine.Administration.WebApp.Core.Auth;
using QA.Engine.Administration.WebApp.Core.Models;
using System.Collections.Generic;
using NLog;

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
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly int _siteId;
        private readonly int _step;
        private readonly int _userId;
        private readonly bool _useHierarchyRegionFilter;

        /// <summary>
        /// Конструктор
        /// </summary>
        public SiteMapController(
            ISiteMapService siteMapService, ISiteMapModifyService siteMapModifyService,
            IOptions<EnvironmentConfiguration> options, IWebAppQpHelper webAppQpHelper)
        {
            _siteMapService = siteMapService;
            _siteMapModifyService = siteMapModifyService;
            _siteId = webAppQpHelper.SavedSiteId;
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
            _logger.ForDebugEvent().Message("getSiteMapTree")
                .Property("regionIds", string.Join(", ", regionIds ?? Array.Empty<int>()))
                .Property("userId", _userId)
                .Log();

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
            _logger.ForDebugEvent().Message("getSiteMapSubTree")
                .Property("regionIds", string.Join(", ", regionIds ?? Array.Empty<int>()))
                .Property("userId", _userId)
                .Log();

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
            _logger.ForDebugEvent().Message("getArchiveTree")
                .Property("userId", _userId)
                .Log();
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
            _logger.ForDebugEvent().Message("getArchiveSubTree")
                .Property("userId", _userId)
                .Property("id", id)
                .Log();

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
            _logger.ForDebugEvent().Message("getPageTree")
                .Property("userId", _userId)
                .Property("parentId", parentId)
                .Property("isArchive", isArchive)
                .Property("regionIds", string.Join(", ", regionIds ?? Array.Empty<int>()))
                .Log();

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
            _logger.ForDebugEvent().Message("getWidgetTree")
                .Property("userId", _userId)
                .Property("parentId", parentId)
                .Property("isArchive", isArchive)
                .Property("regionIds", string.Join(", ", regionIds ?? Array.Empty<int>()))
                .Log();

            var widgets = _siteMapService.GetWidgetItems(_siteId, isArchive, parentId, regionIds, _useHierarchyRegionFilter);
            return ApiResult<List<WidgetModel>>.Success(widgets);
        }

        /// <summary>
        /// Возвращает поля расширения для статьи контента
        /// </summary>
        /// <param name="id">Id статьи</param>
        /// <param name="extensionId">Id расширения</param>
        /// <returns></returns>
        [HttpGet("getExtensionFields")]
        public ApiResult<List<ExtensionFieldModel>> GetExtensionFields(int id, int extensionId)
        {
            _logger.ForDebugEvent().Message("getExtensionFields")
                .Property("userId", _userId)
                .Property("id", id)
                .Property("extensionId", extensionId)
                .Log();

            var fields = _siteMapService.GetItemExtensionFields(_siteId, id, extensionId);
            return ApiResult<List<ExtensionFieldModel>>.Success(fields);
        }

        /// <summary>
        /// Возвращает значение поля связанного элемента по id и id поля
        /// </summary>
        /// <param name="id">id элемента расширения</param>
        /// <param name="attributeId">id поля</param>
        /// <returns></returns>
        [HttpGet("getRelatedItemName")]
        public ApiResult<string> GetRelatedItemName(int id, int attributeId)
        {
            _logger.ForDebugEvent().Message("getRelatedItemName")
                .Property("userId", _userId)
                .Property("id", id)
                .Property("attributeId", attributeId)
                .Log();

            var fields = _siteMapService.GetRelatedItemName(_siteId, id, attributeId);
            return ApiResult<string>.Success(fields);
        }

        /// <summary>
        /// Возвращает значения полей связанных элементов (many-to-one)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="attributeId"></param>
        /// <returns></returns>
        [HttpGet("getManyToOneRelatedItemNames")]
        public ApiResult<Dictionary<int, string>> GetManyToOneRelatedItemNames(int id, int value, int attributeId)
        {
            _logger.ForDebugEvent().Message("getRelatedItemName")
                .Property("userId", _userId)
                .Property("id", id)
                .Property("value", value)
                .Property("attributeId", attributeId)
                .Log();

            var fields = _siteMapService.GetManyToOneRelatedItemNames(_siteId, id, value, attributeId);
            return ApiResult<Dictionary<int, string>>.Success(fields);
        }

        /// <summary>
        /// Опубликовать страницу
        /// </summary>
        /// <param name="itemIds">Id страниц</param>
        /// <returns></returns>
        [HttpPost("publish")]
        public ApiResult Publish([FromBody]List<int> itemIds)
        {
            _logger.ForDebugEvent().Message("publish")
                .Property("userId", _userId)
                .Property("itemIds", itemIds)
                .Log();

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
            _logger.ForDebugEvent().Message("reorder")
                .Property("userId", _userId)
                .Property("model", model)
                .Log();

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
            _logger.ForDebugEvent().Message("move")
                .Property("userId", _userId)
                .Property("model", model)
                .Log();

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
            _logger.ForDebugEvent().Message("edit")
                .Property("userId", _userId)
                .Property("model", model)
                .Log();

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
            _logger.ForDebugEvent().Message("archive")
                .Property("userId", _userId)
                .Property("model", model)
                .Log();

            _siteMapModifyService.ArchiveSiteMapItems(
                _siteId,
                _userId,
                model.ItemId,
                model.IsDeleteAllVersions ?? false,
                model.IsDeleteContentVersions ?? false,
                model.ContentVersionId
            );
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
            _logger.ForDebugEvent().Message("restore")
                .Property("userId", _userId)
                .Property("model", model)
                .Log();

            _siteMapModifyService.RestoreSiteMapItems(
                _siteId,
                _userId,
                model.ItemId,
                model.IsRestoreAllVersions ?? false,
                model.IsRestoreChildren ?? false,
                model.IsRestoreContentVersions ?? false,
                model.IsRestoreWidgets ?? false
            );
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
            _logger.ForDebugEvent().Message("delete")
                .Property("userId", _userId)
                .Property("model", model)
                .Log();

            _siteMapModifyService.DeleteSiteMapItems(
                _siteId,
                _userId,
                model.ItemId,
                model.IsDeleteAllVersions ?? false
            );
            return ApiResult.Success();
        }

    }


}
