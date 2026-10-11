using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.BLL.Services.Interfaces;
using PhuKienMayTinh.Common.Enums;
using PhuKienMayTinh.GUI.Forms;

namespace PhuKienMayTinh.GUI.UserControls
{
    public class EmployeeUC : UserControl
    {
        private readonly IEmployeeService _employeeService;

        // UI Controls
        private TextBox txtSearch = null!;
        private ComboBox cboDepartmentFilter = null!;
        private ComboBox cboStatusFilter = null!;
        private Button btnSearch = null!;
        private Button btnRefresh = null!;

        private Button btnAdd = null!;
        private Button btnEdit = null!;
        private Button btnToggleStatus = null!;
        private Button btnExportExcel = null!;

        private DataGridView dgvEmployees = null!;
        private Label lblTotal = null!;

        public EmployeeUC(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 9f);

            // 1. Top Panel (Tiêu đề + Thao tác chính)
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(30, 41, 59), // Slate 800
                Padding = new Padding(20, 12, 20, 12)
            };

            var lblTitle = new Label
            {
                Text = "👥 HỒ SƠ NHÂN SỰ & QUẢN LÝ NHÂN VIÊN",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 250, 252),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 18)
            };
            pnlTop.Controls.Add(lblTitle);

            // Nút Thao tác bên phải (530px bảo đảm không bao giờ rớt dòng)
            var pnlActionButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 530,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            btnExportExcel = CreateActionButton("Xuất Excel", Color.FromArgb(16, 185, 129), "📊");
            btnExportExcel.Click += async (s, e) => await ExportExcelAsync();

            btnToggleStatus = CreateActionButton("Khóa / Nghỉ việc", Color.FromArgb(239, 68, 68), "🔒");
            btnToggleStatus.Click += async (s, e) => await ToggleStatusAsync();

            btnEdit = CreateActionButton("Chỉnh sửa", Color.FromArgb(245, 158, 11), "✏️");
            btnEdit.Click += async (s, e) => await EditEmployeeAsync();

            btnAdd = CreateActionButton("Thêm nhân viên", Color.FromArgb(37, 99, 235), "➕");
            btnAdd.Click += async (s, e) => await AddEmployeeAsync();

            pnlActionButtons.Controls.Add(btnExportExcel);
            pnlActionButtons.Controls.Add(btnToggleStatus);
            pnlActionButtons.Controls.Add(btnEdit);
            pnlActionButtons.Controls.Add(btnAdd);

            pnlTop.Controls.Add(pnlActionButtons);
            Controls.Add(pnlTop);

            // 2. Filter Bar (Slate 800)
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20, 12, 20, 12),
                Margin = new Padding(0, 1, 0, 0)
            };

            var lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, Location = new Point(20, 18), ForeColor = Color.FromArgb(148, 163, 184) };
            txtSearch = new TextBox
            {
                Location = new Point(88, 14),
                Width = 230,
                PlaceholderText = "Tên, mã NV, SĐT, CCCD...",
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(241, 245, 249),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await LoadEmployeesAsync(); };

            var lblDept = new Label { Text = "Phòng ban:", AutoSize = true, Location = new Point(335, 18), ForeColor = Color.FromArgb(148, 163, 184) };
            cboDepartmentFilter = new ComboBox { Location = new Point(410, 14), Width = 135, DropDownStyle = ComboBoxStyle.DropDownList };
            cboDepartmentFilter.Items.AddRange(new object[] { "Tất cả", "Bán hàng", "Kho", "Kỹ thuật", "Kế toán", "Quản lý" });
            cboDepartmentFilter.SelectedIndex = 0;
            cboDepartmentFilter.SelectedIndexChanged += async (s, e) => await LoadEmployeesAsync();

            var lblStat = new Label { Text = "Trạng thái:", AutoSize = true, Location = new Point(560, 18), ForeColor = Color.FromArgb(148, 163, 184) };
            cboStatusFilter = new ComboBox { Location = new Point(630, 14), Width = 135, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatusFilter.Items.AddRange(new object[] { "Tất cả", "Đang làm việc", "Tạm nghỉ", "Đã nghỉ việc" });
            cboStatusFilter.SelectedIndex = 0;
            cboStatusFilter.SelectedIndexChanged += async (s, e) => await LoadEmployeesAsync();

            btnSearch = new Button
            {
                Text = "🔍 Tìm",
                Location = new Point(780, 12),
                Width = 80,
                Height = 32,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += async (s, e) => await LoadEmployeesAsync();

            btnRefresh = new Button
            {
                Text = "🔄 Làm mới",
                Location = new Point(870, 12),
                Width = 95,
                Height = 32,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (s, e) =>
            {
                txtSearch.Clear();
                cboDepartmentFilter.SelectedIndex = 0;
                cboStatusFilter.SelectedIndex = 0;
                await LoadEmployeesAsync();
            };

            pnlFilter.Controls.Add(lblSearch);
            pnlFilter.Controls.Add(txtSearch);
            pnlFilter.Controls.Add(lblDept);
            pnlFilter.Controls.Add(cboDepartmentFilter);
            pnlFilter.Controls.Add(lblStat);
            pnlFilter.Controls.Add(cboStatusFilter);
            pnlFilter.Controls.Add(btnSearch);
            pnlFilter.Controls.Add(btnRefresh);

            Controls.Add(pnlFilter);

            // 3. Grid Data Container (Slate 900)
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.FromArgb(15, 23, 42)
            };

            dgvEmployees = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(51, 65, 85),
                RowTemplate = { Height = 38 }
            };

            dgvEmployees.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvEmployees.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
            dgvEmployees.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvEmployees.ColumnHeadersHeight = 40;
            dgvEmployees.DefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvEmployees.DefaultCellStyle.ForeColor = Color.FromArgb(241, 245, 249);
            dgvEmployees.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 58, 138); // Blue 900
            dgvEmployees.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Visible = false });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã NV", FillWeight = 60 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Họ và Tên", FillWeight = 110 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Gender", HeaderText = "Giới tính", FillWeight = 45 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Phone", HeaderText = "SĐT", FillWeight = 75 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", FillWeight = 110 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Dept", HeaderText = "Phòng ban", FillWeight = 70 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Pos", HeaderText = "Chức vụ", FillWeight = 90 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "BaseSal", HeaderText = "Lương CB", FillWeight = 75, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Allowance", HeaderText = "Phụ cấp", FillWeight = 65, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái", FillWeight = 75, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });

            dgvEmployees.CellFormatting += DgvEmployees_CellFormatting;
            dgvEmployees.CellDoubleClick += async (s, e) => await EditEmployeeAsync();

            pnlGrid.Controls.Add(dgvEmployees);

            // Bottom status label
            lblTotal = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                Text = "Tổng số: 0 nhân viên",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlGrid.Controls.Add(lblTotal);

            Controls.Add(pnlGrid);
            pnlGrid.BringToFront();
        }

        private void DgvEmployees_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Highlight cột Trạng thái
            if (dgvEmployees.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString() ?? "";
                if (status == "Đang làm việc")
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(52, 211, 153); // Emerald
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else if (status == "Tạm nghỉ")
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(251, 191, 36); // Amber
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else if (status == "Đã nghỉ việc")
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(248, 113, 113); // Rose
                }
            }

            // Alternating Row Styling
            if (e.RowIndex % 2 == 1 && dgvEmployees.Columns[e.ColumnIndex].Name != "Status")
            {
                e.CellStyle!.BackColor = Color.FromArgb(22, 32, 50);
            }
        }

        public async Task LoadEmployeesAsync()
        {
            try
            {
                string? keyword = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
                string? dept = cboDepartmentFilter.SelectedIndex > 0 ? cboDepartmentFilter.SelectedItem?.ToString() : null;
                EmployeeStatus? status = cboStatusFilter.SelectedIndex switch
                {
                    1 => EmployeeStatus.Active,
                    2 => EmployeeStatus.OnLeave,
                    3 => EmployeeStatus.Resigned,
                    _ => null
                };

                var list = (await _employeeService.SearchEmployeesAsync(keyword, dept, status)).ToList();

                dgvEmployees.Rows.Clear();
                foreach (var emp in list)
                {
                    decimal totalAllowance = emp.LunchAllowance + emp.TravelAllowance + emp.ResponsibilityAllowance;
                    dgvEmployees.Rows.Add(
                        emp.Id,
                        emp.EmployeeCode,
                        emp.FullName,
                        emp.Gender,
                        emp.PhoneNumber,
                        emp.Email,
                        emp.Department,
                        emp.Position,
                        $"{emp.BaseSalary:N0} đ",
                        $"{totalAllowance:N0} đ",
                        emp.StatusDisplay
                    );
                }

                lblTotal.Text = $"Tổng cộng: {list.Count} nhân viên trong hệ thống";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách nhân viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddEmployeeAsync()
        {
            try
            {
                string nextCode = await _employeeService.GetNextEmployeeCodeAsync();
                using (var form = new EmployeeEditForm(null, nextCode))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        await _employeeService.AddEmployeeAsync(form.EmployeeData);
                        MessageBox.Show("Thêm mới hồ sơ nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadEmployeesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm nhân viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task EditEmployeeAsync()
        {
            if (dgvEmployees.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["Id"].Value);
            var emp = await _employeeService.GetEmployeeByIdAsync(id);
            if (emp == null) return;

            var editDto = new AddOrUpdateEmployeeDTO
            {
                Id = emp.Id,
                EmployeeCode = emp.EmployeeCode,
                FullName = emp.FullName,
                Gender = emp.Gender,
                DateOfBirth = emp.DateOfBirth,
                IdentityCard = emp.IdentityCard,
                PhoneNumber = emp.PhoneNumber,
                Email = emp.Email,
                Address = emp.Address,
                Department = emp.Department,
                Position = emp.Position,
                HireDate = emp.HireDate,
                BaseSalary = emp.BaseSalary,
                HourlyRate = emp.HourlyRate,
                LunchAllowance = emp.LunchAllowance,
                TravelAllowance = emp.TravelAllowance,
                ResponsibilityAllowance = emp.ResponsibilityAllowance,
                Status = emp.Status,
                Notes = emp.Notes
            };

            using (var form = new EmployeeEditForm(editDto))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    await _employeeService.UpdateEmployeeAsync(form.EmployeeData);
                    MessageBox.Show("Cập nhật thông tin nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadEmployeesAsync();
                }
            }
        }

        private async Task ToggleStatusAsync()
        {
            if (dgvEmployees.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần thao tác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["Id"].Value);
            string name = dgvEmployees.SelectedRows[0].Cells["Name"].Value?.ToString() ?? "";
            string currentStatus = dgvEmployees.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "";

            EmployeeStatus newStatus = (currentStatus == "Đang làm việc") ? EmployeeStatus.Resigned : EmployeeStatus.Active;
            string actionText = (newStatus == EmployeeStatus.Resigned) ? "chuyển sang trạng thái ĐÃ NGHỈ VIỆC" : "kích hoạt ĐANG LÀM VIỆC trở lại";

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn {actionText} cho nhân viên '{name}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                await _employeeService.ToggleEmployeeStatusAsync(id, newStatus);
                await LoadEmployeesAsync();
            }
        }

        private async Task ExportExcelAsync()
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"DanhSachNhanVien_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string? keyword = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
                    string? dept = cboDepartmentFilter.SelectedIndex > 0 ? cboDepartmentFilter.SelectedItem?.ToString() : null;
                    EmployeeStatus? status = cboStatusFilter.SelectedIndex switch
                    {
                        1 => EmployeeStatus.Active,
                        2 => EmployeeStatus.OnLeave,
                        3 => EmployeeStatus.Resigned,
                        _ => null
                    };

                    await _employeeService.ExportEmployeesToExcelAsync(sfd.FileName, keyword, dept, status);
                    MessageBox.Show("Xuất danh sách nhân viên ra Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private static Button CreateActionButton(string text, Color color, string icon)
        {
            var btn = new Button
            {
                Text = $"{icon} {text}",
                Height = 36,
                AutoSize = true,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Margin = new Padding(4, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
