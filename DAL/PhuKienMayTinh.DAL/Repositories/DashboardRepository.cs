using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhuKienMayTinh.Common.Enums;
using PhuKienMayTinh.DAL.Context;
using PhuKienMayTinh.DAL.Entities;

namespace PhuKienMayTinh.DAL.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<decimal> GetTodayRevenueAsync();
        Task<decimal> GetMonthRevenueAsync(int month, int year);
        Task<int> GetTodayOrderCountAsync();
        Task<int> GetMonthOrderCountAsync(int month, int year);
        Task<int> GetActiveEmployeeCountAsync();
        Task<int> GetTodayPresentEmployeeCountAsync();
        Task<int> GetTotalProductCountAsync();
        Task<int> GetLowStockProductCountAsync();
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int take = 5);
        Task<IEnumerable<(DateTime Date, decimal Revenue)>> GetDailyRevenueAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<(string Category, decimal Revenue)>> GetCategoryRevenueAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<(string EmployeeName, int OrderCount, decimal TotalSales)>> GetTopSellingEmployeesAsync(DateTime fromDate, DateTime toDate, int take = 5);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int take = 6);
    }
}

namespace PhuKienMayTinh.DAL.Repositories.Implementations
{
    using PhuKienMayTinh.DAL.Repositories.Interfaces;

    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTodayRevenueAsync()
        {
            var today = DateTime.Today;
            return await _context.Orders
                .Where(o => o.OrderDate.Date == today && o.Status == "Hoàn thành")
                .SumAsync(o => (decimal?)o.FinalAmount) ?? 0m;
        }

        public async Task<decimal> GetMonthRevenueAsync(int month, int year)
        {
            return await _context.Orders
                .Where(o => o.OrderDate.Month == month && o.OrderDate.Year == year && o.Status == "Hoàn thành")
                .SumAsync(o => (decimal?)o.FinalAmount) ?? 0m;
        }

        public async Task<int> GetTodayOrderCountAsync()
        {
            var today = DateTime.Today;
            return await _context.Orders
                .CountAsync(o => o.OrderDate.Date == today && o.Status == "Hoàn thành");
        }

        public async Task<int> GetMonthOrderCountAsync(int month, int year)
        {
            return await _context.Orders
                .CountAsync(o => o.OrderDate.Month == month && o.OrderDate.Year == year && o.Status == "Hoàn thành");
        }

        public async Task<int> GetActiveEmployeeCountAsync()
        {
            return await _context.Employees.CountAsync(e => e.Status == EmployeeStatus.Active);
        }

        public async Task<int> GetTodayPresentEmployeeCountAsync()
        {
            var today = DateTime.Today;
            return await _context.Attendances
                .Where(a => a.WorkDate.Date == today &&
                           (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late))
                .Select(a => a.EmployeeId)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetTotalProductCountAsync()
        {
            return await _context.Products.CountAsync(p => p.IsActive);
        }

        public async Task<int> GetLowStockProductCountAsync()
        {
            return await _context.Products.CountAsync(p => p.IsActive && p.StockQuantity <= p.MinimumStockAlert);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int take = 5)
        {
            return await _context.Products
                .Where(p => p.IsActive && p.StockQuantity <= p.MinimumStockAlert)
                .OrderBy(p => p.StockQuantity)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<(DateTime Date, decimal Revenue)>> GetDailyRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            var from = fromDate.Date;
            var to = toDate.Date;

            var rawData = await _context.Orders
                .Where(o => o.OrderDate.Date >= from && o.OrderDate.Date <= to && o.Status == "Hoàn thành")
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.FinalAmount) })
                .ToListAsync();

            var result = new List<(DateTime Date, decimal Revenue)>();
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                var match = rawData.FirstOrDefault(x => x.Date == d);
                result.Add((d, match?.Revenue ?? 0m));
            }

            return result;
        }

        public async Task<IEnumerable<(string Category, decimal Revenue)>> GetCategoryRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            var from = fromDate.Date;
            var to = toDate.Date;

            var items = await _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .Where(od => od.Order.OrderDate.Date >= from &&
                             od.Order.OrderDate.Date <= to &&
                             od.Order.Status == "Hoàn thành")
                .GroupBy(od => od.Product.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Revenue = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            return items.Select(x => (x.Category, x.Revenue));
        }

        public async Task<IEnumerable<(string EmployeeName, int OrderCount, decimal TotalSales)>> GetTopSellingEmployeesAsync(DateTime fromDate, DateTime toDate, int take = 5)
        {
            var from = fromDate.Date;
            var to = toDate.Date;

            var list = await _context.Orders
                .Include(o => o.Employee)
                .Where(o => o.OrderDate.Date >= from &&
                             o.OrderDate.Date <= to &&
                             o.Status == "Hoàn thành" &&
                             o.EmployeeId != null)
                .GroupBy(o => o.Employee!.FullName)
                .Select(g => new
                {
                    EmployeeName = g.Key,
                    OrderCount = g.Count(),
                    TotalSales = g.Sum(x => x.FinalAmount)
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(take)
                .ToListAsync();

            return list.Select(x => (x.EmployeeName, x.OrderCount, x.TotalSales));
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int take = 6)
        {
            return await _context.Orders
                .Include(o => o.Employee)
                .OrderByDescending(o => o.OrderDate)
                .Take(take)
                .ToListAsync();
        }
    }
}
