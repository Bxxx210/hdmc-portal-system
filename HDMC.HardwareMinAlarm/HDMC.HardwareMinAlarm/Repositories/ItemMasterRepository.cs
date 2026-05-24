using System.Collections.Generic;
using System.Linq;
using Dapper;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Models;

namespace HDMC.HardwareMinAlarm.Repositories
{
    public class ItemMasterRepository
    {
        public List<ItemMasterModel> Search(
            string company,
            string searchText)
        {
            using (var connection =
                   DbConnectionFactory.CreateHardwareConnection())
            {
                const string sql = @"
                    SELECT TOP 200
                        company AS Company,
                        Part,
                        Description
                    FROM Item_master
                    WHERE company = @Company
                    AND
                    (
                        @SearchText IS NULL
                        OR Part LIKE @SearchPattern
                        OR Description LIKE @SearchPattern
                    )
                    ORDER BY Part";

                return connection.Query<ItemMasterModel>(
                        sql,
                        new
                        {
                            Company = company,
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

        public int DeleteParts(
            string company,
            IEnumerable<string> parts)
        {
            var partList =
                parts
                    .Where(part => !string.IsNullOrWhiteSpace(part))
                    .Select(part => part.Trim())
                    .Distinct()
                    .ToArray();

            if (!partList.Any())
            {
                return 0;
            }

            using (var connection =
                   DbConnectionFactory.CreateHardwareConnection())
            {
                const string sql = @"
                    DELETE FROM Item_master
                    WHERE company = @Company
                    AND Part IN @Parts";

                return connection.Execute(
                    sql,
                    new
                    {
                        Company = company,
                        Parts = partList
                    });
            }
        }
    }
}
