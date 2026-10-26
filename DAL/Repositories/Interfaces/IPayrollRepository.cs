using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IPayrollRepository : IRepository<Payroll>
    {
        Task<Payroll?> GetByMonthYearWithDetailsAsync(int month, int year);
        Task<Payroll?> GetByIdWithDetailsAsync(int id);
        Task<bool> IsPeriodExistsAsync(int month, int year, int? excludeId = null);
        Task<IEnumerable<Payroll>> GetPayrollsByYearAsync(int year);
    }
}
