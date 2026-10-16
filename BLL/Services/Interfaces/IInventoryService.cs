using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    /// <summary>Lập và tra cứu phiếu nhập kho, phiếu xuất kho, phiếu kiểm kê.</summary>
    public interface IInventoryService
    {
        // Danh sách sản phẩm (kèm tồn kho) cho ô chọn sản phẩm, sắp xếp theo tên
        Task<IReadOnlyList<ProductStockDTO>> GetProductsAsync();

        // Lập phiếu và TỰ ĐỘNG cập nhật tồn kho: nhập => cộng, xuất => trừ (không cho âm), kiểm kê => đặt bằng số thực tế.
        // Kết quả gồm Id phiếu và danh sách sản phẩm trong phiếu đang ở mức cảnh báo.
        Task<InventoryResultDTO> CreateReceiptAsync(int supplierId, int userId, DateTime receiptDate, IReadOnlyList<ReceiptLineInput> lines);
        Task<InventoryResultDTO> CreateIssueAsync(int userId, DateTime issueDate, string reason, string? note, IReadOnlyList<IssueLineInput> lines);
        Task<InventoryResultDTO> CreateStockTakeAsync(int userId, DateTime takeDate, string? note, IReadOnlyList<StockTakeLineInput> lines);

        // Lịch sử phiếu theo khoảng ngày (tính trọn ngày 'to')
        Task<IReadOnlyList<ReceiptSummaryDTO>> GetReceiptsAsync(DateTime from, DateTime to);
        Task<IReadOnlyList<IssueSummaryDTO>> GetIssuesAsync(DateTime from, DateTime to);
        Task<IReadOnlyList<StockTakeSummaryDTO>> GetStockTakesAsync(DateTime from, DateTime to);

        // Các dòng chi tiết của một phiếu
        Task<IReadOnlyList<ReceiptLineDTO>> GetReceiptLinesAsync(int receiptId);
        Task<IReadOnlyList<IssueLineDTO>> GetIssueLinesAsync(int issueId);
        Task<IReadOnlyList<StockTakeLineDTO>> GetStockTakeLinesAsync(int takeId);
    }
}
