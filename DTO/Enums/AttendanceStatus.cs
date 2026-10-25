namespace Common.Enums
{
    /// <summary>
    /// Trạng thái ghi nhận chấm công
    /// </summary>
    public enum AttendanceStatus
    {
        Present = 0,     // Đi làm đúng giờ
        Late = 1,        // Đi muộn
        EarlyLeave = 2,  // Về sớm
        Overtime = 3,    // Có tăng ca
        Absent = 4       // Vắng mặt
    }
}
