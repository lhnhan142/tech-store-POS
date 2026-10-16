using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Repositories.Interfaces;

namespace BLL.Services.Implementations
{
    public class StockAlertService : IStockAlertService
    {
        private readonly IProductRepository _products;
        private readonly InventorySettings _settings;

        public StockAlertService(IProductRepository products, InventorySettings settings)
        {
            _products = products;
            _settings = settings;
        }

        public int Threshold => Math.Max(0, _settings.LowStockThreshold);

        public async Task<IReadOnlyList<StockAlertDTO>> GetAlertsAsync(IEnumerable<int>? productIds = null)
        {
            var threshold = Threshold;
            var low = await _products.GetLowStockProductsAsync(threshold);
            var only = productIds?.ToHashSet();

            return low
                .Where(p => only is null || only.Contains(p.Id))
                .OrderBy(p => p.StockQuantity).ThenBy(p => p.Name)
                .Select(p => new StockAlertDTO(p.Id, p.Name, p.Barcode, p.StockQuantity, threshold))
                .ToList();
        }
    }
}
