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
    public class AttendanceUC : UserControl
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IEmployeeService _employeeService;

        // Clock Timer
        private System.Windows.Forms.Timer clockTimer = null!;
        private Label lblClock = null!;

        // Quick Check-in/out Controls
        private ComboBox cboEmpQuick = null!;
        private ComboBox cboShiftQuick = null!;
        private TextBox txtNoteQuick = null!;
        private Button btnCheckIn = null!;
        private Button btnCheckOut = null!;
        private Label lblFeedback = null!;

        // Filter Controls
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private ComboBox cboEmpFilter = null!;
        private ComboBox cboStatusFilter = null!;
        private Button btnFilter = null!;
        private Button btnAdjust = null!;
        private Button btnExportExcel = null!;

        // Grid & Summary
        private DataGridView dgvAttendance = null!;
        private Label lblSummary = null!;

        public AttendanceUC(IAttendanceService attendanceService, IEmployeeService employeeService)
        {
            _attendanceService = attendanceService;
            _employeeService = employeeService;
            InitializeComponent();
            StartClock();
        }

        private void InitializeComponent()
        {
            BackColor = Color.FromArgb(15, 23, 42); // Slate 900 #0F172A
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 9f);
            AutoScroll = true;

            // 1. Top Panel: Quick Check-in / Check-out Box
            var pnlQuick = new Panel
            {
                Dock = DockStyle.Top,
                Height = 155,
                BackColor = Color.FromArgb(30, 41, 59), // Slate 800
                Padding = new Padding(20, 15, 20, 15)
            };

            // Tiêu đề & Đồng hồ thời gian thực
            var lblTitle = new Label
            {
                Text = "⏱️ CHẤM CÔNG THỜI GIAN THỰC (REAL-TIME ATTENDANCE)",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 250, 252),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 15)
            };
            pnlQuick.Controls.Add(lblTitle);

            var pnlClockContainer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 380,
                Height = 45,
                BackColor = Color.Transparent
            };

            lblClock = new Label
            {
                Text = DateTime.Now.ToString("HH:mm:ss - dddd, dd/MM/yyyy"),
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(12, 6, 12, 6),
                AutoSize = true,
                Location = new Point(10, 8)
            };
            pnlClockContainer.Controls.Add(lblClock);
            pnlQuick.Controls.Add(pnlClockContainer);

            // Row điều khiển check-in
            int rowY = 56;
            pnlQuick.Controls.Add(new Label { Text = "Chọn nhân viên:", Location = new Point(20, rowY), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) });
            cboEmpQuick = new ComboBox { Location = new Point(20, rowY + 22), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            pnlQuick.Controls.Add(cboEmpQuick);

            pnlQuick.Controls.Add(new Label { Text = "Ca làm việc:", Location = new Point(275, rowY), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) });
            cboShiftQuick = new ComboBox { Location = new Point(275, rowY + 22), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboShiftQuick.Items.AddRange(new object[] { "Hành chính (08:00 - 17:00)", "Ca Sáng (08:00 - 12:00)", "Ca Chiều (13:00 - 17:00)", "Ca Tối (17:30 - 21:30)" });
            cboShiftQuick.SelectedIndex = 0;
            pnlQuick.Controls.Add(cboShiftQuick);

            pnlQuick.Controls.Add(new Label { Text = "Ghi chú chấm công:", Location = new Point(470, rowY), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) });
            txtNoteQuick = new TextBox
            {
                Location = new Point(470, rowY + 22),
                Width = 200,
                PlaceholderText = "Ghi chú nếu có...",
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(241, 245, 249),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlQuick.Controls.Add(txtNoteQuick);

            // Nút Check-in
            btnCheckIn = new Button
            {
                Text = "🟢 CHECK-IN",
                Location = new Point(685, rowY + 16),
                Width = 130,
                Height = 36,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            btnCheckIn.FlatAppearance.BorderSize = 0;
            btnCheckIn.Click += async (s, e) => await HandleCheckInAsync();
            pnlQuick.Controls.Add(btnCheckIn);

            // Nút Check-out
            btnCheckOut = new Button
            {
                Text = "🔴 CHECK-OUT",
                Location = new Point(825, rowY + 16),
                Width = 130,
                Height = 36,
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            btnCheckOut.FlatAppearance.BorderSize = 0;
            btnCheckOut.Click += async (s, e) => await HandleCheckOutAsync();
            pnlQuick.Controls.Add(btnCheckOut);

            // Dòng thông báo kết quả
            lblFeedback = new Label
            {
                Text = "Sẵn sàng ghi nhận chấm công.",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(20, 122),
                AutoSize = true
            };
            pnlQuick.Controls.Add(lblFeedback);

            Controls.Add(pnlQuick);

            // 2. Filter Bar Lịch sử chấm công (Slate 800)
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20, 12, 20, 12),
                Margin = new Padding(0, 1, 0, 0)
            };

            // Nút Điều chỉnh công & Xuất Excel góc phải (FlowLayoutPanel ngăn chặn việc đè chữ)
            var pnlFilterActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 285,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            btnExportExcel = new Button
            {
                Text = "📊 Xuất Excel",
                Width = 115,
                Height = 32,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Margin = new Padding(2, 2, 4, 0)
            };
            btnExportExcel.FlatAppearance.BorderSize = 0;
            btnExportExcel.Click += async (s, e) => await ExportExcelAsync();

            btnAdjust = new Button
            {
                Text = "✏️ Điều chỉnh công",
                Width = 140,
                Height = 32,
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Margin = new Padding(2, 2, 4, 0)
            };
            btnAdjust.FlatAppearance.BorderSize = 0;
            btnAdjust.Click += async (s, e) => await AdjustAttendanceAsync();

            pnlFilterActions.Controls.Add(btnExportExcel);
            pnlFilterActions.Controls.Add(btnAdjust);
            pnlFilter.Controls.Add(pnlFilterActions);

            pnlFilter.Controls.Add(new Label { Text = "Từ ngày:", AutoSize = true, Location = new Point(20, 18), ForeColor = Color.FromArgb(148, 163, 184) });
            dtpFrom = new DateTimePicker { Location = new Point(80, 14), Width = 105, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-14) };
            pnlFilter.Controls.Add(dtpFrom);

            pnlFilter.Controls.Add(new Label { Text = "Đến:", AutoSize = true, Location = new Point(195, 18), ForeColor = Color.FromArgb(148, 163, 184) });
            dtpTo = new DateTimePicker { Location = new Point(232, 14), Width = 105, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            pnlFilter.Controls.Add(dtpTo);

            pnlFilter.Controls.Add(new Label { Text = "Nhân viên:", AutoSize = true, Location = new Point(350, 18), ForeColor = Color.FromArgb(148, 163, 184) });
            cboEmpFilter = new ComboBox { Location = new Point(420, 14), Width = 175, DropDownStyle = ComboBoxStyle.DropDownList };
            pnlFilter.Controls.Add(cboEmpFilter);

            pnlFilter.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Location = new Point(605, 18), ForeColor = Color.FromArgb(148, 163, 184) });
            cboStatusFilter = new ComboBox { Location = new Point(675, 14), Width = 125, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatusFilter.Items.AddRange(new object[] { "Tất cả", "Đúng giờ", "Đi trễ", "Về sớm", "Nghỉ có phép", "Vắng không phép" });
            cboStatusFilter.SelectedIndex = 0;
            pnlFilter.Controls.Add(cboStatusFilter);

            btnFilter = new Button
            {
                Text = "🔍 Lọc",
                Location = new Point(810, 12),
                Width = 75,
                Height = 32,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += async (s, e) => await LoadAttendanceHistoryAsync();
            pnlFilter.Controls.Add(btnFilter);

            Controls.Add(pnlFilter);

            // 3. Grid Lịch sử (Slate 900)
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.FromArgb(15, 23, 42)
            };

            dgvAttendance = new DataGridView
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
                RowTemplate = { Height = 36 }
            };

            dgvAttendance.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvAttendance.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
            dgvAttendance.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvAttendance.ColumnHeadersHeight = 38;
            dgvAttendance.DefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvAttendance.DefaultCellStyle.ForeColor = Color.FromArgb(241, 245, 249);
            dgvAttendance.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 58, 138);
            dgvAttendance.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Visible = false });
            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã NV", FillWeight = 60 });
            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Họ và Tên", FillWeight = 110 });
            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Ngày", FillWeight = 75, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Shift", HeaderText = "Ca làm việc", FillWeight = 90 });
            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "In", HeaderText = "Giờ vào", FillWeight = 65, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Out", HeaderText = "Giờ ra", FillWeight = 65, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hours", HeaderText = "Số giờ", FillWeight = 55, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Late", HeaderText = "Đi trễ", FillWeight = 55, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái", FillWeight = 90, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Note", HeaderText = "Ghi chú / Điều chỉnh", FillWeight = 130 });

            dgvAttendance.CellFormatting += DgvAttendance_CellFormatting;

            pnlGrid.Controls.Add(dgvAttendance);

            lblSummary = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                Text = "Tổng số lượt chấm công: 0",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlGrid.Controls.Add(lblSummary);

            Controls.Add(pnlGrid);
            pnlGrid.BringToFront();
        }

        private void StartClock()
        {
            clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) =>
            {
                lblClock.Text = DateTime.Now.ToString("HH:mm:ss - dddd, dd/MM/yyyy");
            };
            clockTimer.Start();
        }

        public async Task LoadInitialDataAsync()
        {
            try
            {
                var employees = (await _employeeService.GetAllEmployeesAsync()).ToList();

                cboEmpQuick.Items.Clear();
                cboEmpFilter.Items.Clear();
                cboEmpFilter.Items.Add(new ComboboxItem { Text = "Tất cả nhân viên", Value = 0 });

                foreach (var emp in employees.Where(e => e.Status == EmployeeStatus.Active))
                {
                    cboEmpQuick.Items.Add(new ComboboxItem { Text = $"{emp.EmployeeCode} - {emp.FullName} ({emp.Department})", Value = emp.Id });
                    cboEmpFilter.Items.Add(new ComboboxItem { Text = $"{emp.EmployeeCode} - {emp.FullName}", Value = emp.Id });
                }

                if (cboEmpQuick.Items.Count > 0) cboEmpQuick.SelectedIndex = 0;
                cboEmpFilter.SelectedIndex = 0;

                await LoadAttendanceHistoryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp danh sách nhân viên chấm công: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task LoadAttendanceHistoryAsync()
        {
            try
            {
                var from = dtpFrom.Value.Date;
                var to = dtpTo.Value.Date;
                int? empId = (cboEmpFilter.SelectedItem as ComboboxItem)?.Value > 0 ? (cboEmpFilter.SelectedItem as ComboboxItem)?.Value : null;

                AttendanceStatus? status = cboStatusFilter.SelectedIndex switch
                {
                    1 => AttendanceStatus.Present,
                    2 => AttendanceStatus.Late,
                    3 => AttendanceStatus.EarlyLeave,
                    4 => AttendanceStatus.ExcusedAbsence,
                    5 => AttendanceStatus.UnexcusedAbsence,
                    _ => null
                };

                var list = (await _attendanceService.GetAttendancesAsync(from, to, empId, status)).ToList();

                dgvAttendance.Rows.Clear();
                foreach (var a in list)
                {
                    string lateStr = a.LateMinutes > 0 ? $"{a.LateMinutes}p" : "-";
                    string noteStr = a.Note ?? "";
                    if (a.IsAdjusted) noteStr = $"[Đã điều chỉnh]: {a.AdjustmentReason} ({a.AdjustedBy})";

                    dgvAttendance.Rows.Add(
                        a.Id,
                        a.EmployeeCode,
                        a.EmployeeName,
                        a.WorkDate.ToString("dd/MM/yyyy"),
                        a.ShiftDisplay,
                        a.CheckInDisplay,
                        a.CheckOutDisplay,
                        $"{a.HoursWorked:N1}h",
                        lateStr,
                        a.StatusDisplay,
                        noteStr
                    );
                }

                int totalCount = list.Count;
                int onTimeCount = list.Count(x => x.Status == AttendanceStatus.Present);
                int lateCount = list.Count(x => x.Status == AttendanceStatus.Late || x.Status == AttendanceStatus.LateAndEarlyLeave);
                int absenceCount = list.Count(x => x.Status == AttendanceStatus.ExcusedAbsence || x.Status == AttendanceStatus.UnexcusedAbsence);

                lblSummary.Text = $"Tổng: {totalCount} lượt | Đúng giờ: {onTimeCount} | Đi trễ: {lateCount} | Vắng mặt: {absenceCount}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải lịch sử chấm công: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task HandleCheckInAsync()
        {
            if (cboEmpQuick.SelectedItem is not ComboboxItem selectedEmp || selectedEmp.Value <= 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần Check-in.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShiftType shift = cboShiftQuick.SelectedIndex switch
            {
                0 => ShiftType.FullDay,
                1 => ShiftType.Morning,
                2 => ShiftType.Afternoon,
                3 => ShiftType.Evening,
                _ => ShiftType.FullDay
            };

            try
            {
                var req = new CheckInRequestDTO
                {
                    EmployeeId = selectedEmp.Value,
                    Shift = shift,
                    CheckTime = DateTime.Now,
                    Note = txtNoteQuick.Text.Trim()
                };

                var res = await _attendanceService.CheckInAsync(req);
                lblFeedback.Text = $"Check-in thành công: {res.EmployeeName} lúc {res.CheckInDisplay} ({res.StatusDisplay})";
                lblFeedback.ForeColor = Color.FromArgb(52, 211, 153);

                MessageBox.Show($"Check-in thành công cho '{res.EmployeeName}'!\nGiờ ghi nhận: {res.CheckInDisplay}\nTình trạng: {res.StatusDisplay}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAttendanceHistoryAsync();
            }
            catch (Exception ex)
            {
                lblFeedback.Text = $"Lỗi: {ex.Message}";
                lblFeedback.ForeColor = Color.FromArgb(248, 113, 113);
                MessageBox.Show(ex.Message, "Không thể Check-in", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task HandleCheckOutAsync()
        {
            if (cboEmpQuick.SelectedItem is not ComboboxItem selectedEmp || selectedEmp.Value <= 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần Check-out.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShiftType shift = cboShiftQuick.SelectedIndex switch
            {
                0 => ShiftType.FullDay,
                1 => ShiftType.Morning,
                2 => ShiftType.Afternoon,
                3 => ShiftType.Evening,
                _ => ShiftType.FullDay
            };

            try
            {
                var req = new CheckOutRequestDTO
                {
                    EmployeeId = selectedEmp.Value,
                    Shift = shift,
                    CheckTime = DateTime.Now,
                    Note = txtNoteQuick.Text.Trim()
                };

                var res = await _attendanceService.CheckOutAsync(req);
                lblFeedback.Text = $"Check-out thành công: {res.EmployeeName} lúc {res.CheckOutDisplay} (Làm việc: {res.HoursWorked} giờ)";
                lblFeedback.ForeColor = Color.FromArgb(56, 189, 248);

                MessageBox.Show($"Check-out thành công cho '{res.EmployeeName}'!\nGiờ ra: {res.CheckOutDisplay}\nTổng giờ làm việc: {res.HoursWorked} giờ", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAttendanceHistoryAsync();
            }
            catch (Exception ex)
            {
                lblFeedback.Text = $"Lỗi: {ex.Message}";
                lblFeedback.ForeColor = Color.FromArgb(248, 113, 113);
                MessageBox.Show(ex.Message, "Không thể Check-out", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task AdjustAttendanceAsync()
        {
            if (dgvAttendance.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một dòng chấm công trong bảng để điều chỉnh.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvAttendance.SelectedRows[0].Cells["Id"].Value);
            var from = dtpFrom.Value.Date;
            var to = dtpTo.Value.Date;
            var all = await _attendanceService.GetAttendancesAsync(from, to, null, null);
            var att = all.FirstOrDefault(x => x.Id == id);
            if (att == null) return;

            using (var form = new AdjustAttendanceForm(att))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    await _attendanceService.AdjustAttendanceAsync(form.AdjustmentData);
                    MessageBox.Show("Đã lưu điều chỉnh chấm công có ghi nhận lý do!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadAttendanceHistoryAsync();
                }
            }
        }

        private async Task ExportExcelAsync()
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"BangChamCong_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var from = dtpFrom.Value.Date;
                    var to = dtpTo.Value.Date;
                    int? empId = (cboEmpFilter.SelectedItem as ComboboxItem)?.Value > 0 ? (cboEmpFilter.SelectedItem as ComboboxItem)?.Value : null;
                    AttendanceStatus? status = cboStatusFilter.SelectedIndex switch
                    {
                        1 => AttendanceStatus.Present,
                        2 => AttendanceStatus.Late,
                        3 => AttendanceStatus.EarlyLeave,
                        4 => AttendanceStatus.ExcusedAbsence,
                        5 => AttendanceStatus.UnexcusedAbsence,
                        _ => null
                    };

                    await _attendanceService.ExportAttendancesToExcelAsync(sfd.FileName, from, to, empId, status);
                    MessageBox.Show("Xuất bảng chấm công ra Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void DgvAttendance_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvAttendance.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string s = e.Value.ToString() ?? "";
                if (s.Contains("Đúng giờ"))
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(52, 211, 153); // Emerald
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else if (s.Contains("trễ") || s.Contains("sớm"))
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(251, 191, 36); // Amber
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else if (s.Contains("Vắng") || s.Contains("Nghỉ"))
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(248, 113, 113); // Rose
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            }

            if (e.RowIndex % 2 == 1 && dgvAttendance.Columns[e.ColumnIndex].Name != "Status")
            {
                e.CellStyle!.BackColor = Color.FromArgb(22, 32, 50);
            }
        }

        private class ComboboxItem
        {
            public string Text { get; set; } = string.Empty;
            public int Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
