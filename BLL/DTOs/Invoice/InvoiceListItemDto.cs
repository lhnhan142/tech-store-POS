using Common.Enums;

namespace BLL.DTOs.Invoice
{
    /// <summary>
    /// DTO dùng cho danh sách hóa đơn (lưới/bảng tìm kiếm) — chỉ các trường cần hiển thị nhanh.
    /// </summary>
    public sealed record InvoiceListItemDto(
        int Id,
        string InvoiceNo,
        DateTime CreatedAt,
        string? CustomerName,
        string? CustomerPhone,
        decimal TotalAmount,
        InvoiceStatus Status,
        InvoiceType Type,
        /// <summary>Tóm tắt phương thức thanh toán VD: "Tiền mặt + Chuyển khoản"</summary>
        string PaymentSummary
    );
}
