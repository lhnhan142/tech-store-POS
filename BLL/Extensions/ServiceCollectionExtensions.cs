using BLL.Services.Implementations;
using BLL.Services.Interfaces;
using DAL.Context;
using DAL.Repositories.Implementations;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Extensions
{
    /// <summary>
    /// Extension method đăng ký toàn bộ DAL và BLL vào DI container.
    /// Giúp GUI chỉ cần gọi services.AddBusinessLayer(configuration) mà không cần tham chiếu trực tiếp DAL.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Đăng ký DbContext (SQL Server)
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Đăng ký Repositories và UnitOfWork (DAL)
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IReturnRepository, ReturnRepository>();
            services.AddScoped<IReturnPolicyRepository, ReturnPolicyRepository>();
            services.AddScoped<IStockMovementRepository, StockMovementRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IPayrollRepository, PayrollRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 3. Đăng ký Services (BLL)
            services.AddScoped(typeof(IService<>), typeof(BaseService<>));
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IReturnService, ReturnService>();
            services.AddScoped<IReturnReportService, ReturnReportService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IPayrollService, PayrollService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IReportExportService, ReportExportService>();

            return services;
        }
    }
}
