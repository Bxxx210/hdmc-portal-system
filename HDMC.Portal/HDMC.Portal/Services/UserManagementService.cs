using System;
using System.Collections.Generic;
using System.Linq;
using HDMC.Portal.Models;
using HDMC.Portal.Repositories;

namespace HDMC.Portal.Services
{
    public class UserManagementService
    {
        private readonly UserManagementRepository _repository;

        public UserManagementService()
            : this(new UserManagementRepository())
        {
        }

        public UserManagementService(UserManagementRepository repository)
        {
            _repository = repository;
        }

        public List<UserManagementModel> GetUsers(string searchText = null)
        {
            return _repository.GetUsers(searchText);
        }

        public UserManagementModel GetUser(string userId)
        {
            return _repository.GetUser(userId);
        }

        public List<CompanyModel> GetActiveCompanies()
        {
            return _repository.GetActiveCompanies();
        }

        public List<RoleModel> GetRoles()
        {
            return _repository.GetRoles();
        }

        public List<ApplicationModel> GetApplications()
        {
            return ApplicationCatalog.GetApplications();
        }

        public void CreateUser(
            UserManagementModel model,
            string password,
            string confirmPassword)
        {
            ValidateUser(model);

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is required");
            }

            if (password != confirmPassword)
            {
                throw new ArgumentException("Password and Confirm Password do not match");
            }

            if (_repository.UserExists(model.UserId))
            {
                throw new ArgumentException("User ID already exists");
            }

            ValidateCompanyAndRole(model);

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _repository.CreateUser(model, passwordHash);
        }

        public void UpdateUser(UserManagementModel model)
        {
            ValidateUser(model);

            if (!_repository.UserExists(model.UserId))
            {
                throw new ArgumentException("User not found");
            }

            ValidateCompanyAndRole(model);

            _repository.UpdateUser(model);
        }

        public void SetActive(string userId, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID is required");
            }

            _repository.SetActive(userId, isActive);
        }

        public void ResetPassword(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID is required");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is required");
            }

            if (!_repository.UserExists(userId))
            {
                throw new ArgumentException("User not found");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _repository.UpdatePasswordHash(userId, passwordHash);
        }

        private void ValidateUser(UserManagementModel model)
        {
            if (model == null)
            {
                throw new ArgumentException("User data is required");
            }

            if (string.IsNullOrWhiteSpace(model.UserId))
            {
                throw new ArgumentException("User ID is required");
            }

            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                throw new ArgumentException("User Name is required");
            }

            if (string.IsNullOrWhiteSpace(model.Company))
            {
                model.Company = model.SelectedCompanyCodes != null
                    ? model.SelectedCompanyCodes.FirstOrDefault()
                    : null;
            }

            if (!model.IsAllCompanies &&
                (model.SelectedCompanyCodes == null ||
                 !model.SelectedCompanyCodes.Any()))
            {
                throw new ArgumentException("Company is required");
            }

            if (model.SelectedAppIds == null ||
                !model.SelectedAppIds.Any())
            {
                throw new ArgumentException("Application access is required");
            }

            if (model.RoleId <= 0)
            {
                throw new ArgumentException("Role is required");
            }
        }

        private void ValidateCompanyAndRole(UserManagementModel model)
        {
            model.SelectedCompanyCodes =
                model.IsAllCompanies
                    ? _repository.GetActiveCompanies()
                        .Select(company => company.Company)
                        .ToArray()
                    : model.SelectedCompanyCodes;

            if (model.SelectedCompanyCodes == null ||
                !model.SelectedCompanyCodes.Any())
            {
                throw new ArgumentException("Company is required");
            }

            model.SelectedCompanyCodes =
                model.SelectedCompanyCodes
                    .Where(company => !string.IsNullOrWhiteSpace(company))
                    .Select(company => company.Trim())
                    .Distinct()
                    .ToArray();

            model.SelectedAppIds =
                model.SelectedAppIds
                    .Distinct()
                    .ToArray();

            model.Company =
                model.SelectedCompanyCodes.FirstOrDefault();

            foreach (var company in model.SelectedCompanyCodes)
            {
                if (!_repository.CompanyExists(company))
                {
                    throw new ArgumentException("Selected company is not active or does not exist");
                }
            }

            foreach (var appId in model.SelectedAppIds)
            {
                if (!ApplicationCatalog.AppExists(appId))
                {
                    throw new ArgumentException("Selected application does not exist");
                }
            }

            if (!_repository.RoleExists(model.RoleId))
            {
                throw new ArgumentException("Selected role does not exist");
            }
        }
    }

    internal static class ApplicationCatalog
    {
        public const int HardwareMinAlarmAppId = 1;

        public const int CountLocationAppId = 2;

        public static List<ApplicationModel> GetApplications()
        {
            return new List<ApplicationModel>
            {
                new ApplicationModel
                {
                    AppId = HardwareMinAlarmAppId,
                    AppName = "Hardware Min Alarm"
                },
                new ApplicationModel
                {
                    AppId = CountLocationAppId,
                    AppName = "Count Location Pick Face"
                }
            };
        }

        public static bool AppExists(int appId)
        {
            return GetApplications()
                .Any(application => application.AppId == appId);
        }
    }
}
