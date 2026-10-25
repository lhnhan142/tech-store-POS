using Common.Enums;
using System;
using System.Collections.Generic;

namespace BLL.DTOs.Payroll
{
    public class PayrollSummaryDto
    {
        public int Id { get; set; }
        public string PayrollCode { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public string PeriodText => $"Tháng {Month:D2}/{Year}";
        public int StandardWorkDays { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public PayrollStatus Status { get; set; }
        public string StatusText => Status == PayrollStatus.Finalized ? "Đã chốt" : "Bản nháp";
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? FinalizedByName { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public string? Note { get; set; }
    }

    public class PayrollDetailDto
    {
        public int Id { get; set; }
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public int StandardWorkDays { get; set; }
        public decimal ActualWorkDays { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal SalaryByWorkDays { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Allowance { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public string? Note { get; set; }
    }

    public class GeneratePayrollRequestDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int StandardWorkDays { get; set; } = 26;
        public string? Note { get; set; }
    }

    public class UpdateSalaryAdjustmentsDto
    {
        public int DetailId { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public string? Note { get; set; }
    }
}
