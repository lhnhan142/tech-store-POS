using BLL.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
        Task<IReadOnlyList<RevenueChartPointDto>> GetDailyRevenueChartAsync(DateTime fromDate, DateTime toDate);
        Task<IReadOnlyList<RevenueChartPointDto>> GetMonthlyRevenueChartAsync(int year);
        Task<IReadOnlyList<TopSellingProductDto>> GetTopSellingProductsAsync(DateTime fromDate, DateTime toDate, int top = 5);
        Task<IReadOnlyList<StockCategorySummaryDto>> GetStockByCategoryAsync();
    }
}
