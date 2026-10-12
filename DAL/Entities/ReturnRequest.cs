using Common.Enums;

namespace DAL.Entities
{
    /// <summary>
    /// Thực thể Yêu cầu đổi trả hàng.
    /// </summary>
    public class ReturnRequest
    {
        public int Id { get; set; }

        /// <summary>Mã yêu cầu duy nhất (VD: YC-0045)</summary>
        public string RequestNo { get; set; } = string.Empty;

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        /// <summary>Loại yêu cầu: Return (Trả hàng hoàn tiền) hoặc Exchange (Đổi sản phẩm)</summary>
        public ReturnType Type { get; set; } = ReturnType.Return;

        /// <summary>Kênh tiếp nhận: Counter (Tại quầy) hoặc Shipping (Gửi hàng/Vận chuyển)</summary>
        public ReturnChannel Channel { get; set; } = ReturnChannel.Counter;

        /// <summary>Trạng thái yêu cầu: Pending, Approved, Received, Completed, Rejected, Cancelled</summary>
        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;

        /// <summary>Mã lý do đổi trả (VD: DEFECT, CHANGE_MIND, WRONG_ITEM)</summary>
        public string? ReasonCode { get; set; }

        /// <summary>Mô tả chi tiết lý do từ khách hàng</summary>
        public string? ReasonNote { get; set; }

        /// <summary>Tổng tiền hoàn trả dự kiến</summary>
        public decimal TotalRefund { get; set; }

        /// <summary>Hóa đơn đổi hàng sinh ra nếu là đổi hàng</summary>
        public int? ExchangeInvoiceId { get; set; }
        public Invoice? ExchangeInvoice { get; set; }

        /// <summary>Nhân viên tạo yêu cầu</summary>
        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        /// <summary>Người phê duyệt (Quản lý/Admin hoặc NV bán hàng nếu tại quầy &lt; ngưỡng)</summary>
        public int? ApprovedByUserId { get; set; }
        public User? ApprovedByUser { get; set; }
        public DateTime? ApprovedAt { get; set; }

        /// <summary>Thủ kho xác nhận đã nhận hàng (với kênh Shipping)</summary>
        public int? ReceivedByUserId { get; set; }
        public User? ReceivedByUser { get; set; }
        public DateTime? ReceivedAt { get; set; }

        /// <summary>Lý do từ chối yêu cầu (nếu bị Rejected)</summary>
        public string? RejectReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>Khóa lạc quan xử lý xung đột đồng thời</summary>
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation collections
        public ICollection<ReturnItem> Items { get; set; } = new List<ReturnItem>();
        public ICollection<ReturnAttachment> Attachments { get; set; } = new List<ReturnAttachment>();
        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}
