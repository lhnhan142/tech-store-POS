using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet.Where(p => p.CategoryId == categoryId).ToListAsync();
        }

        public async Task<Product> GetByBarcodeAsync(string barcode)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Barcode == barcode);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
        {
            return await _dbSet.Where(p => p.StockQuantity <= threshold).ToListAsync();
        }

        public async Task<List<Product>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null)
        {
            return await _dbSet.AnyAsync(p => p.Barcode == barcode && (excludeId == null || p.Id != excludeId));
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var lower = name.ToLower();
            return await _dbSet.AnyAsync(p => p.Name.ToLower() == lower && (excludeId == null || p.Id != excludeId));
        }

        public async Task<bool> HasTransactionsAsync(int id)
        {
            return await _context.OrderDetails.AnyAsync(d => d.ProductId == id)
                || await _context.InventoryReceiptDetails.AnyAsync(d => d.ProductId == id)
                || await _context.StockIssueDetails.AnyAsync(d => d.ProductId == id)
                || await _context.StockTakeDetails.AnyAsync(d => d.ProductId == id);
        }
    }
}
