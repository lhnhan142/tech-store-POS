using BLL.DTOs;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BLL.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private const int MaxLength = 100;
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo) => _repo = repo;

        public async Task<IReadOnlyList<CatalogItemDTO>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            var counts = await _repo.CountProductsAsync();
            return items.OrderBy(c => c.Name)
                .Select(c => new CatalogItemDTO(c.Id, c.Name, counts.GetValueOrDefault(c.Id)))
                .ToList();
        }

        public async Task CreateAsync(string name)
        {
            var n = CatalogRules.RequireName(name, "Tên danh mục", MaxLength);
            if (await _repo.NameExistsAsync(n))
                throw new BusinessException($"Danh mục '{n}' đã tồn tại.");

            await _repo.AddAsync(new Category { Name = n });
            await CatalogRules.SaveAsync(_repo.SaveChangesAsync, "Không thể thêm danh mục. Vui lòng thử lại.");
        }

        public async Task UpdateAsync(int id, string name)
        {
            var n = CatalogRules.RequireName(name, "Tên danh mục", MaxLength);
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new BusinessException("Không tìm thấy danh mục (có thể đã bị xóa).");
            if (await _repo.NameExistsAsync(n, id))
                throw new BusinessException($"Danh mục '{n}' đã tồn tại.");

            entity.Name = n;
            _repo.Update(entity);
            await CatalogRules.SaveAsync(_repo.SaveChangesAsync, "Không thể cập nhật danh mục. Vui lòng thử lại.");
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new BusinessException("Không tìm thấy danh mục (có thể đã bị xóa).");

            var counts = await _repo.CountProductsAsync();
            if (counts.GetValueOrDefault(id) > 0)
                throw new BusinessException(
                    $"Không thể xóa danh mục '{entity.Name}' vì còn {counts[id]} sản phẩm thuộc danh mục này.");

            _repo.Delete(entity);
            await CatalogRules.SaveAsync(_repo.SaveChangesAsync, "Không thể xóa danh mục. Vui lòng thử lại.");
        }
    }

    public class BrandService : IBrandService
    {
        private const int MaxLength = 100;
        private readonly IBrandRepository _repo;

        public BrandService(IBrandRepository repo) => _repo = repo;

        public async Task<IReadOnlyList<CatalogItemDTO>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            var counts = await _repo.CountProductsAsync();
            return items.OrderBy(b => b.Name)
                .Select(b => new CatalogItemDTO(b.Id, b.Name, counts.GetValueOrDefault(b.Id)))
                .ToList();
        }

        public async Task CreateAsync(string name)
        {
            var n = CatalogRules.RequireName(name, "Tên thương hiệu", MaxLength);
            if (await _repo.NameExistsAsync(n))
                throw new BusinessException($"Thương hiệu '{n}' đã tồn tại.");

            await _repo.AddAsync(new Brand { Name = n });
            await CatalogRules.SaveAsync(_repo.SaveChangesAsync, "Không thể thêm thương hiệu. Vui lòng thử lại.");
        }

        public async Task UpdateAsync(int id, string name)
        {
            var n = CatalogRules.RequireName(name, "Tên thương hiệu", MaxLength);
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new BusinessException("Không tìm thấy thương hiệu (có thể đã bị xóa).");
            if (await _repo.NameExistsAsync(n, id))
                throw new BusinessException($"Thương hiệu '{n}' đã tồn tại.");

            entity.Name = n;
            _repo.Update(entity);
            await CatalogRules.SaveAsync(_repo.SaveChangesAsync, "Không thể cập nhật thương hiệu. Vui lòng thử lại.");
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new BusinessException("Không tìm thấy thương hiệu (có thể đã bị xóa).");

            var counts = await _repo.CountProductsAsync();
            if (counts.GetValueOrDefault(id) > 0)
                throw new BusinessException(
                    $"Không thể xóa thương hiệu '{entity.Name}' vì còn {counts[id]} sản phẩm thuộc thương hiệu này.");

            _repo.Delete(entity);
            await CatalogRules.SaveAsync(_repo.SaveChangesAsync, "Không thể xóa thương hiệu. Vui lòng thử lại.");
        }
    }
}
