namespace BLL.Services
{
    /// <summary>Cấu hình kho, đọc từ mục "Inventory" trong appsettings.json.</summary>
    public class InventorySettings
    {
        // Sản phẩm có tồn kho nhỏ hơn hoặc bằng giá trị này bị coi là "sắp hết hàng"
        public int LowStockThreshold { get; set; } = 10;
    }
}
