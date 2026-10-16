using BLL.DTOs;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using ReaLTaiizor.Util;
using System.ComponentModel;
using System.Globalization;

namespace GUI.Forms.Warehouse
{
    /// <summary>Danh sách sản phẩm sắp hết hàng / hết hàng; tự làm mới khi tồn kho thay đổi.</summary>
    [DesignerCategory("Code")]
    public class StockAlertForm : Form
    {
        private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
        private static readonly Color InputBg = Color.FromArgb(243, 245, 249);
        private static readonly Color InputFocusBg = Color.FromArgb(231, 240, 255);
        private static readonly Color LabelGray = Color.FromArgb(95, 100, 120);
        private static readonly Color OutBg = Color.FromArgb(255, 236, 236);
        private static readonly Color LowBg = Color.FromArgb(255, 245, 225);
        private static readonly Color OutText = Color.FromArgb(200, 40, 40);
        private static readonly Color LowText = Color.FromArgb(190, 110, 0);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DataGridView _grid = new();
        private readonly Label _lblThreshold = new();
        private readonly Label _lblCount = new();
        private readonly HopeRoundButton _btnRefresh = new();

        public StockAlertForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Cảnh báo tồn kho";
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(S(1100), S(640));
            MinimumSize = new Size(S(900), S(520));
            BackColor = AppTheme.Background;

