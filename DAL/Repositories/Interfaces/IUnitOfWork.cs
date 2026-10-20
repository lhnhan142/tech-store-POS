using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IInvoiceRepository Invoices { get; }
        IReturnRepository Returns { get; }
        IReturnPolicyRepository ReturnPolicies { get; }
        IStockMovementRepository StockMovements { get; }
        IRepository<InvoiceItem> InvoiceItems { get; }
        IRepository<InvoicePayment> InvoicePayments { get; }
        IRepository<ReturnItem> ReturnItems { get; }
        IRepository<ReturnAttachment> ReturnAttachments { get; }
        IRepository<Refund> Refunds { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IRepository<Product> Products { get; }
        IRepository<Customer> Customers { get; }
        IRepository<User> Users { get; }

        IRepository<T> Repository<T>() where T : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Thực thi một khối logic nghiệp vụ trong database transaction, tự động rollback nếu có lỗi.
        /// </summary>
        Task ExecuteInTransactionAsync(Func<Task> action);

        /// <summary>
        /// Thực thi một khối logic nghiệp vụ trong transaction có trả về kết quả.
        /// </summary>
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action);
    }
}
