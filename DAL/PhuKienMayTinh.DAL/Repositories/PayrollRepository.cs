using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhuKienMayTinh.Common.Enums;
using PhuKienMayTinh.DAL.Context;
using PhuKienMayTinh.DAL.Entities;
using PhuKienMayTinh.DAL.Repositories.Implementations;
using PhuKienMayTinh.DAL.Repositories.Interfaces;

namespace PhuKienMayTinh.DAL.Repositories.Interfaces
{
    public interface IPayrollRepository : IRepository<Payroll>
    {
        Task<Payroll?> GetPayrollWithDetailsAsync(int payrollId);
        Task<Payroll?> GetPayrollByMonthYearAsync(int month, int year);
        Task<IEnumerable<Payroll>> GetPayrollsAsync(int? year);
        Task<SalaryConfiguration?> GetActiveSalaryConfigAsync();
    }
}

namespace PhuKienMayTinh.DAL.Repositories.Implementations
{
    public class PayrollRepository : Repository<Payroll>, IPayrollRepository
    {
        public PayrollRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Payroll?> GetPayrollWithDetailsAsync(int payrollId)
        {
            return await _dbSet.Include(p => p.Details)
                               .ThenInclude(d => d.Employee)
                               .FirstOrDefaultAsync(p => p.Id == payrollId);
        }

        public async Task<Payroll?> GetPayrollByMonthYearAsync(int month, int year)
        {
            return await _dbSet.Include(p => p.Details)
                               .ThenInclude(d => d.Employee)
                               .FirstOrDefaultAsync(p => p.Month == month && p.Year == year);
        }

        public async Task<IEnumerable<Payroll>> GetPayrollsAsync(int? year)
        {
            var query = _dbSet.AsQueryable();
            if (year.HasValue)
            {
                query = query.Where(p => p.Year == year.Value);
            }
            return await query.OrderByDescending(p => p.Year)
                              .ThenByDescending(p => p.Month)
                              .ToListAsync();
        }

        public async Task<SalaryConfiguration?> GetActiveSalaryConfigAsync()
        {
            return await _context.SalaryConfigurations.FirstOrDefaultAsync(s => s.IsActive);
        }
    }
}
