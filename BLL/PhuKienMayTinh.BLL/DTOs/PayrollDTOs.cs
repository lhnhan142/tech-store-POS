using System;
using System.Collections.Generic;
using PhuKienMayTinh.Common.Enums;

namespace PhuKienMayTinh.BLL.DTOs
{
    public class PayrollDTO
    {
        public int Id { get; set; }
        public string PayrollCode { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime CreatedDate { get; set; }

        public PayrollStatus Status { get; set; }
        public string StatusDisplay => Status switch
        {
            PayrollStatus.Draft => "Bản nháp",
            PayrollStatus.Approved => "Đã duyệt",
            PayrollStatus.Paid => "Đã thanh toán",
            _ => "Khác"
        };

        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public decimal TotalBaseSalary { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal TotalBonus { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }

        public string? Note { get; set; }
        public List<PayrollDetailDTO> Details { get; set; } = new List<PayrollDetailDTO>();
    }

    public class PayrollDetailDTO
    {
        public int Id { get; set; }
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;

        // Công & Giờ
        public double StandardDays { get; set; } = 26;
        public double ActualWorkDays { get; set; }
        public double TotalHours { get; set; }
        public int TotalLateMinutes { get; set; }
        public int TotalLateCount { get; set; }
        public int TotalAbsenceCount { get; set; }

        // Lương
        public decimal BaseSalary { get; set; }
        public decimal ActualBaseSalary { get; set; }

        // Phụ cấp
        public decimal LunchAllowance { get; set; }
        public decimal TravelAllowance { get; set; }
        public decimal ResponsibilityAllowance { get; set; }
        public decimal TotalAllowance { get; set; }

        // Thưởng
        public decimal DiligenceBonus { get; set; }
        public decimal PerformanceBonus { get; set; }
        public decimal TotalBonus { get; set; }

        // Giảm trừ
        public decimal LatePenalty { get; set; }
        public decimal AbsencePenalty { get; set; }
        public decimal InsuranceDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }

        // Thực lĩnh
        public decimal NetSalary { get; set; }
        public string? Notes { get; set; }
    }

    public class CalculatePayrollRequestDTO
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
