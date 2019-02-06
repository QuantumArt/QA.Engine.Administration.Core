using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.Engine.Administration.Services.Core.Interfaces;
using QA.Engine.Administration.WebApp.Core.Auth;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Engine.Administration.WebApp.Core.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISiteMapService _siteMapService;
        private readonly IRegionService _regionService;
        private readonly HttpContext _httpContext;
        private readonly IWebAppQpHelper _webAppQpHelper;
        private readonly int _siteId;
        private readonly int _userId;
        private readonly bool _useHierarchyRegionFilter;

        public HomeController(
            ISiteMapService siteMapService, IRegionService regionService, IHttpContextAccessor httpContextAccessor,
            IWebAppQpHelper webAppQpHelper, IOptions<EnvironmentConfiguration> options)
        {
            _siteMapService = siteMapService;
            _regionService = regionService;
            _httpContext = httpContextAccessor.HttpContext;
            _webAppQpHelper = webAppQpHelper;

            _siteId = webAppQpHelper.SiteId;
            _userId = webAppQpHelper.UserId;
            _useHierarchyRegionFilter = options.Value?.UseHierarchyRegionFilter ?? false;
        }

        public ActionResult Index()
        {
            var model = _siteMapService.GetSiteMapStructure(_siteId);
            var archive = _siteMapService.GetArchiveStructure(_siteId);
            ViewData["model"] = model;
            ViewData["archive"] = archive;
            ViewData["userId"] = _webAppQpHelper.UserId;
            return View();
        }

        public ActionResult Dev()
        {
            var model = _siteMapService.GetSiteMapStructure(_siteId, new[] { 98082 }, _useHierarchyRegionFilter);
            var archive = _siteMapService.GetArchiveStructure(_siteId);
            var regions = _regionService.GetRegionStructure(_siteId);
            ViewData["model"] = model;
            ViewData["archive"] = archive;
            ViewData["regions"] = regions;
            ViewData["userId"] = _webAppQpHelper.UserId;
            return View();
        }
    }
}
