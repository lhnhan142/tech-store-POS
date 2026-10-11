namespace PhuKienMayTinh.Common.Enums
{
    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        SalesStaff = 3,
        WarehouseStaff = 4
    }

    public enum EmployeeStatus
    {
        Active = 1,       // Đang làm việc
        OnLeave = 2,      // Tạm nghỉ / Nghỉ phép
        Resigned = 3      // Đã nghỉ việc
    }

    public enum ShiftType
    {
        Morning = 1,      // Ca sáng: 08:00 - 12:00
        Afternoon = 2,    // Ca chiều: 13:00 - 17:00
        Evening = 3,      // Ca tối: 17:30 - 21:30
        FullDay = 4       // Hành chính: 08:00 - 17:00 (nghỉ trưa 1h)
    }

    public enum AttendanceStatus
    {
        Present = 1,            // Đúng giờ
        Late = 2,               // Đi trễ
        EarlyLeave = 3,         // Về sớm
        LateAndEarlyLeave = 4,  // Đi trễ & Về sớm
        ExcusedAbsence = 5,     // Nghỉ có phép
        UnexcusedAbsence = 6    // Nghỉ không phép
    }

    public enum PayrollStatus
    {
        Draft = 1,     // Bản nháp / Dự thảo
        Approved = 2,  // Đã duyệt
        Paid = 3       // Đã thanh toán / Đã chi lương
    }
}
