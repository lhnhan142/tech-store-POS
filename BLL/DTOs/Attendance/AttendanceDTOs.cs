using Common.Enums;
using System;

namespace BLL.DTOs.Attendance
{
    public class AttendanceRecordDto
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal OvertimeHours { get; set; }
        public AttendanceStatus Status { get; set; }
        public string StatusText => Status switch
        {
            AttendanceStatus.Present => "Đúng giờ",
            AttendanceStatus.Late => "Đi muộn",
            AttendanceStatus.EarlyLeave => "Về sớm",
            AttendanceStatus.Overtime => "Tăng ca",
            AttendanceStatus.Absent => "Vắng mặt",
            _ => "Khác"
        };
        public string? Note { get; set; }
    }

    public class CheckInRequestDto
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public TimeSpan CheckInTime { get; set; } = DateTime.Now.TimeOfDay;
        public string? Note { get; set; }
    }

    public class CheckOutRequestDto
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public TimeSpan CheckOutTime { get; set; } = DateTime.Now.TimeOfDay;
        public string? Note { get; set; }
    }

    public class AttendanceAdjustmentDto
    {
        public int Id { get; set; }
        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int TotalPresent { get; set; }
        public int TotalLate { get; set; }
        public int TotalEarlyLeave { get; set; }
        public int TotalOvertime { get; set; }
        public int TotalAbsent { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal TotalOvertimeHours { get; set; }
    }
}
