using BLL.DTOs.Employee;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using Common.Enums;
using GUI.Utils;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GUI.Forms.HumanResource
{
    [DesignerCategory("Code")]
    public class EmployeeDetailForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int? _employeeId;

        // UI Controls
        private readonly TextBox _txtCode = new();
        private readonly TextBox _txtName = new();
        private readonly DateTimePicker _dtpDob = new();
        private readonly ComboBox _cboGender = new();
        private readonly TextBox _txtPhone = new();
        private readonly TextBox _txtEmail = new();
        private readonly TextBox _txtAddress = new();
        private readonly ComboBox _cboPosition = new();
        private readonly DateTimePicker _dtpHireDate = new();
        private readonly NumericUpDown _nudBaseSalary = new();
        private readonly NumericUpDown _nudAllowance = new();
        private readonly ComboBox _cboStatus = new();

        private readonly HopeRoundButton _btnSave = new();
        private readonly HopeRoundButton _btnCancel = new();
        private readonly Label _lblTitle = new();

        public EmployeeDetailForm(IServiceScopeFactory scopeFactory, int? employeeId = null)
        {
            _scopeFactory = scopeFactory;
            _employeeId = employeeId;

            Text = _employeeId.HasValue ? "Cập nhật hồ sơ nhân viên" : "Thêm nhân viên mới";
            Font = new Font("Segoe UI", 9.5F);
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(S(650), S(620));
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            BuildUi();
            Load += async (_, _) => await LoadDataAsync();
        }

        private int S(int px) => (int)Math.Round(px * DeviceDpi / 96.0);

        private void BuildUi()
        {
            // Panel Header
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(60),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            _lblTitle.Text = _employeeId.HasValue ? "CẬP NHẬT THÔNG TIN NHÂN VIÊN" : "THÊM MỚI NHÂN VIÊN";
            _lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _lblTitle.ForeColor = Color.FromArgb(15, 15, 60);
            _lblTitle.Dock = DockStyle.Fill;
            _lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            _lblTitle.Padding = new Padding(S(25), 0, 0, 0);
            _lblTitle.UseMnemonic = false;
            pnlHeader.Controls.Add(_lblTitle);

            // Panel Footer với các nút bấm
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = S(65),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlFooter.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(229, 231, 235), 1);
                e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };

            var pnlFooterRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, S(14), S(20), 0)
            };

            _btnCancel.Text = "Hủy bỏ";
            _btnCancel.Size = new Size(S(100), S(38));
            _btnCancel.Margin = new Padding(S(8), 0, 0, 0);
            _btnCancel.PrimaryColor = Color.FromArgb(229, 231, 235);
            _btnCancel.TextColor = Color.FromArgb(55, 65, 81);
            _btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

            _btnSave.Text = "Lưu hồ sơ";
            _btnSave.Size = new Size(S(120), S(38));
            _btnSave.Margin = new Padding(S(8), 0, 0, 0);
            _btnSave.PrimaryColor = Color.FromArgb(0, 112, 243);
            _btnSave.TextColor = Color.White;
            _btnSave.Click += async (_, _) => await SaveAsync();

            pnlFooterRight.Controls.Add(_btnCancel);
            pnlFooterRight.Controls.Add(_btnSave);

            pnlFooter.Controls.Add(pnlFooterRight);

            // Body Layout
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(S(25), S(15), S(25), S(15)),
                AutoScroll = true
            };

            int y = S(15);
            int colW = S(280);
            int gapX = S(35);
            int rowH = S(62);

            // Cột 1
            AddFormField(pnlBody, "Mã nhân viên (*)", _txtCode, S(25), y, colW);
            AddFormField(pnlBody, "Họ và tên (*)", _txtName, S(25) + colW + gapX, y, colW);

            y += rowH;
            _dtpDob.Format = DateTimePickerFormat.Short;
            _dtpDob.Value = new DateTime(1995, 1, 1);
            AddFormField(pnlBody, "Ngày sinh", _dtpDob, S(25), y, colW);

            _cboGender.Items.AddRange(new object[] { "Nam", "Nữ" });
            _cboGender.SelectedIndex = 0;
            AddFormField(pnlBody, "Giới tính", _cboGender, S(25) + colW + gapX, y, colW);

            y += rowH;
            AddFormField(pnlBody, "Số điện thoại", _txtPhone, S(25), y, colW);
            AddFormField(pnlBody, "Email", _txtEmail, S(25) + colW + gapX, y, colW);

            y += rowH;
            _cboPosition.Items.AddRange(new object[] {
                "Quản lý cửa hàng", "Nhân viên bán hàng", "Thủ kho", "Nhân viên thu ngân", "Kỹ thuật viên"
            });
            _cboPosition.SelectedIndex = 1;
            AddFormField(pnlBody, "Chức vụ (*)", _cboPosition, S(25), y, colW);

            _dtpHireDate.Format = DateTimePickerFormat.Short;
            _dtpHireDate.Value = DateTime.Today;
            AddFormField(pnlBody, "Ngày vào làm", _dtpHireDate, S(25) + colW + gapX, y, colW);

            y += rowH;
            _nudBaseSalary.Maximum = 1_000_000_000;
            _nudBaseSalary.Increment = 500_000;
            _nudBaseSalary.ThousandsSeparator = true;
            _nudBaseSalary.Value = 8_000_000;
            AddFormField(pnlBody, "Lương cơ bản (VNĐ)", _nudBaseSalary, S(25), y, colW);

            _nudAllowance.Maximum = 100_000_000;
            _nudAllowance.Increment = 100_000;
            _nudAllowance.ThousandsSeparator = true;
            _nudAllowance.Value = 1_000_000;
            AddFormField(pnlBody, "Phụ cấp cố định (VNĐ)", _nudAllowance, S(25) + colW + gapX, y, colW);

            y += rowH;
            _cboStatus.Items.AddRange(new object[] { "Đang làm việc", "Tạm nghỉ", "Đã thôi việc" });
            _cboStatus.SelectedIndex = 0;
            AddFormField(pnlBody, "Trạng thái làm việc", _cboStatus, S(25), y, colW);

            AddFormField(pnlBody, "Địa chỉ liên lạc", _txtAddress, S(25) + colW + gapX, y, colW);

            Controls.Add(pnlBody);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
        }

        private void AddFormField(Panel parent, string label, Control ctrl, int x, int y, int w)
        {
            var lbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(x, y),
                AutoSize = true
            };
            ctrl.Location = new Point(x, y + S(22));
            ctrl.Width = w;
            ctrl.Height = S(28);

            parent.Controls.Add(lbl);
            parent.Controls.Add(ctrl);
        }

        private async Task LoadDataAsync()
        {
            if (!_employeeId.HasValue)
            {
                // Tự động gợi ý mã nhân viên
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
                var all = await svc.GetAllAsync();
                int nextNum = all.Count + 1;
                _txtCode.Text = $"NV{nextNum:D3}";
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
                var emp = await svc.GetByIdAsync(_employeeId.Value);
                if (emp == null)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu nhân viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                _txtCode.Text = emp.EmployeeCode;
                _txtName.Text = emp.FullName;
                if (emp.DateOfBirth.HasValue) _dtpDob.Value = emp.DateOfBirth.Value;
                _cboGender.Text = emp.Gender ?? "Nam";
                _txtPhone.Text = emp.PhoneNumber ?? string.Empty;
                _txtEmail.Text = emp.Email ?? string.Empty;
                _txtAddress.Text = emp.Address ?? string.Empty;
                _cboPosition.Text = emp.Position;
                _dtpHireDate.Value = emp.HireDate;
                _nudBaseSalary.Value = emp.BaseSalary;
                _nudAllowance.Value = emp.Allowance;
                _cboStatus.SelectedIndex = (int)emp.Status;
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(_txtCode.Text))
            {
                MessageBox.Show("Vui lòng nhập mã nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCode.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IEmployeeService>();

                var status = (EmployeeStatus)_cboStatus.SelectedIndex;

                if (_employeeId.HasValue)
                {
                    var dto = new UpdateEmployeeDto
                    {
                        Id = _employeeId.Value,
                        EmployeeCode = _txtCode.Text.Trim(),
                        FullName = _txtName.Text.Trim(),
                        DateOfBirth = _dtpDob.Value,
                        Gender = _cboGender.Text,
                        PhoneNumber = _txtPhone.Text.Trim(),
                        Email = _txtEmail.Text.Trim(),
                        Address = _txtAddress.Text.Trim(),
                        Position = _cboPosition.Text.Trim(),
                        HireDate = _dtpHireDate.Value,
                        BaseSalary = _nudBaseSalary.Value,
                        Allowance = _nudAllowance.Value,
                        Status = status
                    };
                    await svc.UpdateAsync(dto);
                    MessageBox.Show("Cập nhật thông tin nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var dto = new CreateEmployeeDto
                    {
                        EmployeeCode = _txtCode.Text.Trim(),
                        FullName = _txtName.Text.Trim(),
                        DateOfBirth = _dtpDob.Value,
                        Gender = _cboGender.Text,
                        PhoneNumber = _txtPhone.Text.Trim(),
                        Email = _txtEmail.Text.Trim(),
                        Address = _txtAddress.Text.Trim(),
                        Position = _cboPosition.Text.Trim(),
                        HireDate = _dtpHireDate.Value,
                        BaseSalary = _nudBaseSalary.Value,
                        Allowance = _nudAllowance.Value,
                        Status = status
                    };
                    await svc.CreateAsync(dto);
                    MessageBox.Show("Thêm mới nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
        }
    }
}
