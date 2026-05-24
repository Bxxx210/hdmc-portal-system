using System;
using HDMC.Portal.Models;
using HDMC.Portal.Repositories;

namespace HDMC.Portal.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;

        public AuthService()
            : this(new UserRepository())
        {
        }

        public AuthService(UserRepository userRepository)
        {
            _userRepository = userRepository;
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

            var isPasswordValid =
                IsPasswordValid(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null;
            }

            return user;
        }

        private bool IsPasswordValid(
            string password,
            string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }

            var normalizedPasswordHash =
                passwordHash.Trim();

            try
            {
                return BCrypt.Net.BCrypt.Verify(
                    password,
                    normalizedPasswordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
