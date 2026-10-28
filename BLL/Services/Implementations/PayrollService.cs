using BLL.DTOs.Payroll;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using Common.Enums;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Implementations
{
    public class PayrollService : IPayrollService
    {
        private readonly IUnitOfWork _uow;

        public PayrollService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IReadOnlyList<PayrollSummaryDto>> GetPayrollsAsync(int year)
        {
            var payrolls = await _uow.Payrolls.GetPayrollsByYearAsync(year);
            return payrolls.Select(p => new PayrollSummaryDto
            {
                Id = p.Id,
                PayrollCode = p.PayrollCode,
                Month = p.Month,
                Year = p.Year,
                StandardWorkDays = p.StandardWorkDays,
                TotalEmployees = p.TotalEmployees,
                TotalGrossAmount = p.TotalGrossAmount,
                TotalNetAmount = p.TotalNetAmount,
                Status = p.Status,
                CreatedByName = p.CreatedByUser?.FullName ?? "Hệ thống",
                CreatedAt = p.CreatedAt,
                FinalizedByName = p.FinalizedByUser?.FullName,
                FinalizedAt = p.FinalizedAt,
                Note = p.Note
            }).ToList();
        }

        public async Task<PayrollSummaryDto?> GetPayrollByIdAsync(int id)
        {
            var p = await _uow.Payrolls.GetByIdWithDetailsAsync(id);
            if (p == null) return null;

            return new PayrollSummaryDto
            {
                Id = p.Id,
                PayrollCode = p.PayrollCode,
                Month = p.Month,
                Year = p.Year,
                StandardWorkDays = p.StandardWorkDays,
                TotalEmployees = p.TotalEmployees,
                TotalGrossAmount = p.TotalGrossAmount,
                TotalNetAmount = p.TotalNetAmount,
                Status = p.Status,
                CreatedByName = p.CreatedByUser?.FullName ?? "Hệ thống",
                CreatedAt = p.CreatedAt,
                FinalizedByName = p.FinalizedByUser?.FullName,
                FinalizedAt = p.FinalizedAt,
                Note = p.Note
            };
        }

        public async Task<IReadOnlyList<PayrollDetailDto>> GetPayrollDetailsAsync(int payrollId)
        {
            var payroll = await _uow.Payrolls.GetByIdWithDetailsAsync(payrollId);
            if (payroll == null) return new List<PayrollDetailDto>();

            return payroll.Details.Select(d => new PayrollDetailDto
            {
                Id = d.Id,
                PayrollId = d.PayrollId,
                EmployeeId = d.EmployeeId,
                EmployeeCode = d.EmployeeCode,
                EmployeeName = d.EmployeeName,
                Position = d.Position,
                BaseSalary = d.BaseSalary,
                StandardWorkDays = d.StandardWorkDays,
                ActualWorkDays = d.ActualWorkDays,
                TotalHoursWorked = d.TotalHoursWorked,
                OvertimeHours = d.OvertimeHours,
                SalaryByWorkDays = d.SalaryByWorkDays,
                OvertimePay = d.OvertimePay,
                Allowance = d.Allowance,
                Bonus = d.Bonus,
                Deductions = d.Deductions,
                NetSalary = d.NetSalary,
                Note = d.Note
            }).ToList();
        }

        public async Task<int> GeneratePayrollAsync(GeneratePayrollRequestDto dto, int currentUserId)
        {
            if (dto.Month < 1 || dto.Month > 12)
                throw new BusinessException("Tháng không hợp lệ (phải từ 1 đến 12).");

            if (dto.Year < 2020 || dto.Year > 2100)
                throw new BusinessException("Năm không hợp lệ.");

            if (dto.StandardWorkDays <= 0)
                throw new BusinessException("Số ngày công tiêu chuẩn phải lớn hơn 0.");

            var exists = await _uow.Payrolls.IsPeriodExistsAsync(dto.Month, dto.Year);
            if (exists)
            {
                throw new BusinessException(BusinessErrorCode.DuplicatePayrollPeriod, $"Bảng lương cho kỳ Tháng {dto.Month:D2}/{dto.Year} đã tồn tại trong hệ thống.");
            }

            int createdPayrollId = 0;

            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var payrollCode = $"BL-{dto.Year}-{dto.Month:D2}";
                var payroll = new Payroll
                {
                    PayrollCode = payrollCode,
                    Month = dto.Month,
                    Year = dto.Year,
                    StandardWorkDays = dto.StandardWorkDays,
                    Status = PayrollStatus.Draft,
                    CreatedByUserId = currentUserId,
                    Note = dto.Note,
                    CreatedAt = DateTime.Now
                };

                await _uow.Payrolls.AddAsync(payroll);
                await _uow.SaveChangesAsync();

                createdPayrollId = payroll.Id;
                await CalculateDetailsForPayrollAsync(payroll);
                await _uow.SaveChangesAsync();
            });

            return createdPayrollId;
        }

        public async Task RecalculatePayrollAsync(int payrollId)
        {
            var payroll = await _uow.Payrolls.GetByIdWithDetailsAsync(payrollId);
            if (payroll == null)
            {
                throw new BusinessException(BusinessErrorCode.PayrollNotFound, "Không tìm thấy bảng lương.");
            }

            if (payroll.Status == PayrollStatus.Finalized)
            {
                throw new BusinessException(BusinessErrorCode.PayrollAlreadyFinalized, "Bảng lương đã chốt. Không thể tính toán lại dữ liệu.");
            }

            await _uow.ExecuteInTransactionAsync(async () =>
            {
                // Xóa chi tiết cũ và tạo lại
                foreach (var detail in payroll.Details.ToList())
                {
                    _uow.PayrollDetails.Delete(detail);
                }
                await _uow.SaveChangesAsync();

                await CalculateDetailsForPayrollAsync(payroll);
                payroll.UpdatedAt = DateTime.Now;
                _uow.Payrolls.Update(payroll);
                await _uow.SaveChangesAsync();
            });
        }

        public async Task FinalizePayrollAsync(int payrollId, int finalizedByUserId)
        {
            var payroll = await _uow.Payrolls.GetByIdWithDetailsAsync(payrollId);
            if (payroll == null)
            {
                throw new BusinessException(BusinessErrorCode.PayrollNotFound, "Không tìm thấy bảng lương.");
            }

            if (payroll.Status == PayrollStatus.Finalized)
            {
                throw new BusinessException(BusinessErrorCode.PayrollAlreadyFinalized, "Bảng lương đã được chốt trước đó.");
            }

            payroll.Status = PayrollStatus.Finalized;
            payroll.FinalizedAt = DateTime.Now;
            payroll.FinalizedByUserId = finalizedByUserId;
            payroll.UpdatedAt = DateTime.Now;

            _uow.Payrolls.Update(payroll);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateDetailAdjustmentsAsync(UpdateSalaryAdjustmentsDto dto)
        {
            var detail = await _uow.PayrollDetails.GetByIdAsync(dto.DetailId);
            if (detail == null)
            {
                throw new BusinessException(BusinessErrorCode.NotFound, "Không tìm thấy chi tiết lương của nhân viên.");
            }

            var payroll = await _uow.Payrolls.GetByIdAsync(detail.PayrollId);
            if (payroll != null && payroll.Status == PayrollStatus.Finalized)
            {
                throw new BusinessException(BusinessErrorCode.PayrollAlreadyFinalized, "Bảng lương đã chốt. Không được phép điều chỉnh thưởng/khấu trừ.");
            }

            if (dto.Bonus < 0) throw new BusinessException("Tiền thưởng không được âm.");
            if (dto.Deductions < 0) throw new BusinessException("Khấu trừ không được âm.");

            detail.Bonus = dto.Bonus;
            detail.Deductions = dto.Deductions;
            detail.Note = dto.Note;

            // Tính lại thực lĩnh
            detail.NetSalary = detail.SalaryByWorkDays + detail.OvertimePay + detail.Allowance + detail.Bonus - detail.Deductions;
            if (detail.NetSalary < 0) detail.NetSalary = 0;
            detail.UpdatedAt = DateTime.Now;

            _uow.PayrollDetails.Update(detail);

            // Cập nhật lại tổng của bảng lương
            if (payroll != null)
            {
                var allDetails = (await _uow.PayrollDetails.FindAsync(d => d.PayrollId == payroll.Id)).ToList();
                payroll.TotalGrossAmount = allDetails.Sum(d => d.SalaryByWorkDays + d.OvertimePay + d.Allowance + d.Bonus);
                payroll.TotalNetAmount = allDetails.Sum(d => d.NetSalary);
                payroll.UpdatedAt = DateTime.Now;
                _uow.Payrolls.Update(payroll);
            }

            await _uow.SaveChangesAsync();
        }

        public async Task DeletePayrollAsync(int payrollId)
        {
            var p = await _uow.Payrolls.GetByIdWithDetailsAsync(payrollId);
            if (p == null) throw new BusinessException(BusinessErrorCode.PayrollNotFound, "Không tìm thấy bảng lương.");
            if (p.Status == PayrollStatus.Finalized)
                throw new BusinessException(BusinessErrorCode.PayrollAlreadyFinalized, "Không thể xóa bảng lương đã chốt.");

            foreach (var detail in p.Details.ToList())
            {
                _uow.PayrollDetails.Delete(detail);
            }
            _uow.Payrolls.Delete(p);
            await _uow.SaveChangesAsync();
        }

        private async Task CalculateDetailsForPayrollAsync(Payroll payroll)
        {
            var activeEmployees = (await _uow.Employees.GetActiveEmployeesAsync()).ToList();

            decimal totalGross = 0;
            decimal totalNet = 0;

            foreach (var emp in activeEmployees)
            {
                // Lấy chấm công của nhân viên trong tháng/năm này
                var attendances = (await _uow.Attendances.FindAsync(a =>
                    (a.EmployeeId == emp.Id || a.UserId == emp.UserId) &&
                    a.Date.Month == payroll.Month &&
                    a.Date.Year == payroll.Year)).ToList();

                // Số ngày công thực tế (có checkin/checkout hoặc HoursWorked > 0)
                decimal actualDays = attendances.Count(a => a.HoursWorked > 0 || (a.CheckIn != null && a.CheckOut != null));
                decimal totalHours = attendances.Sum(a => a.HoursWorked);
                decimal overtimeHours = attendances.Sum(a => a.OvertimeHours);

                // Lương theo ngày công: (Lương cơ bản / ngày chuẩn) * ngày làm việc
                decimal salaryByWorkDays = Math.Round((emp.BaseSalary / payroll.StandardWorkDays) * actualDays, 0);

                // Tiền tăng ca: (Lương cơ bản / (ngày chuẩn * 8)) * giờ tăng ca * 1.5
                decimal hourlyRate = emp.BaseSalary / (payroll.StandardWorkDays * 8m);
                decimal overtimePay = Math.Round(hourlyRate * overtimeHours * 1.5m, 0);

                decimal allowance = emp.Allowance;
                decimal bonus = 0;
                decimal deductions = 0;

                decimal netSalary = salaryByWorkDays + overtimePay + allowance + bonus - deductions;
                if (netSalary < 0) netSalary = 0;

                var detail = new PayrollDetail
                {
                    PayrollId = payroll.Id,
                    EmployeeId = emp.Id,
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = emp.FullName,
                    Position = emp.Position,
                    BaseSalary = emp.BaseSalary,
                    StandardWorkDays = payroll.StandardWorkDays,
                    ActualWorkDays = actualDays,
                    TotalHoursWorked = totalHours,
                    OvertimeHours = overtimeHours,
                    SalaryByWorkDays = salaryByWorkDays,
                    OvertimePay = overtimePay,
                    Allowance = allowance,
                    Bonus = bonus,
                    Deductions = deductions,
                    NetSalary = netSalary,
                    CreatedAt = DateTime.Now
                };

                await _uow.PayrollDetails.AddAsync(detail);

                totalGross += (salaryByWorkDays + overtimePay + allowance + bonus);
                totalNet += netSalary;
            }

            payroll.TotalEmployees = activeEmployees.Count;
            payroll.TotalGrossAmount = totalGross;
            payroll.TotalNetAmount = totalNet;
        }
    }
}
