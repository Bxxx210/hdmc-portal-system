using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using HDMC.HardwareMinAlarm.Models;

namespace HDMC.HardwareMinAlarm.Services
{
    public class ItemMasterWorkbookService
    {
        public const string ExcelContentType =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public byte[] CreateTemplate(string company)
        {
            return CreateWorkbook(
                new[]
                {
                    new ItemMasterModel
                    {
                        Company = company,
                        Part = "PART001",
                        Description = "Sample description"
                    }
                });
        }

        public byte[] CreateExport(IEnumerable<ItemMasterModel> items)
        {
            return CreateWorkbook(items);
        }

        private byte[] CreateWorkbook(IEnumerable<ItemMasterModel> items)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet =
                    workbook.Worksheets.Add("Item Master");

                worksheet.Cell(1, 1).Value = "Company";
                worksheet.Cell(1, 2).Value = "Part";
                worksheet.Cell(1, 3).Value = "Description";

                var rowNumber = 2;

                foreach (var item in items)
                {
                    worksheet.Cell(rowNumber, 1).Value = item.Company;
                    worksheet.Cell(rowNumber, 2).Value = item.Part;
                    worksheet.Cell(rowNumber, 3).Value = item.Description;

                    rowNumber++;
                }

                var header =
                    worksheet.Range(1, 1, 1, 3);

                header.Style.Font.Bold = true;
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#1A56DB");

                worksheet.SheetView.FreezeRows(1);
                worksheet.Range(1, 1, rowNumber - 1, 3)
                    .SetAutoFilter();
                worksheet.Column(1).Width = 16;
                worksheet.Column(2).Width = 24;
                worksheet.Column(3).Width = 54;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return stream.ToArray();
                }
            }
        }
    }
}
