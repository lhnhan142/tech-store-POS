using Common.Enums;
using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Invoice?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.CreatedByUser)
                .Include(i => i.CancelledByUser)
                .Include(i => i.ParentInvoice)
                .Include(i => i.Order)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Product)
                .Include(i => i.Payments)
                .Include(i => i.ReturnRequests)
                    .ThenInclude(r => r.Items)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Invoice?> GetByInvoiceNoWithDetailsAsync(string invoiceNo)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.CreatedByUser)
                .Include(i => i.CancelledByUser)
                .Include(i => i.ParentInvoice)
                .Include(i => i.Order)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Product)
                .Include(i => i.Payments)
                .Include(i => i.ReturnRequests)
                    .ThenInclude(r => r.Items)
                .FirstOrDefaultAsync(i => i.InvoiceNo == invoiceNo);
        }

        public async Task<Invoice?> GetByOrderIdWithDetailsAsync(int orderId)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.CreatedByUser)
                .Include(i => i.CancelledByUser)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Product)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.OrderId == orderId);
        }

        public async Task<(IReadOnlyList<Invoice> Items, int TotalCount)> SearchInvoicesAsync(
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate,
            InvoiceStatus? status,
            int pageIndex,
            int pageSize)
        {
            IQueryable<Invoice> query = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Payments);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var cleanKeyword = keyword.Trim().ToLower();
                query = query.Where(i => i.InvoiceNo.ToLower().Contains(cleanKeyword) ||
                                         (i.Customer != null && (i.Customer.Phone.Contains(cleanKeyword) || i.Customer.Name.ToLower().Contains(cleanKeyword))));
            }

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                query = query.Where(i => i.CreatedAt >= from);
            }

            if (toDate.HasValue)
            {
                var to = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.CreatedAt <= to);
            }

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            int totalCount = await query.CountAsync();

            int safePageIndex = Math.Max(0, pageIndex);
            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip(safePageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
