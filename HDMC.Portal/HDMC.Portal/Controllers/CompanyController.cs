using System.Web.Mvc;
using HDMC.Portal.Services;

namespace HDMC.Portal.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly CompanySelectionService _companySelectionService;

        public CompanyController()
            : this(new CompanySelectionService())
        {
        }

        public CompanyController(CompanySelectionService companySelectionService)
        {
            _companySelectionService = companySelectionService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (!_companySelectionService.IsLoggedIn(Session))
            {
                return RedirectToAction("Index", "Login");
            }

            var companies =
                _companySelectionService.GetCompaniesForCurrentUser(Session);

            return View(companies);
        }

        [HttpGet]
        public ActionResult EnterHardware()
        {
            if (!_companySelectionService.IsElevatedRole(Session))
            {
                return RedirectToAction("Index");
            }

            return Redirect(
                _companySelectionService.BuildHardwareUrl(Session));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Select(string company)
        {
            var hardwareUrl =
                _companySelectionService.BuildHardwareUrl(
                    Session,
                    company);

            return Redirect(hardwareUrl);
        }
    }
}
