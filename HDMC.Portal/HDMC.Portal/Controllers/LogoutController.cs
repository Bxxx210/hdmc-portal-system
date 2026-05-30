using System.Web.Mvc;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class LogoutController : Controller
    {
        private readonly SessionService _sessionService;

        public LogoutController()
            : this(new SessionService())
        {
        }

        public LogoutController(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpPost]
        public ActionResult Index()
        {
            _sessionService.ClearSession(Session);
            return RedirectToAction("Index", "Login");
        }
    }
}