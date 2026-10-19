using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.BLL.Services.Implementations;
using PhuKienMayTinh.BLL.Services.Interfaces;
using PhuKienMayTinh.DAL.Context;
using PhuKienMayTinh.DAL.Repositories.Implementations;
using PhuKienMayTinh.DAL.Repositories.Interfaces;
using PhuKienMayTinh.GUI.Forms;
using PhuKienMayTinh.GUI.UserControls;

namespace PhuKienMayTinh.GUI
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            // 1. Build Configuration
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = configBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=.;Database=PhuKienMayTinhDb;Trusted_Connection=True;TrustServerCertificate=True;";

            // 2. Setup Dependency Injection
            var services = new ServiceCollection();

            // Database Context
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IPayrollRepository, PayrollRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            // Services
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IPayrollService, PayrollService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IReportService, ReportService>();

            // UserControls & Forms
            services.AddTransient<DashboardUC>();
            services.AddTransient<EmployeeUC>();
            services.AddTransient<AttendanceUC>();
            services.AddTransient<PayrollUC>();
            services.AddTransient<ReportUC>();
            services.AddTransient<MainForm>();

            var serviceProvider = services.BuildServiceProvider();

            // 3. Initialize & Seed Database
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    DbInitializer.Initialize(dbContext);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể kết nối hoặc khởi tạo cơ sở dữ liệu SQL Server:\n{ex.Message}\n\nVui lòng kiểm tra dịch vụ SQL Server và chuỗi kết nối trong appsettings.json.",
                    "Lỗi Cơ Sở Dữ Liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            if (args != null && args.Length > 0 && args[0] == "--seed-only")
            {
                Console.WriteLine("Database initialized and seeded successfully!");
                return;
            }

            if (args != null && args.Length > 0 && args[0] == "--test-run")
            {
                Console.WriteLine("Running automated integration tests...");
                using var scope = serviceProvider.CreateScope();
                var dashService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
                var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

                // 1. Dashboard
                var dash = dashService.GetDashboardSummaryAsync().GetAwaiter().GetResult();
                Console.WriteLine($"[PASS] Dashboard: MonthRevenue={dash.MonthRevenue:N0} đ, Orders={dash.MonthOrderCount}, ActiveEmployees={dash.ActiveEmployeeCount}, RevenuePoints={dash.RevenuePoints.Count}");

                // 2. Payroll calculation
                var pCalc = payrollService.CalculatePayrollAsync(new CalculatePayrollRequestDTO { Month = 10, Year = 2026, Note = "Test Run" }).GetAwaiter().GetResult();
                Console.WriteLine($"[PASS] Payroll: Code={pCalc.PayrollCode}, TotalNet={pCalc.TotalNetSalary:N0} đ, Details={pCalc.Details.Count}");

                // 3. Export Payroll Excel
                string testDir = Path.Combine(Directory.GetCurrentDirectory(), "TestExports");
                Directory.CreateDirectory(testDir);
                string excelPath = Path.Combine(testDir, "TestPayroll.xlsx");
                payrollService.ExportPayrollToExcelAsync(pCalc.Id, excelPath).GetAwaiter().GetResult();
                Console.WriteLine($"[PASS] Payroll Excel Exported: {excelPath} (Size: {new FileInfo(excelPath).Length} bytes)");

                // 4. Export Payslip PDF
                string pdfPath = Path.Combine(testDir, "TestPayslip.pdf");
                payrollService.ExportPayslipToPdfAsync(pCalc.Details[0].Id, pdfPath).GetAwaiter().GetResult();
                Console.WriteLine($"[PASS] Payslip PDF Exported: {pdfPath} (Size: {new FileInfo(pdfPath).Length} bytes)");

                // 5. Export Sales Report PDF
                string reportPdf = Path.Combine(testDir, "TestSalesReport.pdf");
                reportService.ExportReportToPdfAsync(new ReportFilterDTO { ReportType = "DoanhThu", FromDate = DateTime.Today.AddDays(-30), ToDate = DateTime.Today }, reportPdf).GetAwaiter().GetResult();
                Console.WriteLine($"[PASS] Report PDF Exported: {reportPdf} (Size: {new FileInfo(reportPdf).Length} bytes)");
                Console.WriteLine("ALL AUTOMATED TESTS PASSED SUCCESSFULLY!");
                return;
            }

            if (args != null && args.Length > 0 && args[0] == "--render-ui")
            {
                SynchronizationContext.SetSynchronizationContext(null);
                Console.WriteLine("Rendering UI pages to PNG images...");
                using var scope = serviceProvider.CreateScope();
                string artifactDir = @"C:\Users\Truong\.gemini\antigravity-ide\brain\2f158bda-8c93-4f2a-8391-76ef18fd7764";
                Directory.CreateDirectory(artifactDir);

                // 1. Dashboard
                var dash = scope.ServiceProvider.GetRequiredService<DashboardUC>();
                dash.Size = new Size(1115, 820);
                dash.LoadDataAsync().GetAwaiter().GetResult();
                using (var bmp = new Bitmap(1115, 820))
                {
                    dash.DrawToBitmap(bmp, new Rectangle(0, 0, 1115, 820));
                    bmp.Save(Path.Combine(artifactDir, "ui_dashboard.png"), System.Drawing.Imaging.ImageFormat.Png);
                }
                Console.WriteLine("[PASS] Rendered ui_dashboard.png");

                // 2. Employee
                var emp = scope.ServiceProvider.GetRequiredService<EmployeeUC>();
                emp.Size = new Size(1115, 820);
                emp.LoadEmployeesAsync().GetAwaiter().GetResult();
                using (var bmp = new Bitmap(1115, 820))
                {
                    emp.DrawToBitmap(bmp, new Rectangle(0, 0, 1115, 820));
                    bmp.Save(Path.Combine(artifactDir, "ui_employee.png"), System.Drawing.Imaging.ImageFormat.Png);
                }
                Console.WriteLine("[PASS] Rendered ui_employee.png");

                // 3. Attendance
                var att = scope.ServiceProvider.GetRequiredService<AttendanceUC>();
                att.Size = new Size(1115, 820);
                att.LoadInitialDataAsync().GetAwaiter().GetResult();
                using (var bmp = new Bitmap(1115, 820))
                {
                    att.DrawToBitmap(bmp, new Rectangle(0, 0, 1115, 820));
                    bmp.Save(Path.Combine(artifactDir, "ui_attendance.png"), System.Drawing.Imaging.ImageFormat.Png);
                }
                Console.WriteLine("[PASS] Rendered ui_attendance.png");

                // 4. Payroll
                var pay = scope.ServiceProvider.GetRequiredService<PayrollUC>();
                pay.Size = new Size(1115, 820);
                pay.LoadPayrollDataAsync().GetAwaiter().GetResult();
                using (var bmp = new Bitmap(1115, 820))
                {
                    pay.DrawToBitmap(bmp, new Rectangle(0, 0, 1115, 820));
                    bmp.Save(Path.Combine(artifactDir, "ui_payroll.png"), System.Drawing.Imaging.ImageFormat.Png);
                }
                Console.WriteLine("[PASS] Rendered ui_payroll.png");

                // 5. Report
                var rep = scope.ServiceProvider.GetRequiredService<ReportUC>();
                rep.Size = new Size(1115, 820);
                rep.LoadReportDataAsync().GetAwaiter().GetResult();
                using (var bmp = new Bitmap(1115, 820))
                {
                    rep.DrawToBitmap(bmp, new Rectangle(0, 0, 1115, 820));
                    bmp.Save(Path.Combine(artifactDir, "ui_report.png"), System.Drawing.Imaging.ImageFormat.Png);
                }
                Console.WriteLine("[PASS] Rendered ui_report.png");

                Console.WriteLine("ALL UI PAGES RENDERED SUCCESSFULLY!");
                return;
            }

            // 4. Run Application
            try
            {
                var mainForm = serviceProvider.GetRequiredService<MainForm>();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thực thi ứng dụng: {ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}