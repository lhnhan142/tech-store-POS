using Common.Enums;

namespace BLL.DTOs.Return
{
    /// <summary>
    /// Input từ nhân viên kho khi xác nhận tình trạng hàng nhận được (bước Received).
    /// </summary>
    public sealed class ReceiveReturnItemDto
    {
        /// <summary>Id của ReturnItem cần cập nhật tình trạng.</summary>
        public int ReturnItemId { get; set; }

        /// <summary>Tình trạng thực tế của hàng nhận được.</summary>
        public ItemCondition Condition { get; set; } = ItemCondition.Good;

        /// <summary>
        /// Có nhập lại kho bán không (chỉ true khi Condition = Good).
        /// BLL sẽ ép false nếu hàng lỗi/hỏng.
        /// </summary>
        public bool Restock { get; set; } = true;
    }
}
