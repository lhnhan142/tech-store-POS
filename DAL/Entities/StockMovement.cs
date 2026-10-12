using Common.Enums;

namespace DAL.Entities
{
    /// <summary>
    /// Sổ theo dõi biến động kho hàng (xuất bán, nhập trả nguyên vẹn, nhập trả hàng lỗi, xuất đổi...).
    /// </summary>
    public class StockMovement
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        /// <summary>Loại biến động: Sale, ReturnRestock, ReturnDefect, ExchangeOut, Purchase, Adjustment</summary>
        public StockMovementType MovementType { get; set; }

        /// <summary>Số lượng thay đổi (dương là tăng kho, âm là giảm kho)</summary>
        public int Quantity { get; set; }

        /// <summary>Đơn giá tại thời điểm phát sinh biến động</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Loại chứng từ tham chiếu (VD: Invoice, ReturnRequest, InventoryReceipt)</summary>
        public string ReferenceType { get; set; } = string.Empty;

        /// <summary>Id của chứng từ tham chiếu</summary>
        public int ReferenceId { get; set; }

        /// <summary>Ghi chú chi tiết</summary>
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
    }
}
