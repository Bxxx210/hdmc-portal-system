using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
                var rows =
                    new List<ItemMasterRow>();
                var keys =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase);

                if (IsCsvFile(file.FileName))
                {
                    ProcessCsvFile(
                        file,
                        result,
                        allowedCompany,
                        rows,
                        keys);
                }
                else
                {
                    ProcessExcelFile(
                        file,
                        result,
                        allowedCompany,
                        rows,
                        keys);
                }

                using (var connection =
                       DbConnectionFactory.CreateHardwareConnection())
                {
                    connection.Open();

                    using (var transaction =
                           connection.BeginTransaction())
                    {
                        BulkUpsertItemMaster(
                            connection,
                            transaction,
                            rows);

                        result.SuccessRows =
                            rows.Count;

                        SaveImportLog(
                            connection,
                            transaction,
                            result,
                            string.IsNullOrWhiteSpace(uploadedBy)
                                ? "SYSTEM"
                                : uploadedBy,
                            allowedCompany);

                        transaction.Commit();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.SuccessRows = 0;
                result.ErrorMessage =
                    ex.Message;

                return result;
            }
        }

        private void ProcessExcelFile(
            HttpPostedFileBase file,
            UploadResultModel result,
            string allowedCompany,
            List<ItemMasterRow> items,
            HashSet<string> keys)
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
                        result,
                        new ItemMasterRow
                        {
                            Company = row.Cell(1).GetString().Trim(),
                            Part = row.Cell(2).GetString().Trim(),
                            Description = row.Cell(3).GetString().Trim()
                        },
                        allowedCompany,
                        items,
                        keys);
                }
            }
        }

        private void ProcessCsvFile(
            HttpPostedFileBase file,
            UploadResultModel result,
            string allowedCompany,
            List<ItemMasterRow> items,
            HashSet<string> keys)
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
                        result,
                        new ItemMasterRow
                        {
                            Company = GetCsvColumn(columns, 0),
                            Part = GetCsvColumn(columns, 1),
                            Description = GetCsvColumn(columns, 2)
                        },
                        allowedCompany,
                        items,
                        keys);
                }
            }
        }

        private void ProcessItemMasterRow(
            UploadResultModel result,
            ItemMasterRow row,
            string allowedCompany,
            List<ItemMasterRow> items,
            HashSet<string> keys)
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

            var key =
                row.Company + "\0" + row.Part;

            if (!keys.Add(key))
            {
                result.Errors.Add(
                    $"Row {result.TotalRows}: Duplicate Part [{row.Part}] in upload file");

                result.FailedRows++;

                return;
            }

            items.Add(row);
        }

        private void BulkUpsertItemMaster(
            SqlConnection connection,
            SqlTransaction transaction,
            List<ItemMasterRow> items)
        {
            if (!items.Any())
            {
                return;
            }

            const string createStagingSql = @"
                SELECT TOP 0
                    company,
                    Part,
                    Description
                INTO #ItemMasterUpload
                FROM Item_master";

            connection.Execute(
                createStagingSql,
                transaction: transaction);

            var table =
                CreateItemMasterTable(items);

            using (var bulkCopy =
                   new SqlBulkCopy(
                       connection,
                       SqlBulkCopyOptions.Default,
                       transaction))
            {
                bulkCopy.DestinationTableName =
                    "#ItemMasterUpload";
                bulkCopy.ColumnMappings.Add("company", "company");
                bulkCopy.ColumnMappings.Add("Part", "Part");
                bulkCopy.ColumnMappings.Add(
                    "Description",
                    "Description");
                bulkCopy.WriteToServer(table);
            }

            const string mergeSql = @"
                MERGE Item_master WITH (HOLDLOCK) AS target
                USING #ItemMasterUpload AS source
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
                mergeSql,
                transaction: transaction);
        }

        private DataTable CreateItemMasterTable(
            IEnumerable<ItemMasterRow> items)
        {
            var table =
                new DataTable();

            table.Columns.Add("company", typeof(string));
            table.Columns.Add("Part", typeof(string));
            table.Columns.Add("Description", typeof(string));

            foreach (var item in items)
            {
                table.Rows.Add(
                    item.Company,
                    item.Part,
                    item.Description);
            }

            return table;
        }

        private void SaveImportLog(
            IDbConnection connection,
            IDbTransaction transaction,
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
                },
                transaction);
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
