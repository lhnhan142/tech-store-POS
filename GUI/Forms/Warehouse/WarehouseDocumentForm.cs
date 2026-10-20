using BLL.DTOs;
using BLL.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using ReaLTaiizor.Util;
using System.ComponentModel;
using System.Globalization;

namespace GUI.Forms.Warehouse
{
    /// <summary>
    /// Khung chung của các màn hình phiếu kho (nhập / xuất / kiểm kê):
    /// bên trái là lịch sử phiếu + chi tiết phiếu đang chọn, bên phải là form lập phiếu mới.
    /// Lớp con chỉ khai báo cột, ô nhập và cách lưu. Cuối constructor của lớp con phải gọi Build().
    /// </summary>
    [DesignerCategory("Code")]
    public abstract class WarehouseDocumentForm : Form
    {
        protected static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
        protected static readonly Color InputBg = Color.FromArgb(243, 245, 249);
        protected static readonly Color InputFocusBg = Color.FromArgb(231, 240, 255);
        protected static readonly Color LabelGray = Color.FromArgb(95, 100, 120);
        protected static readonly Color WarnBg = Color.FromArgb(255, 243, 224);
        protected static readonly Color DangerText = Color.FromArgb(200, 40, 40);

        protected readonly IServiceScopeFactory ScopeFactory;

        protected readonly DataGridView HistoryGrid = new();
        protected readonly DataGridView DetailGrid = new();
        protected readonly DataGridView LineGrid = new();     // các dòng của phiếu đang lập
        protected readonly Label SummaryLabel = new();        // dòng tổng kết phía dưới form lập phiếu
        protected readonly HopeRoundButton BtnSave = new();
        protected readonly HopeRoundButton BtnReset = new();

        private readonly DateTimePicker _dtpFrom = new();
        private readonly DateTimePicker _dtpTo = new();
        private readonly HopeRoundButton _btnFilter = new();
        private readonly Label _lblHistoryCount = new();
        private readonly Label _lblDetailCaption = new();
        private TableLayoutPanel _editor = null!;
        private bool _binding;

        protected WarehouseDocumentForm(IServiceScopeFactory scopeFactory)
        {
            ScopeFactory = scopeFactory;
        }

        // ------------------------------------------------ Lớp con cung cấp
        protected abstract string PageTitle { get; }
        protected abstract string PageSubtitle { get; }
        protected abstract string EditorTitle { get; }
        protected abstract string SaveButtonText { get; }
        protected abstract string HistoryCountFormat { get; }   // vd "{0} phiếu nhập"

        protected abstract void ConfigureHistoryColumns(DataGridView grid);
        protected abstract void ConfigureDetailColumns(DataGridView grid);
        /// <summary>Thêm các ô nhập bằng AddEditorRow / AddEditorFill.</summary>
        protected abstract void BuildEditor();
        /// <summary>Nạp danh sách dùng cho ô chọn (nhà cung cấp, sản phẩm...).</summary>
        protected abstract Task LoadLookupsAsync();
        /// <summary>Tải lịch sử phiếu; trả về danh sách để gán vào bảng.</summary>
        protected abstract Task<System.Collections.IList> LoadHistoryAsync(DateTime from, DateTime to);
        /// <summary>Tải chi tiết của phiếu đang chọn; trả về danh sách để gán vào bảng chi tiết.</summary>
        protected abstract Task<System.Collections.IList> LoadDetailAsync(object historyItem);
        protected abstract Task SaveDocumentAsync();
        protected abstract void ResetEditor();
        /// <summary>Id phiếu của dòng lịch sử (để chọn lại sau khi lưu).</summary>
        protected abstract int IdOf(object historyItem);

        // ------------------------------------------------ Dựng giao diện
        protected int S(int px) => (int)Math.Round(px * DeviceDpi / 96.0);

