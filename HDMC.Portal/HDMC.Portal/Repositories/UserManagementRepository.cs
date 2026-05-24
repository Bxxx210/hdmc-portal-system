using System.Collections.Generic;
using System.Linq;
using Dapper;
using HDMC.Portal.Helpers;
using HDMC.Portal.Models;

namespace HDMC.Portal.Repositories
{
    public class UserManagementRepository
    {
        public List<UserManagementModel> GetUsers(string searchText = null)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                            SELECT
                                u.user_id      AS UserId,
                                u.user_name    AS UserName,
                                u.is_active    AS IsActive,
                                access.Companies AS Companies,
                                access.Applications AS Applications,
                                r.role_id      AS RoleId,
                                r.role_name    AS RoleName
                            FROM Users u
                            LEFT JOIN User_Role ur
                                ON u.user_id = ur.user_id
                            LEFT JOIN Roles r
                                ON ur.role_id = r.role_id
                            OUTER APPLY
                            (
                                SELECT
                                    STUFF((
                                        SELECT DISTINCT ', ' + uc2.company
                                        FROM User_Company uc2
                                        WHERE uc2.user_id = u.user_id
                                        FOR XML PATH(''), TYPE
                                    ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Companies,
                                    STUFF((
                                        SELECT DISTINCT ', ' +
                                            CASE uc3.app_id
                                                WHEN 1 THEN 'Hardware Min Alarm'
                                                WHEN 2 THEN 'Count Location Pick Face'
                                                ELSE 'App ' + CAST(uc3.app_id AS NVARCHAR(20))
                                            END
                                        FROM User_Company uc3
                                        WHERE uc3.user_id = u.user_id
                                        FOR XML PATH(''), TYPE
                                    ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Applications
                            ) access
                            WHERE
                                (
                                    @SearchText IS NULL
                                    OR u.user_id LIKE @SearchPattern
                                    OR u.user_name LIKE @SearchPattern
                                    OR access.Companies LIKE @SearchPattern
                                    OR access.Applications LIKE @SearchPattern
                                    OR r.role_name LIKE @SearchPattern
                                )
                            ORDER BY u.user_id";

                return connection.Query<UserManagementModel>(
                    sql,
                    new
                    {
                        SearchText = string.IsNullOrWhiteSpace(searchText)
                            ? null
                            : searchText,
                        SearchPattern = "%"
                            + (searchText ?? string.Empty).Trim()
                            + "%"
                    })
                    .ToList();
            }
        }

        public UserManagementModel GetUser(string userId)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                    SELECT
                        u.user_id      AS UserId,
                        u.user_name    AS UserName,
                        u.is_active    AS IsActive,
                        r.role_id      AS RoleId,
                        r.role_name    AS RoleName
                    FROM Users u
                    LEFT JOIN User_Role ur
                        ON u.user_id = ur.user_id
                    LEFT JOIN Roles r
                        ON ur.role_id = r.role_id
                    WHERE u.user_id = @UserId";

                var user =
                    connection.QueryFirstOrDefault<UserManagementModel>(
                    sql,
                    new
                    {
                        UserId = userId
                    });

                if (user == null)
                {
                    return null;
                }

                user.SelectedCompanyCodes =
                    GetUserCompanies(connection, userId);

                user.SelectedAppIds =
                    GetUserAppIds(connection, userId);

                user.Company =
                    user.SelectedCompanyCodes.FirstOrDefault();

                return user;
            }
        }

        public List<CompanyModel> GetActiveCompanies()
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                    SELECT
                        1 AS AppId,
                        company_code AS Company
                    FROM Companies
                    WHERE is_active = 1
                    ORDER BY company_code";

