using BLL.DTOs;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using ReaLTaiizor.Util;
using System.ComponentModel;

namespace GUI.Forms.Products
{
    /// <summary>Cấu hình để dùng chung một màn hình cho Danh mục và Thương hiệu.</summary>
    public sealed record CatalogFormOptions(
        string Title,          // "Quản lý danh mục"
        string Subtitle,       // dòng mô tả dưới tiêu đề
        string Noun,           // "danh mục" / "thương hiệu" (dùng trong thông báo)
        string NameLabel,      // "Tên danh mục *"
        Func<IServiceProvider, ICatalogService> Resolve);

    /// <summary>Màn hình CRUD đơn giản cho các bảng chỉ có Tên (Danh mục, Thương hiệu).</summary>
    [DesignerCategory("Code")]
    public class CatalogForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CatalogFormOptions _opt;

        private readonly TextBox _txtSearch = new();
        private readonly TextBox _txtName = new();
        private readonly DataGridView _grid = new();
        private readonly Label _lblCount = new();
        private readonly HopeRoundButton _btnAdd = new();
        private readonly HopeRoundButton _btnUpdate = new();
        private readonly HopeRoundButton _btnDelete = new();
        private readonly HopeRoundButton _btnClear = new();

        private List<CatalogItemDTO> _all = new();
        private bool _binding;

        protected CatalogForm(IServiceScopeFactory scopeFactory, CatalogFormOptions options)
        {
            _scopeFactory = scopeFactory;
            _opt = options;

            Text = options.Title;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(this.S(1000), this.S(600));
            MinimumSize = new Size(this.S(860), this.S(520));
            BackColor = AppTheme.Background;

            BuildUi();
            Load += async (_, _) => await RunAsync(() => ReloadAsync());
        }

