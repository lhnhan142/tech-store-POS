using BLL.DTOs.Attendance;
using BLL.DTOs.Dashboard;
using BLL.DTOs.Employee;
using BLL.DTOs.Payroll;
using BLL.Services.Interfaces;
using Common.Utilities;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BLL.Services.Implementations
{
    public class ReportExportService : IReportExportService
    {
        // =========================================================================
        // 1. BÁO CÁO NHÂN SỰ
        // =========================================================================
        public void ExportEmployeeListToExcel(string filePath, IEnumerable<EmployeeListItemDto> employees)
        {
            var headers = new[]
            {
                "STT", "Mã NV", "Họ và tên", "Chức vụ", "Số điện thoại",
                "Email", "Lương cơ bản", "Phụ cấp", "Trạng thái", "Ngày vào làm"
            };

            int stt = 1;
            var rows = employees.Select(e => new object?[]
            {
                stt++,
                e.EmployeeCode,
                e.FullName,
                e.Position,
                e.PhoneNumber ?? string.Empty,
                e.Email ?? string.Empty,
                e.BaseSalary,
                e.Allowance,
                e.StatusText,
                e.HireDate.ToString("dd/MM/yyyy")
            });

            ExcelHelper.WriteSheet(filePath, "Danh sách nhân viên", headers, rows);
        }

        public void ExportEmployeeListToPdf(string filePath, IEnumerable<EmployeeListItemDto> employees)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Báo cáo danh sách nhân viên - Tech Store POS";

            var page = doc.AddPage();
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Portrait;

            var gfx = XGraphics.FromPdfPage(page);
            var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            var fontSub = new XFont("Arial", 9, XFontStyle.Italic);
            var fontHeader = new XFont("Arial", 9, XFontStyle.Bold);
            var fontBody = new XFont("Arial", 8.5, XFontStyle.Regular);

            double y = 40;
            gfx.DrawString("TECH STORE POS - HỆ THỐNG QUẢN LÝ BÁN PHỤ KIỆN MÁY TÍNH", fontSub, XBrushes.DarkBlue, new XPoint(40, y));
            y += 25;
            gfx.DrawString("BÁO CÁO DANH SÁCH NHÂN VIÊN", fontTitle, XBrushes.Black, new XPoint(40, y));
            y += 18;
            gfx.DrawString($"Ngày lập báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm} | Tổng số nhân viên: {employees.Count()}", fontSub, XBrushes.Gray, new XPoint(40, y));
            y += 25;

            // Bảng nhân sự
            double[] colWidths = { 30, 55, 120, 95, 75, 75, 70 };
            string[] headers = { "STT", "Mã NV", "Họ và tên", "Chức vụ", "SĐT", "Lương CB", "Trạng thái" };

            y = DrawTableHeader(gfx, 40, y, colWidths, headers, fontHeader);

            int stt = 1;
            foreach (var e in employees)
            {
                if (y > page.Height - 50)
                {
                    page = doc.AddPage();
                    page.Size = PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                    y = DrawTableHeader(gfx, 40, y, colWidths, headers, fontHeader);
                }

                string[] rowData = {
                    (stt++).ToString(),
                    e.EmployeeCode,
                    e.FullName,
                    e.Position,
                    e.PhoneNumber ?? "-",
                    e.BaseSalary.ToString("N0") + " đ",
                    e.StatusText
                };

                y = DrawTableRow(gfx, 40, y, colWidths, rowData, fontBody);
            }

            doc.Save(filePath);
        }

        // =========================================================================
        // 2. BÁO CÁO CHẤM CÔNG
        // =========================================================================
        public void ExportAttendanceReportToExcel(string filePath, IEnumerable<AttendanceRecordDto> records, DateTime from, DateTime to)
        {
            var headers = new[]
            {
                "STT", "Ngày", "Mã NV", "Họ và tên", "Chức vụ",
                "Check-in", "Check-out", "Giờ làm", "Tăng ca (h)", "Trạng thái", "Ghi chú"
            };

            int stt = 1;
            var rows = records.Select(r => new object?[]
            {
                stt++,
                r.Date.ToString("dd/MM/yyyy"),
                r.EmployeeCode,
                r.EmployeeName,
                r.Position,
                r.CheckIn.HasValue ? r.CheckIn.Value.ToString(@"hh\:mm") : string.Empty,
                r.CheckOut.HasValue ? r.CheckOut.Value.ToString(@"hh\:mm") : string.Empty,
                r.HoursWorked,
                r.OvertimeHours,
                r.StatusText,
                r.Note ?? string.Empty
            });

            ExcelHelper.WriteSheet(filePath, "Bảng chấm công", headers, rows);
        }

        public void ExportAttendanceReportToPdf(string filePath, IEnumerable<AttendanceRecordDto> records, DateTime from, DateTime to)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Báo cáo chấm công - Tech Store POS";

            var page = doc.AddPage();
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Landscape;

            var gfx = XGraphics.FromPdfPage(page);
            var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            var fontSub = new XFont("Arial", 9, XFontStyle.Italic);
            var fontHeader = new XFont("Arial", 9, XFontStyle.Bold);
            var fontBody = new XFont("Arial", 8.5, XFontStyle.Regular);

            double y = 35;
            gfx.DrawString("TECH STORE POS - BÁO CÁO CHẤM CÔNG NHÂN VIÊN", fontTitle, XBrushes.Black, new XPoint(40, y));
            y += 18;
            gfx.DrawString($"Thời gian: Từ ngày {from:dd/MM/yyyy} đến ngày {to:dd/MM/yyyy} | Tổng số lượt: {records.Count()}", fontSub, XBrushes.Gray, new XPoint(40, y));
            y += 20;

            double[] colWidths = { 30, 65, 55, 130, 95, 60, 60, 60, 60, 80, 80 };
            string[] headers = { "STT", "Ngày", "Mã NV", "Họ và tên", "Chức vụ", "CheckIn", "CheckOut", "Giờ làm", "Tăng ca", "Trạng thái", "Ghi chú" };

            y = DrawTableHeader(gfx, 40, y, colWidths, headers, fontHeader);

            int stt = 1;
            foreach (var r in records)
            {
                if (y > page.Height - 45)
                {
                    page = doc.AddPage();
                    page.Size = PageSize.A4;
                    page.Orientation = PageOrientation.Landscape;
                    gfx = XGraphics.FromPdfPage(page);
                    y = 35;
                    y = DrawTableHeader(gfx, 40, y, colWidths, headers, fontHeader);
                }

                string[] rowData = {
                    (stt++).ToString(),
                    r.Date.ToString("dd/MM/yyyy"),
                    r.EmployeeCode,
                    r.EmployeeName,
                    r.Position,
                    r.CheckIn.HasValue ? r.CheckIn.Value.ToString(@"hh\:mm") : "-",
                    r.CheckOut.HasValue ? r.CheckOut.Value.ToString(@"hh\:mm") : "-",
                    r.HoursWorked.ToString("0.#"),
                    r.OvertimeHours.ToString("0.#"),
                    r.StatusText,
                    r.Note ?? string.Empty
                };

                y = DrawTableRow(gfx, 40, y, colWidths, rowData, fontBody);
            }

            doc.Save(filePath);
        }

        // =========================================================================
        // 3. BÁO CÁO BẢNG LƯƠNG
        // =========================================================================
        public void ExportPayrollReportToExcel(string filePath, PayrollSummaryDto summary, IEnumerable<PayrollDetailDto> details)
        {
            var headers = new[]
            {
                "STT", "Mã NV", "Họ và tên", "Chức vụ", "Lương cơ bản",
                "Ngày công", "Giờ làm", "Tăng ca (h)", "Lương theo công",
                "Tiền tăng ca", "Phụ cấp", "Thưởng", "Khấu trừ", "THỰC LĨNH"
            };

            int stt = 1;
            var rows = details.Select(d => new object?[]
            {
                stt++,
                d.EmployeeCode,
                d.EmployeeName,
                d.Position,
                d.BaseSalary,
                d.ActualWorkDays,
                d.TotalHoursWorked,
                d.OvertimeHours,
                d.SalaryByWorkDays,
                d.OvertimePay,
                d.Allowance,
                d.Bonus,
                d.Deductions,
                d.NetSalary
            });

            ExcelHelper.WriteSheet(filePath, $"Bảng lương {summary.Month:D2}-{summary.Year}", headers, rows);
        }

        public void ExportPayrollReportToPdf(string filePath, PayrollSummaryDto summary, IEnumerable<PayrollDetailDto> details)
        {
            var doc = new PdfDocument();
            doc.Info.Title = $"Bảng lương {summary.PeriodText} - Tech Store POS";

            var page = doc.AddPage();
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Landscape;

            var gfx = XGraphics.FromPdfPage(page);
            var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            var fontSub = new XFont("Arial", 9, XFontStyle.Italic);
            var fontHeader = new XFont("Arial", 8.5, XFontStyle.Bold);
            var fontBody = new XFont("Arial", 8, XFontStyle.Regular);
            var fontBold = new XFont("Arial", 8.5, XFontStyle.Bold);

            double y = 35;
            gfx.DrawString($"TECH STORE POS - BẢNG THANH TOÁN LƯƠNG {summary.PeriodText.ToUpper()}", fontTitle, XBrushes.Black, new XPoint(40, y));
            y += 18;
            gfx.DrawString($"Mã bảng lương: {summary.PayrollCode} | Trạng thái: {summary.StatusText} | Ngày tạo: {summary.CreatedAt:dd/MM/yyyy} | Ngày chốt: {(summary.FinalizedAt.HasValue ? summary.FinalizedAt.Value.ToString("dd/MM/yyyy") : "Chưa chốt")}", fontSub, XBrushes.Gray, new XPoint(40, y));
            y += 20;

            double[] colWidths = { 25, 45, 110, 80, 65, 40, 40, 65, 55, 50, 50, 50, 75 };
            string[] headers = { "STT", "Mã NV", "Họ và tên", "Chức vụ", "Lương CB", "Công", "TC (h)", "Lương công", "Tiền TC", "Phụ cấp", "Thưởng", "Khấu trừ", "THỰC LĨNH" };

            y = DrawTableHeader(gfx, 40, y, colWidths, headers, fontHeader);

            int stt = 1;
            foreach (var d in details)
            {
                if (y > page.Height - 60)
                {
                    page = doc.AddPage();
                    page.Size = PageSize.A4;
                    page.Orientation = PageOrientation.Landscape;
                    gfx = XGraphics.FromPdfPage(page);
                    y = 35;
                    y = DrawTableHeader(gfx, 40, y, colWidths, headers, fontHeader);
                }

                string[] rowData = {
                    (stt++).ToString(),
                    d.EmployeeCode,
                    d.EmployeeName,
                    d.Position,
                    d.BaseSalary.ToString("N0"),
                    d.ActualWorkDays.ToString("0.#"),
                    d.OvertimeHours.ToString("0.#"),
                    d.SalaryByWorkDays.ToString("N0"),
                    d.OvertimePay.ToString("N0"),
                    d.Allowance.ToString("N0"),
                    d.Bonus.ToString("N0"),
                    d.Deductions.ToString("N0"),
                    d.NetSalary.ToString("N0") + " đ"
                };

                y = DrawTableRow(gfx, 40, y, colWidths, rowData, fontBody);
            }

            // Dòng tổng cộng
            y += 5;
            gfx.DrawLine(XPens.DarkBlue, 40, y, 40 + colWidths.Sum(), y);
            y += 15;
            gfx.DrawString($"TỔNG CỘNG THỰC CHI: {summary.TotalNetAmount:N0} VNĐ  |  Tổng chi phí lương: {summary.TotalGrossAmount:N0} VNĐ", fontBold, XBrushes.DarkRed, new XPoint(40, y));

            doc.Save(filePath);
        }

        // =========================================================================
        // 4. BÁO CÁO KINH DOANH & TỒN KHO
        // =========================================================================
        public void ExportBusinessReportToExcel(string filePath, DashboardSummaryDto summary, IEnumerable<TopSellingProductDto> topProducts, DateTime from, DateTime to)
        {
            var headers = new[] { "Chỉ số kinh doanh & Kho", "Giá trị", "Đơn vị tính" };
            var summaryRows = new List<object?[]>
            {
                new object?[] { "Khoảng thời gian thống kê", $"{from:dd/MM/yyyy} - {to:dd/MM/yyyy}", "" },
                new object?[] { "Tổng doanh thu thực", summary.TotalRevenue, "VNĐ" },
                new object?[] { "Tổng số đơn hàng", summary.TotalOrders, "Đơn" },
                new object?[] { "Số đơn hàng hoàn thành", summary.CompletedOrders, "Đơn" },
                new object?[] { "Tổng số loại sản phẩm", summary.TotalProducts, "Sản phẩm" },
                new object?[] { "Tổng số lượng tồn kho", summary.TotalStockQuantity, "Cái" },
                new object?[] { "Số sản phẩm sắp hết hàng (<=10)", summary.LowStockProductsCount, "Sản phẩm" },
                new object?[] { "Số nhân viên đang làm việc", summary.ActiveEmployees, "Người" },
                new object?[] { "Chi phí lương tháng hiện tại", summary.MonthlyPayrollExpense, "VNĐ" },
                new object?[] { "", "", "" },
                new object?[] { "TOP SẢN PHẨM BÁN CHẠY", "SỐ LƯỢNG ĐÃ BÁN", "DOANH THU (VNĐ)" }
            };

            foreach (var p in topProducts)
            {
                summaryRows.Add(new object?[] { p.ProductName, p.QuantitySold, p.TotalRevenue });
            }

            ExcelHelper.WriteSheet(filePath, "Báo cáo kinh doanh", headers, summaryRows);
        }

        public void ExportBusinessReportToPdf(string filePath, DashboardSummaryDto summary, IEnumerable<TopSellingProductDto> topProducts, DateTime from, DateTime to)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Báo cáo tình hình kinh doanh - Tech Store POS";

            var page = doc.AddPage();
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Portrait;

            var gfx = XGraphics.FromPdfPage(page);
            var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            var fontSub = new XFont("Arial", 9, XFontStyle.Italic);
            var fontHeader = new XFont("Arial", 10, XFontStyle.Bold);
            var fontBody = new XFont("Arial", 9.5, XFontStyle.Regular);
            var fontBoldVal = new XFont("Arial", 9.5, XFontStyle.Bold);

            double y = 40;
            gfx.DrawString("TECH STORE POS - BÁO CÁO HOẠT ĐỘNG KINH DOANH & TỒN KHO", fontTitle, XBrushes.Black, new XPoint(40, y));
            y += 20;
            gfx.DrawString($"Thời gian thống kê: Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy} | Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSub, XBrushes.Gray, new XPoint(40, y));
            y += 25;

            // Bảng chỉ số tổng quan
            double[] colWidths = { 250, 160, 100 };
            string[] headers = { "Chỉ số hoạt động", "Giá trị", "Đơn vị" };
            y = DrawTableHeader(gfx, 40, y, colWidths, headers, fontHeader);

            var kpis = new (string Name, string Value, string Unit)[]
            {
                ("Tổng doanh thu thuần (đã trừ hoàn trả)", summary.TotalRevenue.ToString("N0") + " đ", "VNĐ"),
                ("Tổng số đơn hàng phát sinh", summary.TotalOrders.ToString("N0"), "Đơn"),
                ("Số đơn hàng đã hoàn tất thanh toán", summary.CompletedOrders.ToString("N0"), "Đơn"),
                ("Tổng số loại sản phẩm trong hệ thống", summary.TotalProducts.ToString("N0"), "Sản phẩm"),
                ("Tổng số lượng hàng trong kho", summary.TotalStockQuantity.ToString("N0"), "Cái"),
                ("Số sản phẩm sắp hết hàng (tồn <= 10)", summary.LowStockProductsCount.ToString("N0"), "Sản phẩm"),
                ("Số lượng nhân viên đang làm việc", summary.ActiveEmployees.ToString("N0"), "Nhân sự"),
                ("Chi phí tiền lương tháng hiện tại", summary.MonthlyPayrollExpense.ToString("N0") + " đ", "VNĐ")
            };

            foreach (var (Name, Value, Unit) in kpis)
            {
                y = DrawTableRow(gfx, 40, y, colWidths, new[] { Name, Value, Unit }, fontBody);
            }

            y += 20;
            gfx.DrawString("TOP SẢN PHẨM BÁN CHẠY NHẤT TRONG KỲ", fontHeader, XBrushes.DarkBlue, new XPoint(40, y));
            y += 12;

            double[] topCols = { 40, 240, 100, 130 };
            string[] topHeaders = { "Top", "Tên sản phẩm", "Số lượng bán", "Doanh thu" };
            y = DrawTableHeader(gfx, 40, y, topCols, topHeaders, fontHeader);

            int rank = 1;
            foreach (var p in topProducts)
            {
                string[] pData = {
                    $"#{rank++}",
                    p.ProductName,
                    p.QuantitySold.ToString("N0") + " cái",
                    p.TotalRevenue.ToString("N0") + " đ"
                };
                y = DrawTableRow(gfx, 40, y, topCols, pData, fontBody);
            }

            doc.Save(filePath);
        }

        // =========================================================================
        // HÀM HỖ TRỢ VẼ BẢNG PDF
        // =========================================================================
        private static double DrawTableHeader(XGraphics gfx, double x, double y, double[] colWidths, string[] headers, XFont font)
        {
            double height = 22;
            double curX = x;
            var headerBrush = new XSolidBrush(XColor.FromArgb(235, 240, 250));

            gfx.DrawRectangle(headerBrush, x, y, colWidths.Sum(), height);
            gfx.DrawRectangle(XPens.LightGray, x, y, colWidths.Sum(), height);

            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawString(headers[i], font, XBrushes.Black, new XPoint(curX + 4, y + 15));
                curX += colWidths[i];
                if (i < headers.Length - 1)
                {
                    gfx.DrawLine(XPens.LightGray, curX, y, curX, y + height);
                }
            }

            return y + height;
        }

        private static double DrawTableRow(XGraphics gfx, double x, double y, double[] colWidths, string[] cells, XFont font)
        {
            double height = 20;
            double curX = x;

            gfx.DrawRectangle(XPens.LightGray, x, y, colWidths.Sum(), height);

            for (int i = 0; i < cells.Length; i++)
            {
                gfx.DrawString(cells[i], font, XBrushes.Black, new XPoint(curX + 4, y + 14));
                curX += colWidths[i];
                if (i < cells.Length - 1)
                {
                    gfx.DrawLine(XPens.LightGray, curX, y, curX, y + height);
                }
            }

            return y + height;
        }
    }
}
