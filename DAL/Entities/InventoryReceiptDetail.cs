using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    public class InventoryReceiptDetail
    {
        [Key]
        public int Id { get; set; }

        public int ReceiptId { get; set; }
        [ForeignKey("ReceiptId")]
        public InventoryReceipt? Receipt { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}