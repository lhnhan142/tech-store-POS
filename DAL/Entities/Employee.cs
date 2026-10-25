using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace DAL.Entities
{
    /// <summary>
    /// Thực thể hồ sơ nhân viên trong hệ thống
    /// </summary>
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public required string EmployeeCode { get; set; } // Mã NV: NV001, NV002,...

        [Required, MaxLength(100)]
        public required string FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; } // Nam / Nữ

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        [Required, MaxLength(50)]
        public required string Position { get; set; } // Chức vụ

        public DateTime HireDate { get; set; } = DateTime.Today; // Ngày vào làm

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; } // Lương cơ bản

        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowance { get; set; } = 0; // Phụ cấp cố định

        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation collections
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
    }
}
