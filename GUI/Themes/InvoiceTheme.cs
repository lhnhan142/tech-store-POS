using System.Drawing;

namespace GUI.Themes
{
    /// <summary>
    /// Bảng màu và quy chuẩn giao diện riêng cho module Hóa đơn & Đổi trả.
    /// Giữ nguyên Theme.cs chung của hệ thống.
    /// </summary>
    public static class InvoiceTheme
    {
        // Sidebar & Header (Đồng bộ màu xanh ngọc #0b6651 với nút Tìm kiếm/Primary)
        public static readonly Color SidebarDark = Color.FromArgb(11, 102, 81);       // Xanh ngọc (#0b6651)
        public static readonly Color HeaderBg = Color.FromArgb(248, 250, 252);        // Nền thanh công cụ/header
        public static readonly Color FormBg = Color.FromArgb(243, 244, 246);          // Nền form (#f3f4f6)
        public static readonly Color CardBg = Color.White;

        // Nút bấm & hành động chính
        public static readonly Color Primary = Color.FromArgb(11, 102, 81);           // Xanh ngọc (#0b6651)
        public static readonly Color PrimaryHover = Color.FromArgb(8, 80, 64);
        public static readonly Color Secondary = Color.FromArgb(229, 231, 235);       // Nút phụ/hủy
        public static readonly Color SecondaryText = Color.FromArgb(55, 65, 81);
        public static readonly Color Danger = Color.FromArgb(220, 38, 38);            // Nút đỏ từ chối/hủy (#dc2626)

        // Phông chữ chuẩn
        public static readonly Font HeaderFont = new("Segoe UI", 13F, FontStyle.Bold);
        public static readonly Font SubHeaderFont = new("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font BodyFont = new("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font BodyBoldFont = new("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font SmallFont = new("Segoe UI", 9F, FontStyle.Regular);

        // Chip trạng thái (dạng viên thuốc)
        // 1. Đã thanh toán (Paid)
        public static readonly Color StatusPaidBg = Color.FromArgb(209, 250, 229);    // #d1fae5
        public static readonly Color StatusPaidText = Color.FromArgb(6, 95, 70);       // #065f46

        // 2. Trả một phần (PartiallyReturned)
        public static readonly Color StatusPartialBg = Color.FromArgb(219, 234, 254); // #dbeafe
        public static readonly Color StatusPartialText = Color.FromArgb(30, 64, 175);  // #1e40af

        // 3. Đã trả hết (FullyReturned)
        public static readonly Color StatusFullBg = Color.FromArgb(243, 244, 246);    // #f3f4f6
        public static readonly Color StatusFullText = Color.FromArgb(55, 65, 81);      // #374151

        // 4. Đã hủy (Cancelled / Rejected)
        public static readonly Color StatusCancelledBg = Color.FromArgb(254, 226, 226); // #fee2e2
        public static readonly Color StatusCancelledText = Color.FromArgb(153, 27, 27);  // #991b1b

        // 5. Chờ duyệt (Pending)
        public static readonly Color StatusPendingBg = Color.FromArgb(254, 243, 199); // #fef3c7
        public static readonly Color StatusPendingText = Color.FromArgb(146, 64, 14);  // #92400e

        // 6. Đã duyệt / Đã nhận (Approved / Received)
        public static readonly Color StatusApprovedBg = Color.FromArgb(224, 231, 255); // tím nhạt
        public static readonly Color StatusApprovedText = Color.FromArgb(67, 56, 202);

        /// <summary>
        /// Lấy cặp màu (Background, Text) cho trạng thái hóa đơn / yêu cầu đổi trả.
        /// </summary>
        public static (Color Bg, Color Text) GetStatusColors(string status) => status switch
        {
            "Paid" or "Completed" => (StatusPaidBg, StatusPaidText),
            "PartiallyReturned" => (StatusPartialBg, StatusPartialText),
            "FullyReturned" => (StatusFullBg, StatusFullText),
            "Cancelled" or "Rejected" => (StatusCancelledBg, StatusCancelledText),
            "Pending" => (StatusPendingBg, StatusPendingText),
            "Approved" or "Received" => (StatusApprovedBg, StatusApprovedText),
            _ => (StatusFullBg, StatusFullText)
        };
    }
}
