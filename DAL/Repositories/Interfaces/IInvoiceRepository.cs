using Common.Enums;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        /// <summary>
        /// Lấy chi tiết hóa đơn kèm theo: Khách hàng, Người tạo, Các dòng sản phẩm (kèm Product),
        /// Các lần thanh toán, Các yêu cầu đổi trả liên quan.
        /// </summary>
        Task<Invoice?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Lấy hóa đơn theo số hóa đơn kèm đầy đủ chi tiết
        /// </summary>
        Task<Invoice?> GetByInvoiceNoWithDetailsAsync(string invoiceNo);

        /// <summary>
        /// Lấy hóa đơn theo OrderId của POS
        /// </summary>
        Task<Invoice?> GetByOrderIdWithDetailsAsync(int orderId);

        /// <summary>
        /// Tìm kiếm hóa đơn theo số hóa đơn / SĐT khách, lọc theo khoảng ngày, trạng thái và phân trang
        /// </summary>
        Task<(IReadOnlyList<Invoice> Items, int TotalCount)> SearchInvoicesAsync(
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate,
            InvoiceStatus? status,
            int pageIndex,
            int pageSize);
    }
}
