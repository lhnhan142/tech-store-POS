using Common.Enums;

namespace BLL.DTOs.Return
{
    /// <summary>
    /// Một dòng sản phẩm trong yêu cầu đổi trả (để xem chi tiết).
    /// </summary>
    public sealed record ReturnItemDto(
        int Id,
        int InvoiceItemId,
        int ProductId,
        string ProductName,
        int Qty,
        ItemCondition Condition,
        bool Restock,
        decimal RefundAmount
    );

    /// <summary>
    /// Thông tin hoàn tiền theo từng phương thức.
    /// </summary>
    public sealed record RefundDto(
        int Id,
        PaymentMethod Method,
        string MethodDisplayName,
        decimal Amount,
        DateTime RefundedAt,
        string ProcessedByUserName,
        string? Note
    );

    /// <summary>
    /// DTO chi tiết đầy đủ của một yêu cầu đổi trả.
    /// </summary>
    public sealed class ReturnRequestDto
    {
        public int Id { get; init; }
        public string RequestNo { get; init; } = string.Empty;

        // Hóa đơn gốc
        public int InvoiceId { get; init; }
        public string InvoiceNo { get; init; } = string.Empty;
        public string? CustomerName { get; init; }
        public string? CustomerPhone { get; init; }

        public ReturnType Type { get; init; }
        public ReturnChannel Channel { get; init; }
        public ReturnStatus Status { get; init; }

        public string? ReasonCode { get; init; }
        public string? ReasonNote { get; init; }

        /// <summary>Tổng tiền hoàn dự kiến (tổng RefundAmount của các dòng).</summary>
        public decimal TotalRefund { get; init; }

        // Người xử lý
        public int CreatedByUserId { get; init; }
        public string CreatedByUserName { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }

        public string? ApprovedByUserName { get; init; }
        public DateTime? ApprovedAt { get; init; }

        public string? ReceivedByUserName { get; init; }
        public DateTime? ReceivedAt { get; init; }

        public string? RejectReason { get; init; }

        // Hóa đơn đổi hàng (nếu Type = Exchange)
        public int? ExchangeInvoiceId { get; init; }
        public string? ExchangeInvoiceNo { get; init; }

        // Chi tiết
        public IReadOnlyList<ReturnItemDto> Items { get; init; } = [];
        public IReadOnlyList<string> AttachmentPaths { get; init; } = [];
        public IReadOnlyList<RefundDto> Refunds { get; init; } = [];
    }
}
