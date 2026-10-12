namespace DAL.Entities
{
    /// <summary>
    /// Nhật ký kiểm toán thao tác hệ thống (Audit log).
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }

        /// <summary>Hành động: CREATE, UPDATE, APPROVE, REJECT, COMPLETE, CANCEL...</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Tên loại thực thể (VD: Invoice, ReturnRequest)</summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>Id của thực thể</summary>
        public int EntityId { get; set; }

        /// <summary>Giá trị cũ trước khi thay đổi (JSON)</summary>
        public string? OldValues { get; set; }

        /// <summary>Giá trị mới sau khi thay đổi (JSON)</summary>
        public string? NewValues { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
