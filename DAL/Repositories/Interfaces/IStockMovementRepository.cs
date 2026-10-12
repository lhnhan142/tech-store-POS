using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IStockMovementRepository : IRepository<StockMovement>
    {
        Task<IReadOnlyList<StockMovement>> GetByProductIdAsync(int productId);

        /// <summary>
        /// Tính tổng lượng hàng lỗi/hỏng đang lưu trong kho hàng lỗi dựa trên các biến động ReturnDefect
        /// </summary>
        Task<int> GetDefectiveStockQuantityAsync(int productId);
    }
}
