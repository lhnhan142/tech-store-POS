using BLL.Exceptions;

namespace GUI
{
    /// <summary>Tên các quyền, phải khớp bảng Roles trong CSDL.</summary>
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Manager = "Quản lý";
        public const string Sales = "Nhân viên bán hàng";
        public const string Warehouse = "Nhân viên kho";
    }

    /// <summary>Thông tin phiên làm việc hiện tại (người dùng đang đăng nhập).</summary>
    public static class AppSession
    {
        public static int? CurrentUserId { get; set; }
        public static string? CurrentFullName { get; set; }
        public static string? CurrentRole { get; set; }

        public static bool IsLoggedIn => CurrentUserId.HasValue;

        /// <summary>LoginForm gọi khi đăng nhập thành công.</summary>
        public static void SignIn(int userId, string fullName, string role)
        {
            CurrentUserId = userId;
            CurrentFullName = fullName;
            CurrentRole = role;
        }

        /// <summary>MainForm gọi khi bấm Đăng xuất.</summary>
        public static void SignOut()
        {
            CurrentUserId = null;
            CurrentFullName = null;
            CurrentRole = null;
        }

        public static int RequireUserId() =>
            CurrentUserId ?? throw new BusinessException("Chưa đăng nhập. Vui lòng đăng nhập để lập phiếu.");
    }
}