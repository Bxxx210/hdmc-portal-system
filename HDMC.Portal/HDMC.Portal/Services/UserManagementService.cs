using System;
using System.Collections.Generic;
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
                throw new ArgumentException("Company is required");
            }

            if (model.RoleId <= 0)
            {
                throw new ArgumentException("Role is required");
            }
        }

        private void ValidateCompanyAndRole(UserManagementModel model)
        {
            if (!_repository.CompanyExists(model.Company))
            {
                throw new ArgumentException("Selected company is not active or does not exist");
            }

            if (!_repository.RoleExists(model.RoleId))
            {
                throw new ArgumentException("Selected role does not exist");
            }
        }
    }
}
