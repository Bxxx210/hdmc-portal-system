using System.Collections.Generic;
using System.Linq;
using System.Web;
using HDMC.Portal.Models;
using HDMC.Portal.Repositories;

namespace HDMC.Portal.Services
{
    public class CompanySelectionService
    {
        public const int HardwareMinAlarmAppId = 1;
        public const int CountLocationAppId = 2;
        private const string DefaultHardwareEntryUrl = "https://localhost:44316/Home/Entry";

        private readonly CompanyRepository _companyRepository;
        private readonly SessionService _sessionService;
        private readonly SystemSettingService _systemSettingService;

        public CompanySelectionService()
            : this(
                  new CompanyRepository(),
                  new SessionService(),
                  new SystemSettingService())
        {
        }

        public CompanySelectionService(
            CompanyRepository companyRepository,
            SessionService sessionService,
            SystemSettingService systemSettingService)
        {
            _companyRepository = companyRepository;
            _sessionService = sessionService;
            _systemSettingService = systemSettingService;
        }

        public bool IsLoggedIn(HttpSessionStateBase session)
        {
            return _sessionService.IsLoggedIn(session);
        }

        public List<CompanyModel> GetCompaniesForCurrentUser(HttpSessionStateBase session)
        {
            var userId = _sessionService.GetUserId(session);

            return _companyRepository.GetCompaniesByUserId(
                userId,
                HardwareMinAlarmAppId);
        }

        public bool HasAccessToApp(
            HttpSessionStateBase session,
            int appId)
        {
            var userId = _sessionService.GetUserId(session);

            return _companyRepository.GetCompaniesByUserId(
                    userId,
                    appId)
                .Any();
        }

        public string SelectCompanyAndBuildHardwareUrl(
            HttpSessionStateBase session,
            string company)
        {
            _sessionService.SetCompany(session, company);

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["userId"] = _sessionService.GetUserId(session);
            queryString["userName"] = _sessionService.GetUserName(session);
            queryString["company"] = company;

            return GetHardwareEntryUrl() + "?" + queryString;
        }

        private string GetHardwareEntryUrl()
        {
            return _systemSettingService.GetValue(
                SystemSettingService.HardwareEntryUrlKey,
                "HardwareEntryUrl",
                DefaultHardwareEntryUrl);
        }
    }
}
