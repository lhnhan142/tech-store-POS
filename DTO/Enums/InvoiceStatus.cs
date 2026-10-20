namespace Common.Enums
{
    /// <summary>
    /// Trạng thái hóa đơn: Đã thanh toán, Đã hủy, Trả một phần, Đã trả hết.
    /// </summary>
    public enum InvoiceStatus
    {
        Paid,
        Cancelled,
        PartiallyReturned,
        FullyReturned
    }
}
