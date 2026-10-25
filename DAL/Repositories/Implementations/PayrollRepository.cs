using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
    public class PayrollRepository : Repository<Payroll>, IPayrollRepository
    {
        public PayrollRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Payroll?> GetByMonthYearWithDetailsAsync(int month, int year)
        {
            return await _dbSet
                .Include(p => p.CreatedByUser)
                .Include(p => p.FinalizedByUser)
                .Include(p => p.Details)
                    .ThenInclude(d => d.Employee)
                .FirstOrDefaultAsync(p => p.Month == month && p.Year == year);
        }

        public async Task<Payroll?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.CreatedByUser)
                .Include(p => p.FinalizedByUser)
                .Include(p => p.Details)
                    .ThenInclude(d => d.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> IsPeriodExistsAsync(int month, int year, int? excludeId = null)
        {
            var query = _dbSet.AsQueryable();
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            return await query.AnyAsync(p => p.Month == month && p.Year == year);
        }

        public async Task<IEnumerable<Payroll>> GetPayrollsByYearAsync(int year)
        {
            return await _dbSet
                .Include(p => p.CreatedByUser)
                .Where(p => p.Year == year)
                .OrderByDescending(p => p.Month)
                .ToListAsync();
        }
    }
}
