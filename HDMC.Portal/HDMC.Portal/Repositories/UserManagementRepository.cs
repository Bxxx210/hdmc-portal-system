using System.Collections.Generic;
using System.Linq;
using Dapper;
using HDMC.Portal.Helpers;
using HDMC.Portal.Models;

namespace HDMC.Portal.Repositories
{
    public class UserManagementRepository
    {
        public List<UserManagementModel> GetUsers()
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                            SELECT
                                u.user_id      AS UserId,
                                u.user_name    AS UserName,
                                u.is_active    AS IsActive,
                                uc.company     AS Company,
                                r.role_id      AS RoleId,
                                r.role_name    AS RoleName
                            FROM Users u
                            LEFT JOIN User_Role ur
                                ON u.user_id = ur.user_id
                            LEFT JOIN Roles r
                                ON ur.role_id = r.role_id
                            LEFT JOIN User_Company uc
                                ON u.user_id = uc.user_id
                            ORDER BY u.user_id";

                return connection.Query<UserManagementModel>(sql)
                    .ToList();
            }
        }

        // Create new user with role/company mapping
        public void CreateUser(
            UserManagementModel model,
            string passwordHash)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                connection.Open();

                using (var transaction =
                       connection.BeginTransaction())
                {
                    // Create user master
                    const string userSql = @"
                            INSERT INTO Users
                            (
                                user_id,
                                user_name,
                                password_hash,
                                is_active,
                                created_date
                            )
                            VALUES
                            (
                                @UserId,
                                @UserName,
                                @PasswordHash,
                                1,
                                GETDATE()
                            )";

                    connection.Execute(
                        userSql,
                        new
                        {
                            model.UserId,
                            model.UserName,
                            PasswordHash = passwordHash
                        },
                        transaction);

                    // Assign role
                    const string roleSql = @"
                            INSERT INTO User_Role
                            (
                                user_id,
                                role_id
                            )
                            VALUES
                            (
                                @UserId,
                                @RoleId
                            )";

                    connection.Execute(
                        roleSql,
                        new
                        {
                            model.UserId,
                            model.RoleId
                        },
                        transaction);

                    // Assign company access
                    const string companySql = @"
                        INSERT INTO User_Company
                        (
                            user_id,
                            app_id,
                            company
                        )
                        VALUES
                        (
                            @UserId,
                            1,
                            @Company
                        )";

                    connection.Execute(
                        companySql,
                        new
                        {
                            model.UserId,
                            model.Company
                        },
                        transaction);

                    transaction.Commit();
                }
            }
        }
    }
}