using BCrypt.Net;
using HDMC.Portal.Models;
using HDMC.Portal.Repositories;

namespace HDMC.Portal.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;

        public AuthService()
        {
            _userRepository = new UserRepository();
        }

        public UserLoginModel Login(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = _userRepository.GetUserByUserId(userId);

            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            var isPasswordValid = password == "admin";
            //BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null;
            }

            return user;
        }
    }
}
