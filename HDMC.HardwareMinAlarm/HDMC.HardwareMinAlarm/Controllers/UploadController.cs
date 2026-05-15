using System.Web;
using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Services;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class UploadController : BaseController
    {
        private readonly UploadService _uploadService;

        public UploadController()
        {
            _uploadService = new UploadService();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
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
                _uploadService.UploadItemMaster(file);

            return View(result);
        }
    }
}