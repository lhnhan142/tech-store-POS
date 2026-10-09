using BLL.Exceptions;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BLL.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private const int NameMaxLength = 100;
        private const int AddressMaxLength = 200;

        // Số điện thoại: tùy chọn "+" ở đầu, sau đó 8-15 chữ số (đã bỏ khoảng trắng, dấu chấm, gạch ngang)
        private static readonly Regex PhonePattern = new(@"^\+?\d{8,15}$", RegexOptions.Compiled);

        private readonly ISupplierRepository _repo;

        public SupplierService(ISupplierRepository repo)
        {
            _repo = repo;
        }

        public async Task<IReadOnlyList<Supplier>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.OrderBy(s => s.Name).ToList();
        }

        public async Task CreateAsync(string name, string? phone, string? address)
        {
            var (n, p, a) = Validate(name, phone, address);

            if (await _repo.NameExistsAsync(n))
                throw new BusinessException($"Nhà cung cấp '{n}' đã tồn tại.");

            await _repo.AddAsync(new Supplier { Name = n, Phone = p, Address = a });
            await SaveAsync("Không thể thêm nhà cung cấp. Vui lòng thử lại.");
        }

        public async Task UpdateAsync(int id, string name, string? phone, string? address)
        {
            var (n, p, a) = Validate(name, phone, address);

            var entity = await _repo.GetByIdAsync(id)
                ?? throw new BusinessException("Không tìm thấy nhà cung cấp (có thể đã bị xóa).");

            if (await _repo.NameExistsAsync(n, id))
                throw new BusinessException($"Nhà cung cấp '{n}' đã tồn tại.");

            entity.Name = n;
            entity.Phone = p;
            entity.Address = a;
            _repo.Update(entity);
            await SaveAsync("Không thể cập nhật nhà cung cấp. Vui lòng thử lại.");
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new BusinessException("Không tìm thấy nhà cung cấp (có thể đã bị xóa).");

            if (await _repo.HasReceiptsAsync(id))
                throw new BusinessException(
                    $"Không thể xóa nhà cung cấp '{entity.Name}' vì đã có phiếu nhập kho liên quan.");

            _repo.Delete(entity);
            await SaveAsync("Không thể xóa nhà cung cấp. Vui lòng thử lại.");
        }

        // ------------------------------------------------------------------ helpers
        private static (string Name, string? Phone, string? Address) Validate(string? name, string? phone, string? address)
        {
            var n = CollapseSpaces(name);
            if (n.Length == 0)
                throw new BusinessException("Tên nhà cung cấp không được để trống.");
            if (n.Length > NameMaxLength)
                throw new BusinessException($"Tên nhà cung cấp tối đa {NameMaxLength} ký tự.");

            string? p = null;
            var rawPhone = (phone ?? string.Empty).Trim();
            if (rawPhone.Length > 0)
            {
                p = rawPhone.Replace(" ", "").Replace(".", "").Replace("-", "");
                if (!PhonePattern.IsMatch(p))
                    throw new BusinessException("Số điện thoại không hợp lệ (chỉ gồm 8-15 chữ số, có thể bắt đầu bằng +).");
            }

            string? a = null;
            var rawAddress = CollapseSpaces(address);
            if (rawAddress.Length > 0)
            {
                if (rawAddress.Length > AddressMaxLength)
                    throw new BusinessException($"Địa chỉ tối đa {AddressMaxLength} ký tự.");
                a = rawAddress;
            }

            return (n, p, a);
        }

        // Cắt khoảng trắng thừa ở đầu, cuối và giữa các từ
        private static string CollapseSpaces(string? value) =>
            string.Join(' ', (value ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        private async Task SaveAsync(string errorMessage)
        {
            try
            {
                await _repo.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new BusinessException(errorMessage);
            }
        }
    }
}