        private void BuildUi()
        {
            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(this.S(24), this.S(16), this.S(24), this.S(22)),
                BackColor = AppTheme.Background
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(78)));
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
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(46)));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(26)));
            header.Controls.Add(UiKit.MakeLabel(_opt.Title, new Font("Segoe UI Semibold", 17F),
                AppTheme.TextDark, ContentAlignment.BottomLeft), 0, 0);
            header.Controls.Add(UiKit.MakeLabel(_opt.Subtitle, new Font("Segoe UI", 9.5F),
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
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, this.S(390)));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // ===== Trái: tìm kiếm + bảng + đếm
            var leftCard = new RoundedPanel
            {
                Radius = this.S(14),
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(this.S(18)),
                Margin = new Padding(0, 0, this.S(18), 0)
            };
            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Surface
            };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(58)));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(30)));

            _txtSearch.PlaceholderText = $"Tìm {_opt.Noun}...";
            _txtSearch.TextChanged += (_, _) => ApplyFilter();
            var searchBox = UiKit.WrapInput(_txtSearch, this);
            searchBox.Margin = new Padding(0, 0, 0, this.S(12));

            UiKit.StyleGrid(_grid, this);
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Mã",
                DataPropertyName = nameof(CatalogItemDTO.Id),
                Width = this.S(64),
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tên",
                DataPropertyName = nameof(CatalogItemDTO.Name),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Số sản phẩm",
                DataPropertyName = nameof(CatalogItemDTO.ProductCount),
                Width = this.S(130),
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
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

            // ===== Phải: form nhập
            var rightCard = new RoundedPanel
            {
                Radius = this.S(14),
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(this.S(22), this.S(20), this.S(22), this.S(20)),
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
                right.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(height)));
                right.Controls.Add(c, 0, right.RowStyles.Count - 1);
            }

            AddRow(UiKit.MakeLabel("Thông tin chi tiết", new Font("Segoe UI Semibold", 12.5F),
                AppTheme.TextDark, ContentAlignment.MiddleLeft), 32);
            AddRow(UiKit.MakeLabel("Chọn một dòng trong bảng để sửa hoặc xóa", new Font("Segoe UI", 9F),
                AppTheme.TextMuted, ContentAlignment.TopLeft), 26);

            _txtName.MaxLength = 100;
            _txtName.KeyPress += (_, e) => UiKit.FilterKey(e, "&(),.-/+'");
            AddRow(UiKit.MakeLabel(_opt.NameLabel, new Font("Segoe UI", 9.5F), UiKit.LabelGray,
                ContentAlignment.BottomLeft), 34);
            AddRow(UiKit.WrapInput(_txtName, this), 46);

            UiKit.ConfigButton(_btnAdd, "Thêm mới", HopeButtonType.Primary, async (_, _) => await RunAsync(AddAsync), this);
            UiKit.ConfigButton(_btnUpdate, "Cập nhật", HopeButtonType.Warning, async (_, _) => await RunAsync(UpdateAsync), this);
            UiKit.ConfigButton(_btnDelete, "Xóa", HopeButtonType.Danger, async (_, _) => await RunAsync(DeleteAsync), this);
            UiKit.ConfigButton(_btnClear, "Làm mới", HopeButtonType.Primary, (_, _) => ClearInput(), this);

            var buttons = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                BackColor = AppTheme.Surface,
                Padding = new Padding(0, this.S(18), 0, 0),
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

            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.RowCount = right.RowStyles.Count;
            rightCard.Controls.Add(right);

            content.Controls.Add(leftCard, 0, 0);
            content.Controls.Add(rightCard, 1, 0);
            page.Controls.Add(header, 0, 0);
            page.Controls.Add(content, 0, 1);
            Controls.Add(page);
        }

        // ------------------------------------------------------------ Dữ liệu
        private CatalogItemDTO? SelectedItem =>
            _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].DataBoundItem as CatalogItemDTO : null;

        private void OnSelectionChanged()
        {
            if (_binding) return;
            if (SelectedItem is { } item) _txtName.Text = item.Name;
        }

        private async Task ReloadAsync(int? selectId = null)
        {
            _all = (await RunQuery(s => s.GetAllAsync())).ToList();
            ApplyFilter();

            if (selectId is int id)
            {
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (row.DataBoundItem is CatalogItemDTO c && c.Id == id)
                    {
                        row.Selected = true;
                        _grid.CurrentCell = row.Cells[1];
                        return;
                    }
                }
            }
            _txtName.Clear();
        }

        private void ApplyFilter()
        {
            var keyword = UiKit.RemoveDiacritics(_txtSearch.Text.Trim());
            _binding = true;
            try
            {
                var list = _all.Where(c => UiKit.RemoveDiacritics(c.Name).Contains(keyword)).ToList();
                _grid.DataSource = list;
                _lblCount.Text = $"Hiển thị {list.Count} / {_all.Count} {_opt.Noun}";
                _grid.CurrentCell = null;
                _grid.ClearSelection();
            }
            finally { _binding = false; }
        }

        private void ClearInput()
        {
            _txtSearch.Clear();
            _txtName.Clear();
            _grid.ClearSelection();
            _txtName.Focus();
        }

        // ------------------------------------------------------------ Nút bấm
        private async Task AddAsync()
        {
            await Run(s => s.CreateAsync(_txtName.Text));
            await ReloadAsync();
            MessageBox.Show($"Đã thêm {_opt.Noun}.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task UpdateAsync()
        {
            if (SelectedItem is not { } item)
            {
                MessageBox.Show($"Hãy chọn một {_opt.Noun} trong bảng để sửa.", "Chưa chọn",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await Run(s => s.UpdateAsync(item.Id, _txtName.Text));
            await ReloadAsync(item.Id);
            MessageBox.Show($"Đã cập nhật {_opt.Noun}.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is not { } item)
            {
                MessageBox.Show($"Hãy chọn một {_opt.Noun} trong bảng để xóa.", "Chưa chọn",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa {_opt.Noun} '{item.Name}'?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            await Run(s => s.DeleteAsync(item.Id));
            await ReloadAsync();
        }

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

        private async Task<TResult> RunQuery<TResult>(Func<ICatalogService, Task<TResult>> action)
        {
            using var scope = _scopeFactory.CreateScope();
            return await action(_opt.Resolve(scope.ServiceProvider));
        }

        private async Task Run(Func<ICatalogService, Task> action)
        {
            using var scope = _scopeFactory.CreateScope();
            await action(_opt.Resolve(scope.ServiceProvider));
        }
    }

    /// <summary>Quản lý danh mục sản phẩm.</summary>
    public sealed class CategoryForm : CatalogForm
    {
        public CategoryForm(IServiceScopeFactory scopeFactory) : base(scopeFactory, new CatalogFormOptions(
            "Quản lý danh mục",
            "Nhóm sản phẩm như Chuột, Bàn phím, Tai nghe...",
            "danh mục",
            "Tên danh mục *",
            sp => sp.GetRequiredService<ICategoryService>()))
        { }
    }

    /// <summary>Quản lý thương hiệu sản phẩm.</summary>
    public sealed class BrandForm : CatalogForm
    {
        public BrandForm(IServiceScopeFactory scopeFactory) : base(scopeFactory, new CatalogFormOptions(
            "Quản lý thương hiệu",
            "Hãng sản xuất như Logitech, Razer, Corsair...",
            "thương hiệu",
            "Tên thương hiệu *",
            sp => sp.GetRequiredService<IBrandService>()))
        { }
    }
}
