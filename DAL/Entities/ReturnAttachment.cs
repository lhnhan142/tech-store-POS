namespace DAL.Entities
{
    /// <summary>
    /// File ảnh/chứng từ đính kèm của yêu cầu đổi trả.
    /// </summary>
    public class ReturnAttachment
    {
        public int Id { get; set; }

        public int ReturnRequestId { get; set; }
        public ReturnRequest? ReturnRequest { get; set; }

        /// <summary>Đường dẫn lưu file trên đĩa</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>Tên file gốc</summary>
        public string FileName { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
