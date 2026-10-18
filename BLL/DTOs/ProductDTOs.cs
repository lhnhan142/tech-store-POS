namespace BLL.DTOs
{
    /// <summary>Một dòng trong danh sách sản phẩm (kèm tên danh mục / thương hiệu).</summary>
    public record ProductListItemDTO(
        int Id,
        string Name,
        string? Barcode,
        decimal Price,
        int StockQuantity,
        string? ImageUrl,
        int CategoryId,
        string CategoryName,
        int? BrandId,
        string BrandName);

    /// <summary>Dữ liệu người dùng nhập khi thêm / sửa sản phẩm.</summary>
    public record ProductInputDTO(
        string Name,
        string? Barcode,
        decimal Price,
        int CategoryId,
        int? BrandId,
        string? ImageUrl);

    /// <summary>Danh mục / thương hiệu kèm số sản phẩm đang dùng.</summary>
    public record CatalogItemDTO(int Id, string Name, int ProductCount);

    public class ImportRowError
    {
        public int Row { get; init; }          // số dòng trong file Excel
        public string Message { get; init; } = "";
    }

    public class ImportResultDTO
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public List<ImportRowError> Errors { get; } = new();
        public int FailedCount => Errors.Count;
    }
}
