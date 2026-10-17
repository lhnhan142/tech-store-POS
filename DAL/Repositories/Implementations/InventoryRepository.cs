using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------- Lưu phiếu + cập nhật tồn kho
        // Mỗi phương thức mở một giao dịch: lưu phiếu và đổi tồn kho cùng thành công hoặc cùng bị hoàn tác.
        // Tồn kho được đổi bằng một câu UPDATE trực tiếp (StockQuantity = StockQuantity +/- n) thay vì đọc rồi
        // ghi lại, nên hai người lập phiếu cùng lúc không ghi đè số của nhau. Các dòng được xử lý theo thứ tự
        // ProductId để hai giao dịch không khóa chéo nhau (deadlock).

        public async Task AddReceiptAsync(InventoryReceipt receipt)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            _context.InventoryReceipts.Add(receipt);
            await _context.SaveChangesAsync();

            foreach (var d in (receipt.Details ?? new List<InventoryReceiptDetail>()).OrderBy(d => d.ProductId))
            {
                var productId = d.ProductId;
                var qty = d.Quantity;
                var affected = await _context.Products
                    .Where(p => p.Id == productId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity + qty));
                if (affected == 0)
                    throw new InvalidOperationException($"Sản phẩm #{productId} không còn tồn tại.");
            }

            await tx.CommitAsync();
        }

        public async Task<int?> TryAddIssueAsync(StockIssue issue)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            foreach (var d in (issue.Details ?? new List<StockIssueDetail>()).OrderBy(d => d.ProductId))
            {
                var productId = d.ProductId;
                var qty = d.Quantity;
                // Điều kiện StockQuantity >= qty nằm ngay trong câu UPDATE nên tồn kho không bao giờ bị âm
                var affected = await _context.Products
                    .Where(p => p.Id == productId && p.StockQuantity >= qty)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity - qty));
                if (affected == 0)
                {
                    await tx.RollbackAsync();
                    return productId;
                }
            }

            _context.StockIssues.Add(issue);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return null;
        }

        public async Task AddStockTakeAsync(StockTake take)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            var details = (take.Details ?? new List<StockTakeDetail>()).OrderBy(d => d.ProductId).ToList();
            var ids = details.Select(d => d.ProductId).ToList();
            var current = await _context.Products
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.StockQuantity);

            foreach (var d in details)
            {
                if (!current.TryGetValue(d.ProductId, out var systemQty))
                    throw new InvalidOperationException($"Sản phẩm #{d.ProductId} không còn tồn tại.");

                d.SystemQuantity = systemQty;   // tồn hệ thống ngay lúc chốt phiếu
                if (d.ActualQuantity != systemQty)
                {
                    var productId = d.ProductId;
                    var actual = d.ActualQuantity;
                    await _context.Products
                        .Where(p => p.Id == productId)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, actual));
                }
            }

            _context.StockTakes.Add(take);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task<IReadOnlyList<Product>> GetProductsByIdsAsync(IEnumerable<int> productIds)
        {
            var ids = productIds.Distinct().ToList();
            return await _context.Products
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();
        }

        // ------------------------------------------------------- Danh sách phiếu
        public async Task<IReadOnlyList<InventoryReceipt>> GetReceiptsAsync(DateTime from, DateTime to)
        {
            var (start, endExclusive) = Range(from, to);
            return await _context.InventoryReceipts
                .AsNoTracking()
                .Include(r => r.Supplier)
                .Include(r => r.User)
                .Where(r => r.ReceiptDate >= start && r.ReceiptDate < endExclusive)
                .OrderByDescending(r => r.ReceiptDate).ThenByDescending(r => r.Id)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<StockIssue>> GetIssuesAsync(DateTime from, DateTime to)
        {
            var (start, endExclusive) = Range(from, to);
            return await _context.StockIssues
                .AsNoTracking()
                .Include(i => i.User)
                .Include(i => i.Details)
                .Where(i => i.IssueDate >= start && i.IssueDate < endExclusive)
                .OrderByDescending(i => i.IssueDate).ThenByDescending(i => i.Id)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<StockTake>> GetStockTakesAsync(DateTime from, DateTime to)
        {
            var (start, endExclusive) = Range(from, to);
            return await _context.StockTakes
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Details)
                .Where(t => t.TakeDate >= start && t.TakeDate < endExclusive)
                .OrderByDescending(t => t.TakeDate).ThenByDescending(t => t.Id)
                .ToListAsync();
        }

        // ------------------------------------------------------ Chi tiết một phiếu
        public async Task<IReadOnlyList<InventoryReceiptDetail>> GetReceiptDetailsAsync(int receiptId)
        {
            return await _context.InventoryReceiptDetails
                .AsNoTracking()
                .Include(d => d.Product)
                .Where(d => d.ReceiptId == receiptId)
                .OrderBy(d => d.Id)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<StockIssueDetail>> GetIssueDetailsAsync(int issueId)
        {
            return await _context.StockIssueDetails
                .AsNoTracking()
                .Include(d => d.Product)
                .Where(d => d.IssueId == issueId)
                .OrderBy(d => d.Id)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<StockTakeDetail>> GetStockTakeDetailsAsync(int takeId)
        {
            return await _context.StockTakeDetails
                .AsNoTracking()
                .Include(d => d.Product)
                .Where(d => d.StockTakeId == takeId)
                .OrderBy(d => d.Id)
                .ToListAsync();
        }

        // 'to' tính trọn ngày: truyền 00:00 thì vẫn lấy hết phiếu của ngày đó (giống OrderRepository)
        private static (DateTime Start, DateTime EndExclusive) Range(DateTime from, DateTime to) =>
            (from.Date, to.Date.AddDays(1));
    }
}
