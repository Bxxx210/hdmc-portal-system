using System.Web.Mvc;
using HDMC.Portal.Models;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class SystemSettingsController : BaseController
    {
        private readonly SystemSettingService _systemSettingService;

        protected override bool RequireAdminAccess
        {
            get { return true; }
        }

        public SystemSettingsController()
            : this(new SystemSettingService())
        {
        }

        public SystemSettingsController(
            SystemSettingService systemSettingService)
        {
            _systemSettingService = systemSettingService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model =
                _systemSettingService.GetSettings();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SystemSettingModel model)
        {
            _systemSettingService.SaveSettings(model);

            TempData["SuccessMessage"] =
                "System settings saved";

            return RedirectToAction("Index");
        }
    }
}
