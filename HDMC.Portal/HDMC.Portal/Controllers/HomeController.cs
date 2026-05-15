using System.Web.Mvc;

namespace HDMC.Portal.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
    }
}
