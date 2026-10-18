using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);

        // Số sản phẩm của mỗi danh mục: Id -> số lượng
        Task<Dictionary<int, int>> CountProductsAsync();
    }

    public interface IBrandRepository : IRepository<Brand>
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);

        // Số sản phẩm của mỗi thương hiệu: Id -> số lượng
        Task<Dictionary<int, int>> CountProductsAsync();
    }
}
