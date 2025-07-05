using ClosedXML.Excel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using HotelManager.ViewModels.ManagerViewModels.UIModel;

namespace HotelManager.Services.Manager
{
    class ExportService
    {
        public void ExportEmployeeListToExcel<T>(IEnumerable<T> data, string filePath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Bui Quoc Bao");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Employees");

            // Lấy tất cả Property trừ UserAccount
            var props = typeof(T)
                .GetProperties()
                .Where(p => p.Name != "UserAccount")
                .ToArray();

            // Header
            for (int i = 0; i < props.Length; i++)
            {
                ws.Cells[1, i + 1].Value = props[i].Name;
                ws.Cells[1, i + 1].Style.Font.Bold = true;
                ws.Column(i + 1).AutoFit();
            }

            // Data
            int row = 2;
            foreach (var item in data)
            {
                for (int i = 0; i < props.Length; i++)
                {
                    ws.Cells[row, i + 1].Value = props[i].GetValue(item)?.ToString();
                }
                row++;
            }

            package.SaveAs(new FileInfo(filePath));
        }



        public void ExportRevenueDataWithChart(Dictionary<string, (decimal revenue, int invoiceCount)> data,
                                               string filePath)
        {
            // EPPlus 8 => dùng EPPlusLicenseContext (nằm trong OfficeOpenXml)
            ExcelPackage.License.SetNonCommercialPersonal("Bui Quoc Bao");


            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("RevenueReport");

            ws.Cells[1, 1].Value = "Date";
            ws.Cells[1, 2].Value = "Revenue";
            ws.Cells[1, 3].Value = "Invoice Count";

            int row = 2;
            foreach (var kvp in data.OrderBy(k => k.Key))
            {
                ws.Cells[row, 1].Value = kvp.Key;
                ws.Cells[row, 2].Value = kvp.Value.revenue;
                ws.Cells[row, 3].Value = kvp.Value.invoiceCount;
                row++;
            }

            var chart = ws.Drawings.AddChart("RevenueChart", eChartType.ColumnClustered) as ExcelChart;
            chart.Title.Text = "Revenue & Invoice Count";
            chart.SetPosition(1, 0, 4, 0);
            chart.SetSize(600, 400);

            chart.Series.Add(ws.Cells[2, 2, row - 1, 2], ws.Cells[2, 1, row - 1, 1]).Header = "Revenue";
            var lineChart = chart.PlotArea.ChartTypes.Add(eChartType.Line);
            lineChart.UseSecondaryAxis = true;
            lineChart.Series.Add(ws.Cells[2, 3, row - 1, 3], ws.Cells[2, 1, row - 1, 1]).Header = "Invoice Count";

            package.SaveAs(new FileInfo(filePath));
        }

        //export receptionist activity report
        public void ExportReceptionistActivitiesToExcel(ReceptionistChartData data, string selectedUnit, string filePath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Bui Quoc Bao");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("ReceptionistActivities");

            // Header
            if (selectedUnit == "Counting")
            {
                ws.Cells[1, 1].Value = "Receptionist";
                ws.Cells[1, 2].Value = "Booking Created";
                ws.Cells[1, 3].Value = "Check-In";
                ws.Cells[1, 4].Value = "Check-Out";

                ws.Row(1).Style.Font.Bold = true;

                // Data
                for (int i = 0; i < data.Labels.Length; i++)
                {
                    ws.Cells[i + 2, 1].Value = data.Labels[i];
                    ws.Cells[i + 2, 2].Value = data.BookingCounts[i];
                    ws.Cells[i + 2, 3].Value = data.CheckInCounts[i];
                    ws.Cells[i + 2, 4].Value = data.CheckOutCounts[i];
                }

                // Add Chart
                var chart = ws.Drawings.AddChart("ReceptionistChart", eChartType.BarClustered) as ExcelChart;
                chart.Title.Text = "Receptionist Activities (Counting)";
                chart.SetPosition(1, 0, 6, 0);
                chart.SetSize(800, 500);

                chart.Series.Add(ws.Cells[2, 2, data.Labels.Length + 1, 2], ws.Cells[2, 1, data.Labels.Length + 1, 1]).Header = "Booking Created";
                chart.Series.Add(ws.Cells[2, 3, data.Labels.Length + 1, 3], ws.Cells[2, 1, data.Labels.Length + 1, 1]).Header = "Check-In";
                chart.Series.Add(ws.Cells[2, 4, data.Labels.Length + 1, 4], ws.Cells[2, 1, data.Labels.Length + 1, 1]).Header = "Check-Out";
            }
            else if (selectedUnit == "Revenue")
            {
                ws.Cells[1, 1].Value = "Receptionist";
                ws.Cells[1, 2].Value = "Booking Revenue";
                ws.Cells[1, 3].Value = "Check-In Revenue";
                ws.Cells[1, 4].Value = "Check-Out Revenue";

                ws.Row(1).Style.Font.Bold = true;

                // Data
                for (int i = 0; i < data.Labels.Length; i++)
                {
                    ws.Cells[i + 2, 1].Value = data.Labels[i];
                    ws.Cells[i + 2, 2].Value = data.BookingRevenues[i];
                    ws.Cells[i + 2, 3].Value = data.CheckInRevenues[i];
                    ws.Cells[i + 2, 4].Value = data.CheckOutRevenues[i];
                }

                // Format tiền tệ
                ws.Cells[2, 2, data.Labels.Length + 1, 4].Style.Numberformat.Format = "#,##0 \"₫\"";

                // Chart dạng Bar (thanh ngang)
                var chart = ws.Drawings.AddChart("ReceptionistChart", eChartType.BarClustered) as ExcelChart;
                chart.Title.Text = "Receptionist Activities (Counting)";
                chart.SetPosition(1, 0, 6, 0);
                chart.SetSize(800, 500);

                chart.Series.Add(ws.Cells[2, 2, data.Labels.Length + 1, 2], ws.Cells[2, 1, data.Labels.Length + 1, 1]).Header = "Booking Created";
                chart.Series.Add(ws.Cells[2, 3, data.Labels.Length + 1, 3], ws.Cells[2, 1, data.Labels.Length + 1, 1]).Header = "Check-In";
                chart.Series.Add(ws.Cells[2, 4, data.Labels.Length + 1, 4], ws.Cells[2, 1, data.Labels.Length + 1, 1]).Header = "Check-Out";

            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            package.SaveAs(new FileInfo(filePath));
        }


    }
}
