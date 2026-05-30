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
        public UploadResultModel UploadItemMaster(
            HttpPostedFileBase file,
            string uploadedBy,
            string allowedCompany)
        {
            var result =
                new UploadResultModel
                {
                    FileName = file.FileName
                };

            if (string.IsNullOrWhiteSpace(allowedCompany))
            {
                result.ErrorMessage =
                    "Please select company before upload";

                return result;
            }

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
                            result,
                            allowedCompany);
                    }
                    else
                    {
                        ProcessExcelFile(
                            file,
                            connection,
                            result,
                            allowedCompany);
                    }

                    SaveImportLog(
                        connection,
                        result,
                        string.IsNullOrWhiteSpace(uploadedBy)
                            ? "SYSTEM"
                            : uploadedBy,
                        allowedCompany);
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
            UploadResultModel result,
            string allowedCompany)
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
                        },
                        allowedCompany);
                }
            }
        }

        private void ProcessCsvFile(
            HttpPostedFileBase file,
            IDbConnection connection,
            UploadResultModel result,
            string allowedCompany)
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
                        },
                        allowedCompany);
                }
            }
        }

        private void ProcessItemMasterRow(
            IDbConnection connection,
            UploadResultModel result,
            ItemMasterRow row,
            string allowedCompany)
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

            if (!string.IsNullOrWhiteSpace(allowedCompany) &&
                !string.Equals(
                    row.Company,
                    allowedCompany,
                    StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add(
                    $"Row {result.TotalRows}: Company [{row.Company}] is not allowed for this session");

                result.FailedRows++;

                return;
            }

            UpsertItemMaster(connection, row);

            result.SuccessRows++;
        }

        private void UpsertItemMaster(
            IDbConnection connection,
            ItemMasterRow row)
        {
            const string sql = @"
                MERGE Item_master AS target
                USING
                (
                    SELECT
                        @Company AS company,
                        @Part AS Part,
                        @Description AS Description
                ) AS source
                ON target.company = source.company
                AND target.Part = source.Part
                WHEN MATCHED THEN
                    UPDATE SET
                        Description = source.Description
                WHEN NOT MATCHED THEN
                    INSERT
                    (
                        company,
                        Part,
                        Description
                    )
                    VALUES
                    (
                        source.company,
                        source.Part,
                        source.Description
                    );";

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
            string uploadedBy,
            string company)
        {
            const string sql = @"
                INSERT INTO Item_Master_Import_Log
                (
                    file_name,
                    company,
                    total_rows,
                    success_rows,
                    failed_rows,
                    uploaded_by
                )
                VALUES
                (
                    @FileName,
                    @Company,
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
                    Company = company,
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
