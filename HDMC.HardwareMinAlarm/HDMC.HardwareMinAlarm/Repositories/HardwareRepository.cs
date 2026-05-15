using System.Linq;
using Dapper;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Models;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace HDMC.HardwareMinAlarm.Repositories
{
    public class HardwareRepository
    {
        public PartSearchResultModel GetPart(
            string partNumber,
            string company)
        {
            using (var connection =
                   DbConnectionFactory.CreateHardwareConnection())
            {
                // Get item master first
                const string itemSql = @"
                    SELECT TOP 1
                        Part        AS PartNumber,
                        Description AS PartDescription,
                        company     AS Company
                    FROM Item_master
                    WHERE Part = @PartNumber
                    AND company = @Company";

                var item =
                    connection.QueryFirstOrDefault<
                        PartSearchResultModel>(
                            itemSql,
                            new
                            {
                                PartNumber = partNumber,
                                Company = company
                            });

                // Part does not exist in master
                if (item == null)
                {
                    return null;
                }

                // Get latest transaction/status
                const string statusSql = @"
                    SELECT TOP 1
                        part_status AS PartStatus,
                        Userstamp   AS UserStamp
                    FROM Input_hardware
                    WHERE part_number = @PartNumber
                    AND part_company = @Company
                    ORDER BY [date] DESC";

                var latest =
                    connection.QueryFirstOrDefault<dynamic>(
                        statusSql,
                        new
                        {
                            PartNumber = partNumber,
                            Company = company
                        });

                // Merge latest transaction data
                if (latest != null)
                {
                    item.PartStatus =
                        latest.PartStatus;

                    item.UserStamp =
                        latest.UserStamp;
                }

                return item;
            }
        }
        public void SaveStatus(
     SaveStatusRequestModel request)
        {
            using (var connection =
                   DbConnectionFactory.CreateHardwareConnection())
            {
                connection.Open();

                using (var transaction =
                       connection.BeginTransaction())
                {
                    var locationColumn =
                        GetLocationColumn(request.StatusCode);

                    if (string.IsNullOrWhiteSpace(locationColumn))
                    {
                        throw new System.Exception("Invalid Status");
                    }

                    var locationTimeColumn =
                        locationColumn.Replace("loc", "loc_time");

                    var statusText =
                        GetStatusText(request.StatusCode);

                    const string descSql = @"
                            SELECT TOP 1 Description
                            FROM Item_master
                            WHERE Part = @PartNumber
                            AND Company = @Company";

                    var partDescription =
                        connection.QueryFirstOrDefault<string>(
                            descSql,
                            new
                            {
                                request.PartNumber,
                                request.Company
                            },
                            transaction);

                    const string latestSql = @"
                                SELECT TOP 1
                                    id,
                                    part_status
                                FROM Input_hardware
                                WHERE part_number = @PartNumber
                                AND part_company = @Company
                                ORDER BY [date] DESC";

                    var latest =
                        connection.QueryFirstOrDefault<dynamic>(
                            latestSql,
                            new
                            {
                                request.PartNumber,
                                request.Company
                            },
                            transaction);

                    var shouldInsert =
                        latest == null ||
                        latest.part_status?.ToString() == "900";

                    if (shouldInsert)
                    {
                        var insertSql = $@"
                            INSERT INTO Input_hardware
                            (
                                part_number,
                                part_description,
                                part_company,
                                Userstamp,
                                part_status,
                                {locationColumn},
                                {locationTimeColumn},
                                [date],
                                user_id
                            )
                            VALUES
                            (
                                @PartNumber,
                                @PartDescription,
                                @Company,
                                @UserName,
                                @StatusCode,
                                @StatusText,
                                GETDATE(),
                                GETDATE(),
                                @UserId
                            )";

                        connection.Execute(
                            insertSql,
                            new
                            {
                                request.PartNumber,
                                PartDescription = partDescription,
                                request.Company,
                                request.UserName,
                                request.StatusCode,
                                StatusText = statusText,
                                request.UserId
                            },
                            transaction);
                    }
                    else
                    {
                        var updateSql = $@"
                            UPDATE Input_hardware
                            SET
                                part_status = @StatusCode,
                                {locationColumn} = @StatusText,
                                {locationTimeColumn} = GETDATE(),
                                [date] = GETDATE(),
                                user_id = @UserId,
                                Userstamp = @UserName
                            WHERE id = @Id";

                        connection.Execute(
                            updateSql,
                            new
                            {
                                Id = latest.id,
                                request.StatusCode,
                                StatusText = statusText,
                                request.UserId,
                                request.UserName
                            },
                            transaction);
                    }

                    const string logSql = @"
                            INSERT INTO Input_hardware_log
                            (
                                part_number,
                                part_company,
                                part_status,
                                location,
                                action_time,
                                user_id
                            )
                            VALUES
                            (
                                @PartNumber,
                                @Company,
                                @StatusCode,
                                @StatusText,
                                GETDATE(),
                                @UserId
                            )";

                    connection.Execute(
                        logSql,
                        new
                        {
                            request.PartNumber,
                            request.Company,
                            request.StatusCode,
                            StatusText = statusText,
                            request.UserId
                        },
                        transaction);

                    transaction.Commit();
                }
            }
        }

        private string GetLocationColumn(string statusCode)
        {
            switch (statusCode)
            {
                case "100":
                    return "loc1";

                case "200":
                    return "loc2";

                case "300":
                    return "loc3";

                case "400":
                    return "loc4";

                case "500":
                    return "loc5";

                case "600":
                    return "loc6";

                case "900":
                    return "loc7";

                default:
                    return null;
            }
        }

        private string GetStatusText(string statusCode)
        {
            switch (statusCode)
            {
                case "100":
                    return "Request Replenish";

                case "200":
                    return "Kanban from MFG";

                case "300":
                    return "Stock min <10 days";

                case "400":
                    return "Location discrepancy";

                case "500":
                    return "Check Shipment ETA at plant";

                case "600":
                    return "ETA Today";

                case "900":
                    return "Replenish Completed";

                default:
                    return string.Empty;
            }
        }


    }
}
