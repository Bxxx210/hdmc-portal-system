using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Repositories;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class UploadHistoryController
        : BaseController
    {
        private readonly UploadRepository
            _repository;

        public UploadHistoryController()
        {
            _repository =
                new UploadRepository();
        }

        [HttpGet]
        public ActionResult Index()
        {
            var history =
                _repository.GetUploadHistory();

            return View(history);
        }
    }
}