using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.BLL.Exceptions;
using PhuKienMayTinh.Common.Constants;
using PhuKienMayTinh.Common.Enums;
using PhuKienMayTinh.Common.Utilities;
using PhuKienMayTinh.DAL.Entities;
using PhuKienMayTinh.DAL.Repositories.Interfaces;

namespace PhuKienMayTinh.BLL.Services.Interfaces
{
    public interface IPayrollService
    {
        Task<PayrollDTO> CalculatePayrollAsync(CalculatePayrollRequestDTO request);
        Task<PayrollDTO?> GetPayrollByIdAsync(int id);
        Task<PayrollDTO?> GetPayrollByMonthYearAsync(int month, int year);
        Task<IEnumerable<PayrollDTO>> GetPayrollsAsync(int? year);
        Task<bool> ApprovePayrollAsync(int payrollId, string approvedBy);
        Task<bool> MarkAsPaidAsync(int payrollId);
        Task ExportPayrollToExcelAsync(int payrollId, string filePath);
        Task ExportPayslipToPdfAsync(int payrollDetailId, string filePath);
    }
}

namespace PhuKienMayTinh.BLL.Services.Implementations
{
    using PhuKienMayTinh.BLL.Services.Interfaces;

    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IAttendanceRepository _attendanceRepo;

        public PayrollService(IPayrollRepository payrollRepo, IEmployeeRepository employeeRepo, IAttendanceRepository attendanceRepo)
        {
            _payrollRepo = payrollRepo;
            _employeeRepo = employeeRepo;
            _attendanceRepo = attendanceRepo;
        }

        public async Task<PayrollDTO> CalculatePayrollAsync(CalculatePayrollRequestDTO request)
        {
            if (request.Month < 1 || request.Month > 12)
            {
                throw new ValidationException("Tháng không hợp lệ (1 - 12).");
            }
            if (request.Year < 2000 || request.Year > 2100)
            {
                throw new ValidationException("Năm không hợp lệ.");
            }

            var config = await _payrollRepo.GetActiveSalaryConfigAsync() ?? new SalaryConfiguration();

            // Kiểm tra xem đã có bảng lương tháng này chưa
            var existingPayroll = await _payrollRepo.GetPayrollByMonthYearAsync(request.Month, request.Year);
            if (existingPayroll != null && existingPayroll.Status == PayrollStatus.Paid)
            {
                throw new BusinessException($"Bảng lương Tháng {request.Month}/{request.Year} đã được thanh toán, không thể tính lại.");
            }

            var employees = (await _employeeRepo.GetAllAsync()).Where(e => e.Status != EmployeeStatus.Resigned).ToList();
            if (!employees.Any())
            {
                throw new BusinessException("Không có nhân viên nào trong hệ thống để tính lương.");
            }

            var payroll = existingPayroll ?? new Payroll
            {
                PayrollCode = $"BL-{request.Year}-{request.Month:D2}",
                Month = request.Month,
                Year = request.Year,
                CreatedDate = DateTime.Now,
                Status = PayrollStatus.Draft,
                Note = string.IsNullOrWhiteSpace(request.Note) ? $"Bảng tính lương Tháng {request.Month}/{request.Year}" : request.Note
            };

            // Xóa chi tiết cũ nếu tính lại
            if (existingPayroll != null)
            {
                payroll.Details.Clear();
            }

            foreach (var emp in employees)
            {
                var monthlyAttendances = (await _attendanceRepo.GetMonthlyAttendancesAsync(emp.Id, request.Month, request.Year)).ToList();

                // 1. Ngày công thực tế
                double standardDays = config.StandardWorkDays;
                double actualDays = monthlyAttendances.Count(a =>
                    a.Status == AttendanceStatus.Present ||
                    a.Status == AttendanceStatus.Late ||
                    a.Status == AttendanceStatus.EarlyLeave ||
                    a.Status == AttendanceStatus.LateAndEarlyLeave);

                // Giả định nếu nhân viên chưa có dữ liệu chấm công nhưng đang Active thì tính đủ công mẫu
                if (!monthlyAttendances.Any() && emp.Status == EmployeeStatus.Active)
                {
                    actualDays = standardDays;
                }

                double totalHours = monthlyAttendances.Sum(a => a.HoursWorked);
                if (totalHours <= 0 && actualDays > 0)
                {
                    totalHours = actualDays * config.StandardWorkHoursPerDay;
                }

                int totalLateMinutes = monthlyAttendances.Sum(a => a.LateMinutes);
                int totalLateCount = monthlyAttendances.Count(a => a.LateMinutes > 0);
                int unexcusedAbsence = monthlyAttendances.Count(a => a.Status == AttendanceStatus.UnexcusedAbsence);

                // 2. Lương cơ bản theo công
                decimal actualBase = Math.Round((emp.BaseSalary / (decimal)standardDays) * (decimal)actualDays, 0);

                // 3. Phụ cấp
                decimal totalAllowance = emp.LunchAllowance + emp.TravelAllowance + emp.ResponsibilityAllowance;

                // 4. Thưởng
                decimal diligenceBonus = (actualDays >= standardDays && totalLateCount == 0 && unexcusedAbsence == 0)
                    ? config.DiligenceBonus : 0m;
                decimal performanceBonus = emp.Department == "Bán hàng" ? 1000000m : (emp.Department == "Quản lý" ? 2000000m : 500000m);
                decimal totalBonus = diligenceBonus + performanceBonus;

                // 5. Khấu trừ / Phạt
                decimal latePenalty = totalLateMinutes * config.LatePenaltyPerMinute;
                decimal absencePenalty = unexcusedAbsence * config.AbsencePenaltyPerDay;
                decimal insurance = Math.Round(actualBase * 0.105m, 0); // 10.5% BHXH, BHYT, BHTN
                decimal totalDeductions = latePenalty + absencePenalty + insurance;

                // 6. Thực lĩnh
                decimal netSalary = actualBase + totalAllowance + totalBonus - totalDeductions;
                if (netSalary < 0) netSalary = 0;

                var detail = new PayrollDetail
                {
                    EmployeeId = emp.Id,
                    StandardDays = standardDays,
                    ActualWorkDays = actualDays,
                    TotalHours = totalHours,
                    TotalLateMinutes = totalLateMinutes,
                    TotalLateCount = totalLateCount,
                    TotalAbsenceCount = unexcusedAbsence,
                    BaseSalary = emp.BaseSalary,
                    ActualBaseSalary = actualBase,
                    LunchAllowance = emp.LunchAllowance,
                    TravelAllowance = emp.TravelAllowance,
                    ResponsibilityAllowance = emp.ResponsibilityAllowance,
                    TotalAllowance = totalAllowance,
                    DiligenceBonus = diligenceBonus,
                    PerformanceBonus = performanceBonus,
                    TotalBonus = totalBonus,
                    LatePenalty = latePenalty,
                    AbsencePenalty = absencePenalty,
                    InsuranceDeduction = insurance,
                    OtherDeductions = 0m,
                    TotalDeductions = totalDeductions,
                    NetSalary = netSalary,
                    Notes = $"Tháng {request.Month}/{request.Year}"
                };

                payroll.Details.Add(detail);
            }

            payroll.TotalBaseSalary = payroll.Details.Sum(d => d.ActualBaseSalary);
            payroll.TotalAllowance = payroll.Details.Sum(d => d.TotalAllowance);
            payroll.TotalBonus = payroll.Details.Sum(d => d.TotalBonus);
            payroll.TotalDeductions = payroll.Details.Sum(d => d.TotalDeductions);
            payroll.TotalNetSalary = payroll.Details.Sum(d => d.NetSalary);

            if (existingPayroll == null)
            {
                await _payrollRepo.AddAsync(payroll);
            }
            else
            {
                _payrollRepo.Update(payroll);
            }

            await _payrollRepo.SaveChangesAsync();

            var loaded = await _payrollRepo.GetPayrollWithDetailsAsync(payroll.Id);
            return MapToDTO(loaded!);
        }

        public async Task<PayrollDTO?> GetPayrollByIdAsync(int id)
        {
            var p = await _payrollRepo.GetPayrollWithDetailsAsync(id);
            return p == null ? null : MapToDTO(p);
        }

        public async Task<PayrollDTO?> GetPayrollByMonthYearAsync(int month, int year)
        {
            var p = await _payrollRepo.GetPayrollByMonthYearAsync(month, year);
            return p == null ? null : MapToDTO(p);
        }

        public async Task<IEnumerable<PayrollDTO>> GetPayrollsAsync(int? year)
        {
            var list = await _payrollRepo.GetPayrollsAsync(year);
            return list.Select(p => new PayrollDTO
            {
                Id = p.Id,
                PayrollCode = p.PayrollCode,
                Month = p.Month,
                Year = p.Year,
                CreatedDate = p.CreatedDate,
                Status = p.Status,
                ApprovedBy = p.ApprovedBy,
                ApprovedDate = p.ApprovedDate,
                TotalBaseSalary = p.TotalBaseSalary,
                TotalAllowance = p.TotalAllowance,
                TotalBonus = p.TotalBonus,
                TotalDeductions = p.TotalDeductions,
                TotalNetSalary = p.TotalNetSalary,
                Note = p.Note
            });
        }

