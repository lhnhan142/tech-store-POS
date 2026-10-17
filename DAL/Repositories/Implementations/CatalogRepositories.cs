using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context) { }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var lower = name.ToLower();
            return await _dbSet.AnyAsync(c => c.Name.ToLower() == lower && (excludeId == null || c.Id != excludeId));
        }

        public async Task<Dictionary<int, int>> CountProductsAsync()
        {
            return await _context.Products
                .GroupBy(p => p.CategoryId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }
    }

    public class BrandRepository : Repository<Brand>, IBrandRepository
    {
        public BrandRepository(AppDbContext context) : base(context) { }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var lower = name.ToLower();
            return await _dbSet.AnyAsync(b => b.Name.ToLower() == lower && (excludeId == null || b.Id != excludeId));
        }

        public async Task<Dictionary<int, int>> CountProductsAsync()
        {
            return await _context.Products
                .Where(p => p.BrandId != null)
                .GroupBy(p => p.BrandId!.Value)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }
    }
}
