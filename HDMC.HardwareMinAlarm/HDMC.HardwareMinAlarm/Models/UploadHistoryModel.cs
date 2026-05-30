using System;

namespace HDMC.HardwareMinAlarm.Models
{
    public class UploadHistoryModel
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public string Company { get; set; }

        public int TotalRows { get; set; }

        public int SuccessRows { get; set; }

        public int FailedRows { get; set; }

        public string UploadedBy { get; set; }

        public DateTime UploadedDate { get; set; }
    }
}