                return connection.Query<CompanyModel>(sql)
                    .ToList();
            }
        }

        public List<RoleModel> GetRoles()
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                    SELECT
                        role_id AS RoleId,
                        role_name AS RoleName
                    FROM Roles
                    ORDER BY role_id";

                return connection.Query<RoleModel>(sql)
                    .ToList();
            }
        }

        public bool UserExists(string userId)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                    SELECT COUNT(1)
                    FROM Users
                    WHERE user_id = @UserId";

                return connection.ExecuteScalar<int>(
                    sql,
                    new
                    {
                        UserId = userId
                    }) > 0;
            }
        }

        public bool CompanyExists(string company)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                    SELECT COUNT(1)
                    FROM Companies
                    WHERE company_code = @Company
                    AND is_active = 1";

                return connection.ExecuteScalar<int>(
                    sql,
                    new
                    {
                        Company = company
                    }) > 0;
            }
        }

        public bool RoleExists(int roleId)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                    SELECT COUNT(1)
                    FROM Roles
                    WHERE role_id = @RoleId";

                return connection.ExecuteScalar<int>(
                    sql,
                    new
                    {
                        RoleId = roleId
                    }) > 0;
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

                    InsertUserCompanyAccess(
                        connection,
                        transaction,
                        model);

                    transaction.Commit();
                }
            }
        }

        public void UpdatePasswordHash(
            string userId,
            string passwordHash)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                    UPDATE Users
                    SET
                        password_hash = @PasswordHash
                    WHERE user_id = @UserId";

                connection.Execute(
                    sql,
                    new
                    {
                        UserId = userId,
                        PasswordHash = passwordHash
                    });
            }
        }

        public void UpdateUser(UserManagementModel model)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                connection.Open();

                using (var transaction =
                       connection.BeginTransaction())
                {
                    const string userSql = @"
                        UPDATE Users
                        SET
                            user_name = @UserName,
                            is_active = @IsActive
                        WHERE user_id = @UserId";

                    connection.Execute(
                        userSql,
                        new
                        {
                            model.UserId,
                            model.UserName,
                            model.IsActive
                        },
                        transaction);

                    UpsertUserRole(connection, transaction, model);
                    ReplaceUserCompanyAccess(connection, transaction, model);

                    transaction.Commit();
                }
            }
        }

        public void SetActive(string userId, bool isActive)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                    UPDATE Users
                    SET is_active = @IsActive
                    WHERE user_id = @UserId";

                connection.Execute(
                    sql,
                    new
                    {
                        UserId = userId,
                        IsActive = isActive
                    });
            }
        }

        private void UpsertUserRole(
            System.Data.IDbConnection connection,
            System.Data.IDbTransaction transaction,
            UserManagementModel model)
        {
            const string existsSql = @"
                SELECT COUNT(1)
                FROM User_Role
                WHERE user_id = @UserId";

            var exists =
                connection.ExecuteScalar<int>(
                    existsSql,
                    new
                    {
                        model.UserId
                    },
                    transaction) > 0;

            var sql = exists
                ? @"
                    UPDATE User_Role
                    SET role_id = @RoleId
                    WHERE user_id = @UserId"
                : @"
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
                sql,
                new
                {
                    model.UserId,
                    model.RoleId
                },
                transaction);
        }

        private void ReplaceUserCompanyAccess(
            System.Data.IDbConnection connection,
            System.Data.IDbTransaction transaction,
            UserManagementModel model)
        {
            const string deleteSql = @"
                DELETE FROM User_Company
                WHERE user_id = @UserId";

            connection.Execute(
                deleteSql,
                new
                {
                    model.UserId
                },
                transaction);

            InsertUserCompanyAccess(
                connection,
                transaction,
                model);
        }

        private void InsertUserCompanyAccess(
            System.Data.IDbConnection connection,
            System.Data.IDbTransaction transaction,
            UserManagementModel model)
        {
            const string sql = @"
                INSERT INTO User_Company
                (
                    user_id,
                    app_id,
                    company
                )
                VALUES
                (
                    @UserId,
                    @AppId,
                    @Company
                )";

            foreach (var appId in model.SelectedAppIds.Distinct())
            {
                foreach (var company in model.SelectedCompanyCodes.Distinct())
                {
                    connection.Execute(
                        sql,
                        new
                        {
                            model.UserId,
                            AppId = appId,
                            Company = company
                        },
                        transaction);
                }
            }
        }

        private string[] GetUserCompanies(
            System.Data.IDbConnection connection,
            string userId)
        {
            const string sql = @"
                SELECT DISTINCT company
                FROM User_Company
                WHERE user_id = @UserId
                ORDER BY company";

            return connection.Query<string>(
                sql,
                new
                {
                    UserId = userId
                })
                .ToArray();
        }

        private int[] GetUserAppIds(
            System.Data.IDbConnection connection,
            string userId)
        {
            const string sql = @"
                SELECT DISTINCT app_id
                FROM User_Company
                WHERE user_id = @UserId
                ORDER BY app_id";

            return connection.Query<int>(
                sql,
                new
                {
                    UserId = userId
                })
                .ToArray();
        }
    }
}
