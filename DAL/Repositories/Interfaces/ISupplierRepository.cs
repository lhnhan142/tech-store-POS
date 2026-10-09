using DAL.Entities;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        // Kiểm tra trùng tên (không phân biệt hoa/thường). excludeId dùng khi sửa để bỏ qua chính nó
        Task<bool> NameExistsAsync(string name, int? excludeId = null);

        // Nhà cung cấp đã có phiếu nhập kho chưa (để chặn xóa)
        Task<bool> HasReceiptsAsync(int id);
    }
}
