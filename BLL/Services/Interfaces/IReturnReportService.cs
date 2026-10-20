using BLL.DTOs.Return;

namespace BLL.Services.Interfaces
{
    /// <summary>
    /// Dịch vụ báo cáo đổi trả — thống kê theo kỳ, theo sản phẩm, theo lý do.
    /// </summary>
    public interface IReturnReportService
    {
        /// <summary>
        /// Tổng hợp báo cáo đổi trả trong khoảng thời gian chỉ định.
        /// Gồm: số lượng, tiền hoàn, thời gian xử lý TB, top sản phẩm, phân bổ lý do.
        /// </summary>
        Task<ReturnReportDto> GetReportAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Tỷ lệ đổi trả theo từng sản phẩm trong kỳ
        /// (TotalReturnedQty / TổngBán * 100).
        /// Trả về danh sách sắp xếp theo tỷ lệ giảm dần.
        /// </summary>
        Task<IReadOnlyList<ReturnByProductDto>> GetReturnRateByProductAsync(DateTime fromDate, DateTime toDate);

        /// <summary>Tổng tiền hoàn theo từng tháng trong năm chỉ định.</summary>
        Task<IReadOnlyList<(int Month, decimal TotalRefund)>> GetMonthlyRefundAsync(int year);
    }
}
