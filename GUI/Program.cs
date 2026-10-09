using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Windows.Forms;
using DAL.Context;
using DAL.Repositories.Interfaces;
using DAL.Repositories.Implementations;
using BLL.Services.Implementations;
using BLL.Services.Interfaces;
using GUI.Forms.Warehouse;

namespace GUI
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
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
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IProductRepository, ProductRepository>();
                    services.AddScoped<IOrderRepository, OrderRepository>();
                    services.AddScoped<ISupplierRepository, SupplierRepository>();

                    // 2b. Đăng ký Services (BLL)
                    services.AddScoped<ISupplierService, SupplierService>();

                    // 3. Đăng ký các Form giao diện chính
                    services.AddTransient<SupplierForm>();
                    // VD: services.AddTransient<MainForm>();
                    // VD: services.AddTransient<LoginForm>();
                })
                .Build();

            ServiceProvider = host.Services;

            // Chạy Form khởi động đầu tiên (Thay MainForm bằng form đăng nhập của bạn sau này)
            // Application.Run(ServiceProvider.GetRequiredService<MainForm>());

            // CHẠY THỬ riêng màn hình Nhà cung cấp: bỏ dấu // ở dòng dưới. Nhớ gắn // lại trước khi commit,
            // vì khi có MainForm/LoginForm thì SupplierForm sẽ được mở từ menu.
            Application.Run(ServiceProvider.GetRequiredService<SupplierForm>());
        }
    }
}