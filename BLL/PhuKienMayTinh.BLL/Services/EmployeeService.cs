using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.BLL.Exceptions;
using PhuKienMayTinh.Common.Enums;
using PhuKienMayTinh.Common.Utilities;
using PhuKienMayTinh.DAL.Entities;
using PhuKienMayTinh.DAL.Repositories.Interfaces;

namespace PhuKienMayTinh.BLL.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDTO>> GetAllEmployeesAsync();
        Task<IEnumerable<EmployeeDTO>> SearchEmployeesAsync(string? keyword, string? department, EmployeeStatus? status);
        Task<EmployeeDTO?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDTO> AddEmployeeAsync(AddOrUpdateEmployeeDTO dto);
        Task<EmployeeDTO> UpdateEmployeeAsync(AddOrUpdateEmployeeDTO dto);
        Task<bool> ToggleEmployeeStatusAsync(int id, EmployeeStatus newStatus);
        Task<string> GetNextEmployeeCodeAsync();
        Task ExportEmployeesToExcelAsync(string filePath, string? keyword, string? department, EmployeeStatus? status);
    }
}

namespace PhuKienMayTinh.BLL.Services.Implementations
{
    using PhuKienMayTinh.BLL.Services.Interfaces;

    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;

        public EmployeeService(IEmployeeRepository employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        public async Task<IEnumerable<EmployeeDTO>> GetAllEmployeesAsync()
        {
            var entities = await _employeeRepo.GetAllAsync();
            return entities.OrderBy(e => e.EmployeeCode).Select(MapToDTO);
        }

        public async Task<IEnumerable<EmployeeDTO>> SearchEmployeesAsync(string? keyword, string? department, EmployeeStatus? status)
        {
            var entities = await _employeeRepo.SearchEmployeesAsync(keyword, department, status);
            return entities.Select(MapToDTO);
        }

        public async Task<EmployeeDTO?> GetEmployeeByIdAsync(int id)
        {
            var entity = await _employeeRepo.GetByIdAsync(id);
            return entity == null ? null : MapToDTO(entity);
        }

        public async Task<string> GetNextEmployeeCodeAsync()
        {
            return await _employeeRepo.GenerateNextEmployeeCodeAsync();
        }

        public async Task<EmployeeDTO> AddEmployeeAsync(AddOrUpdateEmployeeDTO dto)
        {
            ValidateEmployeeDTO(dto, isUpdate: false);

            if (await _employeeRepo.IsCodeExistsAsync(dto.EmployeeCode))
            {
                throw new ValidationException($"Mã nhân viên '{dto.EmployeeCode}' đã tồn tại trong hệ thống.");
            }

            if (!string.IsNullOrWhiteSpace(dto.IdentityCard) && await _employeeRepo.IsIdentityCardExistsAsync(dto.IdentityCard))
            {
                throw new ValidationException($"Số CCCD/CMND '{dto.IdentityCard}' đã được đăng ký bởi nhân viên khác.");
            }

            var employee = new Employee
            {
                EmployeeCode = dto.EmployeeCode.Trim().ToUpper(),
                FullName = dto.FullName.Trim(),
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                IdentityCard = dto.IdentityCard.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Email = dto.Email.Trim(),
                Address = dto.Address.Trim(),
                Department = dto.Department,
                Position = dto.Position,
                HireDate = dto.HireDate,
                BaseSalary = dto.BaseSalary,
                HourlyRate = dto.HourlyRate,
                LunchAllowance = dto.LunchAllowance,
                TravelAllowance = dto.TravelAllowance,
                ResponsibilityAllowance = dto.ResponsibilityAllowance,
                Status = dto.Status,
                Notes = dto.Notes,
                CreatedAt = DateTime.Now
            };

            await _employeeRepo.AddAsync(employee);
            await _employeeRepo.SaveChangesAsync();

            return MapToDTO(employee);
        }

        public async Task<EmployeeDTO> UpdateEmployeeAsync(AddOrUpdateEmployeeDTO dto)
        {
            ValidateEmployeeDTO(dto, isUpdate: true);

            var employee = await _employeeRepo.GetByIdAsync(dto.Id);
            if (employee == null)
            {
                throw new NotFoundException($"Không tìm thấy nhân viên có ID = {dto.Id}");
            }

            if (await _employeeRepo.IsCodeExistsAsync(dto.EmployeeCode, dto.Id))
            {
                throw new ValidationException($"Mã nhân viên '{dto.EmployeeCode}' đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(dto.IdentityCard) && await _employeeRepo.IsIdentityCardExistsAsync(dto.IdentityCard, dto.Id))
            {
                throw new ValidationException($"Số CCCD/CMND '{dto.IdentityCard}' đã thuộc về nhân viên khác.");
            }

            employee.EmployeeCode = dto.EmployeeCode.Trim().ToUpper();
            employee.FullName = dto.FullName.Trim();
            employee.Gender = dto.Gender;
            employee.DateOfBirth = dto.DateOfBirth;
            employee.IdentityCard = dto.IdentityCard.Trim();
            employee.PhoneNumber = dto.PhoneNumber.Trim();
            employee.Email = dto.Email.Trim();
            employee.Address = dto.Address.Trim();
            employee.Department = dto.Department;
            employee.Position = dto.Position;
            employee.HireDate = dto.HireDate;
            employee.BaseSalary = dto.BaseSalary;
            employee.HourlyRate = dto.HourlyRate;
            employee.LunchAllowance = dto.LunchAllowance;
            employee.TravelAllowance = dto.TravelAllowance;
            employee.ResponsibilityAllowance = dto.ResponsibilityAllowance;
            employee.Status = dto.Status;
            employee.Notes = dto.Notes;
            employee.UpdatedAt = DateTime.Now;

            _employeeRepo.Update(employee);
            await _employeeRepo.SaveChangesAsync();

            return MapToDTO(employee);
        }

        public async Task<bool> ToggleEmployeeStatusAsync(int id, EmployeeStatus newStatus)
        {
            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null)
            {
                throw new NotFoundException($"Không tìm thấy nhân viên ID = {id}");
            }

            employee.Status = newStatus;
            employee.UpdatedAt = DateTime.Now;
            _employeeRepo.Update(employee);
            return await _employeeRepo.SaveChangesAsync();
        }

        public async Task ExportEmployeesToExcelAsync(string filePath, string? keyword, string? department, EmployeeStatus? status)
        {
            var employees = await _employeeRepo.SearchEmployeesAsync(keyword, department, status);

            var dt = new DataTable("DanhSachNhanVien");
            dt.Columns.Add("STT", typeof(int));
            dt.Columns.Add("Mã NV", typeof(string));
            dt.Columns.Add("Họ và Tên", typeof(string));
            dt.Columns.Add("Giới tính", typeof(string));
            dt.Columns.Add("Ngày sinh", typeof(string));
            dt.Columns.Add("CCCD/CMND", typeof(string));
            dt.Columns.Add("Số điện thoại", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Phòng ban", typeof(string));
            dt.Columns.Add("Chức vụ", typeof(string));
            dt.Columns.Add("Ngày vào làm", typeof(string));
            dt.Columns.Add("Lương cơ bản", typeof(decimal));
            dt.Columns.Add("Tổng phụ cấp", typeof(decimal));
            dt.Columns.Add("Trạng thái", typeof(string));

            int stt = 1;
            foreach (var e in employees)
            {
                decimal totalAllowance = e.LunchAllowance + e.TravelAllowance + e.ResponsibilityAllowance;
                string statusText = e.Status switch
                {
                    EmployeeStatus.Active => "Đang làm việc",
                    EmployeeStatus.OnLeave => "Tạm nghỉ",
                    EmployeeStatus.Resigned => "Đã nghỉ việc",
                    _ => ""
                };

                dt.Rows.Add(
                    stt++,
                    e.EmployeeCode,
                    e.FullName,
                    e.Gender,
                    e.DateOfBirth.ToString("dd/MM/yyyy"),
                    e.IdentityCard,
                    e.PhoneNumber,
                    e.Email,
                    e.Department,
                    e.Position,
                    e.HireDate.ToString("dd/MM/yyyy"),
                    e.BaseSalary,
                    totalAllowance,
                    statusText
                );
            }

            ExcelHelper.ExportToExcel(dt, "DANH SÁCH NHÂN VIÊN CỬA HÀNG", filePath);
        }

        private void ValidateEmployeeDTO(AddOrUpdateEmployeeDTO dto, bool isUpdate)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new ValidationException("Họ và tên nhân viên không được để trống.");

            if (dto.FullName.Trim().Length < 2)
                throw new ValidationException("Họ và tên phải có ít nhất 2 ký tự.");

            if (string.IsNullOrWhiteSpace(dto.EmployeeCode))
                throw new ValidationException("Mã nhân viên không được để trống.");

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !ValidationHelper.IsValidPhoneNumber(dto.PhoneNumber))
                throw new ValidationException("Số điện thoại không đúng định dạng (VD: 0912345678).");

            if (!string.IsNullOrWhiteSpace(dto.Email) && !ValidationHelper.IsValidEmail(dto.Email))
                throw new ValidationException("Địa chỉ Email không hợp lệ (VD: user@example.com).");

            if (!string.IsNullOrWhiteSpace(dto.IdentityCard) && !ValidationHelper.IsValidIdentityCard(dto.IdentityCard))
                throw new ValidationException("Số CCCD/CMND phải gồm 9 hoặc 12 chữ số.");

            if (dto.DateOfBirth > DateTime.Today.AddYears(-16))
                throw new ValidationException("Nhân viên phải từ 16 tuổi trở lên.");

            if (dto.BaseSalary < 0)
                throw new ValidationException("Lương cơ bản không được âm.");

            if (dto.LunchAllowance < 0 || dto.TravelAllowance < 0 || dto.ResponsibilityAllowance < 0)
                throw new ValidationException("Các khoản phụ cấp không được âm.");
        }

        private static EmployeeDTO MapToDTO(Employee e)
        {
            return new EmployeeDTO
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Gender = e.Gender,
                DateOfBirth = e.DateOfBirth,
                IdentityCard = e.IdentityCard,
                PhoneNumber = e.PhoneNumber,
                Email = e.Email,
                Address = e.Address,
                Department = e.Department,
                Position = e.Position,
                HireDate = e.HireDate,
                BaseSalary = e.BaseSalary,
                HourlyRate = e.HourlyRate,
                LunchAllowance = e.LunchAllowance,
                TravelAllowance = e.TravelAllowance,
                ResponsibilityAllowance = e.ResponsibilityAllowance,
                Status = e.Status,
                Notes = e.Notes
            };
        }
    }
}
