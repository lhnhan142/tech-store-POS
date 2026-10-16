using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    public class StockIssueDetail
    {
        [Key]
        public int Id { get; set; }

        public int IssueId { get; set; }
        [ForeignKey("IssueId")]
        public StockIssue? Issue { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public int Quantity { get; set; }
    }
}
