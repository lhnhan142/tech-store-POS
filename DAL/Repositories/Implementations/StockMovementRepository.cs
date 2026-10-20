using Common.Enums;
using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations
{
    public class StockMovementRepository : Repository<StockMovement>, IStockMovementRepository
    {
        public StockMovementRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<StockMovement>> GetByProductIdAsync(int productId)
        {
            return await _context.StockMovements
                .Include(sm => sm.CreatedByUser)
                .Where(sm => sm.ProductId == productId)
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetDefectiveStockQuantityAsync(int productId)
        {
            return await _context.StockMovements
                .Where(sm => sm.ProductId == productId && sm.MovementType == StockMovementType.ReturnDefect)
                .SumAsync(sm => (int?)sm.Quantity) ?? 0;
        }
    }
}
