using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using POATax.DataStructures;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Globalization;
using OfficeOpenXml;

namespace POATax
{
    public class WorkSheetClass
    {
        CultureInfo provider = CultureInfo.InvariantCulture;
        public void GenerateExcelWorkSheet(string address)
        {
            //Initialize ExcelEngine.
            using (ExcelPackage excel = new ExcelPackage())
            {
                var format = new OfficeOpenXml.ExcelTextFormat();
                format.Delimiter = ',';
                format.TextQualifier = '"';
                excel.Workbook.Worksheets.Add("Worksheet1");
                excel.Workbook.Worksheets.Add("Worksheet2");
                excel.Workbook.Worksheets.Add("Worksheet3");

                var headerRow = new List<string[]>
                                          {
                                            new string[] { "address", "Amount of POA", "Date of Transaction", "Symbol", "POA USD Daily Price","Total Dollar Amount" }
                                          };

                // Determine the header range (e.g. A1:D1)
                string headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                // Target a worksheet
                var worksheet = excel.Workbook.Worksheets["Worksheet1"];

                // Popular header row data
                worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                //Format Columns
                worksheet.Column(3).Style.Numberformat.Format = "yyyy/mm/dd";



                worksheet.Cells[headerRange].Style.Font.Bold = true;
                worksheet.Cells[headerRange].Style.Font.Size = 14;
                worksheet.Cells[headerRange].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                using (var db = new LiteDatabase(@"POATax.db"))
                {
                    LiteCollection<POATax.DataStructures.Transactions> transactions = db.GetCollection<POATax.DataStructures.Transactions>("Transactions");
                    LiteCollection<POATax.DataStructures.DatePrices> datePrices = db.GetCollection<POATax.DataStructures.DatePrices>("DatePrices");
                    var results = transactions.Find(Query.All(Query.Ascending));
                    var results2 = datePrices.Find(Query.All(Query.Ascending));
                    int c = results2.Count();
                    int worksheetindex = 2;
                    foreach (Transactions t in results)
                    {
                        DatePrices dp = (from x in results2 where x.date.ToString("MM/dd/yyyy", provider) == t.date.ToString("MM/dd/yyyy", provider) select x).Single();
                        worksheet.Cells[worksheetindex, 1].LoadFromText(t.address + "," + t.transactionAmtPOA + "," + t.date.ToString("MM/dd/yyyy", provider) + "," + "POA" + "," + dp.price + "," + (dp.price * t.transactionAmtPOA));
                        worksheetindex = worksheetindex + 1;
                    }
                    worksheet.Cells["A" + worksheetindex.ToString()].Value = "Totals";
                    worksheet.Cells["B" + worksheetindex.ToString()].Formula = "=SUM(B2:B" + (worksheetindex - 1).ToString() + ")";
                    worksheet.Cells["F" + worksheetindex.ToString()].Formula = "=SUM(F2:F" + (worksheetindex - 1).ToString() + ")";
                }
                if(!OperatingSystem.IsLinux())
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }
                string s = System.AppContext.BaseDirectory;
                FileInfo excelFile = new FileInfo(@"" + address + "_POATax.xlsx");

                excel.SaveAs(excelFile);
                Console.WriteLine("Excel Document has been generated. Press ENTER to continue.");
            }
        }
    }
}
