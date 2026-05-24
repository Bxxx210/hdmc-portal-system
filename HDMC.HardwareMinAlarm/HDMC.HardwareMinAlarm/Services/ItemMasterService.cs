using System.Collections.Generic;
using HDMC.HardwareMinAlarm.Models;
using HDMC.HardwareMinAlarm.Repositories;

namespace HDMC.HardwareMinAlarm.Services
{
    public class ItemMasterService
    {
        private readonly ItemMasterRepository _repository;

        public ItemMasterService()
            : this(new ItemMasterRepository())
        {
        }

        public ItemMasterService(ItemMasterRepository repository)
        {
            _repository = repository;
        }

        public ItemMasterSearchViewModel Search(
            string company,
            string searchText)
        {
            return new ItemMasterSearchViewModel
            {
                SearchText = searchText,
                Items = _repository.Search(company, searchText)
            };
        }

        public int DeleteParts(
            string company,
            IEnumerable<string> selectedParts)
        {
            return _repository.DeleteParts(company, selectedParts);
        }
    }
}
