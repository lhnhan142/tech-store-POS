using BLL.DTOs.Payroll;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IPayrollService
    {
        Task<IReadOnlyList<PayrollSummaryDto>> GetPayrollsAsync(int year);
        Task<PayrollSummaryDto?> GetPayrollByIdAsync(int id);
        Task<IReadOnlyList<PayrollDetailDto>> GetPayrollDetailsAsync(int payrollId);
        Task<int> GeneratePayrollAsync(GeneratePayrollRequestDto dto, int currentUserId);
        Task RecalculatePayrollAsync(int payrollId);
        Task FinalizePayrollAsync(int payrollId, int finalizedByUserId);
        Task UpdateDetailAdjustmentsAsync(UpdateSalaryAdjustmentsDto dto);
        Task DeletePayrollAsync(int payrollId);
    }
}
