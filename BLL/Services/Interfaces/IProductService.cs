using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<IReadOnlyList<ProductListItemDTO>> GetAllAsync();

        Task<int> CreateAsync(ProductInputDTO input);

        Task UpdateAsync(int id, ProductInputDTO input);

        Task DeleteAsync(int id);

        /// <summary>
        /// Chép ảnh người dùng chọn vào thư mục ảnh sản phẩm, trả về TÊN FILE để lưu vào Product.ImageUrl.
        /// </summary>
        string SaveImage(string sourcePath);
    }
}