        public async Task<bool> ApprovePayrollAsync(int payrollId, string approvedBy)
        {
            var payroll = await _payrollRepo.GetByIdAsync(payrollId);
            if (payroll == null) throw new NotFoundException($"Không tìm thấy bảng lương ID = {payrollId}");

            payroll.Status = PayrollStatus.Approved;
            payroll.ApprovedBy = approvedBy;
            payroll.ApprovedDate = DateTime.Now;

            _payrollRepo.Update(payroll);
            return await _payrollRepo.SaveChangesAsync();
        }

        public async Task<bool> MarkAsPaidAsync(int payrollId)
        {
            var payroll = await _payrollRepo.GetByIdAsync(payrollId);
            if (payroll == null) throw new NotFoundException($"Không tìm thấy bảng lương ID = {payrollId}");

            payroll.Status = PayrollStatus.Paid;
            _payrollRepo.Update(payroll);
            return await _payrollRepo.SaveChangesAsync();
        }

        public async Task ExportPayrollToExcelAsync(int payrollId, string filePath)
        {
            var payroll = await _payrollRepo.GetPayrollWithDetailsAsync(payrollId);
            if (payroll == null) throw new NotFoundException("Bảng lương không tồn tại.");

            var dt = new DataTable("BangLuongChiTiet");
            dt.Columns.Add("STT", typeof(int));
            dt.Columns.Add("Mã NV", typeof(string));
            dt.Columns.Add("Họ và Tên", typeof(string));
            dt.Columns.Add("Chức vụ", typeof(string));
            dt.Columns.Add("Công chuẩn", typeof(double));
            dt.Columns.Add("Công thực tế", typeof(double));
            dt.Columns.Add("Lương cơ bản", typeof(decimal));
            dt.Columns.Add("Lương theo công", typeof(decimal));
            dt.Columns.Add("Tổng phụ cấp", typeof(decimal));
            dt.Columns.Add("Thưởng", typeof(decimal));
            dt.Columns.Add("Phạt trễ/vắng", typeof(decimal));
            dt.Columns.Add("BHXH (10.5%)", typeof(decimal));
            dt.Columns.Add("Tổng khấu trừ", typeof(decimal));
            dt.Columns.Add("THỰC LĨNH", typeof(decimal));

            int stt = 1;
            foreach (var d in payroll.Details)
            {
                dt.Rows.Add(
                    stt++,
                    d.Employee.EmployeeCode,
                    d.Employee.FullName,
                    d.Employee.Position,
                    d.StandardDays,
                    d.ActualWorkDays,
                    d.BaseSalary,
                    d.ActualBaseSalary,
                    d.TotalAllowance,
                    d.TotalBonus,
                    d.LatePenalty + d.AbsencePenalty,
                    d.InsuranceDeduction,
                    d.TotalDeductions,
                    d.NetSalary
                );
            }

            string title = $"BẢNG THANH TOÁN LƯƠNG NHÂN VIÊN THÁNG {payroll.Month:D2}/{payroll.Year}";
            ExcelHelper.ExportToExcel(dt, title, filePath);
        }

