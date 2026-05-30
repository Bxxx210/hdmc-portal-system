using System.Collections.Generic;

namespace HDMC.HardwareMinAlarm.Models
{
    public class HardwareAccessModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public int RoleId { get; set; }

        public List<CompanyAccessModel> Companies { get; set; }

        public HardwareAccessModel()
        {
            Companies = new List<CompanyAccessModel>();
        }
    }
}
