namespace BLL.DTOs
{
    // ====================== Dữ liệu GUI gửi xuống khi lập phiếu ======================
    public record ReceiptLineInput(int ProductId, int Quantity, decimal UnitPrice);

    public record IssueLineInput(int ProductId, int Quantity);

    public record StockTakeLineInput(int ProductId, int ActualQuantity);

    // ====================== Dữ liệu trả lên GUI để hiển thị ======================

    /// <summary>Sản phẩm kèm tồn kho, dùng cho ô chọn sản phẩm.</summary>
    public record ProductStockDTO(int Id, string Name, string? Barcode, int StockQuantity)
    {
        public string Display => $"{Name}  (tồn: {StockQuantity})";
    }

    public record ReceiptSummaryDTO(int Id, DateTime ReceiptDate, string SupplierName, string UserName, decimal TotalAmount);

    public record ReceiptLineDTO(string ProductName, int Quantity, decimal UnitPrice)
    {
        public decimal LineTotal => Quantity * UnitPrice;
    }

    public record IssueSummaryDTO(int Id, DateTime IssueDate, string Reason, string? Note, string UserName, int TotalQuantity);

    public record IssueLineDTO(string ProductName, int Quantity);

    public record StockTakeSummaryDTO(int Id, DateTime TakeDate, string UserName, string? Note, int LineCount, int DifferenceCount);

    public record StockTakeLineDTO(string ProductName, int SystemQuantity, int ActualQuantity)
    {
        // Dương = thừa so với hệ thống, âm = thiếu
        public int Difference => ActualQuantity - SystemQuantity;
    }

    // ====================== Cảnh báo tồn kho ======================
    public enum StockLevel
    {
        Low,         // sắp hết: 0 < tồn <= ngưỡng
        OutOfStock   // hết hàng: tồn <= 0
    }

    public record StockAlertDTO(int ProductId, string Name, string? Barcode, int StockQuantity, int Threshold)
    {
        public StockLevel Level => StockQuantity <= 0 ? StockLevel.OutOfStock : StockLevel.Low;
        public string LevelText => Level == StockLevel.OutOfStock ? "Hết hàng" : "Sắp hết";
    }

    /// <summary>Kết quả lập phiếu: Id phiếu + các sản phẩm trong phiếu đang bị cảnh báo tồn thấp.</summary>
    public record InventoryResultDTO(int DocumentId, IReadOnlyList<StockAlertDTO> Alerts);
}
