using System.Web.Mvc;
using HDMC.Portal.Models;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class UserManagementController : BaseController
    {
        private readonly UserManagementService
            _userManagementService;

        public UserManagementController()
            : this(new UserManagementService())
        {
        }

        public UserManagementController(
            UserManagementService userManagementService)
        {
            _userManagementService =
                userManagementService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var users =
                _userManagementService.GetUsers();

            return View(users);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(
            UserManagementModel model,
            string password)
        {
            _userManagementService.CreateUser(
                model,
                password);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ResetPassword(string id)
        {
            ViewBag.UserId = id;

            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(
            string userId,
            string password)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.UserId = userId;
                ViewBag.ErrorMessage =
                    "User ID and Password are required";

                return View();
            }

            _userManagementService.ResetPassword(
                userId,
                password);

            TempData["SuccessMessage"] =
                "Password reset success";

            return RedirectToAction("Index");
        }
    }
}
