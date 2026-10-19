using System;

namespace PhuKienMayTinh.Common.Constants
{
    public static class AppConstants
    {
        public const string StoreName = "CỬA HÀNG PHỤ KIỆN MÁY TÍNH & GAMING GEAR";
        public const string StoreAddress = "Số 123 Đường Công Nghệ, Quận Cầu Giấy, Hà Nội";
        public const string StorePhone = "0988.888.999 - 024.3333.6666";
        public const string StoreEmail = "contact@gearcomputervn.com";

        // Giờ chuẩn các ca làm việc
        public static readonly TimeSpan MorningShiftStart = new TimeSpan(8, 0, 0);
        public static readonly TimeSpan MorningShiftEnd = new TimeSpan(12, 0, 0);

        public static readonly TimeSpan AfternoonShiftStart = new TimeSpan(13, 0, 0);
        public static readonly TimeSpan AfternoonShiftEnd = new TimeSpan(17, 0, 0);

        public static readonly TimeSpan EveningShiftStart = new TimeSpan(17, 30, 0);
        public static readonly TimeSpan EveningShiftEnd = new TimeSpan(21, 30, 0);

        public static readonly TimeSpan FullDayShiftStart = new TimeSpan(8, 0, 0);
        public static readonly TimeSpan FullDayShiftEnd = new TimeSpan(17, 0, 0);

        // Mặc định công và giờ
        public const int DefaultStandardWorkDays = 26;
        public const double DefaultStandardWorkHours = 8.0;

        // Phạt mặc định
        public const decimal DefaultLatePenaltyPerMinute = 2000m; // 2,000 VND / phút
        public const decimal DefaultAbsencePenalty = 200000m;       // 200,000 VND / buổi không phép
        public const decimal DefaultDiligenceBonus = 500000m;       // 500,000 VND chuyên cần
    }
}
