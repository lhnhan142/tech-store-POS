using System;
using PhuKienMayTinh.Common.Enums;

namespace PhuKienMayTinh.BLL.DTOs
{
    public class AttendanceDTO
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        public DateTime WorkDate { get; set; }
        public ShiftType Shift { get; set; }
        public string ShiftDisplay => Shift switch
        {
            ShiftType.Morning => "Ca Sáng (08:00 - 12:00)",
            ShiftType.Afternoon => "Ca Chiều (13:00 - 17:00)",
            ShiftType.Evening => "Ca Tối (17:30 - 21:30)",
            ShiftType.FullDay => "Hành chính (08:00 - 17:00)",
            _ => "Khác"
        };

        public DateTime? CheckInTime { get; set; }
        public string CheckInDisplay => CheckInTime.HasValue ? CheckInTime.Value.ToString("HH:mm:ss") : "--:--";

        public DateTime? CheckOutTime { get; set; }
        public string CheckOutDisplay => CheckOutTime.HasValue ? CheckOutTime.Value.ToString("HH:mm:ss") : "--:--";

        public double HoursWorked { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }

        public AttendanceStatus Status { get; set; }
        public string StatusDisplay => Status switch
        {
            AttendanceStatus.Present => "Đúng giờ",
            AttendanceStatus.Late => $"Đi trễ ({LateMinutes}p)",
            AttendanceStatus.EarlyLeave => $"Về sớm ({EarlyLeaveMinutes}p)",
            AttendanceStatus.LateAndEarlyLeave => $"Trễ & Về sớm",
            AttendanceStatus.ExcusedAbsence => "Nghỉ có phép",
            AttendanceStatus.UnexcusedAbsence => "Vắng không phép",
            _ => "Chưa xác định"
        };

        public string? Note { get; set; }
        public bool IsAdjusted { get; set; }
        public string? AdjustedBy { get; set; }
        public DateTime? AdjustedAt { get; set; }
        public string? AdjustmentReason { get; set; }
    }

    public class CheckInRequestDTO
    {
        public int EmployeeId { get; set; }
        public ShiftType Shift { get; set; } = ShiftType.FullDay;
        public DateTime CheckTime { get; set; } = DateTime.Now;
        public string? Note { get; set; }
    }

    public class CheckOutRequestDTO
    {
        public int EmployeeId { get; set; }
        public ShiftType Shift { get; set; } = ShiftType.FullDay;
        public DateTime CheckTime { get; set; } = DateTime.Now;
        public string? Note { get; set; }
    }

    public class AdjustAttendanceDTO
    {
        public int AttendanceId { get; set; }
        public DateTime? NewCheckInTime { get; set; }
        public DateTime? NewCheckOutTime { get; set; }
        public double NewHoursWorked { get; set; }
        public AttendanceStatus NewStatus { get; set; }
        public string AdjustedBy { get; set; } = "Quản lý";
        public string Reason { get; set; } = string.Empty;
    }
}
