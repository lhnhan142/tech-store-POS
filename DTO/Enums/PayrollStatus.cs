namespace Common.Enums
{
    /// <summary>
    /// Trạng thái của bảng lương
    /// </summary>
    public enum PayrollStatus
    {
        Draft = 0,      // Bản nháp (có thể tính lại, điều chỉnh)
        Finalized = 1   // Đã chốt (khóa dữ liệu, không được chỉnh sửa)
    }
}
