using Common.Enums;

namespace BLL.DTOs.Invoice
{
    /// <summary>
    /// DTO chi tiết đầy đủ của một hóa đơn — dùng cho form xem hóa đơn và form tạo đổi trả.
    /// </summary>
    public sealed class InvoiceDetailDto
    {
        public int Id { get; init; }
        public string InvoiceNo { get; init; } = string.Empty;
        public InvoiceStatus Status { get; init; }
        public InvoiceType Type { get; init; }
        public DateTime CreatedAt { get; init; }

        // Khách hàng
        public int? CustomerId { get; init; }
        public string? CustomerName { get; init; }
        public string? CustomerPhone { get; init; }

        // Người tạo
        public int CreatedByUserId { get; init; }
        public string CreatedByUserName { get; init; } = string.Empty;

        // Tiền
        public decimal SubTotal { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal TotalAmount { get; init; }

        // Hủy
        public string? CancelReason { get; init; }
        public DateTime? CancelledAt { get; init; }

        // Liên kết
        public int? OrderId { get; init; }
        public int? ParentInvoiceId { get; init; }

        // Chi tiết sản phẩm (kèm Available đã tính)
        public IReadOnlyList<InvoiceItemDetailDto> Items { get; init; } = [];

        // Thanh toán
        public IReadOnlyList<InvoicePaymentDto> Payments { get; init; } = [];

        /// <summary>
        /// Số ngày còn lại trong thời hạn đổi trả (âm = quá hạn).
        /// Được tính từ CreatedAt + ReturnWindowDays trong chính sách.
        /// </summary>
        public int DaysRemainingForReturn { get; init; }

        /// <summary>Tóm tắt trạng thái đổi trả (VD: "2 yêu cầu (1 đang xử lý)")</summary>
        public string ReturnSummary { get; init; } = string.Empty;
    }
}
