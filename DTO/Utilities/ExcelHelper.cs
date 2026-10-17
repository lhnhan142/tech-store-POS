using ClosedXML.Excel;
using System.Globalization;

namespace Common.Utilities;

/// <summary>Một dòng dữ liệu đọc từ Excel: số dòng thực tế trong file + giá trị theo tên cột.</summary>
public sealed class ExcelRow
{
    public int RowNumber { get; }
    private readonly Dictionary<string, string> _cells;

    public ExcelRow(int rowNumber, Dictionary<string, string> cells)
    {
        RowNumber = rowNumber;
        _cells = cells;
    }

    /// <summary>Giá trị ô (đã Trim); cột không có thì trả về chuỗi rỗng.</summary>
    public string Get(string column) =>
        _cells.TryGetValue(column, out var v) ? v.Trim() : string.Empty;
}

/// <summary>Kết quả đọc một sheet: danh sách tiêu đề cột + các dòng dữ liệu (đã bỏ dòng trống).</summary>
public sealed record ExcelSheet(IReadOnlyList<string> Headers, IReadOnlyList<ExcelRow> Rows);

public static class ExcelHelper
{
    /// <summary>Đọc sheet đầu tiên: dòng 1 là tiêu đề, dòng trống hoàn toàn bị bỏ qua.</summary>
    public static ExcelSheet ReadSheet(string path)
    {
        // FileShare.ReadWrite: vẫn đọc được khi file đang mở trong Excel
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var wb = new XLWorkbook(fs);
        var ws = wb.Worksheets.First();

        var headerRow = ws.Row(1);
        int lastCol = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
        var headers = Enumerable.Range(1, lastCol)
            .Select(c => headerRow.Cell(c).GetString().Trim())
            .ToList();

        var rows = new List<ExcelRow>();
        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int r = 2; r <= lastRow; r++)
        {
            var cells = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool empty = true;
            for (int c = 1; c <= lastCol; c++)
            {
                string value = CellText(ws.Cell(r, c));
                if (value.Trim().Length > 0) empty = false;
                if (headers[c - 1].Length > 0) cells[headers[c - 1]] = value;
            }
            if (!empty) rows.Add(new ExcelRow(r, cells));
        }
        return new ExcelSheet(headers, rows);
    }

    // Số trong Excel -> chuỗi theo InvariantCulture (không bị E+, không phụ thuộc máy)
    private static string CellText(IXLCell cell)
    {
        if (cell.IsEmpty()) return string.Empty;
        if (cell.DataType == XLDataType.Number)
        {
            decimal d = (decimal)cell.GetDouble();
            return d.ToString("0.##", CultureInfo.InvariantCulture);
        }
        return cell.GetString();
    }

    /// <summary>Ghi dữ liệu ra file .xlsx (tiêu đề in đậm, cố định hàng đầu, tự giãn cột).</summary>
    public static void WriteSheet(string path, string sheetName, string[] headers, IEnumerable<object?[]> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(sheetName);

        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xE7, 0xF0, 0xFF);
        }

        int r = 2;
        foreach (var row in rows)
        {
            for (int c = 0; c < row.Length; c++)
            {
                var cell = ws.Cell(r, c + 1);
                switch (row[c])
                {
                    case null:
                        break;
                    case decimal d:
                        cell.Value = (double)d;
                        cell.Style.NumberFormat.Format = "#,##0";
                        break;
                    case int i:
                        cell.Value = i;
                        break;
                    case DateTime dt:
                        cell.Value = dt;
                        cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                        break;
                    default:
                        // Chuỗi luôn ghi dạng văn bản (mã vạch dài không bị đổi sang 8.8E+11)
                        cell.Value = row[c]!.ToString() ?? string.Empty;
                        break;
                }
            }
            r++;
        }

        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();
        wb.SaveAs(path);
    }
}
