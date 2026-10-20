using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    /// <summary>Phiếu kiểm kê: đối chiếu số lượng thực tế với tồn kho trên hệ thống.</summary>
    public class StockTake
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public DateTime TakeDate { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        public ICollection<StockTakeDetail>? Details { get; set; }
    }
}
