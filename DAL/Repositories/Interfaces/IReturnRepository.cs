using Common.Enums;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IReturnRepository : IRepository<ReturnRequest>
    {
        /// <summary>
        /// Lấy chi tiết yêu cầu đổi trả kèm: Invoice (kèm Items, Customer), Items (kèm Product, InvoiceItem),
        /// Attachments, Refunds, và các User liên quan (Người tạo, Người duyệt, Thủ kho).
        /// </summary>
        Task<ReturnRequest?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Lấy yêu cầu đổi trả theo mã yêu cầu (VD: YC-0045)
        /// </summary>
        Task<ReturnRequest?> GetByRequestNoWithDetailsAsync(string requestNo);

        /// <summary>
        /// Lọc danh sách yêu cầu đổi trả theo trạng thái và phân trang
        /// </summary>
        Task<(IReadOnlyList<ReturnRequest> Items, int TotalCount)> SearchRequestsAsync(
            ReturnStatus? status,
            int pageIndex,
            int pageSize);

        /// <summary>
        /// Tính tổng số lượng của một dòng hàng đang bị giữ chỗ bởi các yêu cầu ở trạng thái Pending, Approved hoặc Received.
        /// </summary>
        Task<int> GetReservedQtyForInvoiceItemAsync(int invoiceItemId, int? excludeRequestId = null);

        /// <summary>
        /// Đếm số lượng yêu cầu theo từng trạng thái (để hiển thị badge trên tab)
        /// </summary>
        Task<int> CountByStatusAsync(ReturnStatus status);
    }
}
