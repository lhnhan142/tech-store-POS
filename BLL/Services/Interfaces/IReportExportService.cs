using BLL.DTOs.Attendance;
using BLL.DTOs.Dashboard;
using BLL.DTOs.Employee;
using BLL.DTOs.Payroll;
using System;
using System.Collections.Generic;

namespace BLL.Services.Interfaces
{
    public interface IReportExportService
    {
        // 1. Báo cáo Nhân sự
        void ExportEmployeeListToExcel(string filePath, IEnumerable<EmployeeListItemDto> employees);
        void ExportEmployeeListToPdf(string filePath, IEnumerable<EmployeeListItemDto> employees);

        // 2. Báo cáo Chấm công
        void ExportAttendanceReportToExcel(string filePath, IEnumerable<AttendanceRecordDto> records, DateTime from, DateTime to);
        void ExportAttendanceReportToPdf(string filePath, IEnumerable<AttendanceRecordDto> records, DateTime from, DateTime to);

        // 3. Báo cáo Bảng lương
        void ExportPayrollReportToExcel(string filePath, PayrollSummaryDto summary, IEnumerable<PayrollDetailDto> details);
        void ExportPayrollReportToPdf(string filePath, PayrollSummaryDto summary, IEnumerable<PayrollDetailDto> details);

        // 4. Báo cáo Kinh doanh & Tồn kho
        void ExportBusinessReportToExcel(string filePath, DashboardSummaryDto summary, IEnumerable<TopSellingProductDto> topProducts, DateTime from, DateTime to);
        void ExportBusinessReportToPdf(string filePath, DashboardSummaryDto summary, IEnumerable<TopSellingProductDto> topProducts, DateTime from, DateTime to);
    }
}
