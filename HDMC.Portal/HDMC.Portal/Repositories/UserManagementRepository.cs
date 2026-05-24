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
                            WHERE
                                (
                                    @SearchText IS NULL
                                    OR u.user_id LIKE @SearchPattern
                                    OR u.user_name LIKE @SearchPattern
                                    OR uc.company LIKE @SearchPattern
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
                    WHERE u.user_id = @UserId";

                return connection.QueryFirstOrDefault<UserManagementModel>(
                    sql,
                    new
                    {
                        UserId = userId
                    });
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
                    UpsertUserCompany(connection, transaction, model);

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

        private void UpsertUserCompany(
            System.Data.IDbConnection connection,
            System.Data.IDbTransaction transaction,
            UserManagementModel model)
        {
            const string existsSql = @"
                SELECT COUNT(1)
                FROM User_Company
                WHERE user_id = @UserId
                AND app_id = 1";

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
                    UPDATE User_Company
                    SET company = @Company
                    WHERE user_id = @UserId
                    AND app_id = 1"
                : @"
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
                sql,
                new
                {
                    model.UserId,
                    model.Company
                },
                transaction);
        }
    }
}
