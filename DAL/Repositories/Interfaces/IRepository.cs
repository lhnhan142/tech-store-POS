using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Lấy tất cả dữ liệu
        Task<IEnumerable<T>> GetAllAsync();

        // Lấy dữ liệu theo điều kiện (vd: x => x.Status == Active)
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // Lấy một đối tượng theo ID
        Task<T?> GetByIdAsync(int id);

        // Thêm mới
        Task AddAsync(T entity);

        // Cập nhật (Sửa)
        void Update(T entity);

        // Xóa
        void Delete(T entity);

        // Lưu các thay đổi xuống DB (Commit)
        Task<int> SaveChangesAsync();
    }
}