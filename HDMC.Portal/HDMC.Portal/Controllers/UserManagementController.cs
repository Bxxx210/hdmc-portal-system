using System;
using System.Linq;
using System.Web.Mvc;
using HDMC.Portal.Models;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class UserManagementController : BaseController
    {
        private readonly UserManagementService
            _userManagementService;

        protected override bool RequireAdminAccess
        {
            get { return true; }
        }

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
        public ActionResult Index(string searchText)
        {
            var users =
                _userManagementService.GetUsers(searchText);

            ViewBag.SearchText = searchText;

            return View(users);
        }

        [HttpGet]
        public ActionResult Create()
        {
            PopulateUserOptions(null);

            return View();
        }

        [HttpPost]
        public ActionResult Create(
            UserManagementModel model,
            string password,
            string confirmPassword)
        {
            try
            {
                _userManagementService.CreateUser(
                    model,
                    password,
                    confirmPassword);
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                PopulateUserOptions(model);

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                PopulateUserOptions(model);

                return View(model);
            }

            TempData["SuccessMessage"] =
                "User created";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            var model =
                _userManagementService.GetUser(id);

            if (model == null)
            {
                TempData["ErrorMessage"] =
                    "User not found";

                return RedirectToAction("Index");
            }

            PopulateUserOptions(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(UserManagementModel model)
        {
            try
            {
                _userManagementService.UpdateUser(model);
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                PopulateUserOptions(model);

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                PopulateUserOptions(model);

                return View(model);
            }

            TempData["SuccessMessage"] =
                "User updated";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SetActive(string userId, bool isActive)
        {
            _userManagementService.SetActive(userId, isActive);

            TempData["SuccessMessage"] =
                isActive
                    ? "User activated"
                    : "User deactivated";

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

            try
            {
                _userManagementService.ResetPassword(
                    userId,
                    password);
            }
            catch (ArgumentException ex)
            {
                ViewBag.UserId = userId;
                ViewBag.ErrorMessage = ex.Message;

                return View();
            }

            TempData["SuccessMessage"] =
                "Password reset success";

            return RedirectToAction("Index");
        }

        private void PopulateUserOptions(UserManagementModel model)
        {
            var companies =
                _userManagementService.GetActiveCompanies()
                    .Select(company => new SelectListItem
                    {
                        Value = company.Company,
                        Text = company.Company,
                        Selected = model != null &&
                            model.Company == company.Company
                    })
                    .ToList();

            var roles =
                _userManagementService.GetRoles()
                    .Select(role => new SelectListItem
                    {
                        Value = role.RoleId.ToString(),
                        Text = role.RoleName,
                        Selected = model != null &&
                            model.RoleId == role.RoleId
                    })
                    .ToList();

            ViewBag.Companies = companies;
            ViewBag.Roles = roles;
        }
    }
}
