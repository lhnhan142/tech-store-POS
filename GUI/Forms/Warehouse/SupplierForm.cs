using BLL.Exceptions;
using BLL.Services.Interfaces;
using DAL.Entities;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using ReaLTaiizor.Util;
using System.ComponentModel;

namespace GUI.Forms.Warehouse
{
    /// <summary>Màn hình quản lý Nhà cung cấp (xem, tìm kiếm, thêm, sửa, xóa).</summary>
    [DesignerCategory("Code")]
    public class SupplierForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly TextBox _txtSearch = new();
        private readonly Label _lblCount = new();
        private readonly DataGridView _grid = new();
        private readonly TextBox _txtName = new();
        private readonly TextBox _txtPhone = new();
        private readonly TextBox _txtAddress = new();
        private readonly HopeRoundButton _btnAdd = new();
        private readonly HopeRoundButton _btnUpdate = new();
        private readonly HopeRoundButton _btnDelete = new();
        private readonly HopeRoundButton _btnClear = new();

        private List<Supplier> _all = new();
        private bool _binding;

        public SupplierForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Quản lý Nhà cung cấp";
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(S(1100), S(640));
            MinimumSize = new Size(S(900), S(560));
            BackColor = AppTheme.Background;

            BuildUi();
            Load += async (_, _) => await RunAsync(() => ReloadAsync());
        }

        // ---------------------------------------------------------------- UI
        private static readonly Color InputBg = Color.FromArgb(243, 245, 249);
        private static readonly Color InputFocusBg = Color.FromArgb(231, 240, 255);
        private static readonly Color LabelGray = Color.FromArgb(95, 100, 120);

        // Đổi pixel thiết kế (96 DPI) sang pixel thật theo độ phóng to của màn hình
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

        private void BuildUi()
        {
            // ===== Khung trang: hàng tiêu đề + hàng nội dung =====
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

            // Tiêu đề xếp dọc bằng lưới nên không bao giờ đè lên nhau
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
            header.Controls.Add(MakeLabel("Quản lý nhà cung cấp", new Font("Segoe UI Semibold", 17F),
                AppTheme.TextDark, ContentAlignment.BottomLeft), 0, 0);
            header.Controls.Add(MakeLabel("Thêm, chỉnh sửa và tra cứu đơn vị cung cấp hàng cho kho", new Font("Segoe UI", 9.5F),
                AppTheme.TextMuted, ContentAlignment.TopLeft), 0, 1);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background,
                Margin = Padding.Empty
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(390)));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // ===== Bên trái: tìm kiếm + bảng + đếm =====
            var leftCard = new RoundedPanel
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
                RowCount = 3,
                BackColor = AppTheme.Surface
            };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, S(58)));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, S(30)));

            _txtSearch.PlaceholderText = "Tìm theo tên, số điện thoại hoặc địa chỉ...";
            _txtSearch.TextChanged += (_, _) => ApplyFilter();
            var searchBox = WrapInput(_txtSearch);
            searchBox.Margin = new Padding(0, 0, 0, S(12));

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
            StyleGrid();
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Mã",
                DataPropertyName = nameof(Supplier.Id),
                Width = S(64),
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tên nhà cung cấp",
                DataPropertyName = nameof(Supplier.Name),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 42
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Số điện thoại",
                DataPropertyName = nameof(Supplier.Phone),
                Width = S(140)
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Địa chỉ",
                DataPropertyName = nameof(Supplier.Address),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 30
            });
            _grid.SelectionChanged += (_, _) => OnSelectionChanged();

            _lblCount.Dock = DockStyle.Fill;
            _lblCount.Margin = Padding.Empty;
            _lblCount.AutoSize = false;
            _lblCount.TextAlign = ContentAlignment.MiddleLeft;
            _lblCount.ForeColor = AppTheme.TextMuted;
            _lblCount.Font = new Font("Segoe UI", 9F);

            left.Controls.Add(searchBox, 0, 0);
            left.Controls.Add(_grid, 0, 1);
            left.Controls.Add(_lblCount, 0, 2);
            leftCard.Controls.Add(left);

            // ===== Bên phải: form nhập (mỗi hàng cao cố định, cột tự giãn hết chiều rộng) =====
            var rightCard = new RoundedPanel
            {
                Radius = S(14),
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(S(22), S(20), S(22), S(20)),
                Margin = Padding.Empty
            };
            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                BackColor = AppTheme.Surface
            };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            void AddRow(Control c, int height)
            {
                c.Dock = DockStyle.Fill;
                right.RowStyles.Add(new RowStyle(SizeType.Absolute, S(height)));
                right.Controls.Add(c, 0, right.RowStyles.Count - 1);
            }

            AddRow(MakeLabel("Thông tin chi tiết", new Font("Segoe UI Semibold", 12.5F),
                AppTheme.TextDark, ContentAlignment.MiddleLeft), 32);
            AddRow(MakeLabel("Chọn một dòng trong bảng để sửa hoặc xóa", new Font("Segoe UI", 9F),
                AppTheme.TextMuted, ContentAlignment.TopLeft), 26);

            _txtName.MaxLength = 100;
            _txtName.KeyPress += TxtName_KeyPress;
            _txtPhone.MaxLength = 20;
            _txtPhone.KeyPress += TxtPhone_KeyPress;
            _txtAddress.MaxLength = 200;
            _txtAddress.KeyPress += TxtAddress_KeyPress;

            var fieldFont = new Font("Segoe UI", 9.5F);
            AddRow(MakeLabel("Tên nhà cung cấp *", fieldFont, LabelGray, ContentAlignment.BottomLeft), 34);
            AddRow(WrapInput(_txtName), 46);
            AddRow(MakeLabel("Số điện thoại", fieldFont, LabelGray, ContentAlignment.BottomLeft), 34);
            AddRow(WrapInput(_txtPhone), 46);
            AddRow(MakeLabel("Địa chỉ", fieldFont, LabelGray, ContentAlignment.BottomLeft), 34);
            AddRow(WrapInput(_txtAddress), 46);

            ConfigButton(_btnAdd, "Thêm mới", HopeButtonType.Primary, async (_, _) => await RunAsync(AddAsync));
            ConfigButton(_btnUpdate, "Cập nhật", HopeButtonType.Warning, async (_, _) => await RunAsync(UpdateAsync));
            ConfigButton(_btnDelete, "Xóa", HopeButtonType.Danger, async (_, _) => await RunAsync(DeleteAsync));
            ConfigButton(_btnClear, "Làm mới", HopeButtonType.Primary, (_, _) => ClearInput());

            // Nút xếp 2 x 2, hai cột bằng nhau, hai hàng bằng nhau
            var buttons = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                BackColor = AppTheme.Surface,
                Padding = new Padding(0, S(18), 0, 0),
                Margin = Padding.Empty
            };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            buttons.Controls.Add(_btnAdd, 0, 0);
            buttons.Controls.Add(_btnUpdate, 1, 0);
            buttons.Controls.Add(_btnDelete, 0, 1);
            buttons.Controls.Add(_btnClear, 1, 1);
            AddRow(buttons, 130);

            // Hàng đệm cuối để mọi thứ dồn lên trên
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.RowCount = right.RowStyles.Count;
            rightCard.Controls.Add(right);

            content.Controls.Add(leftCard, 0, 0);
            content.Controls.Add(rightCard, 1, 0);
            page.Controls.Add(header, 0, 0);
            page.Controls.Add(content, 0, 1);
            Controls.Add(page);
        }

        // Ô nhập bo góc: nền xám nhạt, xanh nhạt khi đang gõ, TextBox luôn nằm giữa theo chiều dọc
        private RoundedPanel WrapInput(TextBox box)
        {
            var wrap = new RoundedPanel
            {
                Radius = S(10),
                BackColor = InputBg,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty
            };
            box.BorderStyle = BorderStyle.None;
            box.BackColor = InputBg;
            box.ForeColor = AppTheme.TextDark;
            box.Dock = DockStyle.None;
            box.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            void Center()
            {
                int pad = S(14);
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

        // Bảng phẳng: không lưới dọc, tiêu đề nhạt, dòng chọn xanh nhạt
        private void StyleGrid()
        {
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
        }

        private void ConfigButton(HopeRoundButton btn, string text, HopeButtonType type, EventHandler onClick)
        {
            btn.Text = text;
            btn.ButtonType = type;
            btn.Font = new Font("Segoe UI Semibold", 10F);
            btn.Dock = DockStyle.Fill;
            btn.Margin = new Padding(S(3));
            btn.Click += onClick;
        }

        // Tên: chữ, số, khoảng trắng và & ( ) , . - / + '
        private static void TxtName_KeyPress(object? sender, KeyPressEventArgs e) =>
            FilterKey(e, "&(),.-/+'");

        // Địa chỉ: chữ, số, khoảng trắng và , . - / # ( ) '
        private static void TxtAddress_KeyPress(object? sender, KeyPressEventArgs e) =>
            FilterKey(e, ",.-/#()'");

        // Số điện thoại: chỉ số, + (đầu số), khoảng trắng, dấu chấm, gạch ngang
        private static void TxtPhone_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            if (!char.IsDigit(e.KeyChar) && "+ .-".IndexOf(e.KeyChar) < 0)
                e.Handled = true;
        }

        private static void FilterKey(KeyPressEventArgs e, string extraAllowed)
        {
            if (char.IsControl(e.KeyChar)) return; // Backspace, Ctrl+C/V...
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) && !extraAllowed.Contains(e.KeyChar))
                e.Handled = true;
        }

        // ------------------------------------------------------------ Dữ liệu
        private Supplier? SelectedItem =>
            _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].DataBoundItem as Supplier : null;

        private void OnSelectionChanged()
        {
            if (_binding) return;
            if (SelectedItem is { } s)
            {
                _txtName.Text = s.Name;
                _txtPhone.Text = s.Phone ?? string.Empty;
                _txtAddress.Text = s.Address ?? string.Empty;
            }
        }

        private async Task ReloadAsync(int? selectId = null)
        {
            _all = (await Run<ISupplierService, IReadOnlyList<Supplier>>(s => s.GetAllAsync())).ToList();
            ApplyFilter();

            if (selectId is int id)
            {
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (row.DataBoundItem is Supplier s && s.Id == id)
                    {
                        row.Selected = true;
                        _grid.CurrentCell = row.Cells[1];
                        return;
                    }
                }
            }
            ClearFields();
        }

        private void ApplyFilter()
        {
            var keyword = _txtSearch.Text.Trim();
            _binding = true;
            try
            {
                var list = _all
                    .Where(s => Contains(s.Name, keyword) || Contains(s.Phone, keyword) || Contains(s.Address, keyword))
                    .ToList();
                _grid.DataSource = list;
                _lblCount.Text = $"Hiển thị {list.Count} / {_all.Count} nhà cung cấp";
                _grid.CurrentCell = null;
                _grid.ClearSelection();
            }
            finally
            {
                _binding = false;
            }
        }

        private static bool Contains(string? text, string keyword) =>
            text is not null && text.Contains(keyword, StringComparison.CurrentCultureIgnoreCase);

        private void ClearFields()
        {
            _txtName.Clear();
            _txtPhone.Clear();
            _txtAddress.Clear();
        }

        private void ClearInput()
        {
            _txtSearch.Clear();
            ClearFields();
            _grid.ClearSelection();
            _txtName.Focus();
        }

        // ------------------------------------------------------------- Nút bấm
        private async Task AddAsync()
        {
            await Run<ISupplierService>(s => s.CreateAsync(_txtName.Text, _txtPhone.Text, _txtAddress.Text));
            await ReloadAsync();
            MessageBox.Show("Đã thêm nhà cung cấp.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task UpdateAsync()
        {
            if (SelectedItem is not { } item)
            {
                MessageBox.Show("Hãy chọn một nhà cung cấp trong bảng để sửa.", "Chưa chọn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await Run<ISupplierService>(s => s.UpdateAsync(item.Id, _txtName.Text, _txtPhone.Text, _txtAddress.Text));
            await ReloadAsync(item.Id);
            MessageBox.Show("Đã cập nhật nhà cung cấp.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is not { } item)
            {
                MessageBox.Show("Hãy chọn một nhà cung cấp trong bảng để xóa.", "Chưa chọn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa nhà cung cấp '{item.Name}'?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            await Run<ISupplierService>(s => s.DeleteAsync(item.Id));
            await ReloadAsync();
        }

        // Chạy một thao tác: khóa nút trong lúc chạy, hiển thị lỗi cho người dùng
        private async Task RunAsync(Func<Task> action)
        {
            SetBusy(true);
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
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy)
        {
            _btnAdd.Enabled = _btnUpdate.Enabled = _btnDelete.Enabled = _btnClear.Enabled = !busy;
            UseWaitCursor = busy;
        }

        // Mỗi thao tác dùng một scope (một DbContext) riêng để dữ liệu luôn mới
        private async Task<TResult> Run<TService, TResult>(Func<TService, Task<TResult>> action) where TService : notnull
        {
            using var scope = _scopeFactory.CreateScope();
            return await action(scope.ServiceProvider.GetRequiredService<TService>());
        }

        private async Task Run<TService>(Func<TService, Task> action) where TService : notnull
        {
            using var scope = _scopeFactory.CreateScope();
            await action(scope.ServiceProvider.GetRequiredService<TService>());
        }
    }
}