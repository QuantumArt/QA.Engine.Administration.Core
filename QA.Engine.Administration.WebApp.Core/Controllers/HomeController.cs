using Microsoft.AspNetCore.Mvc;

namespace QA.Engine.Administration.WebApp.Core.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
