using BLL.Exceptions;

namespace GUI
{
    /// <summary>Thông tin phiên làm việc hiện tại (người dùng đang đăng nhập).</summary>
    public static class AppSession
    {
        // LoginForm gán giá trị này khi đăng nhập thành công.
#if DEBUG
        // Tạm thời để chạy thử khi chưa có đăng nhập: 5 = 'kho1' trong dữ liệu mẫu. Bỏ khi LoginForm xong.
        public static int? CurrentUserId { get; set; } = 5;
#else
        public static int? CurrentUserId { get; set; }
#endif

        public static int RequireUserId() =>
            CurrentUserId ?? throw new BusinessException("Chưa đăng nhập. Vui lòng đăng nhập để lập phiếu.");
    }
}
