using BLL.DTOs.Invoice;
using Common.Enums;

namespace BLL.Services.Interfaces
{
    /// <summary>
    /// Dịch vụ quản lý hóa đơn — tìm kiếm, xem chi tiết, hủy hóa đơn.
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Tìm kiếm hóa đơn theo từ khóa (số hóa đơn, SĐT khách), khoảng ngày, trạng thái và phân trang.
        /// </summary>
        Task<(IReadOnlyList<InvoiceListItemDto> Items, int TotalCount)> SearchAsync(
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate,
            InvoiceStatus? status,
            int pageIndex = 0,
            int pageSize = 20);

        /// <summary>Lấy chi tiết hóa đơn theo Id, kèm Available từng dòng đã tính.</summary>
        Task<InvoiceDetailDto> GetDetailAsync(int invoiceId);

        /// <summary>Lấy chi tiết hóa đơn theo số hóa đơn.</summary>
        Task<InvoiceDetailDto> GetDetailByNoAsync(string invoiceNo);

        /// <summary>
        /// Hủy hóa đơn (chỉ được hủy khi Status = Paid và chưa có yêu cầu đổi trả đang xử lý).
        /// Quản lý và Admin mới được hủy.
        /// </summary>
        Task CancelAsync(int invoiceId, string reason);

        /// <summary>
        /// Tạo hóa đơn bán lẻ trực tiếp từ màn hình POS và lưu vào CSDL.
        /// Sinh mã HD-xxxxxx duy nhất, trừ kho và ghi biến động kho StockMovement.
        /// </summary>
        Task<string> CreateSaleInvoiceAsync(CreateSaleInvoiceDto dto);
    }
}
