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
    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<Attendance?> GetTodayAttendanceAsync(int employeeId, DateTime date, ShiftType shift);
        Task<IEnumerable<Attendance>> GetAttendancesByFilterAsync(DateTime? fromDate, DateTime? toDate, int? employeeId, AttendanceStatus? status);
        Task<IEnumerable<Attendance>> GetMonthlyAttendancesAsync(int employeeId, int month, int year);
    }
}

namespace PhuKienMayTinh.DAL.Repositories.Implementations
{
    public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Attendance?> GetTodayAttendanceAsync(int employeeId, DateTime date, ShiftType shift)
        {
            var dateOnly = date.Date;
            return await _dbSet.Include(a => a.Employee)
                               .FirstOrDefaultAsync(a => a.EmployeeId == employeeId &&
                                                         a.WorkDate.Date == dateOnly &&
                                                         a.Shift == shift);
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByFilterAsync(DateTime? fromDate, DateTime? toDate, int? employeeId, AttendanceStatus? status)
        {
            var query = _dbSet.Include(a => a.Employee).AsQueryable();

            if (fromDate.HasValue)
            {
                var fDate = fromDate.Value.Date;
                query = query.Where(a => a.WorkDate.Date >= fDate);
            }

            if (toDate.HasValue)
            {
                var tDate = toDate.Value.Date;
                query = query.Where(a => a.WorkDate.Date <= tDate);
            }

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                query = query.Where(a => a.EmployeeId == employeeId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            return await query.OrderByDescending(a => a.WorkDate)
                              .ThenBy(a => a.Employee.EmployeeCode)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetMonthlyAttendancesAsync(int employeeId, int month, int year)
        {
            return await _dbSet.Where(a => a.EmployeeId == employeeId &&
                                           a.WorkDate.Month == month &&
                                           a.WorkDate.Year == year)
                               .ToListAsync();
        }
    }
}
