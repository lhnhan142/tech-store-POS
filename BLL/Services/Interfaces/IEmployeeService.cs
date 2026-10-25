using BLL.DTOs.Employee;
using Common.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IReadOnlyList<EmployeeListItemDto>> GetAllAsync(string? keyword = null, string? position = null, EmployeeStatus? status = null);
        Task<EmployeeDetailDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateEmployeeDto dto);
        Task UpdateAsync(UpdateEmployeeDto dto);
        Task UpdateStatusAsync(int id, EmployeeStatus status);
        Task<IReadOnlyList<string>> GetPositionsAsync();
    }
}
