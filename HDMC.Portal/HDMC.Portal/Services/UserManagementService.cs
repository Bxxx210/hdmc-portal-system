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

        public List<UserManagementModel> GetUsers()
        {
            return _repository.GetUsers();
        }

        public void CreateUser(UserManagementModel model, string password)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _repository.CreateUser(model, passwordHash);
        }

        public void ResetPassword(string userId, string password)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _repository.UpdatePasswordHash(userId, passwordHash);
        }
    }
}
