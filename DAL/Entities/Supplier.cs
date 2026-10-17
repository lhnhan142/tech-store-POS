using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        public ICollection<InventoryReceipt>? InventoryReceipts { get; set; }
    }
}