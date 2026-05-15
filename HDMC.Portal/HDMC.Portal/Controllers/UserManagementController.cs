using System.Web.Mvc;
using HDMC.Portal.Models;
using HDMC.Portal.Repositories;

namespace HDMC.Portal.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly UserManagementRepository
            _repository;

        public UserManagementController()
        {
            _repository =
                new UserManagementRepository();
        }

        [HttpGet]
        public ActionResult Index()
        {
            var users =
                _repository.GetUsers();

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
            // Temporary password hash
            // TODO: Replace with proper BCrypt hash
            var passwordHash = password;

            _repository.CreateUser(
                model,
                passwordHash);

            return RedirectToAction("Index");
        }
    }
}