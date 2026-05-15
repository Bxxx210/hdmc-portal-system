using System.Collections.Generic;
using System.Linq;
using Dapper;
using HDMC.Portal.Helpers;
using HDMC.Portal.Models;

namespace HDMC.Portal.Repositories
{
    public class CompanyRepository
    {
        public List<CompanyModel> GetCompaniesByUserId(
            string userId,
            int appId)
        {
            using (var connection = DbConnectionFactory.CreateSharedAuthConnection())
            {
                const string sql = @"
                            SELECT
                                uc.app_id AS AppId,
                                c.company_code AS Company
                            FROM User_Company uc
                            INNER JOIN Companies c
                                ON uc.company = c.company_code
                            WHERE uc.user_id = @UserId
                            AND uc.app_id = @AppId
                            AND c.is_active = 1
                            ORDER BY c.company_code";
                return connection.Query<CompanyModel>(
                    sql,
                    new
                    {
                        UserId = userId,
                        AppId = appId
                    })
                    .ToList();
            }
        }
    }
}
