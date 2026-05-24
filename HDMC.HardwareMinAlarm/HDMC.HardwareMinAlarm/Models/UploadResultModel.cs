using System.Collections.Generic;

namespace HDMC.HardwareMinAlarm.Models
{
    public class UploadResultModel
    {
        public int TotalRows { get; set; }

        public int SuccessRows { get; set; }

        public int FailedRows { get; set; }

        public string ErrorMessage { get; set; }
        public List<string> Errors { get; set; }
        public UploadResultModel()
        {
            Errors = new List<string>();
        }
        public string FileName { get; set; }
    }

}