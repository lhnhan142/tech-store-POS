using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace DAL.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}