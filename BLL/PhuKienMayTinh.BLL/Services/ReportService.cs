using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.Common.Constants;
using PhuKienMayTinh.Common.Utilities;
using PhuKienMayTinh.DAL.Context;

namespace PhuKienMayTinh.BLL.Services.Interfaces
{
    public interface IReportService
    {
        Task<DataTable> GenerateReportDataTableAsync(ReportFilterDTO filter);
        Task ExportReportToExcelAsync(ReportFilterDTO filter, string filePath);
        Task ExportReportToPdfAsync(ReportFilterDTO filter, string filePath);
    }
}

namespace PhuKienMayTinh.BLL.Services.Implementations
{
    using PhuKienMayTinh.BLL.Services.Interfaces;

    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DataTable> GenerateReportDataTableAsync(ReportFilterDTO filter)
        {
            return filter.ReportType switch
            {
                "DoanhThu" => await GenerateSalesReportTableAsync(filter),
                "TonKho" => await GenerateInventoryReportTableAsync(filter),
                "ChamCong" => await GenerateAttendanceReportTableAsync(filter),
                "BangLuong" => await GeneratePayrollReportTableAsync(filter),
                _ => await GenerateSalesReportTableAsync(filter)
            };
        }

        public async Task ExportReportToExcelAsync(ReportFilterDTO filter, string filePath)
        {
            var dt = await GenerateReportDataTableAsync(filter);
            string title = GetReportTitle(filter);
            ExcelHelper.ExportToExcel(dt, title, filePath);
        }

        public async Task ExportReportToPdfAsync(ReportFilterDTO filter, string filePath)
        {
            var dt = await GenerateReportDataTableAsync(filter);
            string title = GetReportTitle(filter);
            PdfHelper.ExportTableToPdf(dt, title, filePath);
        }

        private async Task<DataTable> GenerateSalesReportTableAsync(ReportFilterDTO filter)
        {
            var from = filter.FromDate.Date;
            var to = filter.ToDate.Date;

            var orders = await _context.Orders
                .Include(o => o.Employee)
                .Where(o => o.OrderDate.Date >= from && o.OrderDate.Date <= to)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var dt = new DataTable("BaoCaoDoanhThu");
            dt.Columns.Add("STT", typeof(int));
            dt.Columns.Add("Mã đơn", typeof(string));
            dt.Columns.Add("Ngày bán", typeof(string));
            dt.Columns.Add("Khách hàng", typeof(string));
            dt.Columns.Add("Nhân viên bán", typeof(string));
            dt.Columns.Add("Tổng tiền", typeof(decimal));
            dt.Columns.Add("Giảm giá", typeof(decimal));
            dt.Columns.Add("Thực thu", typeof(decimal));
            dt.Columns.Add("Phương thức", typeof(string));

            int stt = 1;
            foreach (var o in orders)
            {
                dt.Rows.Add(
                    stt++,
                    o.OrderCode,
                    o.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                    o.CustomerName,
                    o.Employee?.FullName ?? "Tại quầy",
                    o.TotalAmount,
                    o.DiscountAmount,
                    o.FinalAmount,
                    o.PaymentMethod
                );
            }

            return dt;
        }

