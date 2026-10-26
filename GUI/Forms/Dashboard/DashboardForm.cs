using BLL.DTOs.Dashboard;
using BLL.Services.Interfaces;
using GUI.Controls;
using GUI.Themes;
using GUI.Utils;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI.Forms.Dashboard
{
    [DesignerCategory("Code")]
    public class DashboardForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // KPI Cards
        private readonly Label _lblRevenue = new();
        private readonly Label _lblRevenueSub = new();
        private readonly Label _lblOrders = new();
        private readonly Label _lblOrdersSub = new();
        private readonly Label _lblEmployees = new();
        private readonly Label _lblEmployeesSub = new();
        private readonly Label _lblStock = new();
        private readonly Label _lblStockSub = new();
        private readonly Label _lblPayroll = new();
        private readonly Label _lblPayrollSub = new();

        // Date Controls
        private readonly ComboBox _cboDatePreset = new();
        private readonly DateTimePicker _dtpFrom = new();
        private readonly DateTimePicker _dtpTo = new();
        private readonly HopeRoundButton _btnRefresh = new();

        // Chart & Toggles
        private readonly ChartControl _chart = new();
        private readonly HopeRoundButton _btnBarChart = new();
        private readonly HopeRoundButton _btnLineChart = new();

        // Grids
        private readonly DataGridView _gridTopProducts = new();
        private readonly DataGridView _gridCategoryStock = new();

        private DashboardSummaryDto? _currentSummary;

        public DashboardForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Tổng quan kinh doanh & nhân sự (Dashboard)";
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.FromArgb(243, 244, 246);
            AutoScroll = true;

            BuildUi();
            Load += async (_, _) => await LoadDataAsync();
        }

        private int S(int px) => (int)Math.Round(px * DeviceDpi / 96.0);

        private void BuildUi()
        {
            // 1. Header & Filters
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(72),
                BackColor = Color.White,
                Padding = new Padding(S(20), S(12), S(20), S(12))
            };
            pnlHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(229, 231, 235), 1);
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "TỔNG QUAN HOẠT ĐỘNG KINH DOAN & NHÂN SỰ",
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = AppTheme.Primary,
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(S(20), S(10))
            };

            var lblSub = new Label
            {
                Text = "Thống kê thời gian thực doanh thu, đơn hàng, tồn kho và chi phí lương nhân viên",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(107, 114, 128),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(S(20), S(38))
            };

            // Date Filters in Header Right
            var pnlFilterRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, S(8), 0, 0),
                BackColor = Color.Transparent
            };

            _cboDatePreset.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboDatePreset.Font = new Font("Segoe UI", 9.5F);
            _cboDatePreset.Size = new Size(S(130), S(32));
            _cboDatePreset.Items.AddRange(new object[] { "7 ngày qua", "30 ngày qua", "Tháng này", "Quý này", "Năm nay", "Tùy chọn" });
            _cboDatePreset.SelectedIndex = 1; // 30 ngày qua
            _cboDatePreset.SelectedIndexChanged += OnPresetChanged;

            _dtpFrom.Format = DateTimePickerFormat.Short;
            _dtpFrom.Font = new Font("Segoe UI", 9.5F);
            _dtpFrom.Size = new Size(S(105), S(32));
            _dtpFrom.Value = DateTime.Today.AddDays(-30);

            var lblArrow = new Label
            {
                Text = "→",
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(107, 114, 128),
                Margin = new Padding(S(4), S(6), S(4), 0)
            };

            _dtpTo.Format = DateTimePickerFormat.Short;
            _dtpTo.Font = new Font("Segoe UI", 9.5F);
            _dtpTo.Size = new Size(S(105), S(32));
            _dtpTo.Value = DateTime.Today;

            _btnRefresh.Text = "Cập nhật";
            _btnRefresh.Size = new Size(S(95), S(34));
            _btnRefresh.TextColor = Color.White;
            _btnRefresh.PrimaryColor = AppTheme.Primary;
            _btnRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _btnRefresh.Cursor = Cursors.Hand;
            _btnRefresh.Margin = new Padding(S(8), 0, 0, 0);
            _btnRefresh.Click += async (_, _) => await LoadDataAsync();

            pnlFilterRight.Controls.AddRange(new Control[] { _cboDatePreset, _dtpFrom, lblArrow, _dtpTo, _btnRefresh });
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub, pnlFilterRight });

            // 2. KPI Cards Panel (5 cards)
            var pnlKpi = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = S(105),
                ColumnCount = 5,
                RowCount = 1,
                Padding = new Padding(S(20), S(12), S(20), S(8)),
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 5; i++)
            {
                pnlKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            }

            pnlKpi.Controls.Add(CreateKpiCard("DOANH THU THUẦN", _lblRevenue, "0 đ", _lblRevenueSub, "Đã trừ hàng hoàn trả", AppTheme.Primary), 0, 0);
            pnlKpi.Controls.Add(CreateKpiCard("ĐƠN HÀNG THÀNH CÔNG", _lblOrders, "0 đơn", _lblOrdersSub, "AOV: 0 đ/đơn", Color.FromArgb(16, 185, 129)), 1, 0);
            pnlKpi.Controls.Add(CreateKpiCard("NHÂN SỰ HOẠT ĐỘNG", _lblEmployees, "0 / 0", _lblEmployeesSub, "Nhân viên làm việc", Color.FromArgb(99, 102, 241)), 2, 0);
            pnlKpi.Controls.Add(CreateKpiCard("TỒN KHO LINH KIỆN", _lblStock, "0 món", _lblStockSub, "Sắp hết: 0 SP", Color.FromArgb(245, 158, 11)), 3, 0);
            pnlKpi.Controls.Add(CreateKpiCard("CHI PHÍ LƯƠNG THÁNG", _lblPayroll, "0 đ", _lblPayrollSub, $"Kỳ lương T{DateTime.Now.Month}", Color.FromArgb(217, 70, 239)), 4, 0);

            // 3. Middle Section: Chart (65%) & Category Inventory (35%)
            var pnlMiddle = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = S(330),
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(S(20), S(6), S(20), S(8)),
                BackColor = Color.Transparent
            };
            pnlMiddle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
            pnlMiddle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));

            // Chart Container
            var pnlChartCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, S(8), 0),
                Padding = new Padding(S(16), S(12), S(16), S(12))
            };
            pnlChartCard.Paint += (s, e) => DrawCardBorder(e.Graphics, pnlChartCard);

            var pnlChartHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(42),
                BackColor = Color.Transparent
            };

            var lblChartTitle = new Label
            {
                Text = "BIỂU ĐỒ DOANH THU KINH DOANH",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Dock = DockStyle.Left,
                AutoSize = true,
                UseMnemonic = false
            };

            var pnlChartToggle = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent
            };

            ConfigureToggleButton(_btnBarChart, "Cột (Bar)", true);
            ConfigureToggleButton(_btnLineChart, "Đường (Line)", false);

            _btnBarChart.Click += (_, _) =>
            {
                _chart.ChartType = ChartType.Bar;
                _btnBarChart.PrimaryColor = AppTheme.Primary;
                _btnLineChart.PrimaryColor = Color.FromArgb(209, 213, 219);
            };

            _btnLineChart.Click += (_, _) =>
            {
                _chart.ChartType = ChartType.Line;
                _btnLineChart.PrimaryColor = AppTheme.Primary;
                _btnBarChart.PrimaryColor = Color.FromArgb(209, 213, 219);
            };

            pnlChartToggle.Controls.AddRange(new Control[] { _btnBarChart, _btnLineChart });
            pnlChartHeader.Controls.AddRange(new Control[] { lblChartTitle, pnlChartToggle });

            _chart.Dock = DockStyle.Fill;
            _chart.Title = "";
            _chart.ValueFormat = "{0:N0} đ";
            _chart.PrimaryColor = AppTheme.Primary;

            pnlChartCard.Controls.Add(_chart);
            pnlChartCard.Controls.Add(pnlChartHeader);

            // Category Inventory Container
            var pnlCategoryCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(S(8), 0, 0, 0),
                Padding = new Padding(S(16), S(12), S(16), S(12))
            };
            pnlCategoryCard.Paint += (s, e) => DrawCardBorder(e.Graphics, pnlCategoryCard);

            var lblCategoryTitle = new Label
            {
                Text = "TỒN KHO THEO DANH MỤC",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Dock = DockStyle.Top,
                Height = S(32)
            };

            BuildCategoryGrid();
            _gridCategoryStock.Dock = DockStyle.Fill;

            pnlCategoryCard.Controls.Add(_gridCategoryStock);
            pnlCategoryCard.Controls.Add(lblCategoryTitle);

            pnlMiddle.Controls.Add(pnlChartCard, 0, 0);
            pnlMiddle.Controls.Add(pnlCategoryCard, 1, 0);

            // 4. Bottom Section: Top 5 Selling Products (Full width)
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(270),
                Padding = new Padding(S(20), S(6), S(20), S(20)),
                BackColor = Color.Transparent
            };

            var pnlTopProductsCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(S(16), S(12), S(16), S(12))
            };
            pnlTopProductsCard.Paint += (s, e) => DrawCardBorder(e.Graphics, pnlTopProductsCard);

            var lblTopProductsTitle = new Label
            {
                Text = "TOP 5 SẢN PHẨM BÁN CHẠY NHẤT TRONG KỲ",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Dock = DockStyle.Top,
                Height = S(32)
            };

            BuildTopProductsGrid();
            _gridTopProducts.Dock = DockStyle.Fill;

            pnlTopProductsCard.Controls.Add(_gridTopProducts);
            pnlTopProductsCard.Controls.Add(lblTopProductsTitle);
            pnlBottom.Controls.Add(pnlTopProductsCard);

            // Assemble into main Form (using reverse order for DockStyle.Top)
            Controls.Add(pnlBottom);
            Controls.Add(pnlMiddle);
            Controls.Add(pnlKpi);
            Controls.Add(pnlHeader);
        }

        private void ConfigureToggleButton(HopeRoundButton btn, string text, bool active)
        {
            btn.Text = text;
            btn.Size = new Size(S(95), S(32));
            btn.TextColor = Color.White;
            btn.PrimaryColor = active ? AppTheme.Primary : Color.FromArgb(209, 213, 219);
            btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Margin = new Padding(S(4), 0, 0, 0);
        }

        private Panel CreateKpiCard(string title, Label lblVal, string defVal, Label lblSub, string defSub, Color accent)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(S(5)),
                Padding = new Padding(S(14), S(10), S(14), S(10))
            };
            card.Paint += (s, e) =>
            {
                using var path = GetRoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), S(8));
                using var pen = new Pen(Color.FromArgb(229, 231, 235), 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);

                using var accentBrush = new SolidBrush(accent);
                e.Graphics.FillRectangle(accentBrush, 0, 0, S(5), card.Height);
            };

            var lblT = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(107, 114, 128),
                Dock = DockStyle.Top,
                Height = S(16)
            };

            lblVal.Text = defVal;
            lblVal.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            lblVal.ForeColor = accent;
            lblVal.Dock = DockStyle.Top;
            lblVal.Height = S(32);
            lblVal.TextAlign = ContentAlignment.MiddleLeft;

            lblSub.Text = defSub;
            lblSub.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            lblSub.ForeColor = Color.FromArgb(156, 163, 175);
            lblSub.Dock = DockStyle.Bottom;
            lblSub.Height = S(16);

            card.Controls.Add(lblSub);
            card.Controls.Add(lblVal);
            card.Controls.Add(lblT);
            return card;
        }

        private void DrawCardBorder(Graphics g, Control c)
        {
            using var path = GetRoundedRect(new Rectangle(0, 0, c.Width - 1, c.Height - 1), S(8));
            using var pen = new Pen(Color.FromArgb(229, 231, 235), 1);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawPath(pen, path);
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void BuildCategoryGrid()
        {
            _gridCategoryStock.AutoGenerateColumns = false;
            _gridCategoryStock.AllowUserToAddRows = false;
            _gridCategoryStock.AllowUserToDeleteRows = false;
            _gridCategoryStock.ReadOnly = true;
            _gridCategoryStock.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridCategoryStock.RowHeadersVisible = false;
            _gridCategoryStock.BackgroundColor = Color.White;
            _gridCategoryStock.BorderStyle = BorderStyle.None;
            _gridCategoryStock.RowTemplate.Height = S(32);
            _gridCategoryStock.ColumnHeadersHeight = S(34);
            _gridCategoryStock.EnableHeadersVisualStyles = false;

            _gridCategoryStock.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            _gridCategoryStock.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            _gridCategoryStock.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            _gridCategoryStock.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "CategoryName", HeaderText = "Danh mục", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { Name = "ProductCount", HeaderText = "Mặt hàng", Width = S(85) },
                new DataGridViewTextBoxColumn { Name = "TotalQuantity", HeaderText = "Tồn kho", Width = S(85) }
            });

            _gridCategoryStock.Columns["ProductCount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _gridCategoryStock.Columns["TotalQuantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void BuildTopProductsGrid()
        {
            _gridTopProducts.AutoGenerateColumns = false;
            _gridTopProducts.AllowUserToAddRows = false;
            _gridTopProducts.AllowUserToDeleteRows = false;
            _gridTopProducts.ReadOnly = true;
            _gridTopProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridTopProducts.RowHeadersVisible = false;
            _gridTopProducts.BackgroundColor = Color.White;
            _gridTopProducts.BorderStyle = BorderStyle.None;
            _gridTopProducts.RowTemplate.Height = S(34);
            _gridTopProducts.ColumnHeadersHeight = S(36);
            _gridTopProducts.EnableHeadersVisualStyles = false;

            _gridTopProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            _gridTopProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            _gridTopProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            _gridTopProducts.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Rank", HeaderText = "Hạng", Width = S(70) },
                new DataGridViewTextBoxColumn { Name = "ProductId", HeaderText = "Mã SP", Width = S(90) },
                new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Tên linh kiện / phụ kiện", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { Name = "QuantitySold", HeaderText = "Đã bán (Chiếc)", Width = S(140) },
                new DataGridViewTextBoxColumn { Name = "TotalRevenue", HeaderText = "Doanh thu mang lại", Width = S(160) }
            });

            _gridTopProducts.Columns["Rank"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridTopProducts.Columns["QuantitySold"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _gridTopProducts.Columns["TotalRevenue"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _gridTopProducts.Columns["TotalRevenue"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridTopProducts.Columns["TotalRevenue"].DefaultCellStyle.ForeColor = AppTheme.Primary;
        }

        private void OnPresetChanged(object? sender, EventArgs e)
        {
            var now = DateTime.Today;
            switch (_cboDatePreset.SelectedIndex)
            {
                case 0: // 7 ngày qua
                    _dtpFrom.Value = now.AddDays(-7);
                    _dtpTo.Value = now;
                    break;
                case 1: // 30 ngày qua
                    _dtpFrom.Value = now.AddDays(-30);
                    _dtpTo.Value = now;
                    break;
                case 2: // Tháng này
                    _dtpFrom.Value = new DateTime(now.Year, now.Month, 1);
                    _dtpTo.Value = now;
                    break;
                case 3: // Quý này
                    int q = (now.Month - 1) / 3;
                    _dtpFrom.Value = new DateTime(now.Year, q * 3 + 1, 1);
                    _dtpTo.Value = now;
                    break;
                case 4: // Năm nay
                    _dtpFrom.Value = new DateTime(now.Year, 1, 1);
                    _dtpTo.Value = now;
                    break;
                case 5: // Tùy chọn
                    // User manually selects
                    return;
            }
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                using var scope = _scopeFactory.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();

                DateTime fromDate = _dtpFrom.Value.Date;
                DateTime toDate = _dtpTo.Value.Date.AddDays(1).AddSeconds(-1);

                _currentSummary = await dashboardService.GetSummaryAsync();
                var chartPoints = await dashboardService.GetDailyRevenueChartAsync(fromDate, toDate);
                var topProducts = await dashboardService.GetTopSellingProductsAsync(fromDate, toDate, 5);
                var stockCategories = await dashboardService.GetStockByCategoryAsync();

                // Update KPI Cards
                _lblRevenue.Text = $"{_currentSummary.TotalRevenue:N0} đ";
                _lblRevenueSub.Text = $"Từ {_dtpFrom.Value:dd/MM} đến {_dtpTo.Value:dd/MM}";

                _lblOrders.Text = $"{_currentSummary.CompletedOrders:N0} đơn";
                _lblOrdersSub.Text = $"Tổng đơn: {_currentSummary.TotalOrders:N0}";

                _lblEmployees.Text = $"{_currentSummary.ActiveEmployees} NV";
                _lblEmployeesSub.Text = "Nhân viên làm việc";

                _lblStock.Text = $"{_currentSummary.TotalStockQuantity:N0} món";
                _lblStockSub.Text = _currentSummary.LowStockProductsCount > 0
                    ? $"Cảnh báo: {_currentSummary.LowStockProductsCount} SP sắp hết!"
                    : "Mức tồn kho ổn định";
                if (_currentSummary.LowStockProductsCount > 0)
                    _lblStockSub.ForeColor = Color.FromArgb(220, 38, 38);
                else
                    _lblStockSub.ForeColor = Color.FromArgb(16, 185, 129);

                _lblPayroll.Text = $"{_currentSummary.MonthlyPayrollExpense:N0} đ";
                _lblPayrollSub.Text = $"Quỹ lương Tháng {DateTime.Now.Month}/{DateTime.Now.Year}";

                // Update Chart
                var chartItems = chartPoints.Select(r => new ChartItem
                {
                    Label = r.Label,
                    Value = r.Revenue,
                    Tooltip = $"{r.Revenue:N0} đ ({r.OrderCount} đơn)"
                }).ToList();
                _chart.SetData(chartItems);

                // Update Stock Categories Grid
                _gridCategoryStock.Rows.Clear();
                foreach (var cat in stockCategories)
                {
                    _gridCategoryStock.Rows.Add(
                        cat.CategoryName,
                        $"{cat.ProductCount} SP",
                        $"{cat.TotalQuantity:N0}"
                    );
                }

                // Update Top Selling Products Grid
                _gridTopProducts.Rows.Clear();
                int rank = 1;
                foreach (var prod in topProducts)
                {
                    string medal = rank switch
                    {
                        1 => "🥇 #1",
                        2 => "🥈 #2",
                        3 => "🥉 #3",
                        _ => $"#{rank}"
                    };

                    _gridTopProducts.Rows.Add(
                        medal,
                        $"SP-{prod.ProductId:D3}",
                        prod.ProductName,
                        $"{prod.QuantitySold:N0}",
                        $"{prod.TotalRevenue:N0} đ"
                    );
                    rank++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu Dashboard: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
