using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Kiểm tra đăng nhập. Thiếu thông tin, sai tên/mật khẩu hoặc tài khoản bị khóa
        /// thì ném BusinessException (GUI hiển thị thẳng thông báo).
        /// </summary>
        Task<AuthUserDTO> LoginAsync(string username, string password);
    }
}