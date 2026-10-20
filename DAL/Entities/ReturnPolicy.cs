namespace DAL.Entities
{
    /// <summary>
    /// Chính sách đổi trả cấu hình được của cửa hàng.
    /// </summary>
    public class ReturnPolicy
    {
        public int Id { get; set; }

        /// <summary>Thời hạn cho phép đổi trả tính theo ngày (mặc định 7 ngày)</summary>
        public int ReturnWindowDays { get; set; } = 7;

        /// <summary>Ngưỡng tiền hoàn cần Quản lý duyệt (mặc định 5.000.000đ)</summary>
        public decimal ApprovalThresholdAmount { get; set; } = 5_000_000m;

        /// <summary>Đang kích hoạt</summary>
        public bool IsActive { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int? UpdatedByUserId { get; set; }
        public User? UpdatedByUser { get; set; }
    }
}
