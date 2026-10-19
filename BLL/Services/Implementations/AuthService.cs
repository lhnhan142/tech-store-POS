namespace BLL.DTOs
{
    /// <summary>Thông tin người dùng sau khi đăng nhập thành công (không chứa mật khẩu).</summary>
    public record AuthUserDTO(int Id, string FullName, string RoleName);
}