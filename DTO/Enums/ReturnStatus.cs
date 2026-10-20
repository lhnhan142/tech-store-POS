namespace Common.Enums
{
    /// <summary>
    /// Vòng đời trạng thái yêu cầu đổi trả:
    /// Pending (Chờ duyệt) -> Approved (Đã duyệt) -> Received (Đã nhận hàng) -> Completed (Hoàn tất)
    /// Hoặc: Rejected (Từ chối), Cancelled (Hủy).
    /// </summary>
    public enum ReturnStatus
    {
        Pending,
        Approved,
        Received,
        Completed,
        Rejected,
        Cancelled
    }
}
