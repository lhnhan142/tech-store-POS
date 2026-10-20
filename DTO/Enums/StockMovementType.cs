namespace Common.Enums
{
    /// <summary>
    /// Các loại biến động kho:
    /// Sale: Xuất bán hàng
    /// ReturnRestock: Nhập lại kho bán hàng (hàng đổi trả nguyên vẹn)
    /// ReturnDefect: Chuyển vào kho hàng lỗi (hàng đổi trả bị lỗi/hỏng)
    /// ExchangeOut: Xuất kho cho sản phẩm đổi mới
    /// Purchase: Nhập kho từ nhà cung cấp
    /// Adjustment: Điều chỉnh kiểm kê kho
    /// </summary>
    public enum StockMovementType
    {
        Sale,
        ReturnRestock,
        ReturnDefect,
        ExchangeOut,
        Purchase,
        Adjustment
    }
}
