using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    public interface IProductExcelService
    {
        /// <summary>Nhập hàng loạt sản phẩm từ file .xlsx, kiểm tra từng dòng.</summary>
        Task<ImportResultDTO> ImportAsync(string filePath);

        /// <summary>Xuất toàn bộ sản phẩm ra file .xlsx.</summary>
        Task ExportAsync(string filePath);

        /// <summary>Tạo file mẫu (có sẵn dòng ví dụ) để người dùng điền rồi nhập.</summary>
        void CreateTemplate(string filePath);
    }
}
