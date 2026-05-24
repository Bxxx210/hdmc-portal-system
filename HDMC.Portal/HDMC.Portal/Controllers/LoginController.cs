using System.Web.Mvc;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthService _authService;
        private readonly SessionService _sessionService;

        public LoginController()
            : this(new AuthService(), new SessionService())
        {
        }

        public LoginController(
            AuthService authService,
            SessionService sessionService)
        {
            _authService = authService;
            _sessionService = sessionService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string userId, string password)
        {
            var user = _authService.Login(userId, password);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid User ID or Password";

                return View();
            }

            _sessionService.SetUser(Session, user);

            return RedirectToAction("Index", "Company");
        }
    }
}
