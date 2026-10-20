using BLL.DTOs;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private const int MaxQuantity = 1_000_000;
        private const decimal MaxUnitPrice = 1_000_000_000m;      // 1 tỷ / sản phẩm
        private const decimal MaxTotalAmount = 9_000_000_000_000_000m; // còn trong giới hạn decimal(18,2)
        private const int ReasonMaxLength = 100;
        private const int NoteMaxLength = 200;

        private readonly IInventoryRepository _inventory;
        private readonly IProductRepository _products;
        private readonly ISupplierRepository _suppliers;
        private readonly IUserRepository _users;
        private readonly IStockAlertService _alerts;

        public InventoryService(
            IInventoryRepository inventory,
            IProductRepository products,
            ISupplierRepository suppliers,
            IUserRepository users,
            IStockAlertService alerts)
        {
            _inventory = inventory;
            _products = products;
            _suppliers = suppliers;
            _users = users;
            _alerts = alerts;
        }

        // ------------------------------------------------------------ Sản phẩm
        public async Task<IReadOnlyList<ProductStockDTO>> GetProductsAsync()
        {
            var items = await _products.GetAllAsync();
            return items
                .OrderBy(p => p.Name)
                .Select(p => new ProductStockDTO(p.Id, p.Name, p.Barcode, p.StockQuantity))
                .ToList();
        }

        // ------------------------------------------------------------ Lập phiếu
        public async Task<InventoryResultDTO> CreateReceiptAsync(int supplierId, int userId, DateTime receiptDate, IReadOnlyList<ReceiptLineInput> lines)
        {
            RequireLines(lines, "Phiếu nhập phải có ít nhất một sản phẩm.");
            ValidateDate(receiptDate, "Ngày nhập");

            _ = await _suppliers.GetByIdAsync(supplierId)
                ?? throw new BusinessException("Hãy chọn một nhà cung cấp hợp lệ.");
            await EnsureUserAsync(userId);

            EnsureNoDuplicates(lines.Select(l => l.ProductId));
            foreach (var l in lines)
            {
                ValidateQuantity(l.Quantity);
                if (l.UnitPrice < 0 || l.UnitPrice > MaxUnitPrice)
                    throw new BusinessException($"Đơn giá nhập phải từ 0 đến {MaxUnitPrice:N0}.");
            }
            await LoadProductsAsync(lines.Select(l => l.ProductId));

            var total = lines.Sum(l => l.Quantity * l.UnitPrice);
            if (total > MaxTotalAmount)
                throw new BusinessException("Tổng tiền phiếu nhập quá lớn.");

            var receipt = new InventoryReceipt
            {
                SupplierId = supplierId,
                UserId = userId,
                ReceiptDate = receiptDate,
                TotalAmount = total,
                Details = lines.Select(l => new InventoryReceiptDetail
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                }).ToList()
            };

            await SaveAsync(() => _inventory.AddReceiptAsync(receipt), "Không thể lưu phiếu nhập kho. Vui lòng thử lại.");
            return await ResultAsync(receipt.Id, lines.Select(l => l.ProductId));
        }

        public async Task<InventoryResultDTO> CreateIssueAsync(int userId, DateTime issueDate, string reason, string? note, IReadOnlyList<IssueLineInput> lines)
        {
            RequireLines(lines, "Phiếu xuất phải có ít nhất một sản phẩm.");
            ValidateDate(issueDate, "Ngày xuất");

            var r = Collapse(reason);
            if (r.Length == 0)
                throw new BusinessException("Hãy nhập lý do xuất kho.");
            if (r.Length > ReasonMaxLength)
                throw new BusinessException($"Lý do xuất kho tối đa {ReasonMaxLength} ký tự.");
            var n = NormalizeNote(note);

            await EnsureUserAsync(userId);
            EnsureNoDuplicates(lines.Select(l => l.ProductId));
            foreach (var l in lines) ValidateQuantity(l.Quantity);
            var products = await LoadProductsAsync(lines.Select(l => l.ProductId));

            // Báo lỗi sớm, kèm tên sản phẩm và số còn lại (kiểm tra lần cuối nằm trong giao dịch lưu phiếu)
            foreach (var l in lines)
            {
                var p = products[l.ProductId];
                if (l.Quantity > p.StockQuantity)
                    throw new BusinessException(
                        $"Sản phẩm '{p.Name}' chỉ còn {p.StockQuantity} trong kho, không thể xuất {l.Quantity}.");
            }

            var issue = new StockIssue
            {
                UserId = userId,
                IssueDate = issueDate,
                Reason = r,
                Note = n,
                Details = lines.Select(l => new StockIssueDetail
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity
                }).ToList()
            };

            int? shortProductId = null;
            await SaveAsync(async () => shortProductId = await _inventory.TryAddIssueAsync(issue),
                "Không thể lưu phiếu xuất kho. Vui lòng thử lại.");

            // Có người vừa bán/xuất hàng làm tồn kho giảm sau lúc kiểm tra: phiếu đã được hoàn tác
            if (shortProductId is int pid)
                throw new BusinessException(
                    $"Tồn kho của '{products[pid].Name}' vừa thay đổi và không còn đủ để xuất. " +
                    "Phiếu chưa được lưu, hãy tải lại danh sách sản phẩm rồi lập lại.");

            return await ResultAsync(issue.Id, lines.Select(l => l.ProductId));
        }

        public async Task<InventoryResultDTO> CreateStockTakeAsync(int userId, DateTime takeDate, string? note, IReadOnlyList<StockTakeLineInput> lines)
        {
            RequireLines(lines, "Phiếu kiểm kê phải có ít nhất một sản phẩm.");
            ValidateDate(takeDate, "Ngày kiểm kê");
            var n = NormalizeNote(note);

            await EnsureUserAsync(userId);
            EnsureNoDuplicates(lines.Select(l => l.ProductId));
            foreach (var l in lines)
            {
                if (l.ActualQuantity < 0 || l.ActualQuantity > MaxQuantity)
                    throw new BusinessException($"Số lượng thực tế phải từ 0 đến {MaxQuantity:N0}.");
            }
            var products = await LoadProductsAsync(lines.Select(l => l.ProductId));

            var take = new StockTake
            {
                UserId = userId,
                TakeDate = takeDate,
                Note = n,
                Details = lines.Select(l => new StockTakeDetail
                {
                    ProductId = l.ProductId,
                    SystemQuantity = products[l.ProductId].StockQuantity,
                    ActualQuantity = l.ActualQuantity
                }).ToList()
            };

            await SaveAsync(() => _inventory.AddStockTakeAsync(take), "Không thể lưu phiếu kiểm kê. Vui lòng thử lại.");
            return await ResultAsync(take.Id, lines.Select(l => l.ProductId));
        }

        // ------------------------------------------------------------ Lịch sử phiếu
        public async Task<IReadOnlyList<ReceiptSummaryDTO>> GetReceiptsAsync(DateTime from, DateTime to)
        {
            var items = await _inventory.GetReceiptsAsync(from, to);
            return items
                .Select(r => new ReceiptSummaryDTO(r.Id, r.ReceiptDate, r.Supplier?.Name ?? "(không rõ)",
                    r.User?.FullName ?? "(không rõ)", r.TotalAmount))
                .ToList();
        }

        public async Task<IReadOnlyList<IssueSummaryDTO>> GetIssuesAsync(DateTime from, DateTime to)
        {
            var items = await _inventory.GetIssuesAsync(from, to);
            return items
                .Select(i => new IssueSummaryDTO(i.Id, i.IssueDate, i.Reason, i.Note,
                    i.User?.FullName ?? "(không rõ)", i.Details?.Sum(d => d.Quantity) ?? 0))
                .ToList();
        }

        public async Task<IReadOnlyList<StockTakeSummaryDTO>> GetStockTakesAsync(DateTime from, DateTime to)
        {
            var items = await _inventory.GetStockTakesAsync(from, to);
            return items
                .Select(t => new StockTakeSummaryDTO(t.Id, t.TakeDate, t.User?.FullName ?? "(không rõ)", t.Note,
                    t.Details?.Count ?? 0, t.Details?.Count(d => d.Difference != 0) ?? 0))
                .ToList();
        }

        public async Task<IReadOnlyList<ReceiptLineDTO>> GetReceiptLinesAsync(int receiptId)
        {
            var items = await _inventory.GetReceiptDetailsAsync(receiptId);
            return items
                .Select(d => new ReceiptLineDTO(d.Product?.Name ?? "(sản phẩm đã xóa)", d.Quantity, d.UnitPrice))
                .ToList();
        }

        public async Task<IReadOnlyList<IssueLineDTO>> GetIssueLinesAsync(int issueId)
        {
            var items = await _inventory.GetIssueDetailsAsync(issueId);
            return items
                .Select(d => new IssueLineDTO(d.Product?.Name ?? "(sản phẩm đã xóa)", d.Quantity))
                .ToList();
        }

        public async Task<IReadOnlyList<StockTakeLineDTO>> GetStockTakeLinesAsync(int takeId)
        {
            var items = await _inventory.GetStockTakeDetailsAsync(takeId);
            return items
                .Select(d => new StockTakeLineDTO(d.Product?.Name ?? "(sản phẩm đã xóa)", d.SystemQuantity, d.ActualQuantity))
                .ToList();
        }

        // ------------------------------------------------------------ helpers
        private static void RequireLines<T>(IReadOnlyList<T>? lines, string message)
        {
            if (lines is null || lines.Count == 0)
                throw new BusinessException(message);
        }

        private static void ValidateQuantity(int quantity)
        {
            if (quantity <= 0 || quantity > MaxQuantity)
                throw new BusinessException($"Số lượng phải từ 1 đến {MaxQuantity:N0}.");
        }

        // Cho phép lệch vài phút do đồng hồ máy, nhưng không cho lập phiếu ở tương lai
        private static void ValidateDate(DateTime date, string label)
        {
            if (date > DateTime.Now.AddMinutes(5))
                throw new BusinessException($"{label} không được ở tương lai.");
        }

        private static void EnsureNoDuplicates(IEnumerable<int> productIds)
        {
            if (productIds.GroupBy(id => id).Any(g => g.Count() > 1))
                throw new BusinessException("Mỗi sản phẩm chỉ được xuất hiện một dòng trong phiếu.");
        }

        private async Task EnsureUserAsync(int userId)
        {
            if (await _users.GetByIdAsync(userId) is null)
                throw new BusinessException("Không xác định được người lập phiếu. Vui lòng đăng nhập lại.");
        }

        // Tải các sản phẩm của phiếu; báo lỗi nếu có sản phẩm không còn tồn tại
        private async Task<Dictionary<int, Product>> LoadProductsAsync(IEnumerable<int> productIds)
        {
            var ids = productIds.Distinct().ToList();
            var found = (await _inventory.GetProductsByIdsAsync(ids)).ToDictionary(p => p.Id);
            if (found.Count != ids.Count)
                throw new BusinessException("Có sản phẩm trong phiếu không còn tồn tại. Hãy tải lại danh sách sản phẩm.");
            return found;
        }

        // Sau khi lưu phiếu: kèm theo các sản phẩm trong phiếu đang ở mức cảnh báo tồn thấp
        private async Task<InventoryResultDTO> ResultAsync(int documentId, IEnumerable<int> productIds) =>
            new(documentId, await _alerts.GetAlertsAsync(productIds));

        private static string? NormalizeNote(string? note)
        {
            var n = Collapse(note);
            if (n.Length == 0) return null;
            if (n.Length > NoteMaxLength)
                throw new BusinessException($"Ghi chú tối đa {NoteMaxLength} ký tự.");
            return n;
        }

        // Cắt khoảng trắng thừa ở đầu, cuối và giữa các từ
        private static string Collapse(string? value) =>
            string.Join(' ', (value ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        private static async Task SaveAsync(Func<Task> save, string errorMessage)
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
