using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using Dapper;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Models;

namespace HDMC.HardwareMinAlarm.Services
{
    public class UploadService
    {
        public UploadResultModel UploadItemMaster(HttpPostedFileBase file)
        {
            return UploadItemMaster(file, "SYSTEM");
        }

        public UploadResultModel UploadItemMaster(
            HttpPostedFileBase file,
            string uploadedBy)
        {
            var result =
                new UploadResultModel
                {
                    FileName = file.FileName
                };

            try
            {
                using (var connection =
                       DbConnectionFactory.CreateHardwareConnection())
                {
                    connection.Open();

                    if (IsCsvFile(file.FileName))
                    {
                        ProcessCsvFile(
                            file,
                            connection,
                            result);
                    }
                    else
                    {
                        ProcessExcelFile(
                            file,
                            connection,
                            result);
                    }

                    SaveImportLog(
                        connection,
                        result,
                        string.IsNullOrWhiteSpace(uploadedBy)
                            ? "SYSTEM"
                            : uploadedBy);
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

        private void ProcessExcelFile(
            HttpPostedFileBase file,
            IDbConnection connection,
            UploadResultModel result)
        {
            using (var workbook = new XLWorkbook(file.InputStream))
            {
                var rows =
                    workbook.Worksheet(1)
                        .RowsUsed()
                        .Skip(1);

                foreach (var row in rows)
                {
                    ProcessItemMasterRow(
                        connection,
                        result,
                        new ItemMasterRow
                        {
                            Company = row.Cell(1).GetString().Trim(),
                            Part = row.Cell(2).GetString().Trim(),
                            Description = row.Cell(3).GetString().Trim()
                        });
                }
            }
        }

        private void ProcessCsvFile(
            HttpPostedFileBase file,
            IDbConnection connection,
            UploadResultModel result)
        {
            using (var reader =
                   new StreamReader(file.InputStream))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line =
                        reader.ReadLine();

                    var columns =
                        (line ?? string.Empty).Split(',');

                    ProcessItemMasterRow(
                        connection,
                        result,
                        new ItemMasterRow
                        {
                            Company = GetCsvColumn(columns, 0),
                            Part = GetCsvColumn(columns, 1),
                            Description = GetCsvColumn(columns, 2)
                        });
                }
            }
        }

        private void ProcessItemMasterRow(
            IDbConnection connection,
            UploadResultModel result,
            ItemMasterRow row)
        {
            result.TotalRows++;

            if (string.IsNullOrWhiteSpace(row.Company) ||
                string.IsNullOrWhiteSpace(row.Part))
            {
                result.Errors.Add(
                    $"Row {result.TotalRows}: Company or Part is empty");

                result.FailedRows++;

                return;
            }

            if (!IsValidCompany(connection, row.Company))
            {
                result.Errors.Add(
                    $"Row {result.TotalRows}: Invalid company [{row.Company}]");

                result.FailedRows++;

                return;
            }

            UpsertItemMaster(connection, row);

            result.SuccessRows++;
        }

        private bool IsValidCompany(
            IDbConnection connection,
            string company)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Companies
                WHERE company_code = @Company
                AND is_active = 1";

            var validCompany =
                connection.ExecuteScalar<int>(
                    sql,
                    new
                    {
                        Company = company
                    });

            return validCompany > 0;
        }

        private void UpsertItemMaster(
            IDbConnection connection,
            ItemMasterRow row)
        {
            if (ItemExists(connection, row))
            {
                UpdateItemMaster(connection, row);

                return;
            }

            InsertItemMaster(connection, row);
        }

        private bool ItemExists(
            IDbConnection connection,
            ItemMasterRow row)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Item_master
                WHERE company = @Company
                AND Part = @Part";

            var exists =
                connection.ExecuteScalar<int>(
                    sql,
                    new
                    {
                        Company = row.Company,
                        Part = row.Part
                    });

            return exists > 0;
        }

        private void UpdateItemMaster(
            IDbConnection connection,
            ItemMasterRow row)
        {
            const string sql = @"
                UPDATE Item_master
                SET Description = @Description
                WHERE company = @Company
                AND Part = @Part";

            connection.Execute(
                sql,
                new
                {
                    Company = row.Company,
                    Part = row.Part,
                    Description = row.Description
                });
        }

        private void InsertItemMaster(
            IDbConnection connection,
            ItemMasterRow row)
        {
            const string sql = @"
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
                sql,
                new
                {
                    Company = row.Company,
                    Part = row.Part,
                    Description = row.Description
                });
        }

        private void SaveImportLog(
            IDbConnection connection,
            UploadResultModel result,
            string uploadedBy)
        {
            const string sql = @"
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
                sql,
                new
                {
                    FileName = result.FileName,
                    TotalRows = result.TotalRows,
                    SuccessRows = result.SuccessRows,
                    FailedRows = result.FailedRows,
                    UploadedBy = uploadedBy
                });
        }

        private bool IsCsvFile(string fileName)
        {
            return string.Equals(
                Path.GetExtension(fileName),
                ".csv",
                StringComparison.OrdinalIgnoreCase);
        }

        private string GetCsvColumn(string[] columns, int index)
        {
            return columns.Length > index
                ? columns[index].Trim()
                : string.Empty;
        }

        private class ItemMasterRow
        {
            public string Company { get; set; }

            public string Part { get; set; }

            public string Description { get; set; }
        }
    }
}
