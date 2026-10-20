using Common.Enums;

namespace DAL.Entities
{
    /// <summary>
    /// Thực thể Hóa đơn (bán lẻ, đổi hàng, điều chỉnh).
    /// </summary>
    public class Invoice
    {
        public int Id { get; set; }

        /// <summary>Mã hóa đơn duy nhất (VD: HD-000123)</summary>
        public string InvoiceNo { get; set; } = string.Empty;

        /// <summary>Liên kết đơn hàng POS (nullable)</summary>
        public int? OrderId { get; set; }
        public Order? Order { get; set; }

        /// <summary>Khách hàng mua hàng (nullable với khách vãng lai)</summary>
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        /// <summary>Nhân viên tạo hóa đơn</summary>
        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        /// <summary>Trạng thái hóa đơn (Paid, Cancelled, PartiallyReturned, FullyReturned)</summary>
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;

        /// <summary>Loại hóa đơn (Sale, Exchange, Adjustment)</summary>
        public InvoiceType Type { get; set; } = InvoiceType.Sale;

        /// <summary>Hóa đơn gốc nếu đây là hóa đơn đổi hàng hoặc điều chỉnh</summary>
        public int? ParentInvoiceId { get; set; }
        public Invoice? ParentInvoice { get; set; }
        public ICollection<Invoice> ChildInvoices { get; set; } = new List<Invoice>();

        /// <summary>Tổng tiền trước giảm giá</summary>
        public decimal SubTotal { get; set; }

        /// <summary>Tổng tiền giảm giá toàn hóa đơn</summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>Tổng tiền thanh toán cuối cùng (đã gồm VAT và trừ chiết khấu)</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Lý do hủy hóa đơn (nếu có)</summary>
        public string? CancelReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public int? CancelledByUserId { get; set; }
        public User? CancelledByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>Khóa lạc quan xử lý xung đột đồng thời</summary>
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation collections
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();
        public ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
    }
}
