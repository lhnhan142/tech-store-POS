using System;
using System.Collections.Generic;

namespace BLL.DTOs.Report
{
    public enum ReportType
    {
        HumanResource,  // Báo cáo nhân sự
        Attendance,     // Báo cáo chấm công
        Payroll,        // Báo cáo lương
        Business        // Báo cáo kinh doanh & tồn kho
    }

    public class ReportFilterDto
    {
        public ReportType Type { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
