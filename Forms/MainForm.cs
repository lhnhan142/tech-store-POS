using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PhuKienMayTinh.Common.Constants;
using PhuKienMayTinh.GUI.UserControls;

namespace PhuKienMayTinh.GUI.Forms
{
    public class MainForm : Form
    {
        private readonly DashboardUC _dashboardUC;
        private readonly EmployeeUC _employeeUC;
        private readonly AttendanceUC _attendanceUC;
        private readonly PayrollUC _payrollUC;
        private readonly ReportUC _reportUC;

        // UI Containers
        private Panel pnlSidebar = null!;
        private Panel pnlTopHeader = null!;
        private Panel pnlMainContent = null!;
        private Label lblActiveSection = null!;
        private Label lblCurrentUser = null!;
        private System.Windows.Forms.Timer headerClockTimer = null!;
        private Label lblClockHeader = null!;

        // Sidebar navigation buttons
        private Button btnNavDashboard = null!;
        private Button btnNavEmployee = null!;
        private Button btnNavAttendance = null!;
        private Button btnNavPayroll = null!;
        private Button btnNavReport = null!;
        private Button? _currentActiveButton;

        public MainForm(
            DashboardUC dashboardUC,
            EmployeeUC employeeUC,
            AttendanceUC attendanceUC,
            PayrollUC payrollUC,
            ReportUC reportUC)
        {
            _dashboardUC = dashboardUC;
            _employeeUC = employeeUC;
            _attendanceUC = attendanceUC;
            _payrollUC = payrollUC;
            _reportUC = reportUC;

            InitializeComponent();
            SetupClock();
        }

        private void InitializeComponent()
        {
            Text = $"{AppConstants.StoreName} - QUẢN LÝ CỬA HÀNG & NHÂN SỰ";
            Size = new Size(1380, 840);
            MinimumSize = new Size(1200, 720);
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            Font = new Font("Segoe UI", 9f);

            // 1. Sidebar bên trái
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 265,
                BackColor = Color.FromArgb(11, 17, 32), // Obsidian Dark #0B1120
                Padding = new Padding(0)
            };
            Controls.Add(pnlSidebar);

            // Logo & Tiêu đề cửa hàng trên Sidebar
            var pnlLogo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.FromArgb(11, 17, 32),
                Padding = new Padding(18, 18, 18, 10)
            };

