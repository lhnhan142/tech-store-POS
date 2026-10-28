using BLL.DTOs.Dashboard;
using BLL.Services.Interfaces;
using Common.Enums;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;

        public DashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            // 1. Doanh thu và Đơn hàng từ Invoices & Orders
            var allInvoices = (await _uow.Invoices.GetAllAsync()).ToList();
            var allRefunds = (await _uow.Refunds.GetAllAsync()).ToList();
            var allOrders = (await _uow.Repository<Order>().GetAllAsync()).ToList();

            decimal totalRevenue = 0;
            int totalOrders = 0;
            int completedOrders = 0;

            if (allInvoices.Count > 0)
            {
                var paidInvoices = allInvoices.Where(i => i.Status == InvoiceStatus.Paid).ToList();
                totalRevenue = paidInvoices.Sum(i => i.TotalAmount) - allRefunds.Sum(r => r.Amount);
                if (totalRevenue < 0) totalRevenue = 0;
                totalOrders = allInvoices.Count;
                completedOrders = paidInvoices.Count;
            }
            else
            {
                var compOrders = allOrders.Where(o => o.Status == OrderStatus.Completed).ToList();
                totalRevenue = compOrders.Sum(o => o.TotalAmount - o.DiscountAmount);
                totalOrders = allOrders.Count;
                completedOrders = compOrders.Count;
            }

            // 2. Nhân sự
            var employees = (await _uow.Employees.GetAllAsync()).ToList();
            int activeEmployees = employees.Count(e => e.Status == EmployeeStatus.Active);

            // 3. Sản phẩm & Tồn kho
            var products = (await _uow.Products.GetAllAsync()).ToList();
            int totalProducts = products.Count;
            int totalStockQty = products.Sum(p => p.StockQuantity);
            int lowStockCount = products.Count(p => p.StockQuantity <= 10);

            // 4. Chi phí lương tháng hiện tại
            var now = DateTime.Now;
            var currentPayroll = (await _uow.Payrolls.FindAsync(p => p.Month == now.Month && p.Year == now.Year)).FirstOrDefault();
            decimal monthlyPayroll = currentPayroll?.TotalNetAmount ?? 0;

            return new DashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                ActiveEmployees = activeEmployees,
                TotalProducts = totalProducts,
                TotalStockQuantity = totalStockQty,
                LowStockProductsCount = lowStockCount,
                MonthlyPayrollExpense = monthlyPayroll
            };
        }

        public async Task<IReadOnlyList<RevenueChartPointDto>> GetDailyRevenueChartAsync(DateTime fromDate, DateTime toDate)
        {
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1).AddTicks(-1);

            var invoices = (await _uow.Invoices.FindAsync(i => i.CreatedAt >= start && i.CreatedAt <= end && i.Status == InvoiceStatus.Paid)).ToList();
            var refunds = (await _uow.Refunds.FindAsync(r => r.RefundedAt >= start && r.RefundedAt <= end)).ToList();

            var points = new List<RevenueChartPointDto>();
            for (var day = start; day <= toDate.Date; day = day.AddDays(1))
            {
                var nextDay = day.AddDays(1);
                var dayInvoices = invoices.Where(i => i.CreatedAt >= day && i.CreatedAt < nextDay).ToList();
                var dayRefunds = refunds.Where(r => r.RefundedAt >= day && r.RefundedAt < nextDay).ToList();

                decimal rev = dayInvoices.Sum(i => i.TotalAmount) - dayRefunds.Sum(r => r.Amount);
                if (rev < 0) rev = 0;

                points.Add(new RevenueChartPointDto
                {
                    Label = day.ToString("dd/MM"),
                    Revenue = rev,
                    OrderCount = dayInvoices.Count
                });
            }

            return points;
        }

        public async Task<IReadOnlyList<RevenueChartPointDto>> GetMonthlyRevenueChartAsync(int year)
        {
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31, 23, 59, 59);

            var invoices = (await _uow.Invoices.FindAsync(i => i.CreatedAt >= start && i.CreatedAt <= end && i.Status == InvoiceStatus.Paid)).ToList();
            var refunds = (await _uow.Refunds.FindAsync(r => r.RefundedAt >= start && r.RefundedAt <= end)).ToList();

            var points = new List<RevenueChartPointDto>();
            for (int m = 1; m <= 12; m++)
            {
                var mInvoices = invoices.Where(i => i.CreatedAt.Month == m).ToList();
                var mRefunds = refunds.Where(r => r.RefundedAt.Month == m).ToList();

                decimal rev = mInvoices.Sum(i => i.TotalAmount) - mRefunds.Sum(r => r.Amount);
                if (rev < 0) rev = 0;

                points.Add(new RevenueChartPointDto
                {
                    Label = $"T{m}",
                    Revenue = rev,
                    OrderCount = mInvoices.Count
                });
            }

            return points;
        }

        public async Task<IReadOnlyList<TopSellingProductDto>> GetTopSellingProductsAsync(DateTime fromDate, DateTime toDate, int top = 5)
        {
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1).AddTicks(-1);

            var paidInvoiceIds = (await _uow.Invoices.FindAsync(i => i.CreatedAt >= start && i.CreatedAt <= end && i.Status == InvoiceStatus.Paid))
                .Select(i => i.Id)
                .ToHashSet();

            var invoiceItems = (await _uow.InvoiceItems.FindAsync(it => paidInvoiceIds.Contains(it.InvoiceId))).ToList();

            if (invoiceItems.Count > 0)
            {
                var grouped = invoiceItems
                    .GroupBy(x => new { x.ProductId, x.ProductName })
                    .Select(g => new TopSellingProductDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        QuantitySold = g.Sum(x => x.Qty - x.ReturnedQty),
                        TotalRevenue = g.Sum(x => x.LineTotal)
                    })
                    .Where(x => x.QuantitySold > 0)
                    .OrderByDescending(x => x.QuantitySold)
                    .Take(top)
                    .ToList();

                return grouped;
            }

            // Fallback: OrderDetails
            var orderDetails = (await _uow.Repository<OrderDetail>().GetAllAsync()).ToList();
            var topFromOrders = orderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new TopSellingProductDto
                {
                    ProductId = g.Key,
                    ProductName = $"Sản phẩm #{g.Key}",
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(top)
                .ToList();

            foreach (var item in topFromOrders)
            {
                var prod = await _uow.Products.GetByIdAsync(item.ProductId);
                if (prod != null) item.ProductName = prod.Name;
            }

            return topFromOrders;
        }

        public async Task<IReadOnlyList<StockCategorySummaryDto>> GetStockByCategoryAsync()
        {
            var products = (await _uow.Products.GetAllAsync()).ToList();
            var categories = (await _uow.Repository<Category>().GetAllAsync()).ToList();
            var catDict = categories.ToDictionary(c => c.Id, c => c.Name);

            var grouped = products
                .GroupBy(p => p.CategoryId)
                .Select(g => new StockCategorySummaryDto
                {
                    CategoryName = catDict.TryGetValue(g.Key, out var name) ? name : "Chưa phân loại",
                    ProductCount = g.Count(),
                    TotalQuantity = g.Sum(p => p.StockQuantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            return grouped;
        }
    }
}
