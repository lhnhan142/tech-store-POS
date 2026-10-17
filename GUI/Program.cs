using BLL.Services;
using BLL.Services.Implementations;
using BLL.Services.Interfaces;
using DAL.Context;
using DAL.Repositories.Implementations;
using DAL.Repositories.Interfaces;
using GUI.Forms.Products;
using GUI.Forms.Warehouse;
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

                    // 3. Services (BLL)
                    services.AddScoped(typeof(IService<>), typeof(BaseService<>));
                    services.AddScoped<ISupplierService, SupplierService>();
                    services.AddScoped<IInventoryService, InventoryService>();
                    services.AddScoped<IStockAlertService, StockAlertService>();
                    services.AddScoped<IProductService, ProductService>();
                    services.AddScoped<ICategoryService, CategoryService>();
                    services.AddScoped<IBrandService, BrandService>();
                    services.AddScoped<IProductExcelService, ProductExcelService>();
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
                    // services.AddTransient<GUI.Auth.LoginForm>()
                    services.AddTransient<GUI.Forms.Main.MainForm>();
                })
                .Build();

            ServiceProvider = host.Services;

            // Mở form khởi động (sau này đổi thành LoginForm)
            Application.Run(ServiceProvider.GetRequiredService<GUI.Forms.Main.MainForm>());
        }
    }
}   