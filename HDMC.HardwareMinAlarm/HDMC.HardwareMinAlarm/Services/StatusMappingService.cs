namespace HDMC.HardwareMinAlarm.Services
{
    public class StatusMappingService
    {
        public string GetLocationName(string statusCode)
        {
            switch (statusCode)
            {
                case "100":
                    return "Request Replenish";

                case "200":
                    return "Kanban from MFG";

                case "300":
                    return "Stock min <10 days";

                case "400":
                    return "Location discrepancy";

                case "500":
                    return "Check Shipment ETA";

                case "600":
                    return "ETA Today";

                case "900":
                    return "Replenish Completed";

                default:
                    return string.Empty;
            }
        }

        public string GetStatusText(string statusCode)
        {
            switch (statusCode)
            {
                case "500":
                    return "Check Shipment ETA at plant";

                default:
                    return GetLocationName(statusCode);
            }
        }

        public string GetLocationColumn(string statusCode)
        {
            switch (statusCode)
            {
                case "100":
                    return "loc1";

                case "200":
                    return "loc2";

                case "300":
                    return "loc3";

                case "400":
                    return "loc4";

                case "500":
                    return "loc5";

                case "600":
                    return "loc6";

                case "900":
                    return "loc7";

                default:
                    return null;
            }
        }

        public bool IsValidStatus(string statusCode)
        {
            return !string.IsNullOrWhiteSpace(GetLocationColumn(statusCode));
        }
    }
}
