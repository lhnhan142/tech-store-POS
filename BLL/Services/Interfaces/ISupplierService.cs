using DAL.Entities;

namespace BLL.Services.Interfaces
{
    public interface ISupplierService
    {
        // Lấy tất cả nhà cung cấp, sắp xếp theo tên
        Task<IReadOnlyList<Supplier>> GetAllAsync();

        Task CreateAsync(string name, string? phone, string? address);

        Task UpdateAsync(int id, string name, string? phone, string? address);

        Task DeleteAsync(int id);
    }
}
