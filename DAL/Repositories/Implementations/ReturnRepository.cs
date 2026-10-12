using Common.Enums;
using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations
{
    public class ReturnRepository : Repository<ReturnRequest>, IReturnRepository
    {
        public ReturnRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ReturnRequest?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.ReturnRequests
                .Include(r => r.CreatedByUser)
                .Include(r => r.ApprovedByUser)
                .Include(r => r.ReceivedByUser)
                .Include(r => r.ExchangeInvoice)
                .Include(r => r.Invoice)
                    .ThenInclude(i => i!.Customer)
                .Include(r => r.Invoice)
                    .ThenInclude(i => i!.Payments)
                .Include(r => r.Items)
                    .ThenInclude(item => item.Product)
                .Include(r => r.Items)
                    .ThenInclude(item => item.InvoiceItem)
                .Include(r => r.Attachments)
                .Include(r => r.Refunds)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<ReturnRequest?> GetByRequestNoWithDetailsAsync(string requestNo)
        {
            return await _context.ReturnRequests
                .Include(r => r.CreatedByUser)
                .Include(r => r.ApprovedByUser)
                .Include(r => r.ReceivedByUser)
                .Include(r => r.ExchangeInvoice)
                .Include(r => r.Invoice)
                    .ThenInclude(i => i!.Customer)
                .Include(r => r.Invoice)
                    .ThenInclude(i => i!.Payments)
                .Include(r => r.Items)
                    .ThenInclude(item => item.Product)
                .Include(r => r.Items)
                    .ThenInclude(item => item.InvoiceItem)
                .Include(r => r.Attachments)
                .Include(r => r.Refunds)
                .FirstOrDefaultAsync(r => r.RequestNo == requestNo);
        }

        public async Task<(IReadOnlyList<ReturnRequest> Items, int TotalCount)> SearchRequestsAsync(
            ReturnStatus? status,
            int pageIndex,
            int pageSize)
        {
            IQueryable<ReturnRequest> query = _context.ReturnRequests
                .Include(r => r.Invoice)
                    .ThenInclude(i => i!.Customer)
                .Include(r => r.CreatedByUser)
                .Include(r => r.Items)
                    .ThenInclude(item => item.Product);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            int totalCount = await query.CountAsync();

            int safePageIndex = Math.Max(0, pageIndex);
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip(safePageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> GetReservedQtyForInvoiceItemAsync(int invoiceItemId, int? excludeRequestId = null)
        {
            // Các trạng thái đang giữ chỗ: Pending, Approved, Received
            var activeStatuses = new[] { ReturnStatus.Pending, ReturnStatus.Approved, ReturnStatus.Received };

            var query = _context.ReturnItems
                .Where(ri => ri.InvoiceItemId == invoiceItemId &&
                             activeStatuses.Contains(ri.ReturnRequest!.Status));

            if (excludeRequestId.HasValue)
            {
                query = query.Where(ri => ri.ReturnRequestId != excludeRequestId.Value);
            }

            return await query.SumAsync(ri => (int?)ri.Qty) ?? 0;
        }

        public async Task<int> CountByStatusAsync(ReturnStatus status)
        {
            return await _context.ReturnRequests.CountAsync(r => r.Status == status);
        }
    }
}
