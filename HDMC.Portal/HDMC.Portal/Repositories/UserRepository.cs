using System.Linq;
using Dapper;
using HDMC.Portal.Helpers;
using HDMC.Portal.Models;

namespace HDMC.Portal.Repositories
{
    public class UserRepository
    {
        public UserLoginModel GetUserByUserId(string userId)
        {
            using (var connection = DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                                    SELECT TOP 1
                                        u.user_id      AS UserId,
                                        u.user_name    AS UserName,
                                        u.password_hash AS PasswordHash,
                                        ur.role_id     AS RoleId,
                                        u.is_active    AS IsActive
                                    FROM Users u
                                    INNER JOIN User_Role ur
                                        ON u.user_id = ur.user_id
                                    WHERE u.user_id = @UserId";

                return connection.Query<UserLoginModel>(
                    sql,
                    new
                    {
                        UserId = userId
                    })
                    .FirstOrDefault();
            }
        }
    }
}
