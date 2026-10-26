using BLL.DTOs.Attendance;
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
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _uow;

        public AttendanceService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IReadOnlyList<AttendanceRecordDto>> GetHistoryAsync(DateTime fromDate, DateTime toDate, int? employeeId = null)
        {
            var records = await _uow.Attendances.GetHistoryAsync(fromDate, toDate, employeeId);
            return records.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<AttendanceRecordDto>> GetAbnormalRecordsAsync(DateTime fromDate, DateTime toDate)
        {
            var records = await _uow.Attendances.GetAbnormalAttendancesAsync(fromDate, toDate);
            return records.Select(MapToDto).ToList();
        }

        public async Task<AttendanceSummaryDto> GetSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            var records = (await _uow.Attendances.GetHistoryAsync(fromDate, toDate)).ToList();

            return new AttendanceSummaryDto
            {
                TotalPresent = records.Count(r => r.Status == AttendanceStatus.Present),
                TotalLate = records.Count(r => r.Status == AttendanceStatus.Late),
                TotalEarlyLeave = records.Count(r => r.Status == AttendanceStatus.EarlyLeave),
                TotalOvertime = records.Count(r => r.Status == AttendanceStatus.Overtime),
                TotalAbsent = records.Count(r => r.Status == AttendanceStatus.Absent),
                TotalHoursWorked = records.Sum(r => r.HoursWorked),
                TotalOvertimeHours = records.Sum(r => r.OvertimeHours)
            };
        }

        public async Task CheckInAsync(CheckInRequestDto dto)
        {
            var employee = await _uow.Employees.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
            {
                throw new BusinessException(BusinessErrorCode.EmployeeNotFound, "Không tìm thấy nhân viên chấm công.");
            }

            var existing = await _uow.Attendances.GetByEmployeeAndDateAsync(dto.EmployeeId, dto.Date);
            if (existing != null && existing.CheckIn != null)
            {
                throw new BusinessException(BusinessErrorCode.AlreadyCheckedIn, $"Nhân viên {employee.FullName} đã check-in ngày hôm nay lúc {existing.CheckIn.Value:hh\\:mm}.");
            }

            // Quy tắc: Bắt đầu ca 08:00. Sau 08:15 coi là đi muộn
            var standardStart = new TimeSpan(8, 0, 0);
            var lateThreshold = new TimeSpan(8, 15, 0);
            var status = AttendanceStatus.Present;
            string? note = dto.Note;

            if (dto.CheckInTime > lateThreshold)
            {
                status = AttendanceStatus.Late;
                var lateMinutes = (int)(dto.CheckInTime - standardStart).TotalMinutes;
                note = string.IsNullOrWhiteSpace(note) ? $"Đi muộn {lateMinutes} phút" : $"{note} (Đi muộn {lateMinutes} phút)";
            }

            if (existing != null)
            {
                existing.CheckIn = dto.CheckInTime;
                existing.Status = status;
                existing.Note = note;
                existing.UpdatedAt = DateTime.Now;
                _uow.Attendances.Update(existing);
            }
            else
            {
                var attendance = new Attendance
                {
                    EmployeeId = employee.Id,
                    UserId = employee.UserId ?? 1,
                    Date = dto.Date.Date,
                    CheckIn = dto.CheckInTime,
                    Status = status,
                    Note = note,
                    CreatedAt = DateTime.Now
                };
                await _uow.Attendances.AddAsync(attendance);
            }

            await _uow.SaveChangesAsync();
        }

        public async Task CheckOutAsync(CheckOutRequestDto dto)
        {
            var employee = await _uow.Employees.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
            {
                throw new BusinessException(BusinessErrorCode.EmployeeNotFound, "Không tìm thấy nhân viên chấm công.");
            }

            var existing = await _uow.Attendances.GetByEmployeeAndDateAsync(dto.EmployeeId, dto.Date);
            if (existing == null || existing.CheckIn == null)
            {
                throw new BusinessException(BusinessErrorCode.NotCheckedIn, $"Nhân viên {employee.FullName} chưa check-in ngày hôm nay, không thể check-out.");
            }

            if (dto.CheckOutTime <= existing.CheckIn.Value)
            {
                throw new BusinessException(BusinessErrorCode.InvalidCheckOutTime, "Thời gian check-out phải lớn hơn thời gian check-in.");
            }

            existing.CheckOut = dto.CheckOutTime;

            // Tính số giờ làm việc (Chuẩn 8h làm việc, trừ 1 tiếng nghỉ trưa nếu làm qua giờ trưa)
            double durationHours = (dto.CheckOutTime - existing.CheckIn.Value).TotalHours;
            if (existing.CheckIn.Value < new TimeSpan(12, 0, 0) && dto.CheckOutTime > new TimeSpan(13, 0, 0) && durationHours > 4.0)
            {
                durationHours -= 1.0; // trừ 1 giờ nghỉ trưa
            }

            decimal standardHours = Math.Min(8.0m, Math.Round((decimal)durationHours, 2));
            decimal overtimeHours = 0;

            // Kiểm tra về sớm hoặc tăng ca (Tan ca chuẩn 17:00)
            var standardEnd = new TimeSpan(17, 0, 0);
            if (dto.CheckOutTime < standardEnd)
            {
                if (existing.Status != AttendanceStatus.Late)
                {
                    existing.Status = AttendanceStatus.EarlyLeave;
                }
            }
            else if (dto.CheckOutTime > new TimeSpan(17, 30, 0))
            {
                // Tăng ca sau 17:30
                overtimeHours = Math.Round((decimal)(dto.CheckOutTime - standardEnd).TotalHours, 2);
                existing.Status = AttendanceStatus.Overtime;
            }

            existing.HoursWorked = standardHours;
            existing.OvertimeHours = overtimeHours;
            if (!string.IsNullOrWhiteSpace(dto.Note))
            {
                existing.Note = string.IsNullOrWhiteSpace(existing.Note) ? dto.Note : $"{existing.Note}; {dto.Note}";
            }
            existing.UpdatedAt = DateTime.Now;

            _uow.Attendances.Update(existing);
            await _uow.SaveChangesAsync();
        }

        public async Task AdjustAttendanceAsync(AttendanceAdjustmentDto dto)
        {
            var record = await _uow.Attendances.GetByIdAsync(dto.Id);
            if (record == null)
            {
                throw new BusinessException(BusinessErrorCode.NotFound, "Không tìm thấy bản ghi chấm công cần điều chỉnh.");
            }

            record.CheckIn = dto.CheckIn;
            record.CheckOut = dto.CheckOut;
            record.Status = dto.Status;
            record.Note = dto.Note;

            if (dto.CheckIn.HasValue && dto.CheckOut.HasValue && dto.CheckOut > dto.CheckIn)
            {
                double duration = (dto.CheckOut.Value - dto.CheckIn.Value).TotalHours;
                if (duration > 4.0) duration -= 1.0;
                record.HoursWorked = Math.Min(8.0m, Math.Round((decimal)duration, 2));

                if (dto.CheckOut.Value > new TimeSpan(17, 30, 0))
                {
                    record.OvertimeHours = Math.Round((decimal)(dto.CheckOut.Value - new TimeSpan(17, 0, 0)).TotalHours, 2);
                }
                else
                {
                    record.OvertimeHours = 0;
                }
            }
            else
            {
                record.HoursWorked = 0;
                record.OvertimeHours = 0;
            }

            record.UpdatedAt = DateTime.Now;
            _uow.Attendances.Update(record);
            await _uow.SaveChangesAsync();
        }

        private static AttendanceRecordDto MapToDto(Attendance a) => new()
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeCode = a.Employee?.EmployeeCode ?? $"NV#{a.UserId}",
            EmployeeName = a.Employee?.FullName ?? a.User?.FullName ?? "N/A",
            Position = a.Employee?.Position ?? a.User?.Role?.Name ?? "Nhân viên",
            Date = a.Date,
            CheckIn = a.CheckIn,
            CheckOut = a.CheckOut,
            HoursWorked = a.HoursWorked,
            OvertimeHours = a.OvertimeHours,
            Status = a.Status,
            Note = a.Note
        };
    }
}
