using BLL.DTOs;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using System.Text.RegularExpressions;

namespace BLL.Services.Implementations
{
    public class ProductService : IProductService
    {
        public const int NameMaxLength = 200;
        public const int BarcodeMaxLength = 50;
        public const decimal MaxPrice = 999_999_999_999m; // decimal(18,2) còn dư rất nhiều

        // Định dạng ảnh mà System.Drawing (POS) đọc được
        public static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private static readonly Regex BarcodePattern = new(@"^[A-Za-z0-9\-]+$", RegexOptions.Compiled);

        private readonly IProductRepository _repo;
        private readonly ICategoryRepository _categories;
        private readonly IBrandRepository _brands;
        private readonly ProductImageSettings _imageSettings;

        public ProductService(IProductRepository repo, ICategoryRepository categories,
            IBrandRepository brands, ProductImageSettings imageSettings)
        {
            _repo = repo;
            _categories = categories;
            _brands = brands;
            _imageSettings = imageSettings;
        }

        public async Task<IReadOnlyList<ProductListItemDTO>> GetAllAsync()
        {
            var items = await _repo.GetAllWithDetailsAsync();
            return items.Select(p => new ProductListItemDTO(
                p.Id, p.Name, p.Barcode, p.Price, p.StockQuantity, p.ImageUrl,
                p.CategoryId, p.Category?.Name ?? "", p.BrandId, p.Brand?.Name ?? "")).ToList();
        }

        public async Task<int> CreateAsync(ProductInputDTO input)
        {
            var v = await ValidateAsync(input, excludeId: null);

            var entity = new Product
            {
                Name = v.Name,
                Barcode = v.Barcode,
                Price = v.Price,
                CategoryId = v.CategoryId,
                BrandId = v.BrandId,
                ImageUrl = v.ImageUrl,
                StockQuantity = 0 // tồn kho chỉ thay đổi qua phiếu nhập / xuất / kiểm kê
            };
            await _repo.AddAsync(entity);
            await CatalogRules.SaveAsync(_repo.SaveChangesAsync, "Không thể thêm sản phẩm. Vui lòng thử lại.");
            return entity.Id;
        }

        public async Task UpdateAsync(int id, ProductInputDTO input)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new BusinessException("Không tìm thấy sản phẩm (có thể đã bị xóa).");
            var v = await ValidateAsync(input, excludeId: id);

            entity.Name = v.Name;
            entity.Barcode = v.Barcode;
            entity.Price = v.Price;
            entity.CategoryId = v.CategoryId;
            entity.BrandId = v.BrandId;
            entity.ImageUrl = v.ImageUrl;
            _repo.Update(entity);
            await CatalogRules.SaveAsync(_repo.SaveChangesAsync, "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new BusinessException("Không tìm thấy sản phẩm (có thể đã bị xóa).");

            if (await _repo.HasTransactionsAsync(id))
                throw new BusinessException(
                    $"Không thể xóa sản phẩm '{entity.Name}' vì đã phát sinh đơn hàng hoặc phiếu kho liên quan.");

            _repo.Delete(entity);
            await CatalogRules.SaveAsync(_repo.SaveChangesAsync, "Không thể xóa sản phẩm. Vui lòng thử lại.");
        }

        public string SaveImage(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                throw new BusinessException("Không tìm thấy file ảnh đã chọn.");

            var ext = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (!ImageExtensions.Contains(ext))
                throw new BusinessException("Chỉ hỗ trợ ảnh định dạng JPG, PNG, BMP hoặc GIF.");
            if (new FileInfo(sourcePath).Length > MaxImageBytes)
                throw new BusinessException("Ảnh quá lớn (tối đa 5 MB).");

            // Tên file an toàn + hậu tố ngẫu nhiên để không ghi đè ảnh của sản phẩm khác
            var baseName = Regex.Replace(Path.GetFileNameWithoutExtension(sourcePath), @"[^A-Za-z0-9_\-]+", "-").Trim('-');
            if (baseName.Length == 0) baseName = "product";
            if (baseName.Length > 40) baseName = baseName[..40];
            var fileName = $"{baseName}-{Guid.NewGuid().ToString("N")[..6]}{ext}";

            try
            {
                Directory.CreateDirectory(_imageSettings.Folder);
                File.Copy(sourcePath, Path.Combine(_imageSettings.Folder, fileName), overwrite: false);
            }
            catch (IOException ex)
            {
                throw new BusinessException("Không thể lưu ảnh: " + ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                throw new BusinessException("Không có quyền ghi vào thư mục ảnh sản phẩm.");
            }
            return fileName;
        }

        // ------------------------------------------------------------ helpers
        private async Task<ProductInputDTO> ValidateAsync(ProductInputDTO input, int? excludeId)
        {
            var name = CatalogRules.RequireName(input.Name, "Tên sản phẩm", NameMaxLength);
            if (await _repo.NameExistsAsync(name, excludeId))
                throw new BusinessException($"Sản phẩm '{name}' đã tồn tại.");

            string? barcode = null;
            var rawBarcode = (input.Barcode ?? string.Empty).Trim();
            if (rawBarcode.Length > 0)
            {
                if (rawBarcode.Length > BarcodeMaxLength)
                    throw new BusinessException($"Mã vạch tối đa {BarcodeMaxLength} ký tự.");
                if (!BarcodePattern.IsMatch(rawBarcode))
                    throw new BusinessException("Mã vạch chỉ gồm chữ, số và dấu gạch ngang.");
                if (await _repo.BarcodeExistsAsync(rawBarcode, excludeId))
                    throw new BusinessException($"Mã vạch '{rawBarcode}' đã được sản phẩm khác sử dụng.");
                barcode = rawBarcode;
            }

            if (input.Price <= 0)
                throw new BusinessException("Giá bán phải lớn hơn 0.");
            if (input.Price > MaxPrice)
                throw new BusinessException("Giá bán quá lớn.");

            if (await _categories.GetByIdAsync(input.CategoryId) is null)
                throw new BusinessException("Hãy chọn danh mục hợp lệ.");

            int? brandId = null;
            if (input.BrandId is int b && b > 0)
            {
                if (await _brands.GetByIdAsync(b) is null)
                    throw new BusinessException("Thương hiệu không hợp lệ (có thể đã bị xóa).");
                brandId = b;
            }

            string? image = null;
            var rawImage = (input.ImageUrl ?? string.Empty).Trim();
            if (rawImage.Length > 0)
            {
                // Chỉ lưu tên file, không cho đường dẫn
                if (rawImage != Path.GetFileName(rawImage) || rawImage.Length > 200)
                    throw new BusinessException("Tên file ảnh không hợp lệ.");
                image = rawImage;
            }

            return new ProductInputDTO(name, barcode, input.Price, input.CategoryId, brandId, image);
        }
    }
}
