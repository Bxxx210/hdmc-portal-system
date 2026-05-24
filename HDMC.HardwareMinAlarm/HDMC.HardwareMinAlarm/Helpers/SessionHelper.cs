using System.Web;
using HDMC.HardwareMinAlarm.Session;

namespace HDMC.HardwareMinAlarm.Helpers
{
    public static class SessionHelper
    {
        public static UserSessionModel GetCurrentUser()
        {
            return new UserSessionModel
            {
                UserId = HttpContext.Current.Session["UserId"]?.ToString(),
                UserName = HttpContext.Current.Session["UserName"]?.ToString(),
                Company = HttpContext.Current.Session["Company"]?.ToString(),
                RoleId = HttpContext.Current.Session["RoleId"] != null
                    ? (int)HttpContext.Current.Session["RoleId"]
                    : 0
            };
        }

        public static bool IsLoggedIn()
        {
            return HttpContext.Current.Session["UserId"] != null;
        }
    }
}
