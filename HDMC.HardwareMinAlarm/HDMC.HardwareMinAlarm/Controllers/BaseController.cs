using System.Configuration;
using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Helpers;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class BaseController : Controller
    {
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
            var configuredUrl =
                ConfigurationManager.AppSettings["PortalLoginUrl"];

            if (string.IsNullOrWhiteSpace(configuredUrl))
            {
                return "https://localhost:44370/Login";
            }

            return configuredUrl;
        }
    }
}
