using Common.Enums;

namespace BLL.DTOs.Return
{
    /// <summary>
    /// DTO đầu vào từ GUI để tạo yêu cầu đổi trả.
    /// </summary>
    public sealed class CreateReturnRequestDto
    {
        /// <summary>Id hóa đơn gốc.</summary>
        public int InvoiceId { get; set; }

        /// <summary>Loại yêu cầu: Return (trả hoàn tiền) hoặc Exchange (đổi sản phẩm).</summary>
        public ReturnType Type { get; set; } = ReturnType.Return;

        /// <summary>Kênh tiếp nhận: Counter (tại quầy) hoặc Shipping (gửi về).</summary>
        public ReturnChannel Channel { get; set; } = ReturnChannel.Counter;

        /// <summary>Mã lý do đổi trả (VD: DEFECT, CHANGE_MIND, WRONG_ITEM).</summary>
        public string? ReasonCode { get; set; }

        /// <summary>Mô tả chi tiết lý do từ khách.</summary>
        public string? ReasonNote { get; set; }

        /// <summary>Danh sách dòng sản phẩm muốn trả.</summary>
        public IReadOnlyList<CreateReturnItemDto> Items { get; set; } = [];

        /// <summary>Đường dẫn file đính kèm (hình ảnh, video hàng lỗi).</summary>
        public IReadOnlyList<string> AttachmentPaths { get; set; } = [];
    }

    /// <summary>
    /// Một dòng trong yêu cầu đổi trả — ánh xạ tới InvoiceItem.
    /// </summary>
    public sealed class CreateReturnItemDto
    {
        /// <summary>Id của dòng hàng trong hóa đơn gốc.</summary>
        public int InvoiceItemId { get; set; }

        /// <summary>Số lượng muốn trả (phải ≤ Available của dòng đó).</summary>
        public int Qty { get; set; }
    }
}
