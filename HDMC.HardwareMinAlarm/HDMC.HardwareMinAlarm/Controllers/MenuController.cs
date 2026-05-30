using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Services;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class MenuController : BaseController
    {
        private readonly HardwareAccessService _hardwareAccessService;

        protected override bool RequireElevatedAccess
        {
            get { return true; }
        }

        public MenuController()
            : this(new HardwareAccessService())
        {
        }

        public MenuController(HardwareAccessService hardwareAccessService)
        {
            _hardwareAccessService = hardwareAccessService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(
                _hardwareAccessService.GetCompaniesForCurrentUser());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectCompany(
            string company,
            string destination)
        {
            if (!_hardwareAccessService.CanCurrentUserAccessCompany(company))
            {
                TempData["ErrorMessage"] =
                    "You do not have access to the selected company";

                return RedirectToAction("Index");
            }

            Session["Company"] = company;

            switch (destination)
            {
                case "Scan":
                    return RedirectToAction("Index", "Home");
                case "Upload":
                    return RedirectToAction("Index", "Upload");
                case "ItemMaster":
                    return RedirectToAction("Index", "ItemMaster");
                default:
                    return RedirectToAction("Index");
            }
        }
    }
}
