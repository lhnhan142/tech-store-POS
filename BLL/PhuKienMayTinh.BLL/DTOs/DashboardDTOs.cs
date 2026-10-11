using System;
using System.Collections.Generic;

namespace PhuKienMayTinh.BLL.DTOs
{
    public class DashboardSummaryDTO
    {
        // 1. Thẻ chỉ số (Stat Cards)
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal MonthRevenueGrowthPercent { get; set; } = 12.5m; // Tăng trưởng so với tháng trước

        public int TodayOrderCount { get; set; }
        public int MonthOrderCount { get; set; }

        public int ActiveEmployeeCount { get; set; }
        public int TodayPresentEmployeeCount { get; set; }

        public int TotalProductCount { get; set; }
        public int LowStockProductCount { get; set; }

        public decimal EstimatedPayrollFund { get; set; }

        // 2. Dữ liệu biểu đồ GDI+
        public List<DailyRevenuePointDTO> RevenuePoints { get; set; } = new List<DailyRevenuePointDTO>();
        public List<CategoryRevenueSliceDTO> CategorySlices { get; set; } = new List<CategoryRevenueSliceDTO>();
        public List<TopEmployeeSalesDTO> TopEmployees { get; set; } = new List<TopEmployeeSalesDTO>();

        // 3. Danh sách cảnh báo và giao dịch gần đây
        public List<LowStockProductDTO> LowStockAlerts { get; set; } = new List<LowStockProductDTO>();
        public List<RecentOrderDTO> RecentOrders { get; set; } = new List<RecentOrderDTO>();
    }

    public class DailyRevenuePointDTO
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public string Label => Date.ToString("dd/MM");
    }

    public class CategoryRevenueSliceDTO
    {
        public string Category { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public double Percentage { get; set; }
    }

    public class TopEmployeeSalesDTO
    {
        public string EmployeeName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class LowStockProductDTO
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int AlertThreshold { get; set; }
    }

    public class RecentOrderDTO
    {
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
    }
}
