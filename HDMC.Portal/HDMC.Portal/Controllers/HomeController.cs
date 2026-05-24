using System.Web.Mvc;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class HomeController : BaseController
    {
        private readonly SessionService _sessionService;
        private readonly CompanySelectionService _companySelectionService;

        public HomeController()
            : this(new SessionService(), new CompanySelectionService())
        {
        }

        public HomeController(
            SessionService sessionService,
            CompanySelectionService companySelectionService)
        {
            _sessionService = sessionService;
            _companySelectionService = companySelectionService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (!_sessionService.IsLoggedIn(Session))
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.IsAdmin =
                _sessionService.GetRoleId(Session) == 1;

            ViewBag.CanAccessHardware =
                _companySelectionService.HasAccessToApp(
                    Session,
                    CompanySelectionService.HardwareMinAlarmAppId);

            ViewBag.CanAccessCountLocation =
                _companySelectionService.HasAccessToApp(
                    Session,
                    CompanySelectionService.CountLocationAppId);

            return View();
        }
    }
}
