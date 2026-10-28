using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace DAL.Entities
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int? EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan? CheckIn { get; set; }

        public TimeSpan? CheckOut { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal HoursWorked { get; set; } = 0; // Giờ làm việc trong ngày

        [Column(TypeName = "decimal(5,2)")]
        public decimal OvertimeHours { get; set; } = 0; // Giờ tăng ca

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [MaxLength(250)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}