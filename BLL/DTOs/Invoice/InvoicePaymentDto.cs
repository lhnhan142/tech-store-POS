using Common.Enums;

namespace BLL.DTOs.Invoice
{
    /// <summary>
    /// DTO thanh toán trên hóa đơn (một dòng = một phương thức thanh toán).
    /// </summary>
    public sealed record InvoicePaymentDto(
        int Id,
        PaymentMethod Method,
        /// <summary>Tên phương thức hiển thị tiếng Việt</summary>
        string MethodDisplayName,
        decimal Amount,
        string? TransactionCode,
        DateTime PaidAt,
        string? Note
    );
}
