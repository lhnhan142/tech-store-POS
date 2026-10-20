namespace DAL.Entities
{
    /// <summary>
    /// Dòng chi tiết sản phẩm trong hóa đơn.
    /// </summary>
    public class InvoiceItem
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        /// <summary>Chụp lại tên sản phẩm lúc xuất hóa đơn để bảo toàn lịch sử</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>Số lượng mua</summary>
        public int Qty { get; set; }

        /// <summary>Đơn giá bán đã bao gồm thuế VAT</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Chiết khấu/khuyến mãi phân bổ riêng cho dòng này</summary>
        public decimal DiscountAlloc { get; set; }

        /// <summary>Thành tiền dòng = UnitPrice * Qty - DiscountAlloc</summary>
        public decimal LineTotal { get; set; }

        /// <summary>Số lượng đã hoàn trả thành công (ràng buộc ReturnedQty &lt;= Qty)</summary>
        public int ReturnedQty { get; set; } = 0;

        /// <summary>Mặt hàng này có được phép đổi trả hay không (mặc định true)</summary>
        public bool IsReturnable { get; set; } = true;

        /// <summary>Khóa lạc quan xử lý xung đột đồng thời khi hoàn trả</summary>
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();
    }
}
