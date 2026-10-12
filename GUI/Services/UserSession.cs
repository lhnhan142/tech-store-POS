using BLL.Services.Interfaces;

namespace GUI.Services
{
    /// <summary>
    /// Lưu trữ phiên người dùng hiện tại trong GUI, cài đặt ICurrentUserService của BLL.
    /// Đăng ký Singleton trong DI.
    /// Hỗ trợ chuyển đổi vai trò nhanh phục vụ kiểm thử phân quyền (DEBUG).
    /// </summary>
    public class UserSession : ICurrentUserService
    {
        public int UserId { get; private set; } = 1;
        public string FullName { get; private set; } = "Quản trị viên";
        public int RoleId { get; private set; } = 1; // 1: Admin, 2: Quản lý, 3: Bán hàng, 4: Kho

        public bool IsAdmin => RoleId == 1;
        public bool IsManager => RoleId == 2;
        public bool IsSeller => RoleId == 3;
        public bool IsWarehouse => RoleId == 4;

        public event Action? OnUserChanged;

        /// <summary>
        /// Được gọi khi đăng nhập thành công từ LoginForm trong tương lai:
        /// userSession.SetCurrentUser(user.Id, user.FullName, user.RoleId);
        /// </summary>
        public void SetCurrentUser(int userId, string fullName, int roleId)
        {
            UserId = userId;
            FullName = fullName;
            RoleId = roleId;
            OnUserChanged?.Invoke();
        }

#if DEBUG
        /// <summary>
        /// Chuyển đổi vai trò nhanh trên giao diện khi đang ở chế độ DEBUG.
        /// </summary>
        public void SwitchRoleForDebug(int roleId)
        {
            RoleId = roleId;
            (UserId, FullName) = roleId switch
            {
                1 => (1, "Admin Hệ Thống"),
                2 => (2, "Nguyễn Văn Quản Lý"),
                3 => (3, "Trần Thị Thu Ngân"),
                4 => (4, "Lê Văn Thủ Kho"),
                _ => (1, "Admin")
            };
            OnUserChanged?.Invoke();
        }
#endif
    }
}
