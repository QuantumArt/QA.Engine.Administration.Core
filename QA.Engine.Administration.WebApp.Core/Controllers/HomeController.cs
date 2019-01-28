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

        public HomeController(ISiteMapService siteMapService, IHttpContextAccessor httpContextAccessor)
        {
            _siteMapService = siteMapService;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public ActionResult Index()
        {
            var model = _siteMapService.GetSiteMapStructure(52, true);
            var userId = (int)_httpContext.Items[DBConnector.LastModifiedByKey];
            ViewData["model"] = model;
            ViewData["userId"] = userId;
            return View();
        }
    }
}