            BuildUi();
            StockEvents.Changed += OnStockChanged;
            Load += async (_, _) => await RunAsync(ReloadAsync);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) StockEvents.Changed -= OnStockChanged;
            base.Dispose(disposing);
        }

        private int S(int px) => (int)Math.Round(px * DeviceDpi / 96.0);

        private static Label MakeLabel(string text, Font font, Color color, ContentAlignment align) => new()
        {
            Text = text,
            Font = font,
            ForeColor = color,
            TextAlign = align,
            AutoSize = false,
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };

        // ---------------------------------------------------------------- UI
        private void BuildUi()
        {
            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(S(24), S(16), S(24), S(22)),
                BackColor = AppTheme.Background
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, S(78)));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.Background,
                Margin = Padding.Empty
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, S(46)));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, S(26)));
            header.Controls.Add(MakeLabel("Cảnh báo tồn kho", new Font("Segoe UI Semibold", 17F), AppTheme.TextDark, ContentAlignment.BottomLeft), 0, 0);
            header.Controls.Add(MakeLabel("Các sản phẩm sắp hết hoặc đã hết hàng, cần nhập thêm", new Font("Segoe UI", 9.5F), AppTheme.TextMuted, ContentAlignment.TopLeft), 0, 1);

            var card = new RoundedPanel
            {
                Radius = S(14),
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(S(18)),
                Margin = Padding.Empty
            };
            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Surface
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, S(52)));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, S(30)));

            // Thanh trên: ngưỡng cảnh báo + nút làm mới
            _lblThreshold.Dock = DockStyle.Fill;
            _lblThreshold.Margin = Padding.Empty;
            _lblThreshold.TextAlign = ContentAlignment.MiddleLeft;
            _lblThreshold.ForeColor = LabelGray;
            _lblThreshold.Font = new Font("Segoe UI", 10F);

            _btnRefresh.Text = "Làm mới";
            _btnRefresh.ButtonType = HopeButtonType.Primary;
            _btnRefresh.Font = new Font("Segoe UI Semibold", 10F);
            _btnRefresh.Dock = DockStyle.Fill;
            _btnRefresh.Margin = new Padding(S(3));
            _btnRefresh.Click += async (_, _) => await RunAsync(ReloadAsync);

            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Surface,
                Margin = Padding.Empty
            };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(130)));
            top.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            top.Controls.Add(_lblThreshold, 0, 0);
            top.Controls.Add(_btnRefresh, 1, 0);

            StyleGrid();
            _grid.Columns.Add(Col("Mã", nameof(StockAlertDTO.ProductId), width: 64));
            _grid.Columns.Add(Col("Sản phẩm", nameof(StockAlertDTO.Name), fillWeight: 50));
            _grid.Columns.Add(Col("Mã vạch", nameof(StockAlertDTO.Barcode), width: 150));
            _grid.Columns.Add(Col("Tồn kho", nameof(StockAlertDTO.StockQuantity), width: 100, right: true));
            _grid.Columns.Add(Col("Ngưỡng", nameof(StockAlertDTO.Threshold), width: 90, right: true));
            _grid.Columns.Add(Col("Trạng thái", nameof(StockAlertDTO.LevelText), width: 120));
            _grid.CellFormatting += Grid_CellFormatting;

            _lblCount.Dock = DockStyle.Fill;
            _lblCount.Margin = Padding.Empty;
            _lblCount.TextAlign = ContentAlignment.MiddleLeft;
            _lblCount.ForeColor = AppTheme.TextMuted;
            _lblCount.Font = new Font("Segoe UI", 9F);

            body.Controls.Add(top, 0, 0);
            body.Controls.Add(_grid, 0, 1);
            body.Controls.Add(_lblCount, 0, 2);
            card.Controls.Add(body);

            page.Controls.Add(header, 0, 0);
            page.Controls.Add(card, 0, 1);
            Controls.Add(page);
        }

        private DataGridViewTextBoxColumn Col(string header, string property, int fillWeight = 0, int width = 100, bool right = false)
        {
            var col = new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                DataPropertyName = property,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            if (fillWeight > 0)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                col.FillWeight = fillWeight;
            }
            else
            {
                col.Width = S(width);
            }
            if (right)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.DefaultCellStyle.Format = "N0";
                col.DefaultCellStyle.FormatProvider = Vi;
            }
            return col;
        }

        private void StyleGrid()
        {
            _grid.Dock = DockStyle.Fill;
            _grid.Margin = Padding.Empty;
            _grid.AutoGenerateColumns = false;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.AllowUserToResizeColumns = false;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.RowHeadersVisible = false;
            _grid.BackgroundColor = AppTheme.Surface;
            _grid.BorderStyle = BorderStyle.None;
            _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _grid.GridColor = Color.FromArgb(238, 240, 245);
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _grid.ColumnHeadersHeight = S(42);
            _grid.ColumnHeadersDefaultCellStyle.BackColor = InputBg;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = LabelGray;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F);
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = InputBg;
            _grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = LabelGray;
            _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(S(8), 0, 0, 0);
            _grid.RowTemplate.Height = S(42);
            _grid.DefaultCellStyle.ForeColor = AppTheme.TextDark;
            _grid.DefaultCellStyle.Padding = new Padding(S(8), 0, 0, 0);
            _grid.DefaultCellStyle.SelectionBackColor = InputFocusBg;
            _grid.DefaultCellStyle.SelectionForeColor = AppTheme.Primary;
            _grid.DataError += (_, e) => e.ThrowException = false;
        }

        // Hết hàng: nền đỏ nhạt, chữ đỏ. Sắp hết: nền cam nhạt. Dòng đang chọn giữ màu chọn mặc định.
        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.Rows[e.RowIndex].DataBoundItem is not StockAlertDTO alert) return;

            bool selected = _grid.Rows[e.RowIndex].Selected;
            bool isStatusOrStock = _grid.Columns[e.ColumnIndex].DataPropertyName is nameof(StockAlertDTO.LevelText) or nameof(StockAlertDTO.StockQuantity);

            if (!selected)
                e.CellStyle!.BackColor = alert.Level == StockLevel.OutOfStock ? OutBg : LowBg;
            if (isStatusOrStock)
            {
                e.CellStyle!.ForeColor = alert.Level == StockLevel.OutOfStock ? OutText : LowText;
                e.CellStyle.Font = new Font("Segoe UI Semibold", 10F);
            }
        }

        // ---------------------------------------------------------------- Dữ liệu
        private async Task ReloadAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IStockAlertService>();
            var alerts = await svc.GetAlertsAsync();
            if (IsDisposed) return;

            _grid.DataSource = alerts.ToList();
            _grid.ClearSelection();
            _lblThreshold.Text = $"Cảnh báo khi tồn kho ≤ {svc.Threshold} sản phẩm (chỉnh trong appsettings.json, mục Inventory)";

            int outCount = alerts.Count(a => a.Level == StockLevel.OutOfStock);
            _lblCount.Text = alerts.Count == 0
                ? "Tất cả sản phẩm đều đủ hàng."
                : $"{alerts.Count} sản phẩm cần chú ý, trong đó {outCount} đã hết hàng";
        }

        // Tồn kho vừa thay đổi ở màn hình khác: làm mới âm thầm, lỗi tạm thời thì giữ nguyên bảng cũ
        private async void OnStockChanged()
        {
            if (IsDisposed) return;
            try { await ReloadAsync(); }
            catch { /* người dùng vẫn có thể bấm "Làm mới" */ }
        }

        private async Task RunAsync(Func<Task> action)
        {
            _btnRefresh.Enabled = false;
            UseWaitCursor = true;
            try
            {
                await action();
            }
            catch (BusinessException ex)
            {
                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi hệ thống:\n{ex.GetBaseException().Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                {
                    _btnRefresh.Enabled = true;
                    UseWaitCursor = false;
                }
            }
        }
    }
}
