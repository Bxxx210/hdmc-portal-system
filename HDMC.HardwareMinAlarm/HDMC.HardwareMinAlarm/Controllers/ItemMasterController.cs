using System.Web.Mvc;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Services;

namespace HDMC.HardwareMinAlarm.Controllers
{
    public class ItemMasterController : BaseController
    {
        private readonly ItemMasterService _itemMasterService;

        public ItemMasterController()
            : this(new ItemMasterService())
        {
        }

        public ItemMasterController(ItemMasterService itemMasterService)
        {
            _itemMasterService = itemMasterService;
        }

        [HttpGet]
        public ActionResult Index(string searchText)
        {
            var user =
                SessionHelper.GetCurrentUser();

            var model =
                _itemMasterService.Search(user.Company, searchText);

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(
            string searchText,
            string[] selectedParts)
        {
            if (selectedParts == null ||
                selectedParts.Length == 0)
            {
                TempData["ErrorMessage"] =
                    "Please select at least one part";

                return RedirectToAction(
                    "Index",
                    new
                    {
                        searchText
                    });
            }

            var user =
                SessionHelper.GetCurrentUser();

            var deletedRows =
                _itemMasterService.DeleteParts(
                    user.Company,
                    selectedParts);

            TempData["SuccessMessage"] =
                deletedRows + " part(s) deleted";

            return RedirectToAction(
                "Index",
                new
                {
                    searchText
                });
        }
    }
}
