using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.DAL.Repositories.Interfaces;

namespace PhuKienMayTinh.BLL.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDTO> GetDashboardSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}

namespace PhuKienMayTinh.BLL.Services.Implementations
{
    using PhuKienMayTinh.BLL.Services.Interfaces;

    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepo;
        private readonly IPayrollRepository _payrollRepo;

        public DashboardService(IDashboardRepository dashboardRepo, IPayrollRepository payrollRepo)
        {
            _dashboardRepo = dashboardRepo;
            _payrollRepo = payrollRepo;
        }

        public async Task<DashboardSummaryDTO> GetDashboardSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var to = (toDate ?? DateTime.Today).Date;
            var from = (fromDate ?? to.AddDays(-14)).Date;
            var now = DateTime.Now;

            var summary = new DashboardSummaryDTO
            {
                TodayRevenue = await _dashboardRepo.GetTodayRevenueAsync(),
                MonthRevenue = await _dashboardRepo.GetMonthRevenueAsync(now.Month, now.Year),
                TodayOrderCount = await _dashboardRepo.GetTodayOrderCountAsync(),
                MonthOrderCount = await _dashboardRepo.GetMonthOrderCountAsync(now.Month, now.Year),
                ActiveEmployeeCount = await _dashboardRepo.GetActiveEmployeeCountAsync(),
                TodayPresentEmployeeCount = await _dashboardRepo.GetTodayPresentEmployeeCountAsync(),
                TotalProductCount = await _dashboardRepo.GetTotalProductCountAsync(),
                LowStockProductCount = await _dashboardRepo.GetLowStockProductCountAsync()
            };

            // Quỹ lương tháng gần nhất
            var latestPayroll = await _payrollRepo.GetPayrollByMonthYearAsync(now.Month, now.Year) ??
                                await _payrollRepo.GetPayrollByMonthYearAsync(now.Month > 1 ? now.Month - 1 : 12, now.Month > 1 ? now.Year : now.Year - 1);
            summary.EstimatedPayrollFund = latestPayroll?.TotalNetSalary ?? 85000000m;

            // Dữ liệu biểu đồ doanh thu theo ngày
            var dailyList = await _dashboardRepo.GetDailyRevenueAsync(from, to);
            summary.RevenuePoints = dailyList.Select(d => new DailyRevenuePointDTO
            {
                Date = d.Date,
                Revenue = d.Revenue
            }).ToList();

            // Dữ liệu biểu đồ tròn tỷ trọng danh mục
            var categoryList = (await _dashboardRepo.GetCategoryRevenueAsync(from, to)).ToList();
            decimal totalCatRev = categoryList.Sum(c => c.Revenue);
            if (totalCatRev == 0) totalCatRev = 1;

            summary.CategorySlices = categoryList.Select(c => new CategoryRevenueSliceDTO
            {
                Category = c.Category,
                Revenue = c.Revenue,
                Percentage = Math.Round((double)(c.Revenue / totalCatRev) * 100.0, 1)
            }).ToList();

            // Top nhân viên bán hàng xuất sắc
            var topList = await _dashboardRepo.GetTopSellingEmployeesAsync(from, to, 5);
            summary.TopEmployees = topList.Select(t => new TopEmployeeSalesDTO
            {
                EmployeeName = t.EmployeeName,
                OrderCount = t.OrderCount,
                TotalSales = t.TotalSales
            }).ToList();

            // Sản phẩm sắp hết hàng
            var lowStock = await _dashboardRepo.GetLowStockProductsAsync(5);
            summary.LowStockAlerts = lowStock.Select(p => new LowStockProductDTO
            {
                ProductCode = p.ProductCode,
                ProductName = p.Name,
                Category = p.Category,
                StockQuantity = p.StockQuantity,
                AlertThreshold = p.MinimumStockAlert
            }).ToList();

            // Đơn hàng gần đây
            var recent = await _dashboardRepo.GetRecentOrdersAsync(6);
            summary.RecentOrders = recent.Select(o => new RecentOrderDTO
            {
                OrderCode = o.OrderCode,
                CustomerName = o.CustomerName,
                OrderDate = o.OrderDate,
                FinalAmount = o.FinalAmount,
                PaymentMethod = o.PaymentMethod,
                SellerName = o.Employee?.FullName ?? "Quầy POS"
            }).ToList();

            return summary;
        }
    }
}
