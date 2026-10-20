namespace BLL.Exceptions
{
    /// <summary>
    /// Lỗi nghiệp vụ có thông báo thân thiện, GUI hiển thị thẳng cho người dùng
    /// (VD: tên trống, trùng tên, không thể xóa vì đã có phiếu nhập).
    /// </summary>
    public class BusinessException : Exception
    {
        /// <summary>Mã lỗi để GUI phân nhánh xử lý mà không cần phân tích chuỗi Message.</summary>
        public BusinessErrorCode Code { get; }

        /// <summary>Constructor tương thích ngược — mã lỗi mặc định là General.</summary>
        public BusinessException(string message) : base(message)
        {
            Code = BusinessErrorCode.General;
        }

        /// <summary>Constructor đầy đủ với mã lỗi cụ thể.</summary>
        public BusinessException(BusinessErrorCode code, string message) : base(message)
        {
            Code = code;
        }
    }
}
