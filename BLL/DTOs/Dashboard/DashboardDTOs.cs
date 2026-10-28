using System;
using System.Collections.Generic;

namespace BLL.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }           // Tổng doanh thu thực
        public int TotalOrders { get; set; }                // Tổng số đơn hàng / hóa đơn
        public int CompletedOrders { get; set; }            // Số đơn hoàn thành / đã thanh toán
        public int ActiveEmployees { get; set; }            // Số nhân viên đang làm việc
        public int TotalProducts { get; set; }              // Tổng số loại sản phẩm
        public int TotalStockQuantity { get; set; }         // Tổng số lượng hàng tồn kho
        public int LowStockProductsCount { get; set; }      // Số sản phẩm sắp hết hàng
        public decimal MonthlyPayrollExpense { get; set; }  // Tổng chi phí lương tháng hiện tại (nếu có)
    }

    public class RevenueChartPointDto
    {
        public string Label { get; set; } = string.Empty; // Ngày "15/10" hoặc Tháng "T10/2026"
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopSellingProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class StockCategorySummaryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalQuantity { get; set; }
    }
}
