using System;
using System.Collections.Generic;
using PhuKienMayTinh.Common.Enums;

namespace PhuKienMayTinh.DAL.Entities
{
    public class Payroll
    {
        public int Id { get; set; }
        public string PayrollCode { get; set; } = string.Empty; // BL-2026-10
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public decimal TotalBaseSalary { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal TotalBonus { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }

        public string? Note { get; set; }

        public virtual ICollection<PayrollDetail> Details { get; set; } = new List<PayrollDetail>();
    }
}
