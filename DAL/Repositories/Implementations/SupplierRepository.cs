using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
    public class SupplierRepository : Repository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var lower = name.ToLower();
            return await _dbSet.AnyAsync(s => s.Name.ToLower() == lower
                                              && (excludeId == null || s.Id != excludeId));
        }

        public async Task<bool> HasReceiptsAsync(int id)
        {
            return await _context.InventoryReceipts.AnyAsync(r => r.SupplierId == id);
        }
    }
}
