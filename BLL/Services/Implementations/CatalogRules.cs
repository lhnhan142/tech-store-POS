using BLL.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implementations
{
    /// <summary>Các quy tắc kiểm tra dùng chung cho Sản phẩm / Danh mục / Thương hiệu.</summary>
    internal static class CatalogRules
    {
        // Cắt khoảng trắng thừa ở đầu, cuối và giữa các từ
        public static string CollapseSpaces(string? value) =>
            string.Join(' ', (value ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        public static string RequireName(string? name, string label, int maxLength)
        {
            var n = CollapseSpaces(name);
            if (n.Length == 0)
                throw new BusinessException($"{label} không được để trống.");
            if (n.Length > maxLength)
                throw new BusinessException($"{label} tối đa {maxLength} ký tự.");
            return n;
        }

        public static async Task SaveAsync(Func<Task<int>> save, string errorMessage)
        {
            try
            {
                await save();
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.GetBaseException().Message);
                throw new BusinessException(errorMessage);
            }
        }
    }
}
