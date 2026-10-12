namespace Common.Enums
{
    /// <summary>
    /// Tình trạng sản phẩm đổi trả:
    /// Good (Nguyên vẹn / tốt - có thể nhập lại kho bán),
    /// Damaged (Hỏng hóc),
    /// Defective (Lỗi do nhà sản xuất - nhập kho hàng lỗi).
    /// </summary>
    public enum ItemCondition
    {
        Good,
        Damaged,
        Defective
    }
}
