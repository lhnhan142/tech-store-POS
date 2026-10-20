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

        // ----- Quản lý sản phẩm -----
        // Toàn bộ sản phẩm kèm Danh mục + Thương hiệu, sắp theo tên
        Task<List<Product>> GetAllWithDetailsAsync();

        // Mã vạch đã có sản phẩm khác dùng chưa (excludeId dùng khi sửa)
        Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null);

        // Tên sản phẩm đã tồn tại chưa (không phân biệt hoa/thường)
        Task<bool> NameExistsAsync(string name, int? excludeId = null);

        // Sản phẩm đã nằm trong đơn hàng / phiếu nhập / xuất / kiểm kê chưa (để chặn xóa)
        Task<bool> HasTransactionsAsync(int id);
    }
}
