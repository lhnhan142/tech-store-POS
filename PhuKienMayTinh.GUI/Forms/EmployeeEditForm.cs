using System;
using System.Drawing;
using System.Windows.Forms;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.Common.Enums;
using PhuKienMayTinh.Common.Utilities;

namespace PhuKienMayTinh.GUI.Forms
{
    public class EmployeeEditForm : Form
    {
        public AddOrUpdateEmployeeDTO EmployeeData { get; private set; }
        private readonly bool _isEditMode;

        // UI Controls
        private TextBox txtCode = null!;
        private TextBox txtFullName = null!;
        private ComboBox cboGender = null!;
        private DateTimePicker dtpDOB = null!;
        private TextBox txtCCCD = null!;
        private TextBox txtPhone = null!;
        private TextBox txtEmail = null!;
        private TextBox txtAddress = null!;
        private ComboBox cboDepartment = null!;
        private TextBox txtPosition = null!;
        private DateTimePicker dtpHireDate = null!;

        private NumericUpDown numBaseSalary = null!;
        private NumericUpDown numHourlyRate = null!;
        private NumericUpDown numLunchAllowance = null!;
        private NumericUpDown numTravelAllowance = null!;
        private NumericUpDown numRespAllowance = null!;

        private ComboBox cboStatus = null!;
        private TextBox txtNotes = null!;

        private Button btnSave = null!;
        private Button btnCancel = null!;
        private ErrorProvider errorProvider = null!;

        public EmployeeEditForm(AddOrUpdateEmployeeDTO? initialData = null, string? autoCode = null)
        {
            _isEditMode = initialData != null;
            EmployeeData = initialData ?? new AddOrUpdateEmployeeDTO
            {
                EmployeeCode = autoCode ?? "NV001"
            };

            InitializeComponent();
            PopulateData();
        }

