using System;
using PhuKienMayTinh.Common.Enums;

namespace PhuKienMayTinh.BLL.DTOs
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = "Nam";
        public DateTime DateOfBirth { get; set; }
        public string IdentityCard { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }

        public decimal BaseSalary { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal LunchAllowance { get; set; }
        public decimal TravelAllowance { get; set; }
        public decimal ResponsibilityAllowance { get; set; }

        public EmployeeStatus Status { get; set; }
        public string StatusDisplay => Status switch
        {
            EmployeeStatus.Active => "Đang làm việc",
            EmployeeStatus.OnLeave => "Tạm nghỉ",
            EmployeeStatus.Resigned => "Đã nghỉ việc",
            _ => "Khác"
        };

        public string? Notes { get; set; }
    }

    public class AddOrUpdateEmployeeDTO
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = "Nam";
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-22);
        public string IdentityCard { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Department { get; set; } = "Bán hàng";
        public string Position { get; set; } = "Nhân viên Bán hàng";
        public DateTime HireDate { get; set; } = DateTime.Today;

        public decimal BaseSalary { get; set; } = 8000000m;
        public decimal HourlyRate { get; set; } = 35000m;
        public decimal LunchAllowance { get; set; } = 730000m;
        public decimal TravelAllowance { get; set; } = 300000m;
        public decimal ResponsibilityAllowance { get; set; } = 0m;

        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
        public string? Notes { get; set; }
    }
}
