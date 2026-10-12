namespace BLL.Services.Interfaces
{
    /// <summary>
    /// Cung cấp thông tin người dùng đang đăng nhập cho BLL.
    /// Được đăng ký bởi GUI layer (không phải AddBusinessLayer).
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>Id người dùng hiện tại.</summary>
        int UserId { get; }

        /// <summary>Họ tên đầy đủ của người dùng.</summary>
        string FullName { get; }

        /// <summary>RoleId: 1=Admin, 2=Quản lý, 3=Nhân viên bán hàng, 4=Nhân viên kho.</summary>
        int RoleId { get; }

        bool IsAdmin { get; }
        bool IsManager { get; }
        bool IsSeller { get; }
        bool IsWarehouse { get; }
    }
}
