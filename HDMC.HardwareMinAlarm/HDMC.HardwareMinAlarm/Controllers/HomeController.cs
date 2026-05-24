using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Services;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class HomeController : BaseController
    {
        private readonly HardwareWorkflowService _hardwareWorkflowService;

        public HomeController()
            : this(new HardwareWorkflowService())
        {
        }

        public HomeController(HardwareWorkflowService hardwareWorkflowService)
        {
            _hardwareWorkflowService = hardwareWorkflowService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Entry(
            string userId,
            string userName,
            string company)
        {
            Session["UserId"] = userId;
            Session["UserName"] = userName;
            Session["Company"] = company;

            return RedirectToAction("Index");
        }

        [HttpPost]
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
