using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Services;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class PortalController : BaseController
    {
        private readonly SystemSettingService _systemSettingService;

        public PortalController()
            : this(new SystemSettingService())
        {
        }

        public PortalController(SystemSettingService systemSettingService)
        {
            _systemSettingService = systemSettingService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return Redirect(
                _systemSettingService.GetPortalLoginUrl());
        }
    }
}
