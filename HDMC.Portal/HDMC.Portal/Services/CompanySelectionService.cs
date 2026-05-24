using System.Collections.Generic;
using System.Configuration;
using System.Web;
using HDMC.Portal.Models;
using HDMC.Portal.Repositories;

namespace HDMC.Portal.Services
{
    public class CompanySelectionService
    {
        private const int HardwareMinAlarmAppId = 1;
        private const string DefaultHardwareEntryUrl = "https://localhost:44316/Home/Entry";

        private readonly CompanyRepository _companyRepository;
        private readonly SessionService _sessionService;

        public CompanySelectionService()
            : this(new CompanyRepository(), new SessionService())
        {
        }

        public CompanySelectionService(
            CompanyRepository companyRepository,
            SessionService sessionService)
        {
            _companyRepository = companyRepository;
            _sessionService = sessionService;
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
            var configuredUrl =
                ConfigurationManager.AppSettings["HardwareEntryUrl"];

            if (string.IsNullOrWhiteSpace(configuredUrl))
            {
                return DefaultHardwareEntryUrl;
            }

            return configuredUrl;
        }
    }
}
