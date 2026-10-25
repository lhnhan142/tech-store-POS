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
    public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateTime date)
        {
            var targetDate = date.Date;
            return await _dbSet
                .Include(a => a.Employee)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == targetDate);
        }

        public async Task<Attendance?> GetByUserAndDateAsync(int userId, DateTime date)
        {
            var targetDate = date.Date;
            return await _dbSet
                .Include(a => a.Employee)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date.Date == targetDate);
        }

        public async Task<IEnumerable<Attendance>> GetHistoryAsync(DateTime fromDate, DateTime toDate, int? employeeId = null)
        {
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1).AddTicks(-1);

            var query = _dbSet
                .Include(a => a.Employee)
                .Include(a => a.User)
                .Where(a => a.Date >= start && a.Date <= end);

            if (employeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeId == employeeId.Value);
            }

            return await query.OrderByDescending(a => a.Date).ThenBy(a => a.CheckIn).ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAbnormalAttendancesAsync(DateTime fromDate, DateTime toDate)
        {
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1).AddTicks(-1);

            // Bất thường: Có check-in nhưng không check-out, hoặc đi muộn, hoặc về sớm
            return await _dbSet
                .Include(a => a.Employee)
                .Include(a => a.User)
                .Where(a => a.Date >= start && a.Date <= end)
                .Where(a => (a.CheckIn != null && a.CheckOut == null && a.Date.Date < DateTime.Today) ||
                            a.Status == Common.Enums.AttendanceStatus.Late ||
                            a.Status == Common.Enums.AttendanceStatus.EarlyLeave ||
                            a.Status == Common.Enums.AttendanceStatus.Absent)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }
    }
}
