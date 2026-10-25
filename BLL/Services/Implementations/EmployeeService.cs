using BLL.DTOs.Employee;
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
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _uow;

        public EmployeeService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IReadOnlyList<EmployeeListItemDto>> GetAllAsync(string? keyword = null, string? position = null, EmployeeStatus? status = null)
        {
            var employees = await _uow.Employees.SearchAsync(keyword, position, status);
            return employees.Select(e => new EmployeeListItemDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Position = e.Position,
                PhoneNumber = e.PhoneNumber,
                Email = e.Email,
                BaseSalary = e.BaseSalary,
                Allowance = e.Allowance,
                Status = e.Status,
                Username = e.User?.Username,
                HireDate = e.HireDate
            }).ToList();
        }

        public async Task<EmployeeDetailDto?> GetByIdAsync(int id)
        {
            var e = await _uow.Employees.GetWithUserAsync(id);
            if (e == null) return null;

            return new EmployeeDetailDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                DateOfBirth = e.DateOfBirth,
                Gender = e.Gender,
                PhoneNumber = e.PhoneNumber,
                Email = e.Email,
                Address = e.Address,
                Position = e.Position,
                HireDate = e.HireDate,
                BaseSalary = e.BaseSalary,
                Allowance = e.Allowance,
                Status = e.Status,
                UserId = e.UserId,
                Username = e.User?.Username,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };
        }

        public async Task<int> CreateAsync(CreateEmployeeDto dto)
        {
            ValidateDto(dto.EmployeeCode, dto.FullName, dto.BaseSalary);

            var codeExists = await _uow.Employees.IsCodeExistsAsync(dto.EmployeeCode);
            if (codeExists)
            {
                throw new BusinessException(BusinessErrorCode.DuplicateEmployeeCode, $"Mã nhân viên '{dto.EmployeeCode}' đã tồn tại trong hệ thống.");
            }

            var employee = new Employee
            {
                EmployeeCode = dto.EmployeeCode.Trim().ToUpper(),
                FullName = dto.FullName.Trim(),
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Email = dto.Email?.Trim(),
                Address = dto.Address?.Trim(),
                Position = dto.Position.Trim(),
                HireDate = dto.HireDate,
                BaseSalary = dto.BaseSalary,
                Allowance = dto.Allowance,
                Status = dto.Status,
                UserId = dto.UserId,
                CreatedAt = DateTime.Now
            };

            await _uow.Employees.AddAsync(employee);
            await _uow.SaveChangesAsync();
            return employee.Id;
        }

        public async Task UpdateAsync(UpdateEmployeeDto dto)
        {
            ValidateDto(dto.EmployeeCode, dto.FullName, dto.BaseSalary);

            var employee = await _uow.Employees.GetByIdAsync(dto.Id);
            if (employee == null)
            {
                throw new BusinessException(BusinessErrorCode.EmployeeNotFound, "Không tìm thấy nhân viên cần cập nhật.");
            }

            var codeExists = await _uow.Employees.IsCodeExistsAsync(dto.EmployeeCode, dto.Id);
            if (codeExists)
            {
                throw new BusinessException(BusinessErrorCode.DuplicateEmployeeCode, $"Mã nhân viên '{dto.EmployeeCode}' đã tồn tại ở nhân viên khác.");
            }

            employee.EmployeeCode = dto.EmployeeCode.Trim().ToUpper();
            employee.FullName = dto.FullName.Trim();
            employee.DateOfBirth = dto.DateOfBirth;
            employee.Gender = dto.Gender;
            employee.PhoneNumber = dto.PhoneNumber?.Trim();
            employee.Email = dto.Email?.Trim();
            employee.Address = dto.Address?.Trim();
            employee.Position = dto.Position.Trim();
            employee.HireDate = dto.HireDate;
            employee.BaseSalary = dto.BaseSalary;
            employee.Allowance = dto.Allowance;
            employee.Status = dto.Status;
            employee.UserId = dto.UserId;
            employee.UpdatedAt = DateTime.Now;

            _uow.Employees.Update(employee);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int id, EmployeeStatus status)
        {
            var employee = await _uow.Employees.GetByIdAsync(id);
            if (employee == null)
            {
                throw new BusinessException(BusinessErrorCode.EmployeeNotFound, "Không tìm thấy nhân viên.");
            }

            employee.Status = status;
            employee.UpdatedAt = DateTime.Now;
            _uow.Employees.Update(employee);
            await _uow.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<string>> GetPositionsAsync()
        {
            var employees = await _uow.Employees.GetAllAsync();
            var positions = employees
                .Select(e => e.Position)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            if (positions.Count == 0)
            {
                return new List<string> { "Quản lý cửa hàng", "Nhân viên bán hàng", "Thủ kho", "Nhân viên thu ngân", "Kỹ thuật viên" };
            }

            return positions;
        }

        private static void ValidateDto(string code, string name, decimal baseSalary)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new BusinessException("Mã nhân viên không được để trống.");

            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessException("Họ tên nhân viên không được để trống.");

            if (baseSalary < 0)
                throw new BusinessException("Lương cơ bản không được âm.");
        }
    }
}
