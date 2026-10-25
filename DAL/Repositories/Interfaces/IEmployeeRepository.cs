using Common.Enums;
using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<Employee?> GetByCodeAsync(string code);
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);
        Task<Employee?> GetWithUserAsync(int id);
        Task<IEnumerable<Employee>> SearchAsync(string? keyword, string? position, EmployeeStatus? status);
        Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
    }
}
