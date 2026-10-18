using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}
