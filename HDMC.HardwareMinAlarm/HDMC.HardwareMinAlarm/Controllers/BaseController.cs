using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Services;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class BaseController : Controller
    {
        private readonly SystemSettingService _systemSettingService;
        private readonly HardwareAccessService _hardwareAccessService;

        protected virtual bool RequireElevatedAccess
        {
            get { return false; }
        }

        protected virtual bool RequireSelectedCompany
        {
            get { return false; }
        }

        public BaseController()
            : this(
                new SystemSettingService(),
                new HardwareAccessService())
        {
        }

        public BaseController(
            SystemSettingService systemSettingService,
            HardwareAccessService hardwareAccessService)
        {
            _systemSettingService = systemSettingService;
            _hardwareAccessService = hardwareAccessService;
        }

        protected override void OnActionExecuting(
     ActionExecutingContext filterContext)
        {
            var controller =
                filterContext.ActionDescriptor
                    .ControllerDescriptor
                    .ControllerName;

            var action =
                filterContext.ActionDescriptor
                    .ActionName;

            // Allow initial auth bridge from Portal
            if (controller == "Home" &&
                action == "Entry")
            {
                base.OnActionExecuting(filterContext);

                return;
            }

            if (!SessionHelper.IsLoggedIn())
            {
                filterContext.Result =
                    Redirect(GetPortalLoginUrl());

                base.OnActionExecuting(filterContext);

                return;
            }

            var user =
                SessionHelper.GetCurrentUser();

            if (RequireElevatedAccess &&
                !_hardwareAccessService.IsElevatedRole(user.RoleId))
            {
                filterContext.Result =
                    RedirectToAction("Index", "Home");

                base.OnActionExecuting(filterContext);

                return;
            }

            if (RequireSelectedCompany &&
                !_hardwareAccessService.CanCurrentUserAccessCompany(
                    user.Company))
            {
                if (_hardwareAccessService.IsElevatedRole(user.RoleId))
                {
                    filterContext.Result =
                        RedirectToAction("Index", "Menu");
                }
                else
                {
                    filterContext.Result =
                        Redirect(GetPortalLoginUrl());
                }

                base.OnActionExecuting(filterContext);

                return;
            }

            base.OnActionExecuting(filterContext);
        }

        private string GetPortalLoginUrl()
        {
            return _systemSettingService.GetPortalLoginUrl();
        }
    }
}
