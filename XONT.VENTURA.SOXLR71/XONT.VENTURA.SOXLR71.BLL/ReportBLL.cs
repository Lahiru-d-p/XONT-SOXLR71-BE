using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System;
using System.Data;
using System.IO;
using XONT.Common.Message;
using XONT.Ventura.AppConsole;
using XONT.Ventura.Common.ConvertDateTime;
using XONT.VENTURA.SOXLR71.DAL;
using XONT.VENTURA.SOXLR71.DOMAIN;

namespace XONT.VENTURA.SOXLR71.BLL
{
    public class ReportBLL
    {
        private readonly ReportDAL _dal;

        public ReportBLL(ReportDAL dal)
        {
            _dal = dal;
        }

        public DataTable GetTerritoryPrompt(string businessUnit, ref MessageSet msg)
        {
            return _dal.GetTerritoryPrompt(businessUnit, ref msg);
        }

        public DataTable GetDistributorPrompt(string businessUnit, ref MessageSet msg)
        {
            return _dal.GetDistributorPrompt(businessUnit, ref msg);
        }

        public DataTable GetReportData(Selection selection, out MessageSet msg)
        {
            return _dal.GetReportData(selection, out msg);
        }

        public byte[] GenerateExcelByDetail(Selection selection, DataTable dataTable, DataTable dtLogo, String reportName, ControlData controlData, BusinessUnit businessUnit, ref MessageSet msg)
        {
            byte[] byteArray = null;

            dataTable.DefaultView.Sort = "RetailerCode ASC, RetailerName ASC, InvoiceDate ASC, InvoiceNo ASC";

            DataTable data = dataTable.DefaultView.ToTable();
            GetDistributorVATRegNo(selection, businessUnit.BusinessUnitCode, ref msg);

            #region Excel Report
            ExcelPackage.License.SetNonCommercialOrganization("Xont");
            using (ExcelPackage xp = new ExcelPackage())
            {
                ExcelWorksheet worksheet = xp.Workbook.Worksheets.Add(reportName);
                worksheet.Cells.Style.Font.Size = 9;
                worksheet.Cells.Style.Font.Name = "Arial";
                worksheet.View.ShowGridLines = false;

                #region Define report header
                DefineLogo(worksheet, dtLogo, selection);
                DefineBusinessUnitDetail(worksheet, businessUnit, selection);
                DefinePrintDetailBox(worksheet, selection);

                int rowIndex = 8;

                rowIndex = DefineSelectionCriteria(worksheet, selection, rowIndex);
                #endregion

                #region Report Body
                int tableRow;
                tableRow = rowIndex + 1;

                tableRow = DefineInvoiceNumberSection(worksheet, data, selection, rowIndex, tableRow);
                tableRow += 2;

                string[] headers = { "Retailer Code", "Retailer Name", "VAT Registration No.", "Invoice No.", "Invoice Date", "VAT Sales", "Non-VAT Sales", "VAT Value"};
                for (int i = 1; i <= headers.Length; i++)
                {
                    worksheet.Cells[tableRow, i].Value = headers[i - 1];
                }

                worksheet.Cells[$"A{tableRow}:H{tableRow}"].Style.Font.Bold = true;
                worksheet.Cells[$"A{tableRow}:H{tableRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ++tableRow;
                // Setting data
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    worksheet.Cells[i + tableRow, 1].Value = data.Rows[i]["RetailerCode"].ToString();
                    worksheet.Cells[i + tableRow, 2].Value = data.Rows[i]["RetailerName"].ToString();
                    worksheet.Cells[i + tableRow, 3].Value = data.Rows[i]["RetailerVatRegistrationNumber"].ToString();
                    worksheet.Cells[i + tableRow, 4].Value = data.Rows[i]["InvoiceNo"].ToString();
                    worksheet.Cells[i + tableRow, 5].Value = ConvertDateTime.DisplayDateTime((DateTime)data.Rows[i]["InvoiceDate"]);
                    worksheet.Cells[i + tableRow, 6].Value = data.Rows[i]["VATSales"];
                    worksheet.Cells[i + tableRow, 6].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[i + tableRow, 7].Value = data.Rows[i]["NonVATSales"];
                    worksheet.Cells[i + tableRow, 7].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[i + tableRow, 8].Value = data.Rows[i]["VATValue"];
                    worksheet.Cells[i + tableRow, 8].Style.Numberformat.Format = "#,##0.00";
                }

                // Add "Total" row
                int totalRow = data.Rows.Count + tableRow;
                worksheet.Cells[totalRow, 1, totalRow, 5].Merge = true;
                worksheet.Cells[totalRow, 1].Value = "Total";

                worksheet.Cells[totalRow, 6].Formula = $"SUM(F{tableRow}:F{totalRow - 1})";
                worksheet.Cells[totalRow, 6].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[totalRow, 7].Formula = $"SUM(G{tableRow}:G{totalRow - 1})";
                worksheet.Cells[totalRow, 7].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[totalRow, 8].Formula = $"SUM(H{tableRow}:H{totalRow - 1})";
                worksheet.Cells[totalRow, 8].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells[totalRow, 1, totalRow, 8].Style.Font.Bold = true;
                worksheet.Cells[totalRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Set individual column widths
                worksheet.Column(2).Width = 30; // Retailer Name
                worksheet.Column(3).Width = 20; // Retailer VAT Registration Number
                worksheet.Column(4).Width = 10; // Invoice Number
                worksheet.Column(5).Width = 12; // Invoice Date
                worksheet.Column(6).Width = 15; // VAT Sales
                worksheet.Column(7).Width = 15; // Non-VAT Sales
                worksheet.Column(8).Width = 15; // VAT Value

                // Add borders to the cells
                using (var range = worksheet.Cells[$"A{tableRow - 1}:H" + totalRow])
                {
                    range.Style.Border.Top.Style    = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style   = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style  = ExcelBorderStyle.Thin;
                }

                if (!selection.VATFlag && selection.NonVATFlag)
                {
                    worksheet.Column(6).Hidden = true; // Hide VAT Sales column
                    worksheet.Column(8).Hidden = true; // Hide VAT Value column
                    worksheet.Column(9).Hidden = true; // Hide Total Sales Column column
                }
                if (!selection.NonVATFlag && selection.VATFlag)
                {
                    worksheet.Column(7).Hidden = true; // Hide Non-VAT Sales column
                    worksheet.Column(9).Hidden = true; // Hide Total Sales Column column
                }

                // Calculate the formulas
                worksheet.Calculate();

                byteArray = xp.GetAsByteArray();
                #endregion
            }
            #endregion

            return byteArray;
        }

        public byte[] GenerateExcelBySummary(Selection selection, DataTable dataTable, DataTable dtLogo, String reportName, ControlData controlData, BusinessUnit businessUnit, ref MessageSet msg)
        {
            byte[] byteArray = null;

            dataTable.DefaultView.Sort = "RetailerCode ASC, RetailerName ASC";

            DataTable data = dataTable.DefaultView.ToTable();
            GetDistributorVATRegNo(selection, businessUnit.BusinessUnitCode, ref msg);

            #region Excel Report
            ExcelPackage.License.SetNonCommercialOrganization("Xont");
            using (ExcelPackage xp = new ExcelPackage())
            {
                ExcelWorksheet worksheet = xp.Workbook.Worksheets.Add(reportName);
                worksheet.Cells.Style.Font.Size = 9;
                worksheet.Cells.Style.Font.Name = "Arial";
                worksheet.View.ShowGridLines = false;

                #region Define report header

                DefineLogo(worksheet, dtLogo, selection);
                DefineBusinessUnitDetail(worksheet, businessUnit, selection);
                DefinePrintDetailBox(worksheet, selection);

                int rowIndex = 8;
                rowIndex = DefineSelectionCriteria(worksheet, selection, rowIndex);

                #endregion

                #region Report Body

                int tableRow;
                tableRow = rowIndex + 1;

                tableRow = DefineInvoiceNumberSection(worksheet, data, selection, rowIndex, tableRow);

                tableRow += 2;

                string[] headers = { "Retailer Code", "Retailer Name", "VAT Registration No.", "VAT Sales", "Non-VAT Sales", "VAT Value", "Total Sales" };
                for (int i = 1; i <= headers.Length; i++)
                {
                    worksheet.Cells[tableRow, i].Value = headers[i - 1];
                }

                worksheet.Cells[$"A{tableRow}:H{tableRow}"].Style.Font.Bold = true;
                worksheet.Cells[$"A{tableRow}:H{tableRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ++tableRow;

                // Setting data
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    worksheet.Cells[i + tableRow, 1].Value = data.Rows[i]["RetailerCode"].ToString();
                    worksheet.Cells[i + tableRow, 2].Value = data.Rows[i]["RetailerName"].ToString();
                    worksheet.Cells[i + tableRow, 3].Value = data.Rows[i]["RetailerVatRegistrationNumber"].ToString();
                    worksheet.Cells[i + tableRow, 4].Value = data.Rows[i]["VATSales"];
                    worksheet.Cells[i + tableRow, 4].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[i + tableRow, 5].Value = data.Rows[i]["NonVATSales"];
                    worksheet.Cells[i + tableRow, 5].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[i + tableRow, 6].Value = data.Rows[i]["VATValue"];
                    worksheet.Cells[i + tableRow, 6].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[i + tableRow, 7].Value = data.Rows[i]["TotalSales"];
                    worksheet.Cells[i + tableRow, 7].Style.Numberformat.Format = "#,##0.00";
                }

                // Add "Total" row
                int totalRow = data.Rows.Count + tableRow;
                worksheet.Cells[totalRow, 1, totalRow, 3].Merge = true;
                worksheet.Cells[totalRow, 1].Value = "Total";

                worksheet.Cells[totalRow, 4].Formula = $"SUM(D{tableRow}:D{totalRow - 1})";
                worksheet.Cells[totalRow, 4].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[totalRow, 5].Formula = $"SUM(E{tableRow}:E{totalRow - 1})";
                worksheet.Cells[totalRow, 5].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[totalRow, 6].Formula = $"SUM(F{tableRow}:F{totalRow - 1})";
                worksheet.Cells[totalRow, 6].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[totalRow, 7].Formula = $"SUM(G{tableRow}:G{totalRow - 1})";
                worksheet.Cells[totalRow, 7].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells[totalRow, 1, totalRow, 7].Style.Font.Bold = true;
                worksheet.Cells[totalRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Set individual column widths
                worksheet.Column(2).Width = 30; // Retailer Name
                worksheet.Column(3).Width = 20; // Retailer VAT Registration Number
                worksheet.Column(4).Width = 15; // VAT Sales
                worksheet.Column(5).Width = 15; // Non-VAT Sales
                worksheet.Column(6).Width = 15; // VAT Value
                worksheet.Column(7).Width = 15; // Total Sales

                // Add borders to the cells
                using (var range = worksheet.Cells[$"A{tableRow - 1}:G" + totalRow])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Set individual column widths based on selection flags
                if (!selection.VATFlag && selection.NonVATFlag)
                {
                    worksheet.Column(4).Hidden = true; // Hide VAT Sales column
                    worksheet.Column(6).Hidden = true; // Hide VAT Value column
                    worksheet.Column(7).Hidden = true; // Hide Total Sales Column column
                }
                if (!selection.NonVATFlag && selection.VATFlag)
                {
                    worksheet.Column(5).Hidden = true; // Hide Non-VAT Sales column
                    worksheet.Column(7).Hidden = true; // Hide Total Sales Column column
                }

                // Calculate the formulas
                worksheet.Calculate();

                byteArray = xp.GetAsByteArray();
                #endregion
            }
            #endregion
            return byteArray;
        }

        private void DefineLogo(ExcelWorksheet worksheet, DataTable dtLogo, Selection selection)
        {
            if (dtLogo != null && dtLogo.Rows.Count > 0)
            {
                if (Convert.ToInt32(dtLogo.Rows[0]["ShowLogo"]) != 0)
                {
                    if (!File.Exists(selection.ImagePath))
                    {
                        File.WriteAllBytes(selection.ImagePath, (byte[])dtLogo.Rows[0]["Logo"]);
                    }

                    AddImage(worksheet, 1, 0, selection.ImagePath);
                    worksheet.Cells[1, 1, 6, 1].Merge = true;
                }
                else
                {
                    if (File.Exists(selection.ImagePath))
                    {
                        File.Delete(selection.ImagePath);
                    }
                    worksheet.Cells[1, 1, 6, 1].Merge = true;
                }
            }
            else
            {
                worksheet.Cells[1, 1, 6, 1].Merge = true;
            }
        }

        private void DefineBusinessUnitDetail(ExcelWorksheet worksheet, BusinessUnit businessUnit, Selection selection)
        {

            worksheet.Cells[2, 2].Value = businessUnit.BusinessUnitName.Trim();
            worksheet.Cells[2, 2, 3, 9].Merge = true;
            worksheet.Cells[2, 2, 3, 9].Style.Font.Size = 16;

            var address = string.Format("{0}{1}{2}{3}{4}", string.IsNullOrEmpty(businessUnit.AddressLine1) ? "" : businessUnit.AddressLine1.Trim(),
                string.IsNullOrEmpty(businessUnit.AddressLine2) ? "" : ", " + businessUnit.AddressLine2.Trim(),
                string.IsNullOrEmpty(businessUnit.AddressLine3) ? "" : ", " + businessUnit.AddressLine3.Trim(),
                string.IsNullOrEmpty(businessUnit.AddressLine4) ? "" : ", " + businessUnit.AddressLine4.Trim(),
                string.IsNullOrEmpty(businessUnit.AddressLine5) ? "" : ", " + businessUnit.AddressLine5.Trim());

            worksheet.Cells[4, 2].Value = address;
            worksheet.Cells[4, 2, 4, 9].Merge = true;
            worksheet.Cells[4, 2, 4, 9].Style.Font.Size = 10;

            var contactDetails = string.Format("{0}{1}{2}", string.IsNullOrEmpty(businessUnit.TelephoneNumber) ? "" : "Tel : " + businessUnit.TelephoneNumber.Trim(),
                string.IsNullOrEmpty(businessUnit.FaxNumber) ? "" : "   Fax : " + businessUnit.FaxNumber.Trim(),
                string.IsNullOrEmpty(businessUnit.EmailAddress) ? "" : "    Email : " + businessUnit.EmailAddress.Trim());

            worksheet.Cells[5, 2].Value = contactDetails;
            worksheet.Cells[5, 2, 5, 9].Merge = true;
            worksheet.Cells[5, 2, 5, 9].Style.Font.Size = 10;

            if (selection.ReportDetailFlag)
                worksheet.Cells[6, 2].Value = "VAT Sales Report (Detailed)";
            else
                worksheet.Cells[6, 2].Value = "VAT Sales Report (Summary)";

            worksheet.Cells[6, 2, 6, 9].Merge = true;
            worksheet.Cells[6, 2, 6, 9].Style.Font.Size = 12;

            var businessUnitDetail = worksheet.Cells[1, 2, 6, 9];
            businessUnitDetail.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            businessUnitDetail.Style.Font.Bold = true;

        }

        private void DefinePrintDetailBox(ExcelWorksheet worksheet, Selection selection)
        {
            var printDetailBox = worksheet.Cells[2, 10, 5, 11];
            printDetailBox.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            printDetailBox.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            printDetailBox.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            printDetailBox.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            printDetailBox.Style.Font.Size = 8;
            worksheet.Cells[2, 10, 5, 10].Style.Font.Bold = true;

            worksheet.Cells[2, 10].Value = "Date";
            worksheet.Cells[3, 10].Value = "Time";
            worksheet.Cells[4, 10].Value = "Req. By";
            worksheet.Cells[5, 10].Value = "Task";
            worksheet.Cells[2, 11].Value = ConvertDateTime.DisplayDateTime(DateTime.Today);
            worksheet.Cells[3, 11].Value = DateTime.Now.ToString("h:mm tt");
            worksheet.Cells[4, 11].Value = selection.UserName;
            worksheet.Cells[5, 11].Value = "SOXLR71";
        }

        private int DefineSelectionCriteria(ExcelWorksheet worksheet, Selection selection, int rowIndex)
        {
            worksheet.Cells[++rowIndex, 1].Value = "Territory Code";
            worksheet.Cells[rowIndex, 2, rowIndex, 9].Value = ": " + (string.IsNullOrEmpty(selection.TerritoryCode) ? "***All***" : selection.TerritoryCode);
            worksheet.Cells[rowIndex, 2, rowIndex, 9].Merge = true;

            worksheet.Cells[++rowIndex, 1].Value = "Distributor Code";
            worksheet.Cells[rowIndex, 2, rowIndex, 9].Value = ": " + (string.IsNullOrEmpty(selection.DistributorCode) ? "***All***" : selection.DistributorCode);
            worksheet.Cells[rowIndex, 2, rowIndex, 9].Merge = true;

            worksheet.Cells[++rowIndex, 1].Value = "VAT Status";
            worksheet.Cells[rowIndex, 2, rowIndex, 9].Value = ": " + ((selection.VATFlag) ? "VAT" : "")+ ((selection.VATFlag && selection.NonVATFlag) ? " / " : "") + ((selection.NonVATFlag) ? "Non-VAT" : "") + ((!selection.NonVATFlag && !selection.VATFlag) ? "VAT | Non-VAT" : "");
            worksheet.Cells[rowIndex, 2, rowIndex, 9].Merge = true;

            worksheet.Cells[++rowIndex, 1].Value = "Date";
            worksheet.Cells[rowIndex, 2, rowIndex, 9].Value = ": " + string.Format("From : {0}    To : {1}", ConvertDateTime.DisplayDateTime(selection.FromDate), ConvertDateTime.DisplayDateTime(selection.ToDate));
            worksheet.Cells[rowIndex, 2, rowIndex, 9].Merge = true;

            worksheet.Cells[++rowIndex, 1].Value = "Distributor VAT Registration No.";
            worksheet.Cells[rowIndex, 2, rowIndex, 9].Value = ": " + (string.IsNullOrEmpty(selection.DistributorVATRegistrationNo) ? "N/A" : selection.DistributorVATRegistrationNo);
            worksheet.Cells[rowIndex, 2, rowIndex, 9].Merge = true;

            worksheet.Cells[8, 1, rowIndex, 1].Style.Font.Bold = true;
            worksheet.Cells[8, 1, rowIndex, 1].AutoFitColumns();

            return rowIndex;
        }

        private int DefineInvoiceNumberSection(ExcelWorksheet worksheet, DataTable data, Selection selection, int rowIndex ,int tableRow)
        {
            worksheet.Cells[++tableRow, 1].Value = "Number of VAT Invoices:";
            worksheet.Cells[tableRow, 2].Value = data.Rows[0]["NonZeroVATCount"];
            if (!selection.VATFlag && selection.NonVATFlag)
            {
                worksheet.Row(tableRow).Hidden = true; // Hide the row for "Number of VAT Invoices"
            }
            worksheet.Cells[++tableRow, 1].Value = "Number of Non-VAT Invoices:";
            worksheet.Cells[tableRow, 2].Value = data.Rows[0]["ZeroVATCount"];
            if (selection.VATFlag && !selection.NonVATFlag)
            {
                worksheet.Row(tableRow).Hidden = true; // Hide the row for "Number of Non-VAT Invoices"
            }
            worksheet.Cells[++tableRow, 1].Value = "Total Number of Invoices:";
            worksheet.Cells[tableRow, 2].Formula = $"(B{tableRow - 1} + B{tableRow - 2})";
            if (!selection.VATFlag || !selection.NonVATFlag)
            {
                worksheet.Row(tableRow).Hidden = true; // Hide the row for "Total Number of Invoices:"
            }
            worksheet.Cells[rowIndex + 1, 1, tableRow, 2].Style.Font.Bold = true;

            // Add borders to the cells
            using (var range = worksheet.Cells[rowIndex + 2, 1, tableRow, 2])
            {
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }
            return tableRow;
        }

        public void GetDistributorVATRegNo(Selection selection, string businessUnit, ref MessageSet msg)
        {
            DataTable dt = _dal.GetDistributorVATRegNo(selection, businessUnit, ref msg);
            try
            {
                if (selection.DistributorFlag && !selection.TerritoryFlag)
                    selection.DistributorVATRegistrationNo = dt.Rows[0][0].ToString().Trim();
                else
                {
                    selection.DistributorVATRegistrationNo = dt.Rows[0][0].ToString().Trim();
                    selection.DistributorCode = dt.Rows[0][1].ToString().Trim();
                }
            }
            catch
            {
                selection.DistributorVATRegistrationNo = "N/A";
                selection.DistributorCode = "***All***";
            }
        }

        public ControlData GetControlData(string businessUnit, out MessageSet msg)
        {
            return _dal.GetControlData(businessUnit, out msg);
        }

        public BusinessUnit GetBusinessUnit(string businessUnit, ref MessageSet msg)
        {
            return _dal.GetBusinessUnit(businessUnit, ref msg);
        }

        public DataTable GetBusinessUnitLogo(string businessUnit, out MessageSet msg)
        {
            return _dal.GetBusinessUnitLogo(businessUnit, out msg);
        }

        private static void AddImage(ExcelWorksheet worksheet, int rowIndex, int columnIndex, string filePath)
        {
            ExcelPicture picture = null;
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                picture = worksheet.Drawings.AddPicture("Logo", filePath);
                picture.From.Column = columnIndex;
                picture.From.Row = rowIndex;
                picture.From.ColumnOff = 2 * 9525;      //Two pixel space for better alignment
                picture.From.RowOff = 2 * 9525;         //Two pixel space for better alignment
                picture.SetSize(120, 100);
            }
        }

    }
}
