using System;
using System.Drawing;
using System.Windows.Forms;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.Common.Enums;

namespace PhuKienMayTinh.GUI.Forms
{
    public class AdjustAttendanceForm : Form
    {
        public AdjustAttendanceDTO AdjustmentData { get; private set; }

        private DateTimePicker dtpCheckIn = null!;
        private DateTimePicker dtpCheckOut = null!;
        private NumericUpDown numHours = null!;
        private ComboBox cboStatus = null!;
        private TextBox txtAdjuster = null!;
        private TextBox txtReason = null!;

        private Button btnSave = null!;
        private Button btnCancel = null!;
        private ErrorProvider errorProvider = null!;

        public AdjustAttendanceForm(AttendanceDTO attendance)
        {
            AdjustmentData = new AdjustAttendanceDTO
            {
                AttendanceId = attendance.Id,
                NewCheckInTime = attendance.CheckInTime,
                NewCheckOutTime = attendance.CheckOutTime,
                NewHoursWorked = attendance.HoursWorked,
                NewStatus = attendance.Status
            };

            InitializeComponent(attendance);
        }

        private void InitializeComponent(AttendanceDTO att)
        {
            Text = "ĐIỀU CHỈNH THÔNG TIN CHẤM CÔNG";
            Size = new Size(560, 500);
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
                Text = $"ĐIỀU CHỈNH CÔNG: {att.EmployeeCode} - {att.EmployeeName}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 20)
            };
            pnlHeader.Controls.Add(lblHeader);
            Controls.Add(pnlHeader);

            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25, 20, 25, 20) };
            Controls.Add(pnlBody);
            pnlBody.BringToFront();

            int leftCol = 25;
            int currentY = 15;

            // Ngày làm việc
            pnlBody.Controls.Add(new Label { Text = $"Ngày làm việc: {att.WorkDate:dd/MM/yyyy} ({att.ShiftDisplay})", Font = new Font("Segoe UI", 9f, FontStyle.Italic), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(leftCol, currentY), AutoSize = true });
            currentY += 30;

            // Giờ Check-in
            pnlBody.Controls.Add(new Label { Text = "Giờ Check-in:", Location = new Point(leftCol, currentY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105) });
            dtpCheckIn = new DateTimePicker
            {
                Location = new Point(leftCol, currentY + 20),
                Width = 200,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = att.CheckInTime ?? att.WorkDate.Date.AddHours(8)
            };
            pnlBody.Controls.Add(dtpCheckIn);

            // Giờ Check-out
            pnlBody.Controls.Add(new Label { Text = "Giờ Check-out:", Location = new Point(leftCol + 230, currentY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105) });
            dtpCheckOut = new DateTimePicker
            {
                Location = new Point(leftCol + 230, currentY + 20),
                Width = 200,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = att.CheckOutTime ?? att.WorkDate.Date.AddHours(17)
            };
            pnlBody.Controls.Add(dtpCheckOut);
            currentY += 60;

            // Số giờ làm & Trạng thái
            pnlBody.Controls.Add(new Label { Text = "Số giờ làm thực tế:", Location = new Point(leftCol, currentY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105) });
            numHours = new NumericUpDown
            {
                Location = new Point(leftCol, currentY + 20),
                Width = 200,
                DecimalPlaces = 1,
                Increment = 0.5m,
                Maximum = 16,
                Minimum = 0,
                Value = (decimal)att.HoursWorked
            };
            pnlBody.Controls.Add(numHours);

            pnlBody.Controls.Add(new Label { Text = "Trạng thái công:", Location = new Point(leftCol + 230, currentY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105) });
            cboStatus = new ComboBox { Location = new Point(leftCol + 230, currentY + 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Đúng giờ", "Đi trễ", "Về sớm", "Trễ & Về sớm", "Nghỉ có phép", "Vắng không phép" });
            cboStatus.SelectedIndex = att.Status switch
            {
                AttendanceStatus.Present => 0,
                AttendanceStatus.Late => 1,
                AttendanceStatus.EarlyLeave => 2,
                AttendanceStatus.LateAndEarlyLeave => 3,
                AttendanceStatus.ExcusedAbsence => 4,
                AttendanceStatus.UnexcusedAbsence => 5,
                _ => 0
            };
            pnlBody.Controls.Add(cboStatus);
            currentY += 60;

            // Người điều chỉnh
            pnlBody.Controls.Add(new Label { Text = "Người điều chỉnh (Quản lý/Admin):", Location = new Point(leftCol, currentY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105) });
            txtAdjuster = new TextBox { Location = new Point(leftCol, currentY + 20), Width = 430, Text = "Quản lý cửa hàng" };
            pnlBody.Controls.Add(txtAdjuster);
            currentY += 55;

            // Lý do bắt buộc
            pnlBody.Controls.Add(new Label { Text = "Lý do điều chỉnh (*) (Bắt buộc):", Location = new Point(leftCol, currentY), AutoSize = true, ForeColor = Color.FromArgb(220, 38, 38), Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            txtReason = new TextBox { Location = new Point(leftCol, currentY + 20), Width = 430, Height = 55, Multiline = true, PlaceholderText = "Ví dụ: Quên quẹt vân tay / Đi công tác giao hàng cho khách..." };
            pnlBody.Controls.Add(txtReason);

            // Bottom Buttons
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 55, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(15, 10, 15, 10) };
            var pnlBottomActions = new FlowLayoutPanel
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
                Width = 90,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(226, 232, 240),
                UseMnemonic = false,
                Margin = new Padding(4, 2, 0, 0)
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            btnSave = new Button
            {
                Text = "Lưu điều chỉnh",
                Width = 125,
                Height = 34,
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

            pnlBottomActions.Controls.Add(btnCancel);
            pnlBottomActions.Controls.Add(btnSave);
            pnlBottom.Controls.Add(pnlBottomActions);
            Controls.Add(pnlBottom);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            errorProvider.Clear();
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                errorProvider.SetError(txtReason, "Bắt buộc phải ghi rõ lý do điều chỉnh công.");
                return;
            }

            AdjustmentData.NewCheckInTime = dtpCheckIn.Value;
            AdjustmentData.NewCheckOutTime = dtpCheckOut.Value;
            AdjustmentData.NewHoursWorked = (double)numHours.Value;
            AdjustmentData.NewStatus = cboStatus.SelectedIndex switch
            {
                0 => AttendanceStatus.Present,
                1 => AttendanceStatus.Late,
                2 => AttendanceStatus.EarlyLeave,
                3 => AttendanceStatus.LateAndEarlyLeave,
                4 => AttendanceStatus.ExcusedAbsence,
                5 => AttendanceStatus.UnexcusedAbsence,
                _ => AttendanceStatus.Present
            };
            AdjustmentData.AdjustedBy = txtAdjuster.Text.Trim();
            AdjustmentData.Reason = txtReason.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
