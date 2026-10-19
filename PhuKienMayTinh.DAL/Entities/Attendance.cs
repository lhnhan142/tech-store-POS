using System;
using PhuKienMayTinh.Common.Enums;

namespace PhuKienMayTinh.DAL.Entities
{
    public class Attendance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        public DateTime WorkDate { get; set; } = DateTime.Today;
        public ShiftType Shift { get; set; } = ShiftType.FullDay;

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public double HoursWorked { get; set; } = 0.0;
        public int LateMinutes { get; set; } = 0;
        public int EarlyLeaveMinutes { get; set; } = 0;

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string? Note { get; set; }

        // Quản lý điều chỉnh
        public bool IsAdjusted { get; set; } = false;
        public string? AdjustedBy { get; set; }
        public DateTime? AdjustedAt { get; set; }
        public string? AdjustmentReason { get; set; }
    }
}
