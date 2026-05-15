using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Models;
using HDMC.HardwareMinAlarm.Repositories;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class HomeController : BaseController
    {
        private readonly HardwareRepository _hardwareRepository;

        public HomeController()
        {
            _hardwareRepository = new HardwareRepository();
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
            var company =  Session["Company"]?.ToString();

            var result =
                _hardwareRepository.GetPart(
                    partNumber,
                    company);

            if (result != null &&  result.PartStatus == "900")
            {
                result.PartStatus = string.Empty;
                result.UserStamp = string.Empty;
            }

            ViewBag.PartNumber = result?.PartNumber;
            ViewBag.PartDescription = result?.PartDescription;
            ViewBag.PartStatus = result?.PartStatus;
            ViewBag.LocationName = GetLocationName(result?.PartStatus);
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
            // Validate required status selection
            if (string.IsNullOrWhiteSpace(statusCode))
            {
                TempData["ErrorMessage"] =
                    "Please select status";

                return RedirectToAction("Index");
            }

            var request = new SaveStatusRequestModel
            {
                PartNumber = partNumber,
                StatusCode = statusCode,
                Company = Session["Company"]?.ToString(),
                UserId = Session["UserId"]?.ToString(),
                UserName = Session["UserName"]?.ToString()
            };

            _hardwareRepository.SaveStatus(request);

            TempData["SuccessMessage"] =
                "Save Success";

            return RedirectToAction("Index");
        }

        private string GetLocationName(
            string statusCode)
        {
            switch (statusCode)
            {
                case "100":
                    return "Request Replenish";

                case "200":
                    return "Kanban from MFG";

                case "300":
                    return "Stock min <10 days";

                case "400":
                    return "Location discrepancy";

                case "500":
                    return "Check Shipment ETA";

                case "600":
                    return "ETA Today";

                case "900":
                    return "Replenish Completed";

                default:
                    return string.Empty;
            }
        }
    }
}
