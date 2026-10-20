using Common.Enums;

namespace BLL.DTOs.Return
{
    /// <summary>
    /// DTO hiển thị danh sách yêu cầu đổi trả (lưới/tab).
    /// </summary>
    public sealed record ReturnRequestListItemDto(
        int Id,
        string RequestNo,
        string InvoiceNo,
        string? CustomerName,
        ReturnType Type,
        ReturnChannel Channel,
        ReturnStatus Status,
        decimal TotalRefund,
        DateTime CreatedAt,
        string CreatedByUserName,
        string? ApprovedByUserName
    );
}
