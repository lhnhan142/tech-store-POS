using BLL.Services.Implementations;
using BLL.Services.Interfaces;
using DAL.Context;
using DAL.Repositories.Implementations;
using DAL.Repositories.Interfaces;
using GUI.Forms.Warehouse;
using GUI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GUI
{
    internal static class Program
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            // Khởi tạo Host cho Dependency Injection
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // 1. Đăng ký Database Context
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    // 2. Đăng ký Repositories (DAL)
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Bắt buộc phải có cho Generic
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IProductRepository, ProductRepository>();
                    services.AddScoped<IOrderRepository, OrderRepository>();
                    services.AddScoped<ISupplierRepository, SupplierRepository>();
                    services.AddScoped<IInvoiceRepository, InvoiceRepository>();
                    services.AddScoped<IReturnRepository, ReturnRepository>();
                    services.AddScoped<IReturnPolicyRepository, ReturnPolicyRepository>();
                    services.AddScoped<IStockMovementRepository, StockMovementRepository>();
                    services.AddScoped<IUnitOfWork, UnitOfWork>();

                    // 3. Đăng ký Services (BLL)
                    services.AddScoped(typeof(IService<>), typeof(BaseService<>)); // BỔ SUNG DÒNG NÀY (Generic Service)
                    services.AddScoped<ISupplierService, SupplierService>();
                    services.AddScoped<IInvoiceService, InvoiceService>();
                    services.AddScoped<IReturnService, ReturnService>();
                    services.AddScoped<IReturnReportService, ReturnReportService>();
                    services.AddSingleton<UserSession>();
                    services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<UserSession>());

                    // 4. Đăng ký các Form giao diện chính
                    services.AddTransient<SupplierForm>();
                    services.AddTransient<GUI.Forms.InvoiceAndReturn.frmDanhSachHoaDon>();
                    services.AddTransient<GUI.Forms.InvoiceAndReturn.frmYeuCauDoiTra>();
                    services.AddTransient<GUI.Forms.InvoiceAndReturn.frmNhanHang>();
                    services.AddTransient<GUI.Forms.InvoiceAndReturn.frmBaoCaoDoiTra>();
                    // VD: services.AddTransient<MainForm>();
                    // VD: services.AddTransient<LoginForm>();
                })
                .Build();

            ServiceProvider = host.Services;

            // Form khởi động:
            // - Mặc định: Mở Quản lý đơn hàng & đổi trả
            // - Tham số --supplier: Mở quản lý nhà cung cấp (dotnet run -- --supplier)
            // - Tham số --pos: Mở màn hình POS thử nghiệm
            Form first;
            if (args.Contains("--supplier"))
            {
                first = ServiceProvider.GetRequiredService<SupplierForm>();
            }
            else if (args.Contains("--pos"))
            {
                first = new GUI.Forms.Sales.POSTestForm();
            }
            else
            {
                first = ServiceProvider.GetRequiredService<GUI.Forms.InvoiceAndReturn.frmDanhSachHoaDon>();
            }

            Application.Run(first);
        }
    }
}