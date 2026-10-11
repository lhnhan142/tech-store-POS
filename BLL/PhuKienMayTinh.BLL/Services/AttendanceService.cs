using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.BLL.Exceptions;
using PhuKienMayTinh.Common.Constants;
using PhuKienMayTinh.Common.Enums;
using PhuKienMayTinh.Common.Utilities;
using PhuKienMayTinh.DAL.Entities;
using PhuKienMayTinh.DAL.Repositories.Interfaces;

namespace PhuKienMayTinh.BLL.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceDTO> CheckInAsync(CheckInRequestDTO request);
        Task<AttendanceDTO> CheckOutAsync(CheckOutRequestDTO request);
        Task<AttendanceDTO> AdjustAttendanceAsync(AdjustAttendanceDTO request);
        Task<IEnumerable<AttendanceDTO>> GetAttendancesAsync(DateTime? fromDate, DateTime? toDate, int? employeeId, AttendanceStatus? status);
        Task ExportAttendancesToExcelAsync(string filePath, DateTime? fromDate, DateTime? toDate, int? employeeId, AttendanceStatus? status);
    }
}

namespace PhuKienMayTinh.BLL.Services.Implementations
{
    using PhuKienMayTinh.BLL.Services.Interfaces;

    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public AttendanceService(IAttendanceRepository attendanceRepo, IEmployeeRepository employeeRepo)
        {
            _attendanceRepo = attendanceRepo;
            _employeeRepo = employeeRepo;
        }

        public async Task<AttendanceDTO> CheckInAsync(CheckInRequestDTO request)
        {
            var employee = await _employeeRepo.GetByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                throw new NotFoundException($"Không tìm thấy nhân viên ID = {request.EmployeeId}");
            }

            if (employee.Status != EmployeeStatus.Active)
            {
                throw new BusinessException($"Nhân viên '{employee.FullName}' hiện không ở trạng thái Đang làm việc (Trạng thái: {employee.Status}).");
            }

            var checkDate = request.CheckTime.Date;
            var existing = await _attendanceRepo.GetTodayAttendanceAsync(request.EmployeeId, checkDate, request.Shift);

            if (existing != null && existing.CheckInTime.HasValue && !existing.CheckOutTime.HasValue)
            {
                throw new BusinessException($"Nhân viên '{employee.FullName}' đã Check-in ca này lúc {existing.CheckInTime.Value:HH:mm:ss} và chưa Check-out!");
            }

            if (existing != null && existing.CheckInTime.HasValue && existing.CheckOutTime.HasValue)
            {
                throw new BusinessException($"Nhân viên '{employee.FullName}' đã hoàn tất cả Check-in và Check-out ca này hôm nay.");
            }

            // Tính thời gian đi trễ
            var shiftStart = GetShiftStartTime(request.Shift, checkDate);
            int lateMinutes = 0;
            AttendanceStatus status = AttendanceStatus.Present;

            if (request.CheckTime > shiftStart.AddMinutes(5)) // Cho phép 5 phút du di
            {
                lateMinutes = (int)(request.CheckTime - shiftStart).TotalMinutes;
                status = AttendanceStatus.Late;
            }

            var attendance = existing ?? new Attendance
            {
                EmployeeId = request.EmployeeId,
                WorkDate = checkDate,
                Shift = request.Shift
            };

            attendance.CheckInTime = request.CheckTime;
            attendance.LateMinutes = lateMinutes;
            attendance.Status = status;
            attendance.Note = request.Note ?? (lateMinutes > 0 ? $"Đi trễ {lateMinutes} phút" : "Đúng giờ");

            if (existing == null)
            {
                await _attendanceRepo.AddAsync(attendance);
            }
            else
            {
                _attendanceRepo.Update(attendance);
            }