        private async Task<DataTable> GenerateInventoryReportTableAsync(ReportFilterDTO filter)
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter.Category) && filter.Category != "Tất cả")
            {
                query = query.Where(p => p.Category == filter.Category);
            }

            var products = await query.OrderBy(p => p.Category).ThenBy(p => p.ProductCode).ToListAsync();

            var dt = new DataTable("BaoCaoTonKho");
            dt.Columns.Add("STT", typeof(int));
            dt.Columns.Add("Mã SP", typeof(string));
            dt.Columns.Add("Tên sản phẩm", typeof(string));
            dt.Columns.Add("Danh mục", typeof(string));
            dt.Columns.Add("Thương hiệu", typeof(string));
            dt.Columns.Add("ĐVT", typeof(string));
            dt.Columns.Add("Giá nhập", typeof(decimal));
            dt.Columns.Add("Giá bán", typeof(decimal));
            dt.Columns.Add("Tồn kho", typeof(int));
            dt.Columns.Add("Giá trị tồn", typeof(decimal));
            dt.Columns.Add("Cảnh báo", typeof(string));

            int stt = 1;
            foreach (var p in products)
            {
                decimal totalValue = p.CostPrice * p.StockQuantity;
                string alert = p.StockQuantity <= p.MinimumStockAlert ? "Sắp hết hàng!" : "Bình thường";

                dt.Rows.Add(
                    stt++,
                    p.ProductCode,
                    p.Name,
                    p.Category,
                    p.Brand,
                    p.Unit,
                    p.CostPrice,
                    p.SellingPrice,
                    p.StockQuantity,
                    totalValue,
                    alert
                );
            }

            return dt;
        }

        private async Task<DataTable> GenerateAttendanceReportTableAsync(ReportFilterDTO filter)
        {
            var from = filter.FromDate.Date;
            var to = filter.ToDate.Date;

            var attendances = await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.WorkDate.Date >= from && a.WorkDate.Date <= to)
                .OrderByDescending(a => a.WorkDate)
                .ToListAsync();

            var dt = new DataTable("BaoCaoChamCong");
            dt.Columns.Add("STT", typeof(int));
            dt.Columns.Add("Mã NV", typeof(string));
            dt.Columns.Add("Họ tên", typeof(string));
            dt.Columns.Add("Phòng ban", typeof(string));
            dt.Columns.Add("Ngày làm việc", typeof(string));
            dt.Columns.Add("Giờ vào", typeof(string));
            dt.Columns.Add("Giờ ra", typeof(string));
            dt.Columns.Add("Số giờ", typeof(double));
            dt.Columns.Add("Trễ (phút)", typeof(int));
            dt.Columns.Add("Trạng thái", typeof(string));

            int stt = 1;
            foreach (var a in attendances)
            {
                dt.Rows.Add(
                    stt++,
                    a.Employee.EmployeeCode,
                    a.Employee.FullName,
                    a.Employee.Department,
                    a.WorkDate.ToString("dd/MM/yyyy"),
                    a.CheckInTime?.ToString("HH:mm:ss") ?? "--:--",
                    a.CheckOutTime?.ToString("HH:mm:ss") ?? "--:--",
                    a.HoursWorked,
                    a.LateMinutes,
                    a.Status.ToString()
                );
            }

            return dt;
        }

        private async Task<DataTable> GeneratePayrollReportTableAsync(ReportFilterDTO filter)
        {
            var payroll = await _context.Payrolls
                .Include(p => p.Details)
                .ThenInclude(d => d.Employee)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .FirstOrDefaultAsync();

            var dt = new DataTable("BaoCaoBangLuong");
            dt.Columns.Add("STT", typeof(int));
            dt.Columns.Add("Mã NV", typeof(string));
            dt.Columns.Add("Họ và Tên", typeof(string));
            dt.Columns.Add("Chức vụ", typeof(string));
            dt.Columns.Add("Ngày công", typeof(double));
            dt.Columns.Add("Lương cơ bản", typeof(decimal));
            dt.Columns.Add("Lương theo công", typeof(decimal));
            dt.Columns.Add("Phụ cấp", typeof(decimal));
            dt.Columns.Add("Thưởng", typeof(decimal));
            dt.Columns.Add("Khấu trừ", typeof(decimal));
            dt.Columns.Add("THỰC LĨNH", typeof(decimal));

            if (payroll != null)
            {
                int stt = 1;
                foreach (var d in payroll.Details)
                {
                    dt.Rows.Add(
                        stt++,
                        d.Employee.EmployeeCode,
                        d.Employee.FullName,
                        d.Employee.Position,
                        d.ActualWorkDays,
                        d.BaseSalary,
                        d.ActualBaseSalary,
                        d.TotalAllowance,
                        d.TotalBonus,
                        d.TotalDeductions,
                        d.NetSalary
                    );
                }
            }

            return dt;
        }

        private static string GetReportTitle(ReportFilterDTO filter)
        {
            return filter.ReportType switch
            {
                "DoanhThu" => $"BÁO CÁO DOANH THU BÁN HÀNG ({filter.FromDate:dd/MM/yyyy} - {filter.ToDate:dd/MM/yyyy})",
                "TonKho" => "BÁO CÁO TỒN KHO VÀ CẢNH BÁO HÀNG HÓA",
                "ChamCong" => $"BÁO CÁO CHẤM CÔNG NHÂN VIÊN ({filter.FromDate:dd/MM/yyyy} - {filter.ToDate:dd/MM/yyyy})",
                "BangLuong" => "BÁO CÁO BẢNG LƯƠNG NHÂN VIÊN",
                _ => "BÁO CÁO THỐNG KÊ HOẠT ĐỘNG KINH DOANH"
            };
        }
    }
}
