using System.Web;
using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Services;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class UploadController : BaseController
    {
        private readonly UploadService _uploadService;
        private readonly ItemMasterWorkbookService _workbookService;

        protected override bool RequireElevatedAccess
        {
            get { return true; }
        }

        protected override bool RequireSelectedCompany
        {
            get { return true; }
        }

        public UploadController()
        {
            _uploadService = new UploadService();
            _workbookService = new ItemMasterWorkbookService();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file == null ||
                file.ContentLength == 0)
            {
                ViewBag.ErrorMessage =
                    "Please select file";

                return View();
            }

            var result =
                _uploadService.UploadItemMaster(
                    file,
                    SessionHelper.GetCurrentUser().UserId,
                    SessionHelper.GetCurrentUser().Company);

            return View(result);
        }

        [HttpGet]
        public ActionResult Template()
        {
            var company =
                SessionHelper.GetCurrentUser().Company;

            return File(
                _workbookService.CreateTemplate(company),
                ItemMasterWorkbookService.ExcelContentType,
                "item-master-template-" + company + ".xlsx");
        }
    }
}
