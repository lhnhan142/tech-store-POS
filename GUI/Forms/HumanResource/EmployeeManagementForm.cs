using BLL.DTOs.Employee;
using BLL.Services.Interfaces;
using Common.Enums;
using GUI.Themes;
using GUI.Utils;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI.Forms.HumanResource
{
    [DesignerCategory("Code")]
    public class EmployeeManagementForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Controls lọc & tìm kiếm
        private readonly TextBox _txtSearch = new();
        private readonly ComboBox _cboPosition = new();
        private readonly ComboBox _cboStatus = new();
        private readonly HopeRoundButton _btnSearch = new();
        private readonly HopeRoundButton _btnReload = new();
        private readonly HopeRoundButton _btnAdd = new();
        private readonly HopeRoundButton _btnEdit = new();
        private readonly HopeRoundButton _btnToggleStatus = new();
        private readonly HopeRoundButton _btnExportExcel = new();
        private readonly HopeRoundButton _btnExportPdf = new();

        private readonly DataGridView _grid = new();
        private readonly Label _lblSummary = new();

        private List<EmployeeListItemDto> _currentEmployees = new();

        public EmployeeManagementForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Quản lý nhân viên - Tech Store POS";
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(S(1150), S(680));
            MinimumSize = new Size(S(950), S(550));
            BackColor = Color.FromArgb(243, 244, 246);

            BuildUi();
            Load += async (_, _) => await LoadDataAsync();
        }

        private int S(int px) => (int)Math.Round(px * DeviceDpi / 96.0);

        private void BuildUi()
        {
            // 1. Header Panel
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(65),
                BackColor = Color.White,
                Padding = new Padding(S(20), 0, S(20), 0)
            };

            var lblTitle = new Label
            {
                Text = "QUẢN LÝ HỒ SƠ NHÂN VIÊN",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 15, 60),
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true,
                UseMnemonic = false
            };

            var pnlHeaderRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, S(14), 0, 0)
            };

            _btnExportPdf.Text = "Xuất PDF";
            _btnExportPdf.Size = new Size(S(95), S(34));
            _btnExportPdf.Margin = new Padding(S(6), 0, 0, 0);
            _btnExportPdf.PrimaryColor = Color.FromArgb(220, 38, 38);
            _btnExportPdf.TextColor = Color.White;
            _btnExportPdf.Click += (_, _) => ExportPdf();

            _btnExportExcel.Text = "Xuất Excel";
            _btnExportExcel.Size = new Size(S(100), S(34));
            _btnExportExcel.Margin = new Padding(S(6), 0, 0, 0);
            _btnExportExcel.PrimaryColor = Color.FromArgb(16, 124, 65);
            _btnExportExcel.TextColor = Color.White;
            _btnExportExcel.Click += (_, _) => ExportExcel();

            _btnAdd.Text = "+ Thêm nhân viên";
            _btnAdd.Size = new Size(S(135), S(34));
            _btnAdd.Margin = new Padding(S(6), 0, 0, 0);
            _btnAdd.PrimaryColor = Color.FromArgb(0, 112, 243);
            _btnAdd.TextColor = Color.White;
            _btnAdd.Click += async (_, _) => await AddEmployeeAsync();

            pnlHeaderRight.Controls.Add(_btnExportPdf);
            pnlHeaderRight.Controls.Add(_btnExportExcel);
            pnlHeaderRight.Controls.Add(_btnAdd);

            pnlHeader.Controls.AddRange(new Control[] { pnlHeaderRight, lblTitle });

            // 2. Filter Bar
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(55),
                BackColor = Color.White,
                Padding = new Padding(S(20), S(8), S(20), S(8))
            };

            _txtSearch.PlaceholderText = "Tìm theo mã NV, họ tên, SĐT...";
            _txtSearch.Size = new Size(S(230), S(30));
            _txtSearch.Location = new Point(S(20), S(12));
            _txtSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await LoadDataAsync(); };

            _cboPosition.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboPosition.Size = new Size(S(160), S(30));
            _cboPosition.Location = new Point(S(260), S(12));
            _cboPosition.Items.Add("Tất cả chức vụ");
            _cboPosition.SelectedIndex = 0;
            _cboPosition.SelectedIndexChanged += async (_, _) => await LoadDataAsync();

            _cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboStatus.Size = new Size(S(150), S(30));
            _cboStatus.Location = new Point(S(430), S(12));
            _cboStatus.Items.AddRange(new object[] { "Tất cả trạng thái", "Đang làm việc", "Tạm nghỉ", "Đã thôi việc" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += async (_, _) => await LoadDataAsync();

            _btnSearch.Text = "Tìm";
            _btnSearch.Size = new Size(S(70), S(32));
            _btnSearch.Location = new Point(S(590), S(11));
            _btnSearch.PrimaryColor = Color.FromArgb(75, 85, 99);
            _btnSearch.TextColor = Color.White;
            _btnSearch.Click += async (_, _) => await LoadDataAsync();

            _btnReload.Text = "Làm mới";
            _btnReload.Size = new Size(S(85), S(32));
            _btnReload.Location = new Point(S(670), S(11));
            _btnReload.PrimaryColor = Color.FromArgb(229, 231, 235);
            _btnReload.TextColor = Color.FromArgb(55, 65, 81);
            _btnReload.Click += async (_, _) =>
            {
                _txtSearch.Clear();
                _cboPosition.SelectedIndex = 0;
                _cboStatus.SelectedIndex = 0;
                await LoadDataAsync();
            };

            pnlFilter.Controls.Add(_txtSearch);
            pnlFilter.Controls.Add(_cboPosition);
            pnlFilter.Controls.Add(_cboStatus);
            pnlFilter.Controls.Add(_btnSearch);
            pnlFilter.Controls.Add(_btnReload);

            // 3. Bottom Action Bar
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = S(50),
                BackColor = Color.White,
                Padding = new Padding(S(20), 0, S(20), 0)
            };

            _lblSummary.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            _lblSummary.ForeColor = Color.FromArgb(100, 110, 130);
            _lblSummary.Dock = DockStyle.Left;
            _lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            _lblSummary.AutoSize = true;

            var pnlBottomRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, S(8), 0, 0)
            };

            _btnToggleStatus.Text = "Đổi trạng thái";
            _btnToggleStatus.Size = new Size(S(115), S(32));
            _btnToggleStatus.Margin = new Padding(S(6), 0, 0, 0);
            _btnToggleStatus.PrimaryColor = Color.FromArgb(245, 158, 11);
            _btnToggleStatus.TextColor = Color.White;
            _btnToggleStatus.Click += async (_, _) => await ToggleStatusAsync();

            _btnEdit.Text = "Sửa hồ sơ";
            _btnEdit.Size = new Size(S(100), S(32));
            _btnEdit.Margin = new Padding(S(6), 0, 0, 0);
            _btnEdit.PrimaryColor = Color.FromArgb(0, 112, 243);
            _btnEdit.TextColor = Color.White;
            _btnEdit.Click += async (_, _) => await EditEmployeeAsync();

            pnlBottomRight.Controls.Add(_btnToggleStatus);
            pnlBottomRight.Controls.Add(_btnEdit);

            _lblSummary.UseMnemonic = false;
            pnlBottom.Controls.AddRange(new Control[] { pnlBottomRight, _lblSummary });

            // 4. DataGridView
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(S(20), S(15), S(20), S(15)),
                BackColor = Color.FromArgb(243, 244, 246)
            };

            UiKit.StyleGrid(_grid, this, 40);
            _grid.AllowUserToResizeColumns = true;
            _grid.DoubleClick += async (_, _) => await EditEmployeeAsync();

            ConfigureGridColumns();
            pnlGrid.Controls.Add(_grid);

            Controls.Add(pnlGrid);
            Controls.Add(pnlBottom);
            Controls.Add(pnlFilter);
            Controls.Add(pnlHeader);
        }

        private void ConfigureGridColumns()
        {
            _grid.Columns.Clear();

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColId",
                HeaderText = "ID",
                DataPropertyName = nameof(EmployeeListItemDto.Id),
                Visible = false
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColCode",
                HeaderText = "MÃ NV",
                DataPropertyName = nameof(EmployeeListItemDto.EmployeeCode),
                Width = S(85)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColName",
                HeaderText = "HỌ VÀ TÊN",
                DataPropertyName = nameof(EmployeeListItemDto.FullName),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = S(160)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColPos",
                HeaderText = "CHỨC VỤ",
                DataPropertyName = nameof(EmployeeListItemDto.Position),
                Width = S(140)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColPhone",
                HeaderText = "SỐ ĐIỆN THOẠI",
                DataPropertyName = nameof(EmployeeListItemDto.PhoneNumber),
                Width = S(120)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColEmail",
                HeaderText = "EMAIL",
                DataPropertyName = nameof(EmployeeListItemDto.Email),
                Width = S(160)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColSalary",
                HeaderText = "LƯƠNG CƠ BẢN",
                DataPropertyName = nameof(EmployeeListItemDto.BaseSalary),
                Width = S(120),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N0",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColAllowance",
                HeaderText = "PHỤ CẤP",
                DataPropertyName = nameof(EmployeeListItemDto.Allowance),
                Width = S(100),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N0",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColStatus",
                HeaderText = "TRẠNG THÁI",
                DataPropertyName = nameof(EmployeeListItemDto.StatusText),
                Width = S(120)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColHireDate",
                HeaderText = "NGÀY VÀO LÀM",
                DataPropertyName = nameof(EmployeeListItemDto.HireDate),
                Width = S(110),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "dd/MM/yyyy",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            // Vẽ viền pill cho ô trạng thái
            _grid.CellPainting += Grid_CellPainting;
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == _grid.Columns["ColStatus"].Index)
            {
                e.PaintBackground(e.ClipBounds, true);

                string statusText = e.Value?.ToString() ?? string.Empty;
                var (bg, fg) = statusText switch
                {
                    "Đang làm việc" => (Color.FromArgb(220, 252, 231), Color.FromArgb(22, 101, 52)), // Xanh lá
                    "Tạm nghỉ" => (Color.FromArgb(254, 243, 199), Color.FromArgb(146, 64, 14)),     // Vàng cam
                    "Đã thôi việc" => (Color.FromArgb(243, 244, 246), Color.FromArgb(107, 114, 128)), // Xám
                    _ => (Color.FromArgb(243, 244, 246), Color.Black)
                };

                using var brush = new SolidBrush(bg);
                var bounds = new Rectangle(e.CellBounds.X + S(6), e.CellBounds.Y + S(8), e.CellBounds.Width - S(12), e.CellBounds.Height - S(16));
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                int r = bounds.Height;
                path.AddArc(bounds.X, bounds.Y, r, r, 90, 180);
                path.AddArc(bounds.Right - r, bounds.Y, r, r, 270, 180);
                path.CloseFigure();
                e.Graphics.FillPath(brush, path);

                using var textBrush = new SolidBrush(fg);
                using var font = new Font("Segoe UI Semibold", 8.5F);
                var size = e.Graphics.MeasureString(statusText, font);
                e.Graphics.DrawString(statusText, font, textBrush,
                    bounds.X + (bounds.Width - size.Width) / 2,
                    bounds.Y + (bounds.Height - size.Height) / 2);

                e.Handled = true;
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IEmployeeService>();

                // Nạp danh mục chức vụ nếu chưa có
                if (_cboPosition.Items.Count <= 1)
                {
                    var positions = await svc.GetPositionsAsync();
                    foreach (var p in positions) _cboPosition.Items.Add(p);
                }

                string? keyword = string.IsNullOrWhiteSpace(_txtSearch.Text) ? null : _txtSearch.Text.Trim();
                string? position = _cboPosition.SelectedIndex > 0 ? _cboPosition.Text : null;
                EmployeeStatus? status = _cboStatus.SelectedIndex > 0 ? (EmployeeStatus)(_cboStatus.SelectedIndex - 1) : null;

                var list = await svc.GetAllAsync(keyword, position, status);
                _currentEmployees = list.ToList();

                _grid.DataSource = _currentEmployees;

                int total = _currentEmployees.Count;
                int active = _currentEmployees.Count(e => e.Status == EmployeeStatus.Active);
                _lblSummary.Text = $"Hiển thị: {total} nhân viên  |  Đang làm việc: {active}  |  Tạm nghỉ/Thôi việc: {total - active}";
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
        }

        private async Task AddEmployeeAsync()
        {
            using var dlg = new EmployeeDetailForm(_scopeFactory);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                await LoadDataAsync();
            }
        }

        private async Task EditEmployeeAsync()
        {
            if (_grid.CurrentRow?.DataBoundItem is not EmployeeListItemDto item)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new EmployeeDetailForm(_scopeFactory, item.Id);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                await LoadDataAsync();
            }
        }

        private async Task ToggleStatusAsync()
        {
            if (_grid.CurrentRow?.DataBoundItem is not EmployeeListItemDto item)
            {
                MessageBox.Show("Vui lòng chọn nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var nextStatus = item.Status == EmployeeStatus.Active ? EmployeeStatus.Resigned : EmployeeStatus.Active;
            string actionName = nextStatus == EmployeeStatus.Resigned ? "chuyển sang trạng thái [ĐÃ THÔI VIỆC]" : "kích hoạt lại trạng thái [ĐANG LÀM VIỆC]";

            var ask = MessageBox.Show($"Bạn có chắc chắn muốn {actionName} cho nhân viên: {item.FullName} ({item.EmployeeCode})?",
                "Xác nhận đổi trạng thái", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (ask != DialogResult.Yes) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
                await svc.UpdateStatusAsync(item.Id, nextStatus);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
        }

        private void ExportExcel()
        {
            if (_currentEmployees.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu nhân viên để xuất file.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"DanhSachNhanVien_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var exp = scope.ServiceProvider.GetRequiredService<IReportExportService>();
                    exp.ExportEmployeeListToExcel(sfd.FileName, _currentEmployees);
                    MessageBox.Show("Xuất báo cáo Excel thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    UiHelper.HandleException(ex);
                }
            }
        }

        private void ExportPdf()
        {
            if (_currentEmployees.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu nhân viên để xuất file.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"DanhSachNhanVien_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var exp = scope.ServiceProvider.GetRequiredService<IReportExportService>();
                    exp.ExportEmployeeListToPdf(sfd.FileName, _currentEmployees);
                    MessageBox.Show("Xuất báo cáo PDF thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    UiHelper.HandleException(ex);
                }
            }
        }
    }
}
