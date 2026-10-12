namespace BLL.Exceptions
{
    /// <summary>
    /// Mã lỗi nghiệp vụ để GUI xử lý phân nhánh (hiển thị thông báo, ẩn/bật nút, vv.)
    /// mà không cần phân tích chuỗi message.
    /// </summary>
    public enum BusinessErrorCode
    {
        /// <summary>Lỗi chung, không có mã cụ thể.</summary>
        General = 0,

        // ---- Hóa đơn ----
        /// <summary>Hóa đơn không hợp lệ để tạo yêu cầu (đã hủy, đã trả đủ).</summary>
        InvoiceNotEligible,

        // ---- Đổi trả ----
        /// <summary>Quá thời hạn đổi trả theo chính sách.</summary>
        ReturnWindowExpired,
        /// <summary>Số lượng yêu cầu trả vượt quá số lượng còn khả dụng.</summary>
        QtyExceedsReturnable,
        /// <summary>Sản phẩm không được phép đổi trả (IsReturnable = false).</summary>
        ItemNotReturnable,
        /// <summary>Tổng tiền hoàn vượt ngưỡng, cần Quản lý hoặc Admin duyệt.</summary>
        ApprovalRequired,

        // ---- Luồng trạng thái ----
        /// <summary>Chuyển trạng thái không hợp lệ (sai thứ tự).</summary>
        InvalidStatusTransition,

        // ---- Đồng thời / xung đột ----
        /// <summary>Dữ liệu đã bị thay đổi bởi người khác; cần tải lại.</summary>
        ConcurrencyConflict,

        // ---- Phân quyền ----
        /// <summary>Không có quyền thực hiện thao tác này.</summary>
        Forbidden,

        // ---- Kho ----
        /// <summary>Số lượng tồn kho không đủ để thực hiện.</summary>
        InsufficientStock,

        // ---- Không tìm thấy ----
        /// <summary>Không tìm thấy bản ghi yêu cầu.</summary>
        NotFound,
    }
}
