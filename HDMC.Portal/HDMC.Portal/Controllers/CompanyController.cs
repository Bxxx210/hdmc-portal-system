using System.Web.Mvc;
using HDMC.Portal.Repositories;

namespace HDMC.Portal.Controllers
{
    public class CompanyController : Controller
    {
        private readonly CompanyRepository _companyRepository;

        public CompanyController()
        {
            _companyRepository = new CompanyRepository();
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var userId = Session["UserId"].ToString();

            var companies =
                _companyRepository.GetCompaniesByUserId(userId, 1);

            return View(companies);
        }

        [HttpPost]
        public ActionResult Select(string company)
        {
            Session["Company"] = company;

            var hardwareUrl =
                "https://localhost:44316/Home/Entry";

            hardwareUrl +=
                "?userId=" +
                Session["UserId"];

            hardwareUrl +=
                "&userName=" +
                Session["UserName"];

            hardwareUrl +=
                "&company=" +
                company;

            return Redirect(hardwareUrl);
        }
    }
}
