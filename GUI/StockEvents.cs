namespace GUI
{
    /// <summary>
    /// Thông báo "tồn kho vừa thay đổi" (sau khi nhập, xuất, kiểm kê...) để các màn hình khác
    /// như danh sách cảnh báo và huy hiệu trên menu tự làm mới. Luôn gọi từ luồng giao diện.
    /// </summary>
    public static class StockEvents
    {
        public static event Action? Changed;

        public static void RaiseChanged() => Changed?.Invoke();
    }
}
