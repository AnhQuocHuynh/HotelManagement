using ClosedXML.Excel;

namespace HotelManager.Services.Manager
{
    class ExportService
    {
        public void ExportToExcel<T>(IEnumerable<T> data, string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Employees");

            var props = typeof(T).GetProperties();
            // Header
            for (int i = 0; i < props.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = props[i].Name;
            }

            // Data
            int row = 2;
            foreach (var item in data)
            {
                for (int i = 0; i < props.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = props[i].GetValue(item)?.ToString();
                }
                row++;
            }

            workbook.SaveAs(filePath);
        }
    }
}
