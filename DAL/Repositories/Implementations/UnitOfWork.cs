using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        private IInvoiceRepository? _invoices;
        private IReturnRepository? _returns;
        private IReturnPolicyRepository? _returnPolicies;
        private IStockMovementRepository? _stockMovements;
        private IEmployeeRepository? _employees;
        private IAttendanceRepository? _attendances;
        private IPayrollRepository? _payrolls;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IInvoiceRepository Invoices => _invoices ??= new InvoiceRepository(_context);
        public IReturnRepository Returns => _returns ??= new ReturnRepository(_context);
        public IReturnPolicyRepository ReturnPolicies => _returnPolicies ??= new ReturnPolicyRepository(_context);
        public IStockMovementRepository StockMovements => _stockMovements ??= new StockMovementRepository(_context);
        public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);
        public IAttendanceRepository Attendances => _attendances ??= new AttendanceRepository(_context);
        public IPayrollRepository Payrolls => _payrolls ??= new PayrollRepository(_context);

        public IRepository<InvoiceItem> InvoiceItems => Repository<InvoiceItem>();
        public IRepository<InvoicePayment> InvoicePayments => Repository<InvoicePayment>();
        public IRepository<ReturnItem> ReturnItems => Repository<ReturnItem>();
        public IRepository<ReturnAttachment> ReturnAttachments => Repository<ReturnAttachment>();
        public IRepository<Refund> Refunds => Repository<Refund>();
        public IRepository<AuditLog> AuditLogs => Repository<AuditLog>();
        public IRepository<Product> Products => Repository<Product>();
        public IRepository<Customer> Customers => Repository<Customer>();
        public IRepository<User> Users => Repository<User>();
        public IRepository<PayrollDetail> PayrollDetails => Repository<PayrollDetail>();

        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.TryGetValue(type, out var repo))
            {
                repo = new Repository<T>(_context);
                _repositories[type] = repo;
            }
            return (IRepository<T>)repo;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await action();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var result = await action();
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}
