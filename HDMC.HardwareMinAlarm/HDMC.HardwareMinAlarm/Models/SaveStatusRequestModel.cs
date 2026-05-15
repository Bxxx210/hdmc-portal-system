namespace HDMC.HardwareMinAlarm.Models
{
    public class SaveStatusRequestModel
    {
        public string PartNumber { get; set; }

        public string Company { get; set; }

        public string StatusCode { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }
    }
}
