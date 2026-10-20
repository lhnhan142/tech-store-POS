using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    /// <summary>Phiếu nhập kho, phiếu xuất kho và phiếu kiểm kê.</summary>
    public interface IInventoryRepository
    {
        // ---- Lưu phiếu + TỰ ĐỘNG cập nhật tồn kho trong cùng một giao dịch (lỗi thì hoàn tác hết) ----

        // Nhập kho: cộng số lượng vào tồn kho của từng sản phẩm
        Task AddReceiptAsync(InventoryReceipt receipt);

        // Xuất kho: trừ tồn kho. Nếu có sản phẩm không đủ hàng thì hoàn tác, KHÔNG lưu phiếu
        // và trả về Id sản phẩm đầu tiên thiếu hàng; thành công thì trả về null
        Task<int?> TryAddIssueAsync(StockIssue issue);

        // Kiểm kê: ghi nhận tồn hệ thống tại thời điểm lưu và đặt lại tồn kho bằng số thực tế
        Task AddStockTakeAsync(StockTake take);

        // Đọc sản phẩm không theo dõi (AsNoTracking) để luôn có tồn kho mới nhất
        Task<IReadOnlyList<Product>> GetProductsByIdsAsync(IEnumerable<int> productIds);

        // ---- Danh sách phiếu theo khoảng ngày (tính trọn ngày 'to'), mới nhất trước ----
        Task<IReadOnlyList<InventoryReceipt>> GetReceiptsAsync(DateTime from, DateTime to);
        Task<IReadOnlyList<StockIssue>> GetIssuesAsync(DateTime from, DateTime to);
        Task<IReadOnlyList<StockTake>> GetStockTakesAsync(DateTime from, DateTime to);

        // ---- Chi tiết của một phiếu (đã kèm sản phẩm) ----
        Task<IReadOnlyList<InventoryReceiptDetail>> GetReceiptDetailsAsync(int receiptId);
        Task<IReadOnlyList<StockIssueDetail>> GetIssueDetailsAsync(int issueId);
        Task<IReadOnlyList<StockTakeDetail>> GetStockTakeDetailsAsync(int takeId);
    }
}
