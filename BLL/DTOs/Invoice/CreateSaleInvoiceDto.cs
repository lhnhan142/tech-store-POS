using Common.Enums;

namespace BLL.DTOs.Invoice
{
    /// <summary>
    /// DTO tạo hóa đơn bán lẻ trực tiếp từ màn hình POS.
    /// </summary>
    public sealed record CreateSaleInvoiceDto(
        int? CustomerId,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal TotalAmount,
        PaymentMethod PaymentMethod,
        IReadOnlyList<CreateSaleInvoiceItemDto> Items
    );

    /// <summary>
    /// Chi tiết từng món trong hóa đơn bán lẻ.
    /// </summary>
    public sealed record CreateSaleInvoiceItemDto(
        int ProductId,
        string ProductName,
        int Qty,
        decimal UnitPrice,
        decimal DiscountAlloc,
        decimal LineTotal
    );
}