        private void InitializeComponent()
        {
            Text = _isEditMode ? "CẬP NHẬT THÔNG TIN NHÂN VIÊN" : "THÊM MỚI NHÂN VIÊN";
            Size = new Size(740, 720);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // Header Banner
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20, 15, 20, 15)
            };
            var lblHeader = new Label
            {
                Text = _isEditMode ? "CHỈNH SỬA HỒ SƠ NHÂN SỰ" : "TIẾP NHẬN HỒ SƠ NHÂN VIÊN MỚI",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 18)
            };
            pnlHeader.Controls.Add(lblHeader);
            Controls.Add(pnlHeader);

            // TabControl hoặc GroupBoxes
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15),
                AutoScroll = true
            };
            Controls.Add(pnlBody);
            pnlBody.BringToFront();

            int leftCol = 25;
            int rightCol = 360;
            int currentY = 15;

            // 1. Nhóm thông tin cá nhân
            var lblSec1 = CreateSectionLabel("1. THÔNG TIN CÁ NHÂN & LIÊN LẠC", leftCol, currentY);
            pnlBody.Controls.Add(lblSec1);
            currentY += 28;

            // Mã NV & Họ tên
            pnlBody.Controls.Add(CreateFieldLabel("Mã nhân viên (*):", leftCol, currentY));
            txtCode = new TextBox { Location = new Point(leftCol, currentY + 20), Width = 140, ReadOnly = _isEditMode };
            pnlBody.Controls.Add(txtCode);

            pnlBody.Controls.Add(CreateFieldLabel("Giới tính:", leftCol + 160, currentY));
            cboGender = new ComboBox { Location = new Point(leftCol + 160, currentY + 20), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            pnlBody.Controls.Add(cboGender);

            pnlBody.Controls.Add(CreateFieldLabel("Họ và tên (*):", rightCol, currentY));
            txtFullName = new TextBox { Location = new Point(rightCol, currentY + 20), Width = 310 };
            pnlBody.Controls.Add(txtFullName);
            currentY += 55;

            // Ngày sinh & CCCD
            pnlBody.Controls.Add(CreateFieldLabel("Ngày sinh:", leftCol, currentY));
            dtpDOB = new DateTimePicker { Location = new Point(leftCol, currentY + 20), Width = 140, Format = DateTimePickerFormat.Short };
            pnlBody.Controls.Add(dtpDOB);

            pnlBody.Controls.Add(CreateFieldLabel("CCCD / CMND (*):", leftCol + 160, currentY));
            txtCCCD = new TextBox { Location = new Point(leftCol + 160, currentY + 20), Width = 130 };
            pnlBody.Controls.Add(txtCCCD);

            pnlBody.Controls.Add(CreateFieldLabel("Số điện thoại (*):", rightCol, currentY));
            txtPhone = new TextBox { Location = new Point(rightCol, currentY + 20), Width = 145 };
            pnlBody.Controls.Add(txtPhone);

            pnlBody.Controls.Add(CreateFieldLabel("Email:", rightCol + 160, currentY));
            txtEmail = new TextBox { Location = new Point(rightCol + 160, currentY + 20), Width = 150 };
            pnlBody.Controls.Add(txtEmail);
            currentY += 55;

            // Địa chỉ thường trú
            pnlBody.Controls.Add(CreateFieldLabel("Địa chỉ thường trú:", leftCol, currentY));
            txtAddress = new TextBox { Location = new Point(leftCol, currentY + 20), Width = 645 };
            pnlBody.Controls.Add(txtAddress);
            currentY += 60;

            // 2. Nhóm công việc & Chức vụ
            var lblSec2 = CreateSectionLabel("2. VỊ TRÍ CÔNG TÁC & PHÂN CÔNG", leftCol, currentY);
            pnlBody.Controls.Add(lblSec2);
            currentY += 28;

            pnlBody.Controls.Add(CreateFieldLabel("Phòng ban:", leftCol, currentY));
            cboDepartment = new ComboBox { Location = new Point(leftCol, currentY + 20), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboDepartment.Items.AddRange(new object[] { "Bán hàng", "Kho", "Kỹ thuật", "Kế toán", "Quản lý" });
            pnlBody.Controls.Add(cboDepartment);

            pnlBody.Controls.Add(CreateFieldLabel("Chức vụ:", leftCol + 200, currentY));
            txtPosition = new TextBox { Location = new Point(leftCol + 200, currentY + 20), Width = 220 };
            pnlBody.Controls.Add(txtPosition);

            pnlBody.Controls.Add(CreateFieldLabel("Ngày vào làm:", leftCol + 440, currentY));
            dtpHireDate = new DateTimePicker { Location = new Point(leftCol + 440, currentY + 20), Width = 130, Format = DateTimePickerFormat.Short };
            pnlBody.Controls.Add(dtpHireDate);
            currentY += 55;

            // Trạng thái nhân viên
            pnlBody.Controls.Add(CreateFieldLabel("Trạng thái lao động:", leftCol, currentY));
            cboStatus = new ComboBox { Location = new Point(leftCol, currentY + 20), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Đang làm việc", "Tạm nghỉ", "Đã nghỉ việc" });
            pnlBody.Controls.Add(cboStatus);

            pnlBody.Controls.Add(CreateFieldLabel("Ghi chú hồ sơ:", leftCol + 200, currentY));
            txtNotes = new TextBox { Location = new Point(leftCol + 200, currentY + 20), Width = 370 };
            pnlBody.Controls.Add(txtNotes);
            currentY += 60;

            // 3. Chế độ lương & Phụ cấp
            var lblSec3 = CreateSectionLabel("3. THIẾT LẬP CHẾ ĐỘ LƯƠNG & PHỤ CẤP THÁNG", leftCol, currentY);
            pnlBody.Controls.Add(lblSec3);
            currentY += 28;

            pnlBody.Controls.Add(CreateFieldLabel("Lương cơ bản (VND):", leftCol, currentY));
            numBaseSalary = CreateSalaryNumeric(leftCol, currentY + 20);
            pnlBody.Controls.Add(numBaseSalary);

            pnlBody.Controls.Add(CreateFieldLabel("Lương theo giờ (VND):", leftCol + 160, currentY));
            numHourlyRate = CreateSalaryNumeric(leftCol + 160, currentY + 20);
            pnlBody.Controls.Add(numHourlyRate);

            pnlBody.Controls.Add(CreateFieldLabel("Phụ cấp ăn trưa:", rightCol, currentY));
            numLunchAllowance = CreateSalaryNumeric(rightCol, currentY + 20);
            pnlBody.Controls.Add(numLunchAllowance);

            pnlBody.Controls.Add(CreateFieldLabel("Phụ cấp xăng xe:", rightCol + 160, currentY));
            numTravelAllowance = CreateSalaryNumeric(rightCol + 160, currentY + 20);
            pnlBody.Controls.Add(numTravelAllowance);
            currentY += 55;

            pnlBody.Controls.Add(CreateFieldLabel("Phụ cấp trách nhiệm:", leftCol, currentY));
            numRespAllowance = CreateSalaryNumeric(leftCol, currentY + 20);
            pnlBody.Controls.Add(numRespAllowance);
            currentY += 60;

            // Bottom Buttons Bar
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(20, 10, 20, 10)
            };

            var pnlBottomButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 260,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            btnCancel = new Button
            {
                Text = "Hủy bỏ",
                DialogResult = DialogResult.Cancel,
                Width = 100,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(71, 85, 105),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Margin = new Padding(4, 2, 0, 0)
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            btnSave = new Button
            {
                Text = "Lưu hồ sơ",
                Width = 120,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Margin = new Padding(4, 2, 0, 0)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            pnlBottomButtons.Controls.Add(btnCancel);
            pnlBottomButtons.Controls.Add(btnSave);
            pnlBottom.Controls.Add(pnlBottomButtons);
            Controls.Add(pnlBottom);
        }

        private void PopulateData()
        {
            txtCode.Text = EmployeeData.EmployeeCode;
            txtFullName.Text = EmployeeData.FullName;
            cboGender.SelectedItem = string.IsNullOrEmpty(EmployeeData.Gender) ? "Nam" : EmployeeData.Gender;
            dtpDOB.Value = EmployeeData.DateOfBirth == default ? DateTime.Today.AddYears(-23) : EmployeeData.DateOfBirth;
            txtCCCD.Text = EmployeeData.IdentityCard;
            txtPhone.Text = EmployeeData.PhoneNumber;
            txtEmail.Text = EmployeeData.Email;
            txtAddress.Text = EmployeeData.Address;

            cboDepartment.SelectedItem = string.IsNullOrEmpty(EmployeeData.Department) ? "Bán hàng" : EmployeeData.Department;
            txtPosition.Text = string.IsNullOrEmpty(EmployeeData.Position) ? "Nhân viên Bán hàng" : EmployeeData.Position;
            dtpHireDate.Value = EmployeeData.HireDate == default ? DateTime.Today : EmployeeData.HireDate;

            cboStatus.SelectedIndex = EmployeeData.Status switch
            {
                EmployeeStatus.Active => 0,
                EmployeeStatus.OnLeave => 1,
                EmployeeStatus.Resigned => 2,
                _ => 0
            };

            txtNotes.Text = EmployeeData.Notes ?? "";

            numBaseSalary.Value = EmployeeData.BaseSalary;
            numHourlyRate.Value = EmployeeData.HourlyRate;
            numLunchAllowance.Value = EmployeeData.LunchAllowance;
            numTravelAllowance.Value = EmployeeData.TravelAllowance;
            numRespAllowance.Value = EmployeeData.ResponsibilityAllowance;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            errorProvider.Clear();
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                errorProvider.SetError(txtFullName, "Vui lòng nhập họ và tên nhân viên.");
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                errorProvider.SetError(txtCode, "Mã nhân viên không được để trống.");
                hasError = true;
            }

            if (!string.IsNullOrWhiteSpace(txtPhone.Text) && !ValidationHelper.IsValidPhoneNumber(txtPhone.Text))
            {
                errorProvider.SetError(txtPhone, "Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0).");
                hasError = true;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !ValidationHelper.IsValidEmail(txtEmail.Text))
            {
                errorProvider.SetError(txtEmail, "Địa chỉ Email không đúng định dạng.");
                hasError = true;
            }

            if (!string.IsNullOrWhiteSpace(txtCCCD.Text) && !ValidationHelper.IsValidIdentityCard(txtCCCD.Text))
            {
                errorProvider.SetError(txtCCCD, "Số CCCD phải gồm 9 hoặc 12 số.");
                hasError = true;
            }

            if (hasError) return;

            EmployeeData.EmployeeCode = txtCode.Text.Trim();
            EmployeeData.FullName = txtFullName.Text.Trim();
            EmployeeData.Gender = cboGender.SelectedItem?.ToString() ?? "Nam";
            EmployeeData.DateOfBirth = dtpDOB.Value;
            EmployeeData.IdentityCard = txtCCCD.Text.Trim();
            EmployeeData.PhoneNumber = txtPhone.Text.Trim();
            EmployeeData.Email = txtEmail.Text.Trim();
            EmployeeData.Address = txtAddress.Text.Trim();
            EmployeeData.Department = cboDepartment.SelectedItem?.ToString() ?? "Bán hàng";
            EmployeeData.Position = txtPosition.Text.Trim();
            EmployeeData.HireDate = dtpHireDate.Value;

            EmployeeData.Status = cboStatus.SelectedIndex switch
            {
                0 => EmployeeStatus.Active,
                1 => EmployeeStatus.OnLeave,
                2 => EmployeeStatus.Resigned,
                _ => EmployeeStatus.Active
            };

            EmployeeData.Notes = txtNotes.Text.Trim();

            EmployeeData.BaseSalary = numBaseSalary.Value;
            EmployeeData.HourlyRate = numHourlyRate.Value;
            EmployeeData.LunchAllowance = numLunchAllowance.Value;
            EmployeeData.TravelAllowance = numTravelAllowance.Value;
            EmployeeData.ResponsibilityAllowance = numRespAllowance.Value;

            DialogResult = DialogResult.OK;
            Close();
        }

        private static Label CreateSectionLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                AutoSize = true,
                Location = new Point(x, y)
            };
        }

        private static Label CreateFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize = true,
                Location = new Point(x, y)
            };
        }

        private static NumericUpDown CreateSalaryNumeric(int x, int y)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Width = 145,
                Maximum = 500000000m,
                ThousandsSeparator = true,
                Increment = 50000m,
                Font = new Font("Segoe UI", 9f)
            };
        }
    }
}
