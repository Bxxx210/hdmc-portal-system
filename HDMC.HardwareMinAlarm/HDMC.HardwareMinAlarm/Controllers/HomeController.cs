using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Services;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class HomeController : BaseController
    {
        private readonly HardwareWorkflowService _hardwareWorkflowService;
        private readonly HardwareAccessService _hardwareAccessService;

        protected override bool RequireSelectedCompany
        {
            get { return true; }
        }

        public HomeController()
            : this(
                new HardwareWorkflowService(),
                new HardwareAccessService())
        {
        }

        public HomeController(HardwareWorkflowService hardwareWorkflowService)
            : this(
                hardwareWorkflowService,
                new HardwareAccessService())
        {
        }

        public HomeController(
            HardwareWorkflowService hardwareWorkflowService,
            HardwareAccessService hardwareAccessService)
        {
            _hardwareWorkflowService = hardwareWorkflowService;
            _hardwareAccessService = hardwareAccessService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Entry(
            string userId,
            string company)
        {
            Session.Clear();

            var access =
                _hardwareAccessService.GetAccess(userId);

            if (access == null ||
                access.Companies.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            Session["UserId"] = access.UserId;
            Session["UserName"] = access.UserName;
            Session["RoleId"] = access.RoleId;

            if (_hardwareAccessService.IsElevatedRole(access.RoleId))
            {
                Session["Company"] = null;

                return RedirectToAction("Index", "Menu");
            }

            if (!_hardwareAccessService.CanAccessCompany(access, company))
            {
                Session.Clear();

                return RedirectToAction("Index", "Home");
            }

            Session["Company"] = company;

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string partNumber)
        {
            var user =
                SessionHelper.GetCurrentUser();

            var result =
                _hardwareWorkflowService.SearchPart(
                    partNumber,
                    user.Company);

            ViewBag.PartNumber = result?.PartNumber;
            ViewBag.PartDescription = result?.PartDescription;
            ViewBag.PartStatus = result?.PartStatus;
            ViewBag.LocationName =
                _hardwareWorkflowService.GetLocationName(result?.PartStatus);
            ViewBag.UserStamp = result?.UserStamp;

            if (result == null)
            {
                ViewBag.ErrorMessage =
                    "Part not found";
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveStatus(string partNumber, string statusCode)
        {
            if (!_hardwareWorkflowService.IsValidStatus(statusCode))
            {
                TempData["ErrorMessage"] =
                    "Please select status";

                return RedirectToAction("Index");
            }

            var request =
                _hardwareWorkflowService.CreateSaveStatusRequest(
                    partNumber,
                    statusCode,
                    SessionHelper.GetCurrentUser());

            _hardwareWorkflowService.SaveStatus(request);

            TempData["SuccessMessage"] =
                "Save Success";

            return RedirectToAction("Index");
        }
    }
}
