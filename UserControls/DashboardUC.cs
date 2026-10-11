using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.BLL.Services.Interfaces;
using PhuKienMayTinh.GUI.Controls;

namespace PhuKienMayTinh.GUI.UserControls
{
    public class DashboardUC : UserControl
    {
        private readonly IDashboardService _dashboardService;

        // Controls
        private FlowLayoutPanel pnlCards = null!;
        private StatCardControl cardRevenue = null!;
        private StatCardControl cardOrders = null!;
        private StatCardControl cardEmployees = null!;
        private StatCardControl cardInventory = null!;

        private RevenueLineChart chartRevenue = null!;
        private CategoryDoughnutChart chartCategory = null!;
        private RankingBarChart chartRanking = null!;
        private DataGridView dgvLowStock = null!;

        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private Button btnFilter = null!;
        private Button btnQuick7Days = null!;
        private Button btnQuick30Days = null!;
        private Button btnQuickThisMonth = null!;

        public DashboardUC(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            BackColor = Color.FromArgb(15, 23, 42); // Slate 900 #0F172A
            Dock = DockStyle.Fill;
            AutoScroll = true;

            // 1. Top Bar: Header & Bộ lọc thời gian (Dark Slate #1E293B)
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20, 12, 20, 12)
            };

            var lblTitle = new Label
            {
                Text = "⚡ TỔNG QUAN KINH DOANH & NHÂN SỰ CỬA HÀNG",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 250, 252),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 18)
            };
            pnlTop.Controls.Add(lblTitle);

            // Filter controls ở góc phải
            var pnlFilterControls = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 720,
                Height = 45,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            btnQuick7Days = CreateFilterButton("7 ngày qua");
            btnQuick7Days.Click += async (s, e) =>
            {
                dtpFrom.Value = DateTime.Today.AddDays(-7);
                dtpTo.Value = DateTime.Today;
                await LoadDataAsync();
            };

            btnQuick30Days = CreateFilterButton("30 ngày qua");
            btnQuick30Days.Click += async (s, e) =>
            {
                dtpFrom.Value = DateTime.Today.AddDays(-30);
                dtpTo.Value = DateTime.Today;
                await LoadDataAsync();
            };

            btnQuickThisMonth = CreateFilterButton("Tháng này");
            btnQuickThisMonth.Click += async (s, e) =>
            {
                dtpFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                dtpTo.Value = DateTime.Today;
                await LoadDataAsync();
            };

            var lblFrom = new Label { Text = "Từ:", AutoSize = true, Font = new Font("Segoe UI", 9f), Margin = new Padding(8, 8, 2, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            dtpFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100, Value = DateTime.Today.AddDays(-14), Font = new Font("Segoe UI", 9f), Margin = new Padding(0, 4, 5, 0) };

            var lblTo = new Label { Text = "Đến:", AutoSize = true, Font = new Font("Segoe UI", 9f), Margin = new Padding(5, 8, 2, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            dtpTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100, Value = DateTime.Today, Font = new Font("Segoe UI", 9f), Margin = new Padding(0, 4, 8, 0) };

            btnFilter = new Button
            {
                Text = "🔍 Lọc",
                Width = 85,
                Height = 32,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Margin = new Padding(0, 2, 0, 0)
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += async (s, e) => await LoadDataAsync();

            pnlFilterControls.Controls.Add(btnQuick7Days);
            pnlFilterControls.Controls.Add(btnQuick30Days);
            pnlFilterControls.Controls.Add(btnQuickThisMonth);
            pnlFilterControls.Controls.Add(lblFrom);
            pnlFilterControls.Controls.Add(dtpFrom);
            pnlFilterControls.Controls.Add(lblTo);
            pnlFilterControls.Controls.Add(dtpTo);
            pnlFilterControls.Controls.Add(btnFilter);

            pnlTop.Controls.Add(pnlFilterControls);
            Controls.Add(pnlTop);

            // 2. Container nội dung chính (Slate 900)
            var pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 20),
                BackColor = Color.FromArgb(15, 23, 42),
                AutoScroll = true
            };
            Controls.Add(pnlContent);
            pnlContent.BringToFront();

            // 3. Layout bảng tổng thể để bảo đảm thứ tự hiển thị chính xác 100%
            var pnlDashboardLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0)
            };
            pnlDashboardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            pnlDashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130f)); // Row 0: Thẻ KPI
            pnlDashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 330f)); // Row 1: Line & Donut
            pnlDashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 305f)); // Row 2: Ranking & Table

            // 4. Stat Cards Flow (Row 0)
            pnlCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Height = 125,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                Margin = new Padding(0, 0, 0, 10)
            };

            cardRevenue = new StatCardControl { Width = 265, Height = 112, CardTitle = "Doanh thu tháng này", CardValue = "...", CardSubtitle = "+14.2% so với tháng trước", AccentColor = Color.FromArgb(56, 189, 248), IconSymbol = "💰" };
            cardOrders = new StatCardControl { Width = 265, Height = 112, CardTitle = "Tổng số đơn hàng", CardValue = "...", CardSubtitle = "Hôm nay: ... đơn", AccentColor = Color.FromArgb(52, 211, 153), IconSymbol = "📦" };
            cardEmployees = new StatCardControl { Width = 265, Height = 112, CardTitle = "Nhân sự hoạt động", CardValue = "...", CardSubtitle = "Đã check-in: ...", AccentColor = Color.FromArgb(168, 85, 247), IconSymbol = "👥" };
            cardInventory = new StatCardControl { Width = 265, Height = 112, CardTitle = "Cảnh báo tồn kho", CardValue = "...", CardSubtitle = "Sản phẩm sắp hết hàng", AccentColor = Color.FromArgb(244, 63, 94), IconSymbol = "⚠️" };

            pnlCards.Controls.Add(cardRevenue);
            pnlCards.Controls.Add(cardOrders);
            pnlCards.Controls.Add(cardEmployees);
            pnlCards.Controls.Add(cardInventory);

            pnlDashboardLayout.Controls.Add(pnlCards, 0, 0);

            // 5. Bảng biểu đồ Row 1 (Line Chart + Doughnut Chart)
            var pnlRow1 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Margin = new Padding(0, 5, 0, 10)
            };
            pnlRow1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            pnlRow1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

            chartRevenue = new RevenueLineChart();
            var pnlChart1Box = CreateChartBox("📈 BIỂU ĐỒ DOANH THU THEO NGÀY (GDI+ Spline Chart)", chartRevenue);

            chartCategory = new CategoryDoughnutChart();
            var pnlChart2Box = CreateChartBox("🍩 CƠ CẤU DOANH THU THEO DANH MỤC (GDI+ Donut Chart)", chartCategory);

            pnlRow1.Controls.Add(pnlChart1Box, 0, 0);
            pnlRow1.Controls.Add(pnlChart2Box, 1, 0);
            pnlDashboardLayout.Controls.Add(pnlRow1, 0, 1);

            // 6. Bảng biểu đồ Row 2 (Ranking Chart + Low Stock Table)
            var pnlRow2 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Margin = new Padding(0, 5, 0, 10)
            };
            pnlRow2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            pnlRow2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            chartRanking = new RankingBarChart();
            var pnlRankingBox = CreateChartBox("🏆 TOP NHÂN VIÊN XUẤT SẮC (GDI+ Ranking Chart)", chartRanking);

            dgvLowStock = new DataGridView
            {
                BackgroundColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(51, 65, 85),
                RowTemplate = { Height = 34 }
            };
            dgvLowStock.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvLowStock.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
            dgvLowStock.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            dgvLowStock.ColumnHeadersHeight = 36;
            dgvLowStock.DefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvLowStock.DefaultCellStyle.ForeColor = Color.FromArgb(241, 245, 249);
            dgvLowStock.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 58, 138); // Blue 900
            dgvLowStock.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvLowStock.Columns.Add("Code", "Mã SP");
            dgvLowStock.Columns.Add("Name", "Tên sản phẩm");
            dgvLowStock.Columns.Add("Cat", "Danh mục");
            dgvLowStock.Columns.Add("Qty", "Tồn kho");
            dgvLowStock.Columns.Add("Status", "Tình trạng");

            var pnlTableBox = CreateChartBox("⚠️ CẢNH BÁO TỒN KHO: SẢN PHẨM CẦN NHẬP THÊM (<= 10 CÁI)", dgvLowStock);

            pnlRow2.Controls.Add(pnlRankingBox, 0, 0);
            pnlRow2.Controls.Add(pnlTableBox, 1, 0);
            pnlDashboardLayout.Controls.Add(pnlRow2, 0, 2);

            pnlContent.Controls.Add(pnlDashboardLayout);
        }

        private Panel CreateChartBox(string title, Control contentControl)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 8, 12, 12),
                BackColor = Color.FromArgb(30, 41, 59),
                Margin = new Padding(6)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var lbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248), // Sky 400
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                UseMnemonic = false,
                Margin = new Padding(0)
            };
            table.Controls.Add(lbl, 0, 0);

            contentControl.Dock = DockStyle.Fill;
            contentControl.Margin = new Padding(0);
            table.Controls.Add(contentControl, 0, 1);

            pnl.Controls.Add(table);
            return pnl;
        }

        private Button CreateFilterButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 30,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.2f),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Margin = new Padding(2, 4, 4, 0)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(71, 85, 105);
            return btn;
        }

        public async Task LoadDataAsync()
        {
            try
            {
                btnFilter.Enabled = false;
                btnFilter.Text = "...";

                var from = dtpFrom.Value.Date;
                var to = dtpTo.Value.Date;

                var data = await _dashboardService.GetDashboardSummaryAsync(from, to);

                // Update Cards
                cardRevenue.SetValues(
                    "Doanh thu tháng này",
                    $"{data.MonthRevenue:N0} đ",
                    $"Hôm nay: {data.TodayRevenue:N0} đ",
                    Color.FromArgb(56, 189, 248),
                    "💰"
                );

                cardOrders.SetValues(
                    "Tổng số đơn hàng",
                    $"{data.MonthOrderCount} đơn",
                    $"Hôm nay: {data.TodayOrderCount} đơn",
                    Color.FromArgb(52, 211, 153),
                    "📦"
                );

                cardEmployees.SetValues(
                    "Nhân sự cửa hàng",
                    $"{data.ActiveEmployeeCount} nhân viên",
                    $"Đã check-in: {data.TodayPresentEmployeeCount}",
                    Color.FromArgb(168, 85, 247),
                    "👥"
                );

                cardInventory.SetValues(
                    "Cảnh báo tồn kho",
                    $"{data.LowStockProductCount} sản phẩm",
                    $"Tổng mặt hàng: {data.TotalProductCount}",
                    Color.FromArgb(244, 63, 94),
                    "⚠️"
                );

                // Update GDI+ Charts
                chartRevenue.SetData(data.RevenuePoints);
                chartCategory.SetData(data.CategorySlices);
                chartRanking.SetData(data.TopEmployees);

                // Update Low Stock DataGridView
                dgvLowStock.Rows.Clear();
                foreach (var item in data.LowStockAlerts)
                {
                    int rIdx = dgvLowStock.Rows.Add(
                        item.ProductCode,
                        item.ProductName,
                        item.Category,
                        item.StockQuantity,
                        "Cần nhập thêm"
                    );

                    dgvLowStock.Rows[rIdx].DefaultCellStyle.ForeColor = Color.FromArgb(248, 113, 113); // Rose 400
                    dgvLowStock.Rows[rIdx].DefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu Dashboard: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnFilter.Enabled = true;
                btnFilter.Text = "🔍 Lọc";
            }
        }
    }
}
