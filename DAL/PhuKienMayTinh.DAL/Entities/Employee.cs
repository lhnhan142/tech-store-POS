using System;
using System.Collections.Generic;
using PhuKienMayTinh.Common.Enums;

namespace PhuKienMayTinh.DAL.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty; // NV001, NV002...
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = "Nam"; // Nam, Nữ, Khác
        public DateTime DateOfBirth { get; set; }
        public string IdentityCard { get; set; } = string.Empty; // CCCD / CMND
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Department { get; set; } = "Bán hàng"; // Bán hàng, Kho, Kỹ thuật, Quản lý, Kế toán
        public string Position { get; set; } = "Nhân viên bán hàng";
        public DateTime HireDate { get; set; } = DateTime.Today;

        // Chế độ lương
        public decimal BaseSalary { get; set; } = 8000000m; // Lương cơ bản tháng
        public decimal HourlyRate { get; set; } = 35000m;    // Lương theo giờ
        public decimal LunchAllowance { get; set; } = 730000m; // Phụ cấp ăn trưa
        public decimal TravelAllowance { get; set; } = 300000m; // Phụ cấp đi lại
        public decimal ResponsibilityAllowance { get; set; } = 0m; // Phụ cấp trách nhiệm

        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
