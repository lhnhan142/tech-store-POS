using System;
using System.Collections.Generic;

namespace PhuKienMayTinh.DAL.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Bàn phím, Chuột, Tai nghe, v.v.
        public string Brand { get; set; } = string.Empty;    // Logitech, Razer, Corsair, Akko
        public decimal CostPrice { get; set; }               // Giá nhập
        public decimal SellingPrice { get; set; }            // Giá bán
        public int StockQuantity { get; set; }               // Tồn kho
        public int MinimumStockAlert { get; set; } = 10;     // Cảnh báo hết hàng
        public string Unit { get; set; } = "Cái";
        public bool IsActive { get; set; } = true;

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

    public class Order
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = "Khách lẻ";
        public string CustomerPhone { get; set; } = "";
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }

        public string PaymentMethod { get; set; } = "Tiền mặt"; // Tiền mặt, Chuyển khoản, Thẻ
        public string Status { get; set; } = "Hoàn thành";

        public int? EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
