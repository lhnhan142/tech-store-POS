using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    /// <summary>CRUD dùng chung cho Danh mục và Thương hiệu (chỉ có Tên).</summary>
    public interface ICatalogService
    {
        Task<IReadOnlyList<CatalogItemDTO>> GetAllAsync();
        Task CreateAsync(string name);
        Task UpdateAsync(int id, string name);
        Task DeleteAsync(int id);
    }

    public interface ICategoryService : ICatalogService { }

    public interface IBrandService : ICatalogService { }
}
