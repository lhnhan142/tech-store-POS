using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateTime date);
        Task<Attendance?> GetByUserAndDateAsync(int userId, DateTime date);
        Task<IEnumerable<Attendance>> GetHistoryAsync(DateTime fromDate, DateTime toDate, int? employeeId = null);
        Task<IEnumerable<Attendance>> GetAbnormalAttendancesAsync(DateTime fromDate, DateTime toDate);
    }
}