        public async Task ExportPayslipToPdfAsync(int payrollDetailId, string filePath)
        {
            var payroll = await _payrollRepo.GetAllAsync();
            PayrollDetail? detail = null;
            Payroll? parentPayroll = null;

            foreach (var p in payroll)
            {
                var full = await _payrollRepo.GetPayrollWithDetailsAsync(p.Id);
                var match = full?.Details.FirstOrDefault(d => d.Id == payrollDetailId);
                if (match != null)
                {
                    detail = match;
                    parentPayroll = full;
                    break;
                }
            }

            if (detail == null || parentPayroll == null)
            {
                throw new NotFoundException("Không tìm thấy chi tiết phiếu lương của nhân viên.");
            }

            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var document = new Document(PageSize.A4, 36f, 36f, 36f, 36f);
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, new BaseColor(37, 99, 235));
                var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(30, 41, 59));
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.Black);
                var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.Black);
                var fontSub = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.Gray);

                // Header
                document.Add(new Paragraph(AppConstants.StoreName, fontHeader) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph($"{AppConstants.StoreAddress} | SĐT: {AppConstants.StorePhone}", fontSub) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 10f });

                var pTitle = new Paragraph($"PHIẾU LƯƠNG CÁ NHÂN (PAYSLIP)\nTHÁNG {parentPayroll.Month:D2}/{parentPayroll.Year}", fontTitle)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 15f
                };
                document.Add(pTitle);

                // Thông tin nhân viên
                var infoTable = new PdfPTable(2) { WidthPercentage = 100f, SpacingAfter = 15f };
                infoTable.SetWidths(new float[] { 50f, 50f });

                infoTable.AddCell(CreateCleanCell($"Mã NV: {detail.Employee.EmployeeCode}", fontBold));
                infoTable.AddCell(CreateCleanCell($"Họ và tên: {detail.Employee.FullName}", fontBold));
                infoTable.AddCell(CreateCleanCell($"Phòng ban: {detail.Employee.Department}", fontNormal));
                infoTable.AddCell(CreateCleanCell($"Chức vụ: {detail.Employee.Position}", fontNormal));
                infoTable.AddCell(CreateCleanCell($"Số ngày công chuẩn: {detail.StandardDays} ngày", fontNormal));
                infoTable.AddCell(CreateCleanCell($"Số ngày công thực tế: {detail.ActualWorkDays} ngày ({detail.TotalHours} giờ)", fontNormal));
                document.Add(infoTable);

                // Bảng chi tiết lương (2 cột: Thu nhập & Khấu trừ)
                var salaryTable = new PdfPTable(2) { WidthPercentage = 100f, SpacingAfter = 15f };
                salaryTable.SetWidths(new float[] { 50f, 50f });

                // Header cột
                salaryTable.AddCell(CreateHeaderCell("I. CÁC KHOẢN THU NHẬP", fontHeader));
                salaryTable.AddCell(CreateHeaderCell("II. CÁC KHOẢN KHẤU TRỪ", fontHeader));

                salaryTable.AddCell(CreateRowCell("Lương cơ bản:", detail.BaseSalary.ToString("N0") + " đ", fontNormal));
                salaryTable.AddCell(CreateRowCell("Trừ đi trễ (" + detail.TotalLateMinutes + "p):", detail.LatePenalty.ToString("N0") + " đ", fontNormal));

                salaryTable.AddCell(CreateRowCell("Lương theo ngày công thực tế:", detail.ActualBaseSalary.ToString("N0") + " đ", fontNormal));
                salaryTable.AddCell(CreateRowCell("Trừ vắng không phép (" + detail.TotalAbsenceCount + "b):", detail.AbsencePenalty.ToString("N0") + " đ", fontNormal));

                salaryTable.AddCell(CreateRowCell("Phụ cấp ăn trưa:", detail.LunchAllowance.ToString("N0") + " đ", fontNormal));
                salaryTable.AddCell(CreateRowCell("BHXH, BHYT, BHTN (10.5%):", detail.InsuranceDeduction.ToString("N0") + " đ", fontNormal));

                salaryTable.AddCell(CreateRowCell("Phụ cấp xăng xe / đi lại:", detail.TravelAllowance.ToString("N0") + " đ", fontNormal));
                salaryTable.AddCell(CreateRowCell("Khấu trừ khác:", detail.OtherDeductions.ToString("N0") + " đ", fontNormal));

                salaryTable.AddCell(CreateRowCell("Phụ cấp trách nhiệm:", detail.ResponsibilityAllowance.ToString("N0") + " đ", fontNormal));
                salaryTable.AddCell(CreateRowCell("", "", fontNormal));

                salaryTable.AddCell(CreateRowCell("Thưởng chuyên cần:", detail.DiligenceBonus.ToString("N0") + " đ", fontNormal));
                salaryTable.AddCell(CreateRowCell("", "", fontNormal));

                salaryTable.AddCell(CreateRowCell("Thưởng hiệu suất (KPI):", detail.PerformanceBonus.ToString("N0") + " đ", fontNormal));
                salaryTable.AddCell(CreateRowCell("", "", fontNormal));

                // Tổng cộng từng bên
                decimal totalGross = detail.ActualBaseSalary + detail.TotalAllowance + detail.TotalBonus;
                salaryTable.AddCell(CreateRowCell("TỔNG THU NHẬP:", totalGross.ToString("N0") + " đ", fontBold, new BaseColor(241, 245, 249)));
                salaryTable.AddCell(CreateRowCell("TỔNG KHẤU TRỪ:", detail.TotalDeductions.ToString("N0") + " đ", fontBold, new BaseColor(254, 242, 242)));

                document.Add(salaryTable);

                // Tổng thực nhận
                var netTable = new PdfPTable(1) { WidthPercentage = 100f, SpacingAfter = 25f };
                var netCell = new PdfPCell(new Phrase($"LƯƠNG THỰC LĨNH (NET): {detail.NetSalary:N0} VNĐ", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, new BaseColor(16, 185, 129))))
                {
                    BackgroundColor = new BaseColor(236, 253, 245),
                    BorderColor = new BaseColor(16, 185, 129),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 12f
                };
                netTable.AddCell(netCell);
                document.Add(netTable);

                // Chữ ký 2 bên
                var signTable = new PdfPTable(2) { WidthPercentage = 100f };
                signTable.AddCell(new PdfPCell(new Phrase("Người nhận tiền lương\n(Ký và ghi rõ họ tên)\n\n\n\n" + detail.Employee.FullName, fontNormal))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                signTable.AddCell(new PdfPCell(new Phrase("Đại diện Cửa hàng / Kế toán\n(Ký và ghi rõ họ tên)\n\n\n\n" + (parentPayroll.ApprovedBy ?? "Kế toán trưởng"), fontNormal))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                document.Add(signTable);

                document.Close();
                writer.Close();
            }
        }

        private static PdfPCell CreateCleanCell(string text, Font font)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                Border = Rectangle.NO_BORDER,
                Padding = 4f
            };
        }

        private static PdfPCell CreateHeaderCell(string text, Font font)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = new BaseColor(241, 245, 249),
                BorderColor = new BaseColor(203, 213, 225),
                Padding = 6f
            };
        }

        private static PdfPCell CreateRowCell(string title, string value, Font font, BaseColor? bgColor = null)
        {
            var p = new Paragraph();
            if (!string.IsNullOrEmpty(title))
            {
                p.Add(new Chunk(title + " ", font));
                p.Add(new Chunk(value, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, font.Size, font.Color)));
            }
            var cell = new PdfPCell(p)
            {
                Padding = 5f,
                BorderColor = new BaseColor(226, 232, 240)
            };
            if (bgColor != null) cell.BackgroundColor = bgColor;
            return cell;
        }

        private static PayrollDTO MapToDTO(Payroll p)
        {
            return new PayrollDTO
            {
                Id = p.Id,
                PayrollCode = p.PayrollCode,
                Month = p.Month,
                Year = p.Year,
                CreatedDate = p.CreatedDate,
                Status = p.Status,
                ApprovedBy = p.ApprovedBy,
                ApprovedDate = p.ApprovedDate,
                TotalBaseSalary = p.TotalBaseSalary,
                TotalAllowance = p.TotalAllowance,
                TotalBonus = p.TotalBonus,
                TotalDeductions = p.TotalDeductions,
                TotalNetSalary = p.TotalNetSalary,
                Note = p.Note,
                Details = p.Details.Select(d => new PayrollDetailDTO
                {
                    Id = d.Id,
                    PayrollId = d.PayrollId,
                    EmployeeId = d.EmployeeId,
                    EmployeeCode = d.Employee.EmployeeCode,
                    EmployeeName = d.Employee.FullName,
                    Department = d.Employee.Department,
                    Position = d.Employee.Position,
                    StandardDays = d.StandardDays,
                    ActualWorkDays = d.ActualWorkDays,
                    TotalHours = d.TotalHours,
                    TotalLateMinutes = d.TotalLateMinutes,
                    TotalLateCount = d.TotalLateCount,
                    TotalAbsenceCount = d.TotalAbsenceCount,
                    BaseSalary = d.BaseSalary,
                    ActualBaseSalary = d.ActualBaseSalary,
                    LunchAllowance = d.LunchAllowance,
                    TravelAllowance = d.TravelAllowance,
                    ResponsibilityAllowance = d.ResponsibilityAllowance,
                    TotalAllowance = d.TotalAllowance,
                    DiligenceBonus = d.DiligenceBonus,
                    PerformanceBonus = d.PerformanceBonus,
                    TotalBonus = d.TotalBonus,
                    LatePenalty = d.LatePenalty,
                    AbsencePenalty = d.AbsencePenalty,
                    InsuranceDeduction = d.InsuranceDeduction,
                    OtherDeductions = d.OtherDeductions,
                    TotalDeductions = d.TotalDeductions,
                    NetSalary = d.NetSalary,
                    Notes = d.Notes
                }).ToList()
            };
        }
    }
}
