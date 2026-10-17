namespace BLL.Exceptions
{
    /// <summary>
    /// Lỗi nghiệp vụ có thông báo thân thiện, GUI hiển thị thẳng cho người dùng
    /// (VD: tên trống, trùng tên, không thể xóa vì đã có phiếu nhập).
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }
    }
}
