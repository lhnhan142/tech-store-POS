using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        // Lấy danh sách sản phẩm theo danh mục
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

        // Tìm sản phẩm bằng mã Barcode (Phục vụ cho máy quét ở quầy POS)
        Task<Product> GetByBarcodeAsync(string barcode);

        // Lấy danh sách các sản phẩm sắp hết hàng (tồn kho < số lượng tối thiểu)
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
    }
}