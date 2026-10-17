using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    /// <summary>Phiếu xuất kho (xuất hàng hỏng, trả nhà cung cấp, dùng nội bộ...).</summary>
    public class StockIssue
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public DateTime IssueDate { get; set; }

        [Required, MaxLength(100)]
        public required string Reason { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        public ICollection<StockIssueDetail>? Details { get; set; }
    }
}
