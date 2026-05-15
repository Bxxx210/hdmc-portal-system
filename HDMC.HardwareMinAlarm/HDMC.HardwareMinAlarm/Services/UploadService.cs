using System;
using System.IO;
using System.Web;
using System.Linq;
using Dapper;
using ClosedXML;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Models;
using ClosedXML.Excel;
using Microsoft.Ajax.Utilities;

namespace HDMC.HardwareMinAlarm.Services
{
    public class UploadService
    {
        // Temporary upload validation/service
        // TODO:
        // - Read Excel
        // - Validate columns
        // - Insert/Update Item_master
        // - Import log
        public UploadResultModel UploadItemMaster(
      HttpPostedFileBase file)
        {
            var result =
                new UploadResultModel();

            result.FileName = file.FileName;

            try
            {
                using (var workbook =
                       new XLWorkbook(file.InputStream))
                {
                    var worksheet =
                        workbook.Worksheet(1);

                    var rows =
                        worksheet.RowsUsed()
                            .Skip(1);

                    using (var connection =
                           DbConnectionFactory
                               .CreateHardwareConnection())
                    {
                        connection.Open();

                        foreach (var row in rows)
                        {
                            // Read excel columns
                            var company =
                                row.Cell(1).GetString().Trim();

                            var part =
                                row.Cell(2).GetString().Trim();

                            var description =
                                row.Cell(3).GetString().Trim();

                            // Validate required fields
                            if (string.IsNullOrWhiteSpace(company) ||
                                string.IsNullOrWhiteSpace(part))
                            {
                                result.Errors.Add(
                                $"Row {result.TotalRows}: Company or Part is empty");

                                result.FailedRows++;

                                continue;
                            }

                            result.TotalRows++;

                            // Skip empty row
                            if (string.IsNullOrWhiteSpace(part))
                            {
                                result.Errors.Add(
                                $"Row {result.TotalRows}: Part is empty [{part}]");

                                result.FailedRows++;

                                continue;
                            }

                            // Validate company master
                            const string companySql = @"
                                SELECT COUNT(1)
                                FROM Companies
                                WHERE company_code = @Company
                                AND is_active = 1";

                            var validCompany =
                                connection.ExecuteScalar<int>(
                                    companySql,
                                    new
                                    {
                                        Company = company
                                    });

                            if (validCompany == 0)
                            {
                                result.Errors.Add(
                                $"Row {result.TotalRows}: Invalid company [{company}]");

                                result.FailedRows++;

                                continue;
                            }


                            // Check existing item
                            const string checkSql = @"
                                SELECT COUNT(1)
                                FROM Item_master
                                WHERE company = @Company
                                AND Part = @Part";

                            var exists =
                                connection.ExecuteScalar<int>(
                                    checkSql,
                                    new
                                    {
                                        Company = company,
                                        Part = part
                                    });

                            if (exists > 0)
                            {
                                // Update existing item
                                const string updateSql = @"
                                    UPDATE Item_master
                                    SET Description = @Description
                                    WHERE company = @Company
                                    AND Part = @Part";

                                connection.Execute(
                                    updateSql,
                                    new
                                    {
                                        Company = company,
                                        Part = part,
                                        Description = description
                                    });
                            }
                            else
                            {
                                // Insert new item
                                const string insertSql = @"
                                    INSERT INTO Item_master
                                    (
                                        company,
                                        Part,
                                        Description
                                    )
                                    VALUES
                                    (
                                        @Company,
                                        @Part,
                                        @Description
                                    )";

                                connection.Execute(
                                    insertSql,
                                    new
                                    {
                                        Company = company,
                                        Part = part,
                                        Description = description
                                    });
                            }

                            result.SuccessRows++;
                        }
                        // Save
                        // import history log
                        const string logSql = @"
                            INSERT INTO Item_Master_Import_Log
                            (
                                file_name,
                                total_rows,
                                success_rows,
                                failed_rows,
                                uploaded_by
                            )
                            VALUES
                            (
                                @FileName,
                                @TotalRows,
                                @SuccessRows,
                                @FailedRows,
                                @UploadedBy
                            )";

                        connection.Execute(
                            logSql,
                            new
                            {
                                FileName = result.FileName,
                                TotalRows = result.TotalRows,
                                SuccessRows = result.SuccessRows,
                                FailedRows = result.FailedRows,
                                UploadedBy = "SYSTEM"
                            });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage =
                    ex.Message;

                return result;
            }
        }
    }
}