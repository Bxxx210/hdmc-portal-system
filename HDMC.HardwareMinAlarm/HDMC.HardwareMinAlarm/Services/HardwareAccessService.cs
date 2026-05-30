using System;
using System.Collections.Generic;
using System.Linq;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Models;
using HDMC.HardwareMinAlarm.Repositories;

namespace HDMC.HardwareMinAlarm.Services
{
    public class HardwareAccessService
    {
        public const int AdminRoleId = 1;
        public const int SuperUserRoleId = 2;
        public const int UserRoleId = 3;

        private readonly HardwareAccessRepository _repository;

        public HardwareAccessService()
            : this(new HardwareAccessRepository())
        {
        }

        public HardwareAccessService(HardwareAccessRepository repository)
        {
            _repository = repository;
        }

        public HardwareAccessModel GetAccess(string userId)
        {
            return _repository.GetAccess(userId);
        }

        public List<CompanyAccessModel> GetCompaniesForCurrentUser()
        {
            var access =
                GetAccess(SessionHelper.GetCurrentUser().UserId);

            return access == null
                ? new List<CompanyAccessModel>()
                : access.Companies;
        }

        public bool IsElevatedRole(int roleId)
        {
            return roleId == AdminRoleId ||
                roleId == SuperUserRoleId;
        }

        public bool CanAccessCompany(
            HardwareAccessModel access,
            string company)
        {
            return access != null &&
                !string.IsNullOrWhiteSpace(company) &&
                access.Companies.Any(item =>
                    string.Equals(
                        item.Company,
                        company,
                        StringComparison.OrdinalIgnoreCase));
        }

        public bool CanCurrentUserAccessCompany(string company)
        {
            return CanAccessCompany(
                GetAccess(SessionHelper.GetCurrentUser().UserId),
                company);
        }
    }
}
