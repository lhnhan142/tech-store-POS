using BLL.Services;
using BLL.Services.Implementations;
using BLL.Services.Interfaces;
using DAL.Context;
using DAL.Repositories.Implementations;
using DAL.Repositories.Interfaces;
using GUI.Forms.Products;
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
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            var host = Host.CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureAppConfiguration((_, builder) =>
                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
                .ConfigureServices((context, services) =>
                {
                    // 1. Database
                    var cs = context.Configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<AppDbContext>(o => o.UseSqlServer(cs));

                    // 2. Repositories (DAL)
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IProductRepository, ProductRepository>();
                    services.AddScoped<IOrderRepository, OrderRepository>();
                    services.AddScoped<ISupplierRepository, SupplierRepository>();
                    services.AddScoped<IInventoryRepository, InventoryRepository>();
                    services.AddScoped<ICategoryRepository, CategoryRepository>();
                    services.AddScoped<IBrandRepository, BrandRepository>();
                    services.AddScoped<IInvoiceRepository, InvoiceRepository>();
                    services.AddScoped<IReturnRepository, ReturnRepository>();
                    services.AddScoped<IReturnPolicyRepository, ReturnPolicyRepository>();
                    services.AddScoped<IStockMovementRepository, StockMovementRepository>();
                    services.AddScoped<IEmployeeRepository, EmployeeRepository>();
                    services.AddScoped<IAttendanceRepository, AttendanceRepository>();
                    services.AddScoped<IPayrollRepository, PayrollRepository>();
                    services.AddScoped<IUnitOfWork, UnitOfWork>();

                    // 3. Services (BLL)
                    services.AddScoped(typeof(IService<>), typeof(BaseService<>));
                    services.AddScoped<ISupplierService, SupplierService>();
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<IInventoryService, InventoryService>();
                    services.AddScoped<IStockAlertService, StockAlertService>();
                    services.AddScoped<IProductService, ProductService>();
                    services.AddScoped<ICategoryService, CategoryService>();
                    services.AddScoped<IBrandService, BrandService>();
                    services.AddScoped<IProductExcelService, ProductExcelService>();
                    services.AddScoped<IInvoiceService, InvoiceService>();
                    services.AddScoped<IReturnService, ReturnService>();
                    services.AddScoped<IReturnReportService, ReturnReportService>();
                    services.AddScoped<IEmployeeService, EmployeeService>();
                    services.AddScoped<IAttendanceService, AttendanceService>();
                    services.AddScoped<IPayrollService, PayrollService>();
                    services.AddScoped<IDashboardService, DashboardService>();
                    services.AddScoped<IReportExportService, ReportExportService>();
                    services.AddSingleton<UserSession>();
                    services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<UserSession>());
                    services.AddSingleton(new ProductImageSettings
                    {
                        // Cùng thư mục mà POS đọc ảnh (Images\Products cạnh file .exe)
                        Folder = Path.Combine(AppContext.BaseDirectory, "Images", "Products")
                    });
                    services.AddSingleton(new InventorySettings
                    {
                        LowStockThreshold = context.Configuration.GetValue("Inventory:LowStockThreshold", 10)
                    });

                    // 4. Forms (Transient, mỗi lần mở là một instance mới)
                    services.AddTransient<ProductForm>();
                    services.AddTransient<CategoryForm>();
                    services.AddTransient<BrandForm>();
                    services.AddTransient<SupplierForm>();
                    services.AddTransient<ReceiptForm>();
                    services.AddTransient<IssueForm>();
                    services.AddTransient<StockTakeForm>();
                    services.AddTransient<StockAlertForm>();
                    services.AddTransient<GUI.Forms.Auth.LoginForm>();
                    services.AddTransient<GUI.Forms.Main.MainForm>();
                    services.AddTransient<GUI.Forms.InvoiceAndReturn.frmDanhSachHoaDon>();
                    services.AddTransient<GUI.Forms.InvoiceAndReturn.frmYeuCauDoiTra>();
                    services.AddTransient<GUI.Forms.InvoiceAndReturn.frmNhanHang>();
                    services.AddTransient<GUI.Forms.InvoiceAndReturn.frmBaoCaoDoiTra>();

                    // 5. Member 5 Modules: HR, Attendance, Payroll, Dashboard, Reports
                    services.AddTransient<GUI.Forms.HumanResource.EmployeeManagementForm>();
                    services.AddTransient<GUI.Forms.HumanResource.AttendanceManagementForm>();
                    services.AddTransient<GUI.Forms.HumanResource.PayrollManagementForm>();
                    services.AddTransient<GUI.Forms.Dashboard.DashboardForm>();
                    services.AddTransient<GUI.Forms.Reports.ReportManagementForm>();
                })
                .Build();

            ServiceProvider = host.Services;

            // Chạy riêng từng màn hình nếu truyền tham số dòng lệnh (tiện lợi cho việc test nhanh)
            if (args.Contains("--supplier"))
            {
                Application.Run(ServiceProvider.GetRequiredService<SupplierForm>());
                return;
            }
            if (args.Contains("--pos"))
            {
                Application.Run(new GUI.Forms.Sales.POSTestForm());
                return;
            }

            if (args.Contains("--don-hang") || args.Contains("--invoice"))
            {
                Application.Run(ServiceProvider.GetRequiredService<GUI.Forms.InvoiceAndReturn.frmDanhSachHoaDon>());
                return;
            }

            // Mặc định: Đăng nhập -> Màn hình chính -> (Đăng xuất thì quay lại Đăng nhập)
            while (true)
            {
                using (var login = ServiceProvider.GetRequiredService<GUI.Forms.Auth.LoginForm>())
                {
                    if (login.ShowDialog() != DialogResult.OK) break;   // đóng form đăng nhập = thoát
                }

                var main = ServiceProvider.GetRequiredService<GUI.Forms.Main.MainForm>();
                Application.Run(main);
                if (!main.LoggedOut) break;                             // bấm X ở MainForm = thoát hẳn
            }
        }
    }
}
