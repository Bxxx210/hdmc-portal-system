using System.Web.Mvc;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class BaseController : Controller
    {
        private readonly SessionService _sessionService;

        public BaseController()
            : this(new SessionService())
        {
        }

        public BaseController(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            if (!_sessionService.IsLoggedIn(Session))
            {
                filterContext.Result =
                    RedirectToAction("Index", "Login");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
