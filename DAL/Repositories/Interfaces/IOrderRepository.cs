using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        // Lấy toàn bộ đơn hàng kèm theo chi tiết đơn hàng (Eager Loading)
        Task<Order> GetOrderWithDetailsAsync(int orderId);

        // Lấy doanh thu theo một khoảng thời gian (Phục vụ Dashboard Báo cáo)
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}