using BLL.DTOs.Return;
using Common.Enums;

namespace BLL.Services.Interfaces
{
    /// <summary>
    /// Dịch vụ xử lý yêu cầu đổi trả — toàn bộ luồng từ tạo đến hoàn tất.
    /// </summary>
    public interface IReturnService
    {
        // ---- Tạo yêu cầu ----

        /// <summary>
        /// Tạo yêu cầu đổi trả mới.
        /// Kiểm tra: hóa đơn hợp lệ, thời hạn, số lượng khả dụng, sản phẩm được phép trả.
        /// Trả về RequestNo (VD: YC-0045).
        /// </summary>
        Task<string> CreateRequestAsync(CreateReturnRequestDto dto);

        // ---- Duyệt / từ chối ----

        /// <summary>
        /// Duyệt yêu cầu đổi trả.
        /// - Counter + dưới ngưỡng: Nhân viên bán hàng được duyệt.
        /// - Quá ngưỡng hoặc kênh Shipping: chỉ Quản lý/Admin.
        /// - Counter dưới ngưỡng: gộp ngay Approved + Received (nhảy thẳng sang Received).
        /// </summary>
        Task ApproveAsync(int requestId);

        /// <summary>Từ chối yêu cầu (Quản lý/Admin). Trạng thái chuyển → Rejected.</summary>
        Task RejectAsync(int requestId, string reason);

        /// <summary>Hủy yêu cầu (người tạo hoặc Quản lý/Admin). Chỉ hủy được khi Pending.</summary>
        Task CancelAsync(int requestId);

        // ---- Nhận hàng (dành cho Nhân viên kho, kênh Shipping) ----

        /// <summary>
        /// Nhân viên kho xác nhận đã nhận hàng và cập nhật tình trạng từng sản phẩm.
        /// Chỉ áp dụng với kênh Shipping ở trạng thái Approved.
        /// Trạng thái chuyển → Received.
        /// </summary>
        Task ReceiveAsync(int requestId, IReadOnlyList<ReceiveReturnItemDto> items);

        // ---- Hoàn tất (trong 1 transaction) ----

        /// <summary>
        /// Hoàn tất yêu cầu — chạy trong ExecuteInTransactionAsync:
        /// 1. Tính lại Available, kiểm tra ràng buộc lần cuối.
        /// 2. Tăng InvoiceItem.ReturnedQty.
        /// 3. Ghi StockMovement (ReturnRestock/ReturnDefect).
        /// 4. Cộng Product.StockQuantity nếu Restock = true.
        /// 5. Tạo Refund records (chia theo InvoicePayment).
        /// 6. Nếu Type = Exchange, tạo Invoice mới (Type = Exchange, ParentInvoiceId).
        /// 7. Cập nhật Invoice.Status (PartiallyReturned / FullyReturned).
        /// 8. Đặt ReturnRequest.Status = Completed.
        /// 9. Ghi AuditLog.
        /// Bắt DbUpdateConcurrencyException → ném BusinessException(ConcurrencyConflict).
        /// </summary>
        Task CompleteAsync(int requestId);

        // ---- Đọc ----

        /// <summary>Lấy chi tiết yêu cầu đổi trả theo Id.</summary>
        Task<ReturnRequestDto> GetDetailAsync(int requestId);

        /// <summary>
        /// Tìm kiếm / lọc danh sách yêu cầu đổi trả.
        /// </summary>
        Task<(IReadOnlyList<ReturnRequestListItemDto> Items, int TotalCount)> SearchAsync(
            ReturnStatus? status,
            string? keyword,
            int pageIndex = 0,
            int pageSize = 20);

        /// <summary>Đếm số yêu cầu theo từng trạng thái — để hiển thị badge trên tab.</summary>
        Task<Dictionary<ReturnStatus, int>> GetStatusCountsAsync();

        // ---- Chính sách ----

        /// <summary>Lấy chính sách đổi trả hiện hành.</summary>
        Task<ReturnPolicyDto> GetPolicyAsync();

        /// <summary>Cập nhật chính sách đổi trả (chỉ Admin).</summary>
        Task UpdatePolicyAsync(ReturnPolicyDto dto);
    }
}
