using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        [Required, MaxLength(20)]
        public required string Phone { get; set; }

        public int RewardPoints { get; set; }
    }
}