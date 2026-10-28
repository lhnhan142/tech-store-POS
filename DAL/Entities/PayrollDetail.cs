using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    /// <summary>
    /// Thực thể Chi tiết lương của từng nhân viên trong kỳ lương
    /// </summary>
    public class PayrollDetail
    {
        [Key]
        public int Id { get; set; }

        public int PayrollId { get; set; }
        [ForeignKey("PayrollId")]
        public Payroll? Payroll { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required, MaxLength(20)]
        public required string EmployeeCode { get; set; }

        [Required, MaxLength(100)]
        public required string EmployeeName { get; set; }

        [Required, MaxLength(50)]
        public required string Position { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; } // Lương cơ bản

        public int StandardWorkDays { get; set; } = 26; // Số ngày công chuẩn

        [Column(TypeName = "decimal(5,2)")]
        public decimal ActualWorkDays { get; set; } = 0; // Số ngày công thực tế

        [Column(TypeName = "decimal(6,2)")]
        public decimal TotalHoursWorked { get; set; } = 0; // Tổng giờ làm việc

        [Column(TypeName = "decimal(5,2)")]
        public decimal OvertimeHours { get; set; } = 0; // Tổng giờ tăng ca

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalaryByWorkDays { get; set; } = 0; // Lương theo ngày công: BaseSalary / StandardDays * ActualDays

        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimePay { get; set; } = 0; // Tiền tăng ca: (BaseSalary / (StandardDays * 8)) * OvertimeHours * 1.5

        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowance { get; set; } = 0; // Phụ cấp chức vụ / trách nhiệm

        [Column(TypeName = "decimal(18,2)")]
        public decimal Bonus { get; set; } = 0; // Tiền thưởng KPI, doanh số

        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; } = 0; // Khấu trừ (đi trễ, vi phạm, bảo hiểm)

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; } = 0; // Thực lĩnh = SalaryByWorkDays + OvertimePay + Allowance + Bonus - Deductions

        [MaxLength(250)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
