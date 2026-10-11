using System;

namespace PhuKienMayTinh.DAL.Entities
{
    public class SalaryConfiguration
    {
        public int Id { get; set; }
        public string ConfigName { get; set; } = "Quy định tính lương chuẩn 2026";
        public int StandardWorkDays { get; set; } = 26; // Ngày công chuẩn trong tháng
        public double StandardWorkHoursPerDay { get; set; } = 8.0;

        public decimal OvertimeMultiplier { get; set; } = 1.5m; // Hệ số làm thêm giờ
        public decimal LatePenaltyPerMinute { get; set; } = 2000m; // 2,000 đ/phút trễ
        public decimal AbsencePenaltyPerDay { get; set; } = 200000m; // 200,000 đ/buổi vắng không phép
        public decimal DiligenceBonus { get; set; } = 500000m; // Thưởng chuyên cần nếu không trễ
        public decimal TaxExemptionLevel { get; set; } = 11000000m; // Giảm trừ gia cảnh bản thân

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
