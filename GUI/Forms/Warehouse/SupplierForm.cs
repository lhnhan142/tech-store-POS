using BLL.Exceptions;
using BLL.Services.Interfaces;
using DAL.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace GUI.Forms.Warehouse
{
    /// <summary>Màn hình quản lý Nhà cung cấp (xem, tìm kiếm, thêm, sửa, xóa).</summary>
    [DesignerCategory("Code")]
    public class SupplierForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly TextBox _txtSearch = new();
        private readonly DataGridView _grid = new();
        private readonly TextBox _txtName = new();
        private readonly TextBox _txtPhone = new();
        private readonly TextBox _txtAddress = new();
        private readonly Button _btnAdd = new();
        private readonly Button _btnUpdate = new();
        private readonly Button _btnDelete = new();
        private readonly Button _btnClear = new();

        private List<Supplier> _all = new();
        private bool _binding;

        public SupplierForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Quản lý Nhà cung cấp";
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1000, 560);
            MinimumSize = new Size(820, 460);

            BuildUi();
            Load += async (_, _) => await RunAsync(() => ReloadAsync());
        }

        // ---------------------------------------------------------------- UI
        private void BuildUi()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));

            // --- Bên trái: ô tìm kiếm + bảng
            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12) };
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _txtSearch.Dock = DockStyle.Fill;
            _txtSearch.PlaceholderText = "Tìm theo tên, số điện thoại hoặc địa chỉ...";
            _txtSearch.Margin = new Padding(0, 0, 0, 8);
            _txtSearch.TextChanged += (_, _) => ApplyFilter();

            _grid.Dock = DockStyle.Fill;
            _grid.AutoGenerateColumns = false;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.RowHeadersVisible = false;
            _grid.BackgroundColor = SystemColors.Window;
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Mã", DataPropertyName = nameof(Supplier.Id), Width = 60
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tên nhà cung cấp", DataPropertyName = nameof(Supplier.Name),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 40
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Số điện thoại", DataPropertyName = nameof(Supplier.Phone), Width = 130
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Địa chỉ", DataPropertyName = nameof(Supplier.Address),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 30
            });
            _grid.SelectionChanged += (_, _) => OnSelectionChanged();

            left.Controls.Add(_txtSearch, 0, 0);
            left.Controls.Add(_grid, 0, 1);

            // --- Bên phải: form nhập liệu + nút
            var right = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, Padding = new Padding(12)
            };

            _txtName.MaxLength = 100;
            _txtName.KeyPress += TxtName_KeyPress;
            _txtPhone.MaxLength = 20;
            _txtPhone.KeyPress += TxtPhone_KeyPress;
            _txtAddress.MaxLength = 200;
            _txtAddress.KeyPress += TxtAddress_KeyPress;

            AddField(right, "Tên nhà cung cấp (*):", _txtName);
            AddField(right, "Số điện thoại:", _txtPhone);
            AddField(right, "Địa chỉ:", _txtAddress);

            ConfigButton(_btnAdd, "Thêm", async (_, _) => await RunAsync(AddAsync));
            ConfigButton(_btnUpdate, "Sửa", async (_, _) => await RunAsync(UpdateAsync));
            ConfigButton(_btnDelete, "Xóa", async (_, _) => await RunAsync(DeleteAsync));
            ConfigButton(_btnClear, "Làm mới", (_, _) => ClearInput());
            right.Controls.Add(_btnAdd);
            right.Controls.Add(_btnUpdate);
            right.Controls.Add(_btnDelete);
            right.Controls.Add(_btnClear);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);
            Controls.Add(root);
        }

        private static void AddField(FlowLayoutPanel panel, string label, TextBox box)
        {
            panel.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 8, 0, 4) });
            box.Width = 290;
            panel.Controls.Add(box);
        }

        private static void ConfigButton(Button btn, string text, EventHandler onClick)
        {
            btn.Text = text;
            btn.Width = 290;
            btn.Height = 36;
            btn.Margin = new Padding(0, 10, 0, 0);
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
                _grid.DataSource = _all
                    .Where(s => Contains(s.Name, keyword) || Contains(s.Phone, keyword) || Contains(s.Address, keyword))
                    .ToList();
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
