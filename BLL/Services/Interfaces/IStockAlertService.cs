using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    /// <summary>Cảnh báo hàng sắp hết / hết hàng.</summary>
    public interface IStockAlertService
    {
        // Ngưỡng cảnh báo đang áp dụng (tồn kho <= ngưỡng thì cảnh báo)
        int Threshold { get; }

        // Các sản phẩm đang bị cảnh báo, ít hàng nhất xếp trước.
        // Truyền productIds để chỉ xét một số sản phẩm (vd: vừa nhập/xuất xong).
        Task<IReadOnlyList<StockAlertDTO>> GetAlertsAsync(IEnumerable<int>? productIds = null);
    }
}
