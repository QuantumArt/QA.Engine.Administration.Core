using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly HttpContext _httpContext;
        private readonly IWebAppQpHelper _webAppQpHelper;

        public HomeController(ISiteMapService siteMapService, IHttpContextAccessor httpContextAccessor, IWebAppQpHelper webAppQpHelper)
        {
            _siteMapService = siteMapService;
            _httpContext = httpContextAccessor.HttpContext;
            _webAppQpHelper = webAppQpHelper;
        }

        public ActionResult Index()
        {
            var model = _siteMapService.GetSiteMapStructure(52);
            var archive = _siteMapService.GetArchiveStructure(52);
            ViewData["model"] = model;
            ViewData["archive"] = archive;
            ViewData["userId"] = _webAppQpHelper.UserId;
            return View();
        }
    }
}
