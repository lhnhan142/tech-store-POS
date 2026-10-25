using Common.Enums;
using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Employee?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeCode == code);
        }

        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _dbSet.AsQueryable();
            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }
            return await query.AnyAsync(e => e.EmployeeCode == code);
        }

        public async Task<Employee?> GetWithUserAsync(int id)
        {
            return await _dbSet
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> SearchAsync(string? keyword, string? position, EmployeeStatus? status)
        {
            var query = _dbSet.Include(e => e.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.EmployeeCode.ToLower().Contains(kw) ||
                    e.FullName.ToLower().Contains(kw) ||
                    (e.PhoneNumber != null && e.PhoneNumber.Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(position))
            {
                query = query.Where(e => e.Position == position);
            }

            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            return await query.OrderBy(e => e.EmployeeCode).ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
        {
            return await _dbSet
                .Where(e => e.Status == EmployeeStatus.Active)
                .OrderBy(e => e.EmployeeCode)
                .ToListAsync();
        }
    }
}
