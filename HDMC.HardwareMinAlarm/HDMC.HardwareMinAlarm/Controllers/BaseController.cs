using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Services;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class BaseController : Controller
    {
        private const string DefaultPortalLoginUrl =
            "https://localhost:44370/Login";

        private readonly SystemSettingService _systemSettingService;

        public BaseController()
            : this(new SystemSettingService())
        {
        }

        public BaseController(SystemSettingService systemSettingService)
        {
            _systemSettingService = systemSettingService;
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
            }

            base.OnActionExecuting(filterContext);
        }

        private string GetPortalLoginUrl()
        {
            return _systemSettingService.GetValue(
                SystemSettingService.PortalLoginUrlKey,
                "PortalLoginUrl",
                DefaultPortalLoginUrl);
        }
    }
}
