using System.Collections.Generic;

namespace HDMC.HardwareMinAlarm.Models
{
    public class ItemMasterSearchViewModel
    {
        public string SearchText { get; set; }

        public List<ItemMasterModel> Items { get; set; }

        public int TotalCount { get; set; }

        public ItemMasterSearchViewModel()
        {
            Items = new List<ItemMasterModel>();
        }
    }
}
