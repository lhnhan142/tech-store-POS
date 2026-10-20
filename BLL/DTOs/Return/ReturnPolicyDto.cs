namespace BLL.DTOs.Return
{
    /// <summary>
    /// DTO chính sách đổi trả — dùng để đọc và cập nhật cấu hình.
    /// </summary>
    public sealed class ReturnPolicyDto
    {
        public int Id { get; init; }

        /// <summary>Thời hạn đổi trả tính theo ngày.</summary>
        public int ReturnWindowDays { get; set; }

        /// <summary>Ngưỡng tiền hoàn (VNĐ) cần Quản lý/Admin duyệt.</summary>
        public decimal ApprovalThresholdAmount { get; set; }

        public bool IsActive { get; set; }
        public DateTime UpdatedAt { get; init; }
        public string? UpdatedByUserName { get; init; }
    }
}
