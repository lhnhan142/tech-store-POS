using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace DAL.Entities
{
    /// <summary>
    /// Thực thể Bảng lương theo kỳ (tháng/năm)
    /// </summary>
    public class Payroll
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public required string PayrollCode { get; set; } // BL-2026-10

        public int Month { get; set; } // 1 - 12
        public int Year { get; set; }  // 2026

        public int StandardWorkDays { get; set; } = 26; // Số ngày công chuẩn

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGrossAmount { get; set; } = 0; // Tổng chi phí lương trước khấu trừ

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalNetAmount { get; set; } = 0; // Tổng thực chi trả cho nhân viên

        public int TotalEmployees { get; set; } = 0;

        public PayrollStatus Status { get; set; } = PayrollStatus.Draft; // 0: Nháp, 1: Đã chốt

        [MaxLength(250)]
        public string? Note { get; set; }

        public int CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public User? CreatedByUser { get; set; }

        public DateTime? FinalizedAt { get; set; }

        public int? FinalizedByUserId { get; set; }
        [ForeignKey("FinalizedByUserId")]
        public User? FinalizedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<PayrollDetail> Details { get; set; } = new List<PayrollDetail>();
    }
}
