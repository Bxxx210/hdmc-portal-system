using System.Web.Mvc;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly SessionService _sessionService;

        public HomeController()
            : this(new SessionService())
        {
        }

        public HomeController(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (!_sessionService.IsLoggedIn(Session))
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
    }
}
