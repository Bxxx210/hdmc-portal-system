using System.Web.Mvc;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class BaseController : Controller
    {
        private const int AdminRoleId = 1;

        private readonly SessionService _sessionService;

        protected virtual bool RequireAdminAccess
        {
            get { return false; }
        }

        protected virtual bool RequiresAdminAccess(
            ActionExecutingContext filterContext)
        {
            return RequireAdminAccess;
        }

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

                base.OnActionExecuting(filterContext);

                return;
            }

            if (RequiresAdminAccess(filterContext) &&
                _sessionService.GetRoleId(Session) != AdminRoleId)
            {
                TempData["ErrorMessage"] =
                    "You do not have permission to access this page";

                filterContext.Result =
                    RedirectToAction("Index", "Home");

                base.OnActionExecuting(filterContext);

                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
