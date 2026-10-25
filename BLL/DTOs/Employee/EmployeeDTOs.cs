using Common.Enums;
using System;

namespace BLL.DTOs.Employee
{
    public class EmployeeListItemDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Allowance { get; set; }
        public EmployeeStatus Status { get; set; }
        public string StatusText => Status switch
        {
            EmployeeStatus.Active => "Đang làm việc",
            EmployeeStatus.OnLeave => "Tạm nghỉ",
            EmployeeStatus.Resigned => "Đã thôi việc",
            _ => "Khác"
        };
        public string? Username { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class EmployeeDetailDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Allowance { get; set; }
        public EmployeeStatus Status { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateEmployeeDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; } = DateTime.Today;
        public decimal BaseSalary { get; set; }
        public decimal Allowance { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
        public int? UserId { get; set; }
    }

    public class UpdateEmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Allowance { get; set; }
        public EmployeeStatus Status { get; set; }
        public int? UserId { get; set; }
    }
}
