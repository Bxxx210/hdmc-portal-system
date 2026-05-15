using System.Web.Mvc;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthService _authService;

        public LoginController()
        {
            _authService = new AuthService();
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

            Session["UserId"] = user.UserId;
            Session["UserName"] = user.UserName;
            Session["RoleId"] = user.RoleId;

            return RedirectToAction("Index", "Company");
        }
    }
}
