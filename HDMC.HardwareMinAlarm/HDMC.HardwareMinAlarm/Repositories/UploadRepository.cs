using System.Collections.Generic;
using System.Linq;
using Dapper;
using HDMC.HardwareMinAlarm.Helpers;
using HDMC.HardwareMinAlarm.Models;

namespace HDMC.HardwareMinAlarm.Repositories
{
    public class UploadRepository
    {
        // Get latest upload history
        public List<UploadHistoryModel> GetUploadHistory(
            List<CompanyAccessModel> allowedCompanies,
            string selectedCompany)
        {
            using (var connection =
                   DbConnectionFactory
                       .CreateHardwareConnection())
            {
                const string sql = @"
                    SELECT TOP 100
                        id              AS Id,
                        file_name       AS FileName,
                        company         AS Company,
                        total_rows      AS TotalRows,
                        success_rows    AS SuccessRows,
                        failed_rows     AS FailedRows,
                        uploaded_by     AS UploadedBy,
                        uploaded_date   AS UploadedDate
                    FROM Item_Master_Import_Log
                    WHERE company IN @AllowedCompanies
                    AND (
                        @SelectedCompany IS NULL
                        OR company = @SelectedCompany
                    )
                    ORDER BY uploaded_date DESC";

                return connection
                    .Query<UploadHistoryModel>(
                        sql,
                        new
                        {
                            AllowedCompanies =
                                allowedCompanies.Select(item => item.Company),
                            SelectedCompany =
                                string.IsNullOrWhiteSpace(selectedCompany)
                                    ? null
                                    : selectedCompany
                        })
                    .ToList();
            }
        }
    }
}