            await _attendanceRepo.SaveChangesAsync();
            return MapToDTO(attendance, employee);
        }

        public async Task<AttendanceDTO> CheckOutAsync(CheckOutRequestDTO request)
        {
            var employee = await _employeeRepo.GetByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                throw new NotFoundException($"Không tìm thấy nhân viên ID = {request.EmployeeId}");
            }

            var checkDate = request.CheckTime.Date;
            var attendance = await _attendanceRepo.GetTodayAttendanceAsync(request.EmployeeId, checkDate, request.Shift);

            if (attendance == null || !attendance.CheckInTime.HasValue)
            {
                throw new BusinessException($"Nhân viên '{employee.FullName}' chưa thực hiện Check-in ca này hôm nay!");
            }

            if (attendance.CheckOutTime.HasValue)
            {
                throw new BusinessException($"Nhân viên '{employee.FullName}' đã Check-out ca này rồi lúc {attendance.CheckOutTime.Value:HH:mm:ss}.");
            }

            attendance.CheckOutTime = request.CheckTime;

            // Tính thời gian về sớm
            var shiftEnd = GetShiftEndTime(request.Shift, checkDate);
            int earlyMinutes = 0;
            if (request.CheckTime < shiftEnd.AddMinutes(-5))
            {
                earlyMinutes = (int)(shiftEnd - request.CheckTime).TotalMinutes;
            }
            attendance.EarlyLeaveMinutes = earlyMinutes;

            // Tính tổng số giờ làm việc thực tế
            double totalHours = (request.CheckTime - attendance.CheckInTime.Value).TotalHours;
            // Trừ giờ nghỉ trưa 1 tiếng nếu làm ca FullDay
            if (request.Shift == ShiftType.FullDay && totalHours > 5.0)
            {
                totalHours -= 1.0;
            }
            attendance.HoursWorked = Math.Max(0.5, Math.Round(Math.Min(8.0, totalHours), 1));

            // Xác định trạng thái
            if (attendance.LateMinutes > 0 && earlyMinutes > 0)
            {
                attendance.Status = AttendanceStatus.LateAndEarlyLeave;
            }
            else if (attendance.LateMinutes > 0)
            {
                attendance.Status = AttendanceStatus.Late;
            }
            else if (earlyMinutes > 0)
            {
                attendance.Status = AttendanceStatus.EarlyLeave;
            }
            else
            {
                attendance.Status = AttendanceStatus.Present;
            }

            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                attendance.Note = (attendance.Note + "; " + request.Note).Trim(';', ' ');
            }

            _attendanceRepo.Update(attendance);
            await _attendanceRepo.SaveChangesAsync();

            return MapToDTO(attendance, employee);
        }

        public async Task<AttendanceDTO> AdjustAttendanceAsync(AdjustAttendanceDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new ValidationException("Bắt buộc phải nhập lý do điều chỉnh công.");
            }

            var attendance = await _attendanceRepo.GetByIdAsync(request.AttendanceId);
            if (attendance == null)
            {
                throw new NotFoundException($"Không tìm thấy bản ghi chấm công ID = {request.AttendanceId}");
            }

            attendance.CheckInTime = request.NewCheckInTime;
            attendance.CheckOutTime = request.NewCheckOutTime;
            attendance.HoursWorked = request.NewHoursWorked;
            attendance.Status = request.NewStatus;
            attendance.IsAdjusted = true;
            attendance.AdjustedBy = string.IsNullOrWhiteSpace(request.AdjustedBy) ? "Quản lý" : request.AdjustedBy;
            attendance.AdjustedAt = DateTime.Now;
            attendance.AdjustmentReason = request.Reason.Trim();

            _attendanceRepo.Update(attendance);
            await _attendanceRepo.SaveChangesAsync();

            var emp = await _employeeRepo.GetByIdAsync(attendance.EmployeeId);
            return MapToDTO(attendance, emp);
        }

        public async Task<IEnumerable<AttendanceDTO>> GetAttendancesAsync(DateTime? fromDate, DateTime? toDate, int? employeeId, AttendanceStatus? status)
        {
            var list = await _attendanceRepo.GetAttendancesByFilterAsync(fromDate, toDate, employeeId, status);
            return list.Select(a => MapToDTO(a, a.Employee));
        }

        public async Task ExportAttendancesToExcelAsync(string filePath, DateTime? fromDate, DateTime? toDate, int? employeeId, AttendanceStatus? status)
        {
            var list = await _attendanceRepo.GetAttendancesByFilterAsync(fromDate, toDate, employeeId, status);

            var dt = new DataTable("BangChamCong");
            dt.Columns.Add("STT", typeof(int));
            dt.Columns.Add("Mã NV", typeof(string));
            dt.Columns.Add("Họ và Tên", typeof(string));
            dt.Columns.Add("Phòng ban", typeof(string));
            dt.Columns.Add("Ngày làm việc", typeof(string));
            dt.Columns.Add("Ca làm việc", typeof(string));
            dt.Columns.Add("Giờ Check-in", typeof(string));
            dt.Columns.Add("Giờ Check-out", typeof(string));
            dt.Columns.Add("Số giờ làm", typeof(double));
            dt.Columns.Add("Đi trễ (phút)", typeof(int));
            dt.Columns.Add("Về sớm (phút)", typeof(int));
            dt.Columns.Add("Trạng thái", typeof(string));
            dt.Columns.Add("Ghi chú", typeof(string));

            int stt = 1;
            foreach (var a in list)
            {
                dt.Rows.Add(
                    stt++,
                    a.Employee.EmployeeCode,
                    a.Employee.FullName,
                    a.Employee.Department,
                    a.WorkDate.ToString("dd/MM/yyyy"),
                    GetShiftName(a.Shift),
                    a.CheckInTime?.ToString("HH:mm:ss") ?? "--:--",
                    a.CheckOutTime?.ToString("HH:mm:ss") ?? "--:--",
                    a.HoursWorked,
                    a.LateMinutes,
                    a.EarlyLeaveMinutes,
                    GetStatusName(a.Status),
                    a.Note ?? ""
                );
            }

            string title = $"BẢNG CHẤM CÔNG NHÂN VIÊN (TỪ {fromDate:dd/MM/yyyy} ĐẾN {toDate:dd/MM/yyyy})";
            ExcelHelper.ExportToExcel(dt, title, filePath);
        }

        private static DateTime GetShiftStartTime(ShiftType shift, DateTime date)
        {
            return shift switch
            {
                ShiftType.Morning => date.Date.Add(AppConstants.MorningShiftStart),
                ShiftType.Afternoon => date.Date.Add(AppConstants.AfternoonShiftStart),
                ShiftType.Evening => date.Date.Add(AppConstants.EveningShiftStart),
                ShiftType.FullDay => date.Date.Add(AppConstants.FullDayShiftStart),
                _ => date.Date.AddHours(8)
            };
        }

        private static DateTime GetShiftEndTime(ShiftType shift, DateTime date)
        {
            return shift switch
            {
                ShiftType.Morning => date.Date.Add(AppConstants.MorningShiftEnd),
                ShiftType.Afternoon => date.Date.Add(AppConstants.AfternoonShiftEnd),
                ShiftType.Evening => date.Date.Add(AppConstants.EveningShiftEnd),
                ShiftType.FullDay => date.Date.Add(AppConstants.FullDayShiftEnd),
                _ => date.Date.AddHours(17)
            };
        }

        private static string GetShiftName(ShiftType shift) => shift switch
        {
            ShiftType.Morning => "Ca Sáng",
            ShiftType.Afternoon => "Ca Chiều",
            ShiftType.Evening => "Ca Tối",
            ShiftType.FullDay => "Hành chính",
            _ => "Khác"
        };

        private static string GetStatusName(AttendanceStatus status) => status switch
        {
            AttendanceStatus.Present => "Đúng giờ",
            AttendanceStatus.Late => "Đi trễ",
            AttendanceStatus.EarlyLeave => "Về sớm",
            AttendanceStatus.LateAndEarlyLeave => "Trễ & Về sớm",
            AttendanceStatus.ExcusedAbsence => "Nghỉ có phép",
            AttendanceStatus.UnexcusedAbsence => "Vắng không phép",
            _ => "Khác"
        };

        private static AttendanceDTO MapToDTO(Attendance a, Employee? e)
        {
            return new AttendanceDTO
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeCode = e?.EmployeeCode ?? "",
                EmployeeName = e?.FullName ?? "",
                Department = e?.Department ?? "",
                WorkDate = a.WorkDate,
                Shift = a.Shift,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                HoursWorked = a.HoursWorked,
                LateMinutes = a.LateMinutes,
                EarlyLeaveMinutes = a.EarlyLeaveMinutes,
                Status = a.Status,
                Note = a.Note,
                IsAdjusted = a.IsAdjusted,
                AdjustedBy = a.AdjustedBy,
                AdjustedAt = a.AdjustedAt,
                AdjustmentReason = a.AdjustmentReason
            };
        }
    }
}
