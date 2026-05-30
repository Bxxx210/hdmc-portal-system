using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Repositories;
using HDMC.HardwareMinAlarm.Services;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class UploadHistoryController
        : BaseController
    {
        private readonly UploadRepository _repository;
        private readonly HardwareAccessService _hardwareAccessService;

        protected override bool RequireElevatedAccess
        {
            get { return true; }
        }

        public UploadHistoryController()
            : this(
                new UploadRepository(),
                new HardwareAccessService())
        {
        }

        public UploadHistoryController(
            UploadRepository repository,
            HardwareAccessService hardwareAccessService)
        {
            _repository = repository;
            _hardwareAccessService = hardwareAccessService;
        }

        [HttpGet]
        public ActionResult Index(string company)
        {
            var companies =
                _hardwareAccessService.GetCompaniesForCurrentUser();

            if (!string.IsNullOrWhiteSpace(company) &&
                !_hardwareAccessService.CanCurrentUserAccessCompany(company))
            {
                TempData["ErrorMessage"] =
                    "You do not have access to the selected company";

                return RedirectToAction("Index");
            }

            var history =
                _repository.GetUploadHistory(
                    companies,
                    company);

            ViewBag.Companies = companies;
            ViewBag.SelectedCompany = company;

            return View(history);
        }
    }
}
