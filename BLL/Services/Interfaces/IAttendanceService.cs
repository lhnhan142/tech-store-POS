using BLL.DTOs.Attendance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<IReadOnlyList<AttendanceRecordDto>> GetHistoryAsync(DateTime fromDate, DateTime toDate, int? employeeId = null);
        Task<IReadOnlyList<AttendanceRecordDto>> GetAbnormalRecordsAsync(DateTime fromDate, DateTime toDate);
        Task<AttendanceSummaryDto> GetSummaryAsync(DateTime fromDate, DateTime toDate);
        Task CheckInAsync(CheckInRequestDto dto);
        Task CheckOutAsync(CheckOutRequestDto dto);
        Task AdjustAttendanceAsync(AttendanceAdjustmentDto dto);
    }
}
