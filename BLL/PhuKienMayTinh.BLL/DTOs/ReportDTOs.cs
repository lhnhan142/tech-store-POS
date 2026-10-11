using System;

namespace PhuKienMayTinh.BLL.DTOs
{
    public class ReportFilterDTO
    {
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Today;
        public string ReportType { get; set; } = "DoanhThu"; // DoanhThu, NhanSu, ChamCong, BangLuong, TonKho
        public int? EmployeeId { get; set; }
        public string? Category { get; set; }
    }
}
