using System;

namespace PhuKienMayTinh.DAL.Entities
{
    public class PayrollDetail
    {
        public int Id { get; set; }
        public int PayrollId { get; set; }
        public virtual Payroll Payroll { get; set; } = null!;

        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        // Thống kê chấm công
        public double StandardDays { get; set; } = 26;
        public double ActualWorkDays { get; set; }
        public double TotalHours { get; set; }
        public int TotalLateMinutes { get; set; }
        public int TotalLateCount { get; set; }
        public int TotalAbsenceCount { get; set; }

        // Chi tiết lương
        public decimal BaseSalary { get; set; }
        public decimal ActualBaseSalary { get; set; } // (BaseSalary / StandardDays) * ActualWorkDays

        // Phụ cấp
        public decimal LunchAllowance { get; set; }
        public decimal TravelAllowance { get; set; }
        public decimal ResponsibilityAllowance { get; set; }
        public decimal TotalAllowance { get; set; }

        // Thưởng
        public decimal DiligenceBonus { get; set; } // Thưởng chuyên cần
        public decimal PerformanceBonus { get; set; } // Thưởng hiệu suất / KPI
        public decimal TotalBonus { get; set; }

        // Giảm trừ / Phạt
        public decimal LatePenalty { get; set; }
        public decimal AbsencePenalty { get; set; }
        public decimal InsuranceDeduction { get; set; } // Bảo hiểm xã hội 10.5% nếu có
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }

        // Thực nhận = ActualBaseSalary + TotalAllowance + TotalBonus - TotalDeductions
        public decimal NetSalary { get; set; }

        public string? Notes { get; set; }
    }
}
