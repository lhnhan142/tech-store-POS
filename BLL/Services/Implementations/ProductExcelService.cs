using BLL.DTOs;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using Common.Utilities;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BLL.Services.Implementations
{
    public class ProductExcelService : IProductExcelService
    {
        // Tên cột trong file Excel
        private const string ColName = "Tên sản phẩm";
        private const string ColBarcode = "Mã vạch";
        private const string ColCategory = "Danh mục";
        private const string ColBrand = "Thương hiệu";
        private const string ColPrice = "Giá bán";
        private const string ColImage = "Hình ảnh";

        private const int MaxRows = 5000;
        private static readonly string[] RequiredColumns = { ColName, ColCategory, ColPrice };
        private static readonly Regex BarcodePattern = new(@"^[A-Za-z0-9\-]+$", RegexOptions.Compiled);

        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;
        private readonly IBrandRepository _brands;

        public ProductExcelService(IProductRepository products, ICategoryRepository categories, IBrandRepository brands)
        {
            _products = products;
            _categories = categories;
            _brands = brands;
        }

        // ================================================================= IMPORT
        public async Task<ImportResultDTO> ImportAsync(string filePath)
        {
            ExcelSheet sheet;
            try
            {
                sheet = ExcelHelper.ReadSheet(filePath);
            }
            catch (IOException)
            {
                throw new BusinessException("Không đọc được file. Hãy đóng file Excel nếu đang mở rồi thử lại.");
            }
            catch (Exception ex) when (ex is not BusinessException)
            {
                throw new BusinessException("File không phải Excel (.xlsx) hợp lệ hoặc đã bị hỏng.");
            }

            var missing = RequiredColumns
                .Where(c => !sheet.Headers.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
            if (missing.Count > 0)
                throw new BusinessException(
                    "File thiếu cột bắt buộc: " + string.Join(", ", missing) +
                    ".\nHãy dùng nút \"Tải file mẫu\" để lấy đúng định dạng.");

            if (sheet.Rows.Count == 0)
                throw new BusinessException("File không có dòng dữ liệu nào.");
            if (sheet.Rows.Count > MaxRows)
                throw new BusinessException($"File có quá nhiều dòng (tối đa {MaxRows} dòng mỗi lần nhập).");

            var result = new ImportResultDTO { TotalRows = sheet.Rows.Count };

            // Nạp dữ liệu tham chiếu 1 lần cho cả file (không truy vấn từng dòng)
            var categories = (await _categories.GetAllAsync())
                .GroupBy(c => Key(c.Name)).ToDictionary(g => g.Key, g => g.First().Id);
            var brands = (await _brands.GetAllAsync())
                .GroupBy(b => Key(b.Name)).ToDictionary(g => g.Key, g => g.First().Id);
            var existing = (await _products.GetAllAsync()).ToList();
            var usedNames = existing.Select(p => Key(p.Name)).ToHashSet();
            var usedBarcodes = existing.Where(p => !string.IsNullOrEmpty(p.Barcode))
                .Select(p => p.Barcode!.ToUpperInvariant()).ToHashSet();

            var toAdd = new List<Product>();

            foreach (var row in sheet.Rows)
            {
                var errors = new List<string>();

                // --- Tên
                var name = CatalogRules.CollapseSpaces(row.Get(ColName));
                if (name.Length == 0) errors.Add("thiếu tên sản phẩm");
                else if (name.Length > ProductService.NameMaxLength)
                    errors.Add($"tên quá dài (tối đa {ProductService.NameMaxLength} ký tự)");
                else if (!usedNames.Add(Key(name)))
                    errors.Add($"sản phẩm '{name}' đã tồn tại (trong hệ thống hoặc trùng trong file)");

                // --- Mã vạch (không bắt buộc)
                string? barcode = null;
                var rawBarcode = row.Get(ColBarcode);
                if (rawBarcode.Length > 0)
                {
                    if (rawBarcode.Length > ProductService.BarcodeMaxLength || !BarcodePattern.IsMatch(rawBarcode))
                        errors.Add("mã vạch chỉ gồm chữ, số, gạch ngang (tối đa 50 ký tự)");
                    else if (!usedBarcodes.Add(rawBarcode.ToUpperInvariant()))
                        errors.Add($"mã vạch '{rawBarcode}' đã tồn tại (trong hệ thống hoặc trùng trong file)");
                    else
                        barcode = rawBarcode;
                }

                // --- Danh mục (bắt buộc)
                int categoryId = 0;
                var catName = row.Get(ColCategory);
                if (catName.Length == 0) errors.Add("thiếu danh mục");
                else if (!categories.TryGetValue(Key(catName), out categoryId))
                    errors.Add($"danh mục '{catName}' không tồn tại");

                // --- Thương hiệu (không bắt buộc, nhưng đã ghi thì phải đúng)
                int? brandId = null;
                var brandName = row.Get(ColBrand);
                if (brandName.Length > 0)
                {
                    if (brands.TryGetValue(Key(brandName), out var bId)) brandId = bId;
                    else errors.Add($"thương hiệu '{brandName}' không tồn tại");
                }

                // --- Giá
                if (!TryParsePrice(row.Get(ColPrice), out var price))
                    errors.Add("giá bán không hợp lệ (phải là số lớn hơn 0)");
                else if (price > ProductService.MaxPrice)
                    errors.Add("giá bán quá lớn");

                // --- Hình ảnh (chỉ là TÊN FILE trong thư mục Images\Products)
                string? image = null;
                var rawImage = row.Get(ColImage);
                if (rawImage.Length > 0)
                {
                    var ext = Path.GetExtension(rawImage).ToLowerInvariant();
                    if (rawImage != Path.GetFileName(rawImage) || rawImage.Length > 200
                        || !ProductService.ImageExtensions.Contains(ext))
                        errors.Add("hình ảnh phải là tên file .jpg/.png/.bmp/.gif (không kèm đường dẫn)");
                    else
                        image = rawImage;
                }

                if (errors.Count > 0)
                {
                    result.Errors.Add(new ImportRowError
                    {
                        Row = row.RowNumber,
                        Message = Capitalize(string.Join("; ", errors))
                    });
                    continue;
                }

                toAdd.Add(new Product
                {
                    Name = name,
                    Barcode = barcode,
                    Price = price,
                    CategoryId = categoryId,
                    BrandId = brandId,
                    ImageUrl = image,
                    StockQuantity = 0
                });
            }

            if (toAdd.Count > 0)
            {
                await _products.AddRangeAsync(toAdd);
                await CatalogRules.SaveAsync(_products.SaveChangesAsync,
                    "Không thể lưu dữ liệu nhập vào CSDL. Vui lòng thử lại.");
            }
            result.SuccessCount = toAdd.Count;
            return result;
        }

        // ================================================================= EXPORT
        public async Task ExportAsync(string filePath)
        {
            var products = await _products.GetAllWithDetailsAsync();

            var data = products.Select(p => new object?[]
            {
                p.Id,
                p.Name,
                p.Barcode,
                p.Category?.Name,
                p.Brand?.Name,
                p.Price,
                p.StockQuantity,
                p.ImageUrl
            }).ToList();

            try
            {
                await Task.Run(() => ExcelHelper.WriteSheet(filePath, "Sản phẩm",
                    new[] { "Mã SP", ColName, ColBarcode, ColCategory, ColBrand, ColPrice, "Tồn kho", ColImage },
                    data));
            }
            catch (IOException)
            {
                throw new BusinessException("Không ghi được file. Hãy đóng file nếu đang mở trong Excel hoặc chọn tên khác.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new BusinessException("Không có quyền ghi vào vị trí đã chọn. Hãy chọn thư mục khác.");
            }
        }

        // ================================================================= TEMPLATE
        public void CreateTemplate(string filePath)
        {
            try
            {
                ExcelHelper.WriteSheet(filePath, "Sản phẩm",
                    new[] { ColName, ColBarcode, ColCategory, ColBrand, ColPrice, ColImage },
                    new[]
                    {
                        new object?[] { "Chuột Logitech G304", "880123456900", "Chuột máy tính", "Logitech", 590000m, "g304.jpg" },
                        new object?[] { "Bàn phím Keychron K2", "", "Bàn phím", "", 1890000m, "" }
                    });
            }
            catch (IOException)
            {
                throw new BusinessException("Không ghi được file. Hãy đóng file nếu đang mở trong Excel hoặc chọn tên khác.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new BusinessException("Không có quyền ghi vào vị trí đã chọn. Hãy chọn thư mục khác.");
            }
        }

        // ================================================================= helpers
        private static string Key(string s) => CatalogRules.CollapseSpaces(s).ToLowerInvariant();

        private static string Capitalize(string s) =>
            s.Length == 0 ? s : char.ToUpper(s[0], CultureInfo.CurrentCulture) + s[1..];

        // Chấp nhận "350000", "350,000", "350.000", "1.500.000đ", "1500000 VND"
        private static bool TryParsePrice(string raw, out decimal value)
        {
            value = 0;
            var s = Regex.Replace(raw, @"(?i)\s|đ|vnd|vnđ", "");
            if (s.Length == 0) return false;

            bool ok = decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
                   || decimal.TryParse(s, NumberStyles.Number, new CultureInfo("vi-VN"), out value);
            return ok && value > 0;
        }
    }
}
