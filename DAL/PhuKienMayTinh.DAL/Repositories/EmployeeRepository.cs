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
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<Employee?> GetByCodeAsync(string employeeCode);
        Task<IEnumerable<Employee>> SearchEmployeesAsync(string? keyword, string? department, EmployeeStatus? status);
        Task<string> GenerateNextEmployeeCodeAsync();
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);
        Task<bool> IsIdentityCardExistsAsync(string identityCard, int? excludeId = null);
    }
}

namespace PhuKienMayTinh.DAL.Repositories.Implementations
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Employee?> GetByCodeAsync(string employeeCode)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
        }

        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string? keyword, string? department, EmployeeStatus? status)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(e => e.FullName.ToLower().Contains(keyword) ||
                                         e.EmployeeCode.ToLower().Contains(keyword) ||
                                         e.PhoneNumber.Contains(keyword) ||
                                         e.IdentityCard.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(department) && department != "Tất cả")
            {
                query = query.Where(e => e.Department == department);
            }

            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            return await query.OrderBy(e => e.EmployeeCode).ToListAsync();
        }

        public async Task<string> GenerateNextEmployeeCodeAsync()
        {
            var codes = await _dbSet.Select(e => e.EmployeeCode).ToListAsync();
            int maxId = 0;
            foreach (var code in codes)
            {
                if (code.StartsWith("NV") && int.TryParse(code.Substring(2), out int num))
                {
                    if (num > maxId) maxId = num;
                }
            }
            return $"NV{(maxId + 1):D3}";
        }

        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            return await _dbSet.AnyAsync(e => e.EmployeeCode == code && (!excludeId.HasValue || e.Id != excludeId.Value));
        }

        public async Task<bool> IsIdentityCardExistsAsync(string identityCard, int? excludeId = null)
        {
            return await _dbSet.AnyAsync(e => e.IdentityCard == identityCard && (!excludeId.HasValue || e.Id != excludeId.Value));
        }
    }
}
