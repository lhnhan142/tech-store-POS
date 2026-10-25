using BLL.DTOs.Attendance;
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
    public class AttendanceManagementForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Chấm công nhanh
        private readonly ComboBox _cboQuickEmployee = new();
        private readonly HopeRoundButton _btnCheckIn = new();
        private readonly HopeRoundButton _btnCheckOut = new();
        private readonly Label _lblClock = new();
        private readonly System.Windows.Forms.Timer _clockTimer = new();

        // Thẻ KPI
        private readonly Label _lblKpiTotal = new();
        private readonly Label _lblKpiPresent = new();
        private readonly Label _lblKpiLate = new();
        private readonly Label _lblKpiOvertime = new();

        // Bộ lọc lịch sử
        private readonly DateTimePicker _dtpFrom = new();
        private readonly DateTimePicker _dtpTo = new();
        private readonly ComboBox _cboFilterEmployee = new();
        private readonly CheckBox _chkAbnormalOnly = new();
        private readonly HopeRoundButton _btnFilter = new();
        private readonly HopeRoundButton _btnAdjust = new();
        private readonly HopeRoundButton _btnExportExcel = new();
        private readonly HopeRoundButton _btnExportPdf = new();

        private readonly DataGridView _grid = new();
        private List<AttendanceRecordDto> _currentRecords = new();
        private List<EmployeeListItemDto> _employees = new();

        public AttendanceManagementForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Quản lý chấm công - Tech Store POS";
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(S(1150), S(700));
            MinimumSize = new Size(S(950), S(580));
            BackColor = Color.FromArgb(243, 244, 246);

            BuildUi();
            InitClock();
            Load += async (_, _) =>
            {
                await LoadEmployeesAsync();
                await LoadHistoryAsync();
            };
        }

        private int S(int px) => (int)Math.Round(px * DeviceDpi / 96.0);

        private void InitClock()
        {
            _clockTimer.Interval = 1000;
            _clockTimer.Tick += (_, _) =>
            {
                _lblClock.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
            };
            _clockTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _clockTimer.Stop();
            _clockTimer.Dispose();
            base.OnFormClosed(e);
        }

        private void BuildUi()
        {
            // 1. Header
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(65),
                BackColor = Color.White,
                Padding = new Padding(S(20), 0, S(20), 0)
            };

            var lblTitle = new Label
            {
                Text = "QUẢN LÝ CHẤM CÔNG & CA LÀM VIỆC",
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

            pnlHeaderRight.Controls.Add(_btnExportPdf);
            pnlHeaderRight.Controls.Add(_btnExportExcel);

            pnlHeader.Controls.AddRange(new Control[] { pnlHeaderRight, lblTitle });

            // 2. Quick Action & KPI Panel
            var pnlTopDashboard = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(125),
                BackColor = Color.FromArgb(243, 244, 246),
                Padding = new Padding(S(20), S(12), S(20), S(10))
            };

            // Card Chấm công nhanh bên trái
            var pnlQuick = new RoundedPanel
            {
                Radius = S(10),
                BackColor = Color.White,
                Location = new Point(S(20), S(10)),
                Size = new Size(S(480), S(105)),
                Padding = new Padding(S(15), S(10), S(15), S(10))
            };

            var lblQuickTitle = new Label
            {
                Text = "CHẤM CÔNG NHANH HÔM NAY",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(S(15), S(10)),
                AutoSize = true
            };

            _lblClock.Font = new Font("Segoe UI Semibold", 9.5F);
            _lblClock.ForeColor = Color.FromArgb(0, 112, 243);
            _lblClock.Location = new Point(S(230), S(10));
            _lblClock.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
            _lblClock.AutoSize = true;

            _cboQuickEmployee.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboQuickEmployee.Size = new Size(S(230), S(30));
            _cboQuickEmployee.Location = new Point(S(15), S(45));

            _btnCheckIn.Text = "Check-in (Vào)";
            _btnCheckIn.Size = new Size(S(110), S(34));
            _btnCheckIn.Location = new Point(S(255), S(43));
            _btnCheckIn.PrimaryColor = Color.FromArgb(22, 163, 74);
            _btnCheckIn.TextColor = Color.White;
            _btnCheckIn.Click += async (_, _) => await DoCheckInAsync();

            _btnCheckOut.Text = "Check-out (Ra)";
            _btnCheckOut.Size = new Size(S(110), S(34));
            _btnCheckOut.Location = new Point(S(370), S(43));
            _btnCheckOut.PrimaryColor = Color.FromArgb(234, 88, 12);
            _btnCheckOut.TextColor = Color.White;
            _btnCheckOut.Click += async (_, _) => await DoCheckOutAsync();

            pnlQuick.Controls.Add(lblQuickTitle);
            pnlQuick.Controls.Add(_lblClock);
            pnlQuick.Controls.Add(_cboQuickEmployee);
            pnlQuick.Controls.Add(_btnCheckIn);
            pnlQuick.Controls.Add(_btnCheckOut);

            // 4 Thẻ KPI bên phải
            int kpiX = S(515);
            int kpiW = S(140);
            int kpiH = S(105);
            int kpiGap = S(12);

            pnlTopDashboard.Controls.Add(pnlQuick);
            pnlTopDashboard.Controls.Add(CreateKpiCard("Tổng công", _lblKpiTotal, Color.FromArgb(59, 130, 246), kpiX, S(10), kpiW, kpiH));
            pnlTopDashboard.Controls.Add(CreateKpiCard("Đúng giờ", _lblKpiPresent, Color.FromArgb(22, 163, 74), kpiX + (kpiW + kpiGap), S(10), kpiW, kpiH));
            pnlTopDashboard.Controls.Add(CreateKpiCard("Đi muộn", _lblKpiLate, Color.FromArgb(234, 88, 12), kpiX + (kpiW + kpiGap) * 2, S(10), kpiW, kpiH));
            pnlTopDashboard.Controls.Add(CreateKpiCard("Tăng ca", _lblKpiOvertime, Color.FromArgb(147, 51, 234), kpiX + (kpiW + kpiGap) * 3, S(10), kpiW, kpiH));

            // 3. Filter Bar
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(55),
                BackColor = Color.White,
                Padding = new Padding(S(20), S(10), S(20), S(10))
            };

            var lblFrom = new Label { Text = "Từ:", AutoSize = true, Location = new Point(S(20), S(16)), ForeColor = Color.FromArgb(75, 85, 99) };
            _dtpFrom.Format = DateTimePickerFormat.Short;
            _dtpFrom.Size = new Size(S(105), S(28));
            _dtpFrom.Location = new Point(S(50), S(12));
            _dtpFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var lblTo = new Label { Text = "Đến:", AutoSize = true, Location = new Point(S(165), S(16)), ForeColor = Color.FromArgb(75, 85, 99) };
            _dtpTo.Format = DateTimePickerFormat.Short;
            _dtpTo.Size = new Size(S(105), S(28));
            _dtpTo.Location = new Point(S(205), S(12));
            _dtpTo.Value = DateTime.Today;

            _cboFilterEmployee.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboFilterEmployee.Size = new Size(S(180), S(28));
            _cboFilterEmployee.Location = new Point(S(325), S(12));

            _chkAbnormalOnly.Text = "Chỉ xem bất thường";
            _chkAbnormalOnly.AutoSize = true;
            _chkAbnormalOnly.Location = new Point(S(515), S(15));
            _chkAbnormalOnly.ForeColor = Color.FromArgb(220, 38, 38);

            _btnFilter.Text = "Lọc dữ liệu";
            _btnFilter.Size = new Size(S(95), S(32));
            _btnFilter.Location = new Point(S(665), S(10));
            _btnFilter.PrimaryColor = Color.FromArgb(0, 112, 243);
            _btnFilter.TextColor = Color.White;
            _btnFilter.Click += async (_, _) => await LoadHistoryAsync();

            _btnAdjust.Text = "Điều chỉnh công";
            _btnAdjust.Size = new Size(S(125), S(32));
            _btnAdjust.Location = new Point(S(770), S(10));
            _btnAdjust.PrimaryColor = Color.FromArgb(75, 85, 99);
            _btnAdjust.TextColor = Color.White;
            _btnAdjust.Click += async (_, _) => await AdjustRecordAsync();

            pnlFilter.Controls.Add(lblFrom);
            pnlFilter.Controls.Add(_dtpFrom);
            pnlFilter.Controls.Add(lblTo);
            pnlFilter.Controls.Add(_dtpTo);
            pnlFilter.Controls.Add(_cboFilterEmployee);
            pnlFilter.Controls.Add(_chkAbnormalOnly);
            pnlFilter.Controls.Add(_btnFilter);
            pnlFilter.Controls.Add(_btnAdjust);

            // 4. DataGridView
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(S(20), S(10), S(20), S(15)),
                BackColor = Color.FromArgb(243, 244, 246)
            };

            UiKit.StyleGrid(_grid, this, 38);
            _grid.AllowUserToResizeColumns = true;
            ConfigureGridColumns();
            pnlGrid.Controls.Add(_grid);

            Controls.Add(pnlGrid);
            Controls.Add(pnlFilter);
            Controls.Add(pnlTopDashboard);
            Controls.Add(pnlHeader);
        }

        private Panel CreateKpiCard(string title, Label lblVal, Color themeColor, int x, int y, int w, int h)
        {
            var card = new RoundedPanel
            {
                Radius = S(10),
                BackColor = Color.White,
                Location = new Point(x, y),
                Size = new Size(w, h),
                Padding = new Padding(S(12))
            };

            var lblT = new Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 130, 150),
                Dock = DockStyle.Top,
                Height = S(20)
            };

            lblVal.Text = "0";
            lblVal.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblVal.ForeColor = themeColor;
            lblVal.Dock = DockStyle.Fill;
            lblVal.TextAlign = ContentAlignment.MiddleLeft;

            card.Controls.Add(lblVal);
            card.Controls.Add(lblT);
            return card;
        }

        private void ConfigureGridColumns()
        {
            _grid.Columns.Clear();

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColId",
                HeaderText = "ID",
                DataPropertyName = nameof(AttendanceRecordDto.Id),
                Visible = false
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColDate",
                HeaderText = "NGÀY",
                DataPropertyName = nameof(AttendanceRecordDto.Date),
                Width = S(105),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy", Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColCode",
                HeaderText = "MÃ NV",
                DataPropertyName = nameof(AttendanceRecordDto.EmployeeCode),
                Width = S(90)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColName",
                HeaderText = "HỌ VÀ TÊN",
                DataPropertyName = nameof(AttendanceRecordDto.EmployeeName),
                Width = S(170)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColPosition",
                HeaderText = "CHỨC VỤ",
                DataPropertyName = nameof(AttendanceRecordDto.Position),
                Width = S(130)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColIn",
                HeaderText = "GIỜ VÀO",
                DataPropertyName = nameof(AttendanceRecordDto.CheckIn),
                Width = S(90),
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColOut",
                HeaderText = "GIỜ RA",
                DataPropertyName = nameof(AttendanceRecordDto.CheckOut),
                Width = S(90),
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColHours",
                HeaderText = "GIỜ LÀM",
                DataPropertyName = nameof(AttendanceRecordDto.HoursWorked),
                Width = S(85),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.#", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColOt",
                HeaderText = "TĂNG CA",
                DataPropertyName = nameof(AttendanceRecordDto.OvertimeHours),
                Width = S(85),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.#", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColStatus",
                HeaderText = "TRẠNG THÁI",
                DataPropertyName = nameof(AttendanceRecordDto.StatusText),
                Width = S(110)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColNote",
                HeaderText = "GHI CHÚ",
                DataPropertyName = nameof(AttendanceRecordDto.Note),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private async Task LoadEmployeesAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var empSvc = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
                var emps = await empSvc.GetAllAsync(status: EmployeeStatus.Active);
                _employees = emps.ToList();

                _cboQuickEmployee.Items.Clear();
                _cboFilterEmployee.Items.Clear();
                _cboFilterEmployee.Items.Add("Tất cả nhân viên");

                foreach (var e in _employees)
                {
                    _cboQuickEmployee.Items.Add($"{e.EmployeeCode} - {e.FullName}");
                    _cboFilterEmployee.Items.Add($"{e.EmployeeCode} - {e.FullName}");
                }

                if (_cboQuickEmployee.Items.Count > 0) _cboQuickEmployee.SelectedIndex = 0;
                _cboFilterEmployee.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var attSvc = scope.ServiceProvider.GetRequiredService<IAttendanceService>();

                int? filterEmpId = null;
                if (_cboFilterEmployee.SelectedIndex > 0 && _cboFilterEmployee.SelectedIndex - 1 < _employees.Count)
                {
                    filterEmpId = _employees[_cboFilterEmployee.SelectedIndex - 1].Id;
                }

                IReadOnlyList<AttendanceRecordDto> records;
                if (_chkAbnormalOnly.Checked)
                {
                    records = await attSvc.GetAbnormalRecordsAsync(_dtpFrom.Value, _dtpTo.Value);
                    if (filterEmpId.HasValue) records = records.Where(r => r.EmployeeId == filterEmpId.Value).ToList();
                }
                else
                {
                    records = await attSvc.GetHistoryAsync(_dtpFrom.Value, _dtpTo.Value, filterEmpId);
                }

                _currentRecords = records.ToList();
                _grid.DataSource = _currentRecords;

                // Thống kê KPI
                var summary = await attSvc.GetSummaryAsync(_dtpFrom.Value, _dtpTo.Value);
                _lblKpiTotal.Text = $"{summary.TotalHoursWorked:0.#}h";
                _lblKpiPresent.Text = summary.TotalPresent.ToString();
                _lblKpiLate.Text = summary.TotalLate.ToString();
                _lblKpiOvertime.Text = $"{summary.TotalOvertimeHours:0.#}h";
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
        }

        private async Task DoCheckInAsync()
        {
            if (_cboQuickEmployee.SelectedIndex < 0 || _cboQuickEmployee.SelectedIndex >= _employees.Count)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần check-in.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var emp = _employees[_cboQuickEmployee.SelectedIndex];
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var attSvc = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
                await attSvc.CheckInAsync(new CheckInRequestDto
                {
                    EmployeeId = emp.Id,
                    Date = DateTime.Today,
                    CheckInTime = DateTime.Now.TimeOfDay
                });

                MessageBox.Show($"Check-in thành công cho nhân viên {emp.FullName} lúc {DateTime.Now:HH:mm}!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadHistoryAsync();
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
        }

        private async Task DoCheckOutAsync()
        {
            if (_cboQuickEmployee.SelectedIndex < 0 || _cboQuickEmployee.SelectedIndex >= _employees.Count)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần check-out.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var emp = _employees[_cboQuickEmployee.SelectedIndex];
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var attSvc = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
                await attSvc.CheckOutAsync(new CheckOutRequestDto
                {
                    EmployeeId = emp.Id,
                    Date = DateTime.Today,
                    CheckOutTime = DateTime.Now.TimeOfDay
                });

                MessageBox.Show($"Check-out thành công cho nhân viên {emp.FullName} lúc {DateTime.Now:HH:mm}!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadHistoryAsync();
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
        }

        private async Task AdjustRecordAsync()
        {
            if (_grid.CurrentRow?.DataBoundItem is not AttendanceRecordDto item)
            {
                MessageBox.Show("Vui lòng chọn dòng chấm công cần điều chỉnh.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Mở dialog nhập giờ
            using var adjustForm = new Form
            {
                Text = $"Điều chỉnh công: {item.EmployeeName} ({item.Date:dd/MM/yyyy})",
                Font = new Font("Segoe UI", 9.5F),
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(S(380), S(260)),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            var lblIn = new Label { Text = "Giờ vào (HH:mm):", Location = new Point(S(25), S(25)), AutoSize = true };
            var txtIn = new TextBox { Text = item.CheckIn.HasValue ? item.CheckIn.Value.ToString(@"hh\:mm") : "08:00", Location = new Point(S(150), S(22)), Width = S(180) };

            var lblOut = new Label { Text = "Giờ ra (HH:mm):", Location = new Point(S(25), S(65)), AutoSize = true };
            var txtOut = new TextBox { Text = item.CheckOut.HasValue ? item.CheckOut.Value.ToString(@"hh\:mm") : "17:00", Location = new Point(S(150), S(62)), Width = S(180) };

            var lblNote = new Label { Text = "Lý do điều chỉnh:", Location = new Point(S(25), S(105)), AutoSize = true };
            var txtNote = new TextBox { Text = item.Note ?? "Điều chỉnh theo phê duyệt quản lý", Location = new Point(S(150), S(102)), Width = S(180) };

            var btnOk = new HopeRoundButton { Text = "Cập nhật", PrimaryColor = Color.FromArgb(0, 112, 243), Location = new Point(S(150), S(160)), Size = new Size(S(100), S(35)) };
            var btnCancel = new HopeRoundButton { Text = "Đóng", PrimaryColor = Color.FromArgb(229, 231, 235), TextColor = Color.FromArgb(55, 65, 81), Location = new Point(S(260), S(160)), Size = new Size(S(70), S(35)) };

            btnOk.Click += async (_, _) =>
            {
                if (!TimeSpan.TryParse(txtIn.Text.Trim(), out var cin) || !TimeSpan.TryParse(txtOut.Text.Trim(), out var cout))
                {
                    MessageBox.Show("Định dạng giờ không hợp lệ (VD chuẩn: 08:00, 17:30).", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var attSvc = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
                    await attSvc.AdjustAttendanceAsync(new AttendanceAdjustmentDto
                    {
                        Id = item.Id,
                        CheckIn = cin,
                        CheckOut = cout,
                        Status = item.Status,
                        Note = txtNote.Text.Trim()
                    });

                    adjustForm.DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    UiHelper.HandleException(ex);
                }
            };

            btnCancel.Click += (_, _) => adjustForm.DialogResult = DialogResult.Cancel;

            adjustForm.Controls.Add(lblIn);
            adjustForm.Controls.Add(txtIn);
            adjustForm.Controls.Add(lblOut);
            adjustForm.Controls.Add(txtOut);
            adjustForm.Controls.Add(lblNote);
            adjustForm.Controls.Add(txtNote);
            adjustForm.Controls.Add(btnOk);
            adjustForm.Controls.Add(btnCancel);

            if (adjustForm.ShowDialog() == DialogResult.OK)
            {
                await LoadHistoryAsync();
            }
        }

        private void ExportExcel()
        {
            if (_currentRecords.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu chấm công để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"BangChamCong_{_dtpFrom.Value:yyyyMMdd}_{_dtpTo.Value:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var exp = scope.ServiceProvider.GetRequiredService<IReportExportService>();
                    exp.ExportAttendanceReportToExcel(sfd.FileName, _currentRecords, _dtpFrom.Value, _dtpTo.Value);
                    MessageBox.Show("Xuất báo cáo chấm công Excel thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    UiHelper.HandleException(ex);
                }
            }
        }

        private void ExportPdf()
        {
            if (_currentRecords.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu chấm công để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"BangChamCong_{_dtpFrom.Value:yyyyMMdd}_{_dtpTo.Value:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var exp = scope.ServiceProvider.GetRequiredService<IReportExportService>();
                    exp.ExportAttendanceReportToPdf(sfd.FileName, _currentRecords, _dtpFrom.Value, _dtpTo.Value);
                    MessageBox.Show("Xuất báo cáo chấm công PDF thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    UiHelper.HandleException(ex);
                }
            }
        }
    }
}