            var lblLogoIcon = new Label
            {
                Text = "⚡ PC GEAR STORE",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248), // Sky 400
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(18, 16)
            };
            var lblLogoSubtitle = new Label
            {
                Text = "PHỤ KIỆN MÁY TÍNH & GAMING",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 44)
            };
            pnlLogo.Controls.Add(lblLogoIcon);
            pnlLogo.Controls.Add(lblLogoSubtitle);
            pnlSidebar.Controls.Add(pnlLogo);

            // Sidebar Menu Items Container
            var pnlMenu = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.FromArgb(11, 17, 32)
            };

            btnNavDashboard = CreateNavButton("📊   Dashboard & Chỉ số", 10);
            btnNavDashboard.Click += async (s, e) => await SwitchSectionAsync(_dashboardUC, btnNavDashboard, "DASHBOARD - TỔNG QUAN KINH DOANH & NHÂN SỰ");

            btnNavEmployee = CreateNavButton("👥   Quản lý Nhân sự", 62);
            btnNavEmployee.Click += async (s, e) => await SwitchSectionAsync(_employeeUC, btnNavEmployee, "QUẢN LÝ NHÂN SỰ & HỒ SƠ NHÂN VIÊN");

            btnNavAttendance = CreateNavButton("⏱️   Chấm công thời gian thực", 114);
            btnNavAttendance.Click += async (s, e) => await SwitchSectionAsync(_attendanceUC, btnNavAttendance, "CHẤM CÔNG THỜI GIAN THỰC & BẢNG CÔNG");

            btnNavPayroll = CreateNavButton("💰   Tính lương & Bảng lương", 166);
            btnNavPayroll.Click += async (s, e) => await SwitchSectionAsync(_payrollUC, btnNavPayroll, "QUẢN LÝ TÍNH LƯƠNG & BẢNG THANH TOÁN LƯƠNG");

            btnNavReport = CreateNavButton("📈   Trung tâm Báo cáo", 218);
            btnNavReport.Click += async (s, e) => await SwitchSectionAsync(_reportUC, btnNavReport, "BÁO CÁO THỐNG KÊ & XUẤT EXCEL / PDF");

            pnlMenu.Controls.Add(btnNavDashboard);
            pnlMenu.Controls.Add(btnNavEmployee);
            pnlMenu.Controls.Add(btnNavAttendance);
            pnlMenu.Controls.Add(btnNavPayroll);
            pnlMenu.Controls.Add(btnNavReport);

            pnlSidebar.Controls.Add(pnlMenu);
            pnlMenu.BringToFront();

            // Profile info dưới cùng Sidebar
            var pnlUserProfile = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 75,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblUserAvatar = new Label
            {
                Text = "👨‍💻",
                Font = new Font("Segoe UI Emoji", 16f),
                Location = new Point(14, 18),
                AutoSize = true
            };
            lblCurrentUser = new Label
            {
                Text = "Nguyễn Văn An  ● Online\nVai trò: Quản lý / Admin",
                Font = new Font("Segoe UI", 8.2f, FontStyle.Regular),
                ForeColor = Color.FromArgb(226, 232, 240),
                Location = new Point(54, 16),
                AutoSize = true,
                UseMnemonic = false
            };
            pnlUserProfile.Controls.Add(lblUserAvatar);
            pnlUserProfile.Controls.Add(lblCurrentUser);
            pnlSidebar.Controls.Add(pnlUserProfile);

            // 2. Top Header (Slate 900)
            pnlTopHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(22, 12, 22, 12)
            };

            lblActiveSection = new Label
            {
                Text = "DASHBOARD - TỔNG QUAN KINH DOANH & NHÂN SỰ",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 250, 252),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 16)
            };
            pnlTopHeader.Controls.Add(lblActiveSection);

            // Clock Container góc phải (tránh đè lên title)
            var pnlClockContainer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 320,
                Height = 58,
                BackColor = Color.Transparent
            };

            lblClockHeader = new Label
            {
                Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10, 5, 10, 5),
                AutoSize = true,
                Location = new Point(10, 12)
            };
            pnlClockContainer.Controls.Add(lblClockHeader);
            pnlTopHeader.Controls.Add(pnlClockContainer);

            Controls.Add(pnlTopHeader);

            // 3. Main Content Panel
            pnlMainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42)
            };
            Controls.Add(pnlMainContent);
            pnlMainContent.BringToFront();
        }

        private void SetupClock()
        {
            headerClockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            headerClockTimer.Tick += (s, e) =>
            {
                lblClockHeader.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy  HH:mm:ss");
            };
            headerClockTimer.Start();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await SwitchSectionAsync(_dashboardUC, btnNavDashboard, "DASHBOARD - TỔNG QUAN KINH DOANH & NHÂN SỰ");
        }

        private async Task SwitchSectionAsync(UserControl uc, Button navButton, string title)
        {
            lblActiveSection.Text = title;

            // Highlight active button
            if (_currentActiveButton != null)
            {
                _currentActiveButton.BackColor = Color.Transparent;
                _currentActiveButton.ForeColor = Color.FromArgb(148, 163, 184);
            }

            _currentActiveButton = navButton;
            _currentActiveButton.BackColor = Color.FromArgb(30, 41, 59); // Slate 800
            _currentActiveButton.ForeColor = Color.FromArgb(56, 189, 248); // Sky 400

            // Change content
            pnlMainContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(uc);

            // Trigger data load
            if (uc is DashboardUC dash)
            {
                await dash.LoadDataAsync();
            }
            else if (uc is EmployeeUC emp)
            {
                await emp.LoadEmployeesAsync();
            }
            else if (uc is AttendanceUC att)
            {
                await att.LoadInitialDataAsync();
            }
            else if (uc is PayrollUC pay)
            {
                await pay.LoadPayrollDataAsync();
            }
            else if (uc is ReportUC rep)
            {
                await rep.LoadReportDataAsync();
            }
        }

        private Button CreateNavButton(string text, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(0, y),
                Size = new Size(240, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (s, e) =>
            {
                if (btn != _currentActiveButton)
                {
                    btn.BackColor = Color.FromArgb(20, 28, 48);
                    btn.ForeColor = Color.FromArgb(226, 232, 240);
                }
            };

            btn.MouseLeave += (s, e) =>
            {
                if (btn != _currentActiveButton)
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = Color.FromArgb(148, 163, 184);
                }
            };

            return btn;
        }
    }
}
