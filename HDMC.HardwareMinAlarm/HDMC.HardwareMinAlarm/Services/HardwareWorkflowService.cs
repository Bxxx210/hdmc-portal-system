using HDMC.HardwareMinAlarm.Models;
using HDMC.HardwareMinAlarm.Repositories;
using HDMC.HardwareMinAlarm.Session;

namespace HDMC.HardwareMinAlarm.Services
{
    public class HardwareWorkflowService
    {
        private readonly HardwareRepository _hardwareRepository;
        private readonly StatusMappingService _statusMappingService;

        public HardwareWorkflowService()
            : this(new HardwareRepository(), new StatusMappingService())
        {
        }

        public HardwareWorkflowService(
            HardwareRepository hardwareRepository,
            StatusMappingService statusMappingService)
        {
            _hardwareRepository = hardwareRepository;
            _statusMappingService = statusMappingService;
        }

        public PartSearchResultModel SearchPart(
            string partNumber,
            string company)
        {
            var result =
                _hardwareRepository.GetPart(partNumber, company);

            if (result != null && result.PartStatus == "900")
            {
                result.PartStatus = string.Empty;
                result.UserStamp = string.Empty;
            }

            return result;
        }

        public string GetLocationName(string statusCode)
        {
            return _statusMappingService.GetLocationName(statusCode);
        }

        public bool IsValidStatus(string statusCode)
        {
            return _statusMappingService.IsValidStatus(statusCode);
        }

        public SaveStatusRequestModel CreateSaveStatusRequest(
            string partNumber,
            string statusCode,
            UserSessionModel user)
        {
            return new SaveStatusRequestModel
            {
                PartNumber = partNumber,
                StatusCode = statusCode,
                Company = user.Company,
                UserId = user.UserId,
                UserName = user.UserName
            };
        }

        public void SaveStatus(SaveStatusRequestModel request)
        {
            _hardwareRepository.SaveStatus(request);
        }
    }
}