        protected void Build()
        {
            Text = PageTitle;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(S(1280), S(720));
            MinimumSize = new Size(S(1080), S(600));
            BackColor = AppTheme.Background;

            BuildLayout();
            Load += async (_, _) => await RunAsync(async () =>
            {
                await LoadLookupsAsync();
                await ReloadHistoryAsync();
                ResetEditor();
            });
        }

        private void BuildLayout()
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
            header.Controls.Add(MakeLabel(PageTitle, new Font("Segoe UI Semibold", 17F), AppTheme.TextDark, ContentAlignment.BottomLeft), 0, 0);
            header.Controls.Add(MakeLabel(PageSubtitle, new Font("Segoe UI", 9.5F), AppTheme.TextMuted, ContentAlignment.TopLeft), 0, 1);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background,
                Margin = Padding.Empty
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.Controls.Add(BuildHistoryCard(), 0, 0);
            content.Controls.Add(BuildEditorCard(), 1, 0);

            page.Controls.Add(header, 0, 0);
            page.Controls.Add(content, 0, 1);
            Controls.Add(page);
        }

        // ===== Bên trái: lọc theo ngày + lịch sử + chi tiết =====
        private Control BuildHistoryCard()
        {
            var card = new RoundedPanel
            {
                Radius = S(14),
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(S(18)),
                Margin = new Padding(0, 0, S(18), 0)
            };
            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = AppTheme.Surface
            };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, S(48)));   // lọc ngày
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 55));       // lịch sử
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, S(30)));   // đếm
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, S(34)));   // tiêu đề chi tiết
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 45));       // chi tiết

            // --- Thanh lọc ngày ---
            _dtpFrom.Format = DateTimePickerFormat.Short;
            _dtpTo.Format = DateTimePickerFormat.Short;
            _dtpFrom.Value = DateTime.Today.AddDays(-30);
            _dtpTo.Value = DateTime.Today;
            _dtpFrom.Dock = DockStyle.Fill;
            _dtpTo.Dock = DockStyle.Fill;
            _dtpFrom.Margin = new Padding(0, S(6), S(8), S(6));
            _dtpTo.Margin = new Padding(0, S(6), S(8), S(6));
            ConfigButton(_btnFilter, "Lọc", HopeButtonType.Primary, async (_, _) => await RunAsync(() => ReloadHistoryAsync()));

            var filter = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                BackColor = AppTheme.Surface,
                Margin = Padding.Empty
            };
            filter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(34)));
            filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            filter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(42)));
            filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            filter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(80)));
            filter.Controls.Add(MakeLabel("Từ", Font, LabelGray, ContentAlignment.MiddleLeft), 0, 0);
            filter.Controls.Add(_dtpFrom, 1, 0);
            filter.Controls.Add(MakeLabel("Đến", Font, LabelGray, ContentAlignment.MiddleLeft), 2, 0);
            filter.Controls.Add(_dtpTo, 3, 0);
            filter.Controls.Add(_btnFilter, 4, 0);

            // --- Bảng lịch sử ---
            SetupGrid(HistoryGrid, readOnly: true);
            ConfigureHistoryColumns(HistoryGrid);
            HistoryGrid.SelectionChanged += async (_, _) => await OnHistorySelectionChangedAsync();

            _lblHistoryCount.Dock = DockStyle.Fill;
            _lblHistoryCount.Margin = Padding.Empty;
            _lblHistoryCount.TextAlign = ContentAlignment.MiddleLeft;
            _lblHistoryCount.ForeColor = AppTheme.TextMuted;
            _lblHistoryCount.Font = new Font("Segoe UI", 9F);

            _lblDetailCaption.Dock = DockStyle.Fill;
            _lblDetailCaption.Margin = Padding.Empty;
            _lblDetailCaption.TextAlign = ContentAlignment.BottomLeft;
            _lblDetailCaption.ForeColor = AppTheme.TextDark;
            _lblDetailCaption.Font = new Font("Segoe UI Semibold", 10.5F);
            _lblDetailCaption.Text = "Chi tiết phiếu";

            // --- Bảng chi tiết ---
            SetupGrid(DetailGrid, readOnly: true);
            ConfigureDetailColumns(DetailGrid);

            left.Controls.Add(filter, 0, 0);
            left.Controls.Add(HistoryGrid, 0, 1);
            left.Controls.Add(_lblHistoryCount, 0, 2);
            left.Controls.Add(_lblDetailCaption, 0, 3);
            left.Controls.Add(DetailGrid, 0, 4);
            card.Controls.Add(left);
            return card;
        }

        // ===== Bên phải: form lập phiếu =====
        private Control BuildEditorCard()
        {
            var card = new RoundedPanel
            {
                Radius = S(14),
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(S(22), S(18), S(22), S(18)),
                Margin = Padding.Empty
            };
            _editor = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                BackColor = AppTheme.Surface
            };
            _editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddEditorRow(MakeLabel(EditorTitle, new Font("Segoe UI Semibold", 12.5F), AppTheme.TextDark, ContentAlignment.MiddleLeft), 34);

            BuildEditor();

            SummaryLabel.Font = new Font("Segoe UI Semibold", 10.5F);
            SummaryLabel.ForeColor = AppTheme.TextDark;
            SummaryLabel.TextAlign = ContentAlignment.MiddleRight;
            AddEditorRow(SummaryLabel, 36);

            ConfigButton(BtnSave, SaveButtonText, HopeButtonType.Primary, async (_, _) => await RunAsync(SaveDocumentAsync));
            ConfigButton(BtnReset, "Làm mới", HopeButtonType.Warning, (_, _) => ResetEditor());
            var buttons = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Surface,
                Margin = Padding.Empty
            };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            buttons.Controls.Add(BtnSave, 0, 0);
            buttons.Controls.Add(BtnReset, 1, 0);
            AddEditorRow(buttons, 56);

            card.Controls.Add(_editor);
            return card;
        }

        /// <summary>Thêm một hàng cao cố định vào form lập phiếu.</summary>
        protected void AddEditorRow(Control c, int height)
        {
            c.Dock = DockStyle.Fill;
            _editor.RowStyles.Add(new RowStyle(SizeType.Absolute, S(height)));
            _editor.Controls.Add(c, 0, _editor.RowStyles.Count - 1);
        }

        /// <summary>Thêm một hàng chiếm hết phần chiều cao còn lại (dùng cho bảng các dòng).</summary>
        protected void AddEditorFill(Control c)
        {
            c.Dock = DockStyle.Fill;
            _editor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _editor.Controls.Add(c, 0, _editor.RowStyles.Count - 1);
        }

        // ------------------------------------------------ Tiện ích dựng UI cho lớp con
        protected static Label MakeLabel(string text, Font font, Color color, ContentAlignment align) => new()
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

        /// <summary>Một ô nhập có nhãn phía trên (nhãn 24px + ô nhập bo góc).</summary>
        protected Control Field(string caption, Control input)
        {
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 0, S(10), 0),
                BackColor = AppTheme.Surface
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, S(24)));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            t.Controls.Add(MakeLabel(caption, new Font("Segoe UI", 9.5F), LabelGray, ContentAlignment.BottomLeft), 0, 0);
            t.Controls.Add(WrapInput(input), 0, 1);
            return t;
        }

        /// <summary>Xếp nhiều ô nhập trên một hàng theo tỉ lệ chiều rộng.</summary>
        protected Control FieldRow(params (Control Field, float Weight)[] columns)
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = columns.Length,
                RowCount = 1,
                Margin = Padding.Empty,
                BackColor = AppTheme.Surface
            };
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < columns.Length; i++)
            {
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, columns[i].Weight));
                row.Controls.Add(columns[i].Field, i, 0);
            }
            return row;
        }

        /// <summary>Hàng các nút nhỏ (Thêm dòng, Xóa dòng...) chia đều chiều ngang.</summary>
        protected Control ButtonRow(params HopeRoundButton[] buttons)
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = buttons.Length,
                RowCount = 1,
                Margin = Padding.Empty,
                BackColor = AppTheme.Surface
            };
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < buttons.Length; i++)
            {
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / buttons.Length));
                row.Controls.Add(buttons[i], i, 0);
            }
            return row;
        }

        // Ô nhập bo góc: nền xám nhạt, xanh nhạt khi đang gõ, control luôn nằm giữa theo chiều dọc
        protected RoundedPanel WrapInput(Control box)
        {
            var wrap = new RoundedPanel
            {
                Radius = S(10),
                BackColor = InputBg,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty
            };
            if (box is TextBox tb) tb.BorderStyle = BorderStyle.None;
            if (box is NumericUpDown nud) nud.BorderStyle = BorderStyle.None;
            if (box is ComboBox cb) cb.FlatStyle = FlatStyle.Flat;
            box.BackColor = InputBg;
            box.ForeColor = AppTheme.TextDark;
            box.Dock = DockStyle.None;
            box.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            void Center()
            {
                int pad = S(12);
                box.SetBounds(pad, Math.Max(0, (wrap.Height - box.Height) / 2), Math.Max(10, wrap.Width - pad * 2), box.Height);
            }
            wrap.Controls.Add(box);
            wrap.Resize += (_, _) => Center();
            wrap.HandleCreated += (_, _) => Center();
            box.Enter += (_, _) => { wrap.BackColor = InputFocusBg; box.BackColor = InputFocusBg; };
            box.Leave += (_, _) => { wrap.BackColor = InputBg; box.BackColor = InputBg; };
            wrap.Click += (_, _) => box.Focus();
            return wrap;
        }

        protected void ConfigButton(HopeRoundButton btn, string text, HopeButtonType type, EventHandler onClick)
        {
            btn.Text = text;
            btn.ButtonType = type;
            btn.Font = new Font("Segoe UI Semibold", 10F);
            btn.Dock = DockStyle.Fill;
            btn.Margin = new Padding(S(3));
            btn.Click += onClick;
        }

        // Bảng phẳng: không lưới dọc, tiêu đề nhạt, dòng chọn xanh nhạt
        protected void SetupGrid(DataGridView grid, bool readOnly)
        {
            grid.Dock = DockStyle.Fill;
            grid.Margin = Padding.Empty;
            grid.AutoGenerateColumns = false;
            grid.ReadOnly = readOnly;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToResizeColumns = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = AppTheme.Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(238, 240, 245);
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = S(38);
            grid.ColumnHeadersDefaultCellStyle.BackColor = InputBg;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = LabelGray;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = InputBg;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = LabelGray;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;   // tiêu đề không bị xuống dòng
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(S(8), 0, 0, 0);
            grid.RowTemplate.Height = S(38);
            grid.DefaultCellStyle.ForeColor = AppTheme.TextDark;
            grid.DefaultCellStyle.Padding = new Padding(S(8), 0, 0, 0);
            grid.DefaultCellStyle.SelectionBackColor = InputFocusBg;
            grid.DefaultCellStyle.SelectionForeColor = AppTheme.Primary;
            grid.DataError += (_, e) => e.ThrowException = false;
        }

        protected DataGridViewTextBoxColumn TextColumn(string header, string property, int fillWeight = 0, int width = 0,
            string? format = null, bool right = false, int minWidth = 80)
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
                col.MinimumWidth = S(minWidth);   // hẹp quá thì hiện thanh cuộn ngang thay vì bóp cột
            }
            else
            {
                col.Width = S(width > 0 ? width : 100);
            }
            if (format is not null)
            {
                col.DefaultCellStyle.Format = format;
                col.DefaultCellStyle.FormatProvider = Vi;
            }
            if (right)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            return col;
        }

        protected static string Money(decimal value) => value.ToString("N0", Vi) + " ₫";

        // ------------------------------------------------ Lịch sử & chi tiết
        protected async Task ReloadHistoryAsync(int? selectId = null)
        {
            if (_dtpFrom.Value.Date > _dtpTo.Value.Date)
                throw new BusinessException("Ngày bắt đầu không được sau ngày kết thúc.");

            var list = await LoadHistoryAsync(_dtpFrom.Value.Date, _dtpTo.Value.Date);

            _binding = true;
            try
            {
                HistoryGrid.DataSource = list;
                _lblHistoryCount.Text = string.Format(HistoryCountFormat, list.Count);
                HistoryGrid.CurrentCell = null;
                HistoryGrid.ClearSelection();
                DetailGrid.DataSource = null;
                _lblDetailCaption.Text = "Chi tiết phiếu";
            }
            finally
            {
                _binding = false;
            }

            if (selectId is int id)
            {
                foreach (DataGridViewRow row in HistoryGrid.Rows)
                {
                    if (row.DataBoundItem is { } item && IdOf(item) == id)
                    {
                        row.Selected = true;
                        HistoryGrid.CurrentCell = row.Cells[0];
                        break;
                    }
                }
            }
        }

        private async Task OnHistorySelectionChangedAsync()
        {
            if (_binding) return;
            if (HistoryGrid.SelectedRows.Count == 0 || HistoryGrid.SelectedRows[0].DataBoundItem is not { } item)
                return;

            var id = IdOf(item);
            try
            {
                var lines = await LoadDetailAsync(item);
                // Người dùng có thể đã chọn dòng khác trong lúc chờ: bỏ kết quả cũ
                if (HistoryGrid.SelectedRows.Count == 0 || HistoryGrid.SelectedRows[0].DataBoundItem is not { } now || IdOf(now) != id)
                    return;
                DetailGrid.DataSource = lines;
                _lblDetailCaption.Text = $"Chi tiết phiếu #{id}";
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // ------------------------------------------------ Chạy thao tác
        // Chạy một thao tác: khóa nút trong lúc chạy, hiển thị lỗi cho người dùng
        protected async Task RunAsync(Func<Task> action)
        {
            SetBusy(true);
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        protected static void ShowError(Exception ex)
        {
            if (ex is BusinessException)
            {
                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show($"Đã xảy ra lỗi hệ thống:\n{ex.GetBaseException().Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected virtual void SetBusy(bool busy)
        {
            BtnSave.Enabled = BtnReset.Enabled = _btnFilter.Enabled = !busy;
            UseWaitCursor = busy;
        }

        // Mỗi thao tác dùng một scope (một DbContext) riêng để dữ liệu luôn mới
        protected async Task<TResult> Run<TService, TResult>(Func<TService, Task<TResult>> action) where TService : notnull
        {
            using var scope = ScopeFactory.CreateScope();
            return await action(scope.ServiceProvider.GetRequiredService<TService>());
        }

        protected async Task Run<TService>(Func<TService, Task> action) where TService : notnull
        {
            using var scope = ScopeFactory.CreateScope();
            await action(scope.ServiceProvider.GetRequiredService<TService>());
        }

        /// <summary>
        /// Gọi sau khi lưu phiếu thành công: nạp lại danh sách (tồn kho mới), lịch sử, làm mới form,
        /// báo cho các màn hình khác biết tồn kho đã đổi và hiện cảnh báo nếu có sản phẩm sắp hết.
        /// </summary>
        protected async Task AfterSavedAsync(InventoryResultDTO result, string message)
        {
            await LoadLookupsAsync();
            await ReloadHistoryAsync(result.DocumentId);
            ResetEditor();
            StockEvents.RaiseChanged();

            if (result.Alerts.Count == 0)
            {
                MessageBox.Show(message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            const int maxShown = 8;
            var lines = result.Alerts.Take(maxShown)
                .Select(a => $"•  {a.Name}: {(a.Level == StockLevel.OutOfStock ? "đã hết hàng" : $"còn {a.StockQuantity}")}");
            var more = result.Alerts.Count > maxShown ? $"\n... và {result.Alerts.Count - maxShown} sản phẩm khác" : "";
            MessageBox.Show($"{message}\n\nCảnh báo tồn kho thấp:\n{string.Join("\n", lines)}{more}",
                "Thành công - có cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
