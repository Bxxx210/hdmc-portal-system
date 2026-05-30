using System.Collections.Generic;
using System.Linq;
using Dapper;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Models;

namespace HDMC.HardwareMinAlarm.Repositories
{
    public class HardwareAccessRepository
    {
        private const int HardwareMinAlarmAppId = 1;

        public HardwareAccessModel GetAccess(string userId)
        {
            using (var connection =
                   DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string userSql = @"
                    SELECT TOP 1
                        u.user_id   AS UserId,
                        u.user_name AS UserName,
                        ur.role_id  AS RoleId
                    FROM Users u
                    INNER JOIN User_Role ur
                        ON u.user_id = ur.user_id
                    WHERE u.user_id = @UserId
                    AND u.is_active = 1";

                var access =
                    connection.QueryFirstOrDefault<HardwareAccessModel>(
                        userSql,
                        new
                        {
                            UserId = userId
                        });

                if (access == null)
                {
                    return null;
                }

                const string companySql = @"
                    SELECT DISTINCT
                        c.company_code AS Company
                    FROM User_Company uc
                    INNER JOIN Companies c
                        ON uc.company = c.company_code
                    WHERE uc.user_id = @UserId
                    AND uc.app_id = @AppId
                    AND c.is_active = 1
                    ORDER BY c.company_code";

                access.Companies =
                    connection.Query<CompanyAccessModel>(
                        companySql,
                        new
                        {
                            UserId = userId,
                            AppId = HardwareMinAlarmAppId
                        })
                    .ToList();

                return access;
            }
        }
    }
}
