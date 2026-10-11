using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using DAL.Context;
using DAL.Repositories.Interfaces;
using DAL.Repositories.Implementations;
using BLL.Services.Interfaces;
using BLL.Services.Implementations;
using GUI.Forms.Warehouse;

namespace GUI
{
    internal static class Program
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

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
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Bắt buộc phải có cho Generic
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IProductRepository, ProductRepository>();
                    services.AddScoped<IOrderRepository, OrderRepository>();
                    services.AddScoped<ISupplierRepository, SupplierRepository>();

                    // 3. Đăng ký Services (BLL)
                    services.AddScoped(typeof(IService<>), typeof(BaseService<>)); // BỔ SUNG DÒNG NÀY (Generic Service)
                    services.AddScoped<ISupplierService, SupplierService>();

                    // 4. Đăng ký các Form giao diện chính
                    services.AddTransient<SupplierForm>();
                    // VD: services.AddTransient<MainForm>();
                    // VD: services.AddTransient<LoginForm>();
                })
                .Build();

            ServiceProvider = host.Services;

            // CHẠY THỬ riêng màn hình Nhà cung cấp. 
            // Nhớ gắn // lại và mở LoginForm/MainForm trước khi commit code lên Github để ráp chung với nhóm.
            Application.Run(ServiceProvider.GetRequiredService<SupplierForm>());
        }
    }
}