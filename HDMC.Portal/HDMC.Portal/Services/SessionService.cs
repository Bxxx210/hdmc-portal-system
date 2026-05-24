using System.Web;
using HDMC.Portal.Models;

namespace HDMC.Portal.Services
{
    public class SessionService
    {
        private const string UserIdKey = "UserId";
        private const string UserNameKey = "UserName";
        private const string RoleIdKey = "RoleId";
        private const string CompanyKey = "Company";

        public bool IsLoggedIn(HttpSessionStateBase session)
        {
            return session != null && session[UserIdKey] != null;
        }

        public void SetUser(HttpSessionStateBase session, UserLoginModel user)
        {
            session[UserIdKey] = user.UserId;
            session[UserNameKey] = user.UserName;
            session[RoleIdKey] = user.RoleId;
        }

        public void SetCompany(HttpSessionStateBase session, string company)
        {
            session[CompanyKey] = company;
        }

        public string GetUserId(HttpSessionStateBase session)
        {
            return session[UserIdKey]?.ToString();
        }

        public string GetUserName(HttpSessionStateBase session)
        {
            return session[UserNameKey]?.ToString();
        }

        public int GetRoleId(HttpSessionStateBase session)
        {
            if (session == null ||
                session[RoleIdKey] == null)
            {
                return 0;
            }

            int roleId;

            return int.TryParse(
                session[RoleIdKey].ToString(),
                out roleId)
                ? roleId
                : 0;
        }
    }
}
