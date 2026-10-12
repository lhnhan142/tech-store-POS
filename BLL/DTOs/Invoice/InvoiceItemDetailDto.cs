namespace BLL.DTOs.Invoice
{
    /// <summary>
    /// DTO chi tiết một dòng sản phẩm trong hóa đơn, kèm số lượng khả dụng để đổi trả.
    /// </summary>
    public sealed record InvoiceItemDetailDto(
        int Id,
        int ProductId,
        string ProductName,
        int Qty,
        decimal UnitPrice,
        decimal DiscountAlloc,
        decimal LineTotal,
        int ReturnedQty,
        bool IsReturnable,
        /// <summary>
        /// Số lượng hiện còn có thể đổi trả = Qty − ReturnedQty − số lượng đang giữ chỗ (Pending/Approved/Received).
        /// Được tính trong BLL, không lưu DB.
        /// </summary>
        int Available
    );
}
