using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    public class StockTakeDetail
    {
        [Key]
        public int Id { get; set; }

        public int StockTakeId { get; set; }
        [ForeignKey("StockTakeId")]
        public StockTake? StockTake { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // Tồn kho trên hệ thống tại thời điểm kiểm kê
        public int SystemQuantity { get; set; }

        // Số lượng đếm thực tế
        public int ActualQuantity { get; set; }

        // Chênh lệch: dương = thừa, âm = thiếu
        [NotMapped]
        public int Difference => ActualQuantity - SystemQuantity;
    }
}
