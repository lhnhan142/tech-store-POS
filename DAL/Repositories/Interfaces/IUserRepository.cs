using DAL.Entities;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        // Thêm hàm đặc thù: Tìm user bằng Username để làm chức năng Đăng nhập
        Task<User> GetByUsernameAsync(string username);

        // Kiểm tra xem Username đã tồn tại chưa khi tạo mới
        Task<bool> IsUsernameExistsAsync(string username);
    }
}