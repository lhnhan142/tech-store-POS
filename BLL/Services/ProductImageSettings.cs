namespace BLL.Services
{
    /// <summary>Thư mục lưu ảnh sản phẩm (POS đọc ảnh từ cùng thư mục này).</summary>
    public class ProductImageSettings
    {
        public string Folder { get; set; } = Path.Combine(AppContext.BaseDirectory, "Images", "Products");
    }
}
