using BLL.DTOs.Attendance;
using BLL.DTOs.Dashboard;
using BLL.DTOs.Employee;
using BLL.DTOs.Payroll;
using BLL.DTOs.Report;
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
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI.Forms.Reports
{
    [DesignerCategory("Code")]
    public class ReportManagementForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Controls
        private readonly ComboBox _cboReportType = new();
        private readonly Panel _pnlDynamicFilters = new();

        // Dynamic Filter Controls
        private readonly DateTimePicker _dtpFrom = new();
        private readonly DateTimePicker _dtpTo = new();
        private readonly ComboBox _cboPayroll = new();
        private readonly ComboBox _cboStatus = new();
        private readonly Label _lblFilter1 = new();
        private readonly Label _lblFilter2 = new();
        private readonly Label _lblArrow = new();

        // Action Buttons
        private readonly HopeRoundButton _btnPreview = new();
        private readonly HopeRoundButton _btnExportExcel = new();
        private readonly HopeRoundButton _btnExportPdf = new();

        // Preview Grid & Status
        private readonly DataGridView _gridPreview = new();
        private readonly Label _lblRecordCount = new();

        // Cached Data for export
        private List<EmployeeListItemDto>? _cachedEmployees;
        private List<AttendanceRecordDto>? _cachedAttendances;
        private PayrollSummaryDto? _cachedPayrollSummary;
        private List<PayrollDetailDto>? _cachedPayrollDetails;
        private DashboardSummaryDto? _cachedDashboard;
        private List<TopSellingProductDto>? _cachedTopProducts;

        private List<PayrollSummaryDto> _availablePayrolls = new();

        public ReportManagementForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Trung tâm Báo cáo & Xuất Dữ liệu";
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.FromArgb(243, 244, 246);

            BuildUi();
            Load += async (_, _) =>
            {
                await LoadPayrollsListAsync();
                UpdateDynamicFilterVisibility();
                await LoadPreviewDataAsync();
            };
        }

        private int S(int px) => (int)Math.Round(px * DeviceDpi / 96.0);

        private void BuildUi()
        {
            // 1. Header
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(65),
                BackColor = Color.White,
                Padding = new Padding(S(20), S(12), S(20), S(12))
            };
            pnlHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(229, 231, 235), 1);
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "TRUNG TÂM BÁO CÁO & XUẤT DỮ LIỆU CHUẨN HOÁ",
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = AppTheme.Primary,
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(S(20), S(10))
            };

            var lblSub = new Label
            {
                Text = "Kết xuất dữ liệu định dạng Excel (.xlsx ClosedXML) và PDF (.pdf PdfSharpCore) có tiêu đề, chữ ký và thống kê",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(107, 114, 128),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(S(20), S(38))
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

            // 2. Filter & Selection Card
            var pnlFilterCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(110),
                BackColor = Color.White,
                Padding = new Padding(S(20), S(14), S(20), S(14))
            };
            pnlFilterCard.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(229, 231, 235), 1);
                e.Graphics.DrawLine(pen, 0, pnlFilterCard.Height - 1, pnlFilterCard.Width, pnlFilterCard.Height - 1);
            };

            var lblReportType = new Label
            {
                Text = "Chọn mẫu báo cáo:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 81),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(S(20), S(14))
            };

            _cboReportType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboReportType.Font = new Font("Segoe UI", 10F);
            _cboReportType.Size = new Size(S(280), S(34));
            _cboReportType.Location = new Point(S(20), S(38));
            _cboReportType.Items.AddRange(new object[]
            {
                "1. Báo cáo Danh sách Nhân sự & Mức lương",
                "2. Báo cáo Chấm công & Tăng ca Chi tiết",
                "3. Báo cáo Bảng lương & Chi phí Lương",
                "4. Báo cáo Tổng hợp Kinh doanh, Doanh thu & Tồn kho"
            });
            _cboReportType.SelectedIndex = 0;
            _cboReportType.SelectedIndexChanged += async (_, _) =>
            {
                UpdateDynamicFilterVisibility();
                await LoadPreviewDataAsync();
            };

            // Dynamic filter panel
            _pnlDynamicFilters.Location = new Point(S(315), S(12));
            _pnlDynamicFilters.Size = new Size(S(360), S(80));
            _pnlDynamicFilters.BackColor = Color.Transparent;

            SetupDynamicFilters();

            // Action Buttons
            var pnlActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, S(28), 0, 0),
                BackColor = Color.Transparent
            };

            ConfigureButton(_btnPreview, "Xem trước", Color.FromArgb(79, 70, 229), S(95), S(36));
            _btnPreview.Margin = new Padding(S(4), 0, 0, 0);
            _btnPreview.Click += async (_, _) => await LoadPreviewDataAsync();

            ConfigureButton(_btnExportExcel, "Xuất Excel", Color.FromArgb(34, 197, 94), S(105), S(36));
            _btnExportExcel.Margin = new Padding(S(4), 0, 0, 0);
            _btnExportExcel.Click += async (_, _) => await ExportExcelAsync();

            ConfigureButton(_btnExportPdf, "Xuất PDF", Color.FromArgb(239, 68, 68), S(95), S(36));
            _btnExportPdf.Margin = new Padding(S(4), 0, 0, 0);
            _btnExportPdf.Click += async (_, _) => await ExportPdfAsync();

            pnlActions.Controls.AddRange(new Control[] { _btnPreview, _btnExportExcel, _btnExportPdf });

            pnlFilterCard.Controls.AddRange(new Control[] { pnlActions, lblReportType, _cboReportType, _pnlDynamicFilters });

            // 3. Preview Container & Record Counter
            var pnlPreviewHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(40),
                BackColor = Color.FromArgb(243, 244, 246),
                Padding = new Padding(S(20), S(10), S(20), S(4))
            };

            var lblPreviewTitle = new Label
            {
                Text = "BẢN XEM TRƯỚC DỮ LIỆU BÁO CÁO",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 81),
                Dock = DockStyle.Left,
                AutoSize = true
            };

            _lblRecordCount.Text = "Đang tải...";
            _lblRecordCount.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            _lblRecordCount.ForeColor = Color.FromArgb(107, 114, 128);
            _lblRecordCount.Dock = DockStyle.Right;
            _lblRecordCount.AutoSize = true;

            pnlPreviewHeader.Controls.AddRange(new Control[] { lblPreviewTitle, _lblRecordCount });

            BuildGrid();

            var pnlGridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(S(20), 0, S(20), S(20)),
                BackColor = Color.FromArgb(243, 244, 246)
            };
            pnlGridContainer.Controls.Add(_gridPreview);

            Controls.Add(pnlGridContainer);
            Controls.Add(pnlPreviewHeader);
            Controls.Add(pnlFilterCard);
            Controls.Add(pnlHeader);
        }

        private void SetupDynamicFilters()
        {
            _lblFilter1.AutoSize = true;
            _lblFilter1.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblFilter1.ForeColor = Color.FromArgb(55, 65, 81);
            _lblFilter1.Location = new Point(0, S(2));

            _lblFilter2.AutoSize = true;
            _lblFilter2.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblFilter2.ForeColor = Color.FromArgb(55, 65, 81);
            _lblFilter2.Location = new Point(S(180), S(2));

            _dtpFrom.Format = DateTimePickerFormat.Short;
            _dtpFrom.Font = new Font("Segoe UI", 9.5F);
            _dtpFrom.Size = new Size(S(120), S(32));
            _dtpFrom.Location = new Point(0, S(26));
            _dtpFrom.Value = DateTime.Today.AddDays(-30);

            _lblArrow.Text = "→";
            _lblArrow.AutoSize = true;
            _lblArrow.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _lblArrow.ForeColor = Color.FromArgb(107, 114, 128);
            _lblArrow.Location = new Point(S(128), S(30));

            _dtpTo.Format = DateTimePickerFormat.Short;
            _dtpTo.Font = new Font("Segoe UI", 9.5F);
            _dtpTo.Size = new Size(S(120), S(32));
            _dtpTo.Location = new Point(S(150), S(26));
            _dtpTo.Value = DateTime.Today;

            _cboPayroll.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboPayroll.Font = new Font("Segoe UI", 9.5F);
            _cboPayroll.Size = new Size(S(260), S(32));
            _cboPayroll.Location = new Point(0, S(26));

            _cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboStatus.Font = new Font("Segoe UI", 9.5F);
            _cboStatus.Size = new Size(S(160), S(32));
            _cboStatus.Location = new Point(0, S(26));
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Đang làm việc", "Nghỉ phép", "Đã nghỉ việc" });
            _cboStatus.SelectedIndex = 0;

            _pnlDynamicFilters.Controls.AddRange(new Control[]
            {
                _lblFilter1, _lblFilter2, _dtpFrom, _lblArrow, _dtpTo, _cboPayroll, _cboStatus
            });
        }

        private void UpdateDynamicFilterVisibility()
        {
            int type = _cboReportType.SelectedIndex;

            _lblFilter1.Visible = false;
            _lblFilter2.Visible = false;
            _dtpFrom.Visible = false;
            _lblArrow.Visible = false;
            _dtpTo.Visible = false;
            _cboPayroll.Visible = false;
            _cboStatus.Visible = false;

            switch (type)
            {
                case 0: // Nhân sự
                    _lblFilter1.Text = "Trạng thái nhân sự:";
                    _lblFilter1.Visible = true;
                    _cboStatus.Visible = true;
                    break;

                case 1: // Chấm công
                    _lblFilter1.Text = "Thời gian từ - đến:";
                    _lblFilter1.Visible = true;
                    _dtpFrom.Visible = true;
                    _lblArrow.Visible = true;
                    _dtpTo.Visible = true;
                    break;

                case 2: // Bảng lương
                    _lblFilter1.Text = "Chọn kỳ bảng lương:";
                    _lblFilter1.Visible = true;
                    _cboPayroll.Visible = true;
                    break;

                case 3: // Kinh doanh
                    _lblFilter1.Text = "Kỳ báo cáo doanh thu:";
                    _lblFilter1.Visible = true;
                    _dtpFrom.Visible = true;
                    _lblArrow.Visible = true;
                    _dtpTo.Visible = true;
                    break;
            }
        }

        private void ConfigureButton(HopeRoundButton btn, string text, Color color, int width, int height)
        {
            btn.Text = text;
            btn.Size = new Size(width, height);
            btn.TextColor = Color.White;
            btn.PrimaryColor = color;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Margin = new Padding(S(6), 0, 0, 0);
        }

        private void BuildGrid()
        {
            _gridPreview.Dock = DockStyle.Fill;
            _gridPreview.AutoGenerateColumns = false;
            _gridPreview.AllowUserToAddRows = false;
            _gridPreview.AllowUserToDeleteRows = false;
            _gridPreview.ReadOnly = true;
            _gridPreview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridPreview.RowHeadersVisible = false;
            _gridPreview.BackgroundColor = Color.White;
            _gridPreview.BorderStyle = BorderStyle.None;
            _gridPreview.RowTemplate.Height = S(36);
            _gridPreview.ColumnHeadersHeight = S(40);
            _gridPreview.EnableHeadersVisualStyles = false;

            _gridPreview.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            _gridPreview.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            _gridPreview.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }

        private async Task LoadPayrollsListAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();

                _availablePayrolls = (await payrollService.GetPayrollsAsync(DateTime.Now.Year)).ToList();
                _cboPayroll.Items.Clear();
                foreach (var p in _availablePayrolls.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month))
                {
                    _cboPayroll.Items.Add($"{p.PeriodText} ({p.TotalEmployees} NV - {p.TotalNetAmount:N0} đ)");
                }

                if (_cboPayroll.Items.Count > 0)
                {
                    _cboPayroll.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading payroll list: {ex.Message}");
            }
        }

        private async Task LoadPreviewDataAsync()
        {
            int reportIndex = _cboReportType.SelectedIndex;

            try
            {
                Cursor = Cursors.WaitCursor;
                using var scope = _scopeFactory.CreateScope();

                switch (reportIndex)
                {
                    case 0: // Nhân sự
                        await LoadEmployeePreviewAsync(scope);
                        break;
                    case 1: // Chấm công
                        await LoadAttendancePreviewAsync(scope);
                        break;
                    case 2: // Bảng lương
                        await LoadPayrollPreviewAsync(scope);
                        break;
                    case 3: // Kinh doanh
                        await LoadBusinessPreviewAsync(scope);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải bản xem trước: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task LoadEmployeePreviewAsync(IServiceScope scope)
        {
            var empService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
            var all = (await empService.GetAllAsync()).ToList();

            if (_cboStatus.SelectedIndex == 1)
                all = all.Where(e => e.Status == EmployeeStatus.Active).ToList();
            else if (_cboStatus.SelectedIndex == 2)
                all = all.Where(e => e.Status == EmployeeStatus.OnLeave).ToList();
            else if (_cboStatus.SelectedIndex == 3)
                all = all.Where(e => e.Status == EmployeeStatus.Resigned).ToList();

            _cachedEmployees = all;

            _gridPreview.Columns.Clear();
            _gridPreview.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã NV", Width = S(90) },
                new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Họ và tên", Width = S(180) },
                new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Vị trí", Width = S(130) },
                new DataGridViewTextBoxColumn { Name = "Phone", HeaderText = "SĐT", Width = S(110) },
                new DataGridViewTextBoxColumn { Name = "BaseSalary", HeaderText = "Lương cơ bản", Width = S(130) },
                new DataGridViewTextBoxColumn { Name = "Allowance", HeaderText = "Phụ cấp", Width = S(110) },
                new DataGridViewTextBoxColumn { Name = "HireDate", HeaderText = "Ngày vào làm", Width = S(110) },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });
            _gridPreview.Columns["BaseSalary"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _gridPreview.Columns["Allowance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            _gridPreview.Rows.Clear();
            foreach (var emp in all)
            {
                _gridPreview.Rows.Add(
                    emp.EmployeeCode,
                    emp.FullName,
                    emp.Position,
                    emp.PhoneNumber ?? "—",
                    $"{emp.BaseSalary:N0} đ",
                    $"{emp.Allowance:N0} đ",
                    emp.HireDate.ToString("dd/MM/yyyy"),
                    emp.StatusText
                );
            }

            _lblRecordCount.Text = $"Tổng số: {all.Count} nhân viên | Quỹ lương cơ bản: {all.Sum(e => e.BaseSalary):N0} đ";
        }

        private async Task LoadAttendancePreviewAsync(IServiceScope scope)
        {
            var attService = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
            var records = (await attService.GetHistoryAsync(_dtpFrom.Value.Date, _dtpTo.Value.Date)).ToList();

            _cachedAttendances = records;

            _gridPreview.Columns.Clear();
            _gridPreview.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Ngày", Width = S(100) },
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã NV", Width = S(90) },
                new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Họ và tên", Width = S(170) },
                new DataGridViewTextBoxColumn { Name = "CheckIn", HeaderText = "Giờ vào", Width = S(90) },
                new DataGridViewTextBoxColumn { Name = "CheckOut", HeaderText = "Giờ ra", Width = S(90) },
                new DataGridViewTextBoxColumn { Name = "Hours", HeaderText = "Giờ làm", Width = S(85) },
                new DataGridViewTextBoxColumn { Name = "OT", HeaderText = "Tăng ca", Width = S(85) },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái", Width = S(120) },
                new DataGridViewTextBoxColumn { Name = "Note", HeaderText = "Ghi chú", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });

            _gridPreview.Rows.Clear();
            foreach (var r in records.OrderByDescending(x => x.Date).ThenBy(x => x.EmployeeCode))
            {
                _gridPreview.Rows.Add(
                    r.Date.ToString("dd/MM/yyyy"),
                    r.EmployeeCode,
                    r.EmployeeName,
                    r.CheckIn.HasValue ? r.CheckIn.Value.ToString(@"hh\:mm") : "—",
                    r.CheckOut.HasValue ? r.CheckOut.Value.ToString(@"hh\:mm") : "—",
                    $"{r.HoursWorked:F1}h",
                    $"{r.OvertimeHours:F1}h",
                    r.StatusText,
                    r.Note ?? ""
                );
            }

            _lblRecordCount.Text = $"Tổng số: {records.Count} lượt chấm công | Đi muộn/Về sớm: {records.Count(r => r.Status == AttendanceStatus.Late || r.Status == AttendanceStatus.EarlyLeave)}";
        }

        private async Task LoadPayrollPreviewAsync(IServiceScope scope)
        {
            if (_availablePayrolls.Count == 0 || _cboPayroll.SelectedIndex < 0)
            {
                _gridPreview.Rows.Clear();
                _lblRecordCount.Text = "Không có kỳ lương nào để xem.";
                return;
            }

            var selectedSummary = _availablePayrolls[_cboPayroll.SelectedIndex];
            _cachedPayrollSummary = selectedSummary;

            var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();
            _cachedPayrollDetails = (await payrollService.GetPayrollDetailsAsync(selectedSummary.Id)).ToList();

            _gridPreview.Columns.Clear();
            _gridPreview.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã NV", Width = S(90) },
                new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Họ và tên", Width = S(170) },
                new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Chức vụ", Width = S(120) },
                new DataGridViewTextBoxColumn { Name = "DaysWorked", HeaderText = "Công thực", Width = S(85) },
                new DataGridViewTextBoxColumn { Name = "OTHours", HeaderText = "Tăng ca (h)", Width = S(85) },
                new DataGridViewTextBoxColumn { Name = "GrossSalary", HeaderText = "Lương gộp", Width = S(130) },
                new DataGridViewTextBoxColumn { Name = "Deductions", HeaderText = "Giảm trừ", Width = S(120) },
                new DataGridViewTextBoxColumn { Name = "NetSalary", HeaderText = "Thực lĩnh", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });

            _gridPreview.Columns["GrossSalary"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _gridPreview.Columns["Deductions"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _gridPreview.Columns["NetSalary"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _gridPreview.Columns["NetSalary"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            _gridPreview.Rows.Clear();
            foreach (var d in _cachedPayrollDetails)
            {
                decimal gross = d.SalaryByWorkDays + d.OvertimePay + d.Allowance + d.Bonus;
                _gridPreview.Rows.Add(
                    d.EmployeeCode,
                    d.EmployeeName,
                    d.Position,
                    d.ActualWorkDays.ToString("F1"),
                    d.OvertimeHours.ToString("F1"),
                    $"{gross:N0} đ",
                    $"{d.Deductions:N0} đ",
                    $"{d.NetSalary:N0} đ"
                );
            }

            _lblRecordCount.Text = $"{_cachedPayrollSummary.PeriodText} | {_cachedPayrollDetails.Count} nhân viên | Tổng thực lĩnh: {_cachedPayrollSummary.TotalNetAmount:N0} đ";
        }

        private async Task LoadBusinessPreviewAsync(IServiceScope scope)
        {
            var dashService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
            _cachedDashboard = await dashService.GetSummaryAsync();
            _cachedTopProducts = (await dashService.GetTopSellingProductsAsync(_dtpFrom.Value.Date, _dtpTo.Value.Date, 5)).ToList();

            _gridPreview.Columns.Clear();
            _gridPreview.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Section", HeaderText = "Chỉ số / Hạng mục", Width = S(260) },
                new DataGridViewTextBoxColumn { Name = "Value", HeaderText = "Giá trị / Số lượng", Width = S(200) },
                new DataGridViewTextBoxColumn { Name = "Note", HeaderText = "Ghi chú & Diễn giải", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });

            _gridPreview.Rows.Clear();
            _gridPreview.Rows.Add("1. Doanh thu thuần", $"{_cachedDashboard.TotalRevenue:N0} đ", "Đã trừ các đơn hoàn trả");
            _gridPreview.Rows.Add("2. Số đơn hàng thành công", $"{_cachedDashboard.CompletedOrders:N0} đơn", $"Tổng đơn: {_cachedDashboard.TotalOrders:N0}");
            _gridPreview.Rows.Add("3. Nhân sự đang hoạt động", $"{_cachedDashboard.ActiveEmployees} NV", "Nhân sự làm việc");
            _gridPreview.Rows.Add("4. Tổng số lượng tồn kho", $"{_cachedDashboard.TotalStockQuantity:N0} linh kiện", $"Sắp hết: {_cachedDashboard.LowStockProductsCount} SP");
            _gridPreview.Rows.Add("5. Chi phí quỹ lương tháng", $"{_cachedDashboard.MonthlyPayrollExpense:N0} đ", "Tháng hiện hành");

            // Add Top products
            _gridPreview.Rows.Add("--- TOP SẢN PHẨM BÁN CHẠY ---", "", "");
            foreach (var p in _cachedTopProducts)
            {
                _gridPreview.Rows.Add($"   • {p.ProductName}", $"{p.QuantitySold} chiếc", $"{p.TotalRevenue:N0} đ");
            }

            _lblRecordCount.Text = $"Báo cáo kinh doanh từ {_dtpFrom.Value:dd/MM/yyyy} đến {_dtpTo.Value:dd/MM/yyyy} | Doanh thu: {_cachedDashboard.TotalRevenue:N0} đ";
        }

        private async Task ExportExcelAsync()
        {
            int reportIndex = _cboReportType.SelectedIndex;

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                Title = "Xuất Báo Cáo Ra Excel"
            };

            switch (reportIndex)
            {
                case 0:
                    sfd.FileName = $"BaoCao_NhanSu_{DateTime.Now:yyyyMMdd}.xlsx";
                    break;
                case 1:
                    sfd.FileName = $"BaoCao_ChamCong_{_dtpFrom.Value:yyyyMMdd}_{_dtpTo.Value:yyyyMMdd}.xlsx";
                    break;
                case 2:
                    sfd.FileName = $"BaoCao_BangLuong_{DateTime.Now:yyyyMMdd}.xlsx";
                    break;
                case 3:
                    sfd.FileName = $"BaoCao_KinhDoanh_{_dtpFrom.Value:yyyyMMdd}_{_dtpTo.Value:yyyyMMdd}.xlsx";
                    break;
            }

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    using var scope = _scopeFactory.CreateScope();
                    var exportService = scope.ServiceProvider.GetRequiredService<IReportExportService>();

                    switch (reportIndex)
                    {
                        case 0:
                            if (_cachedEmployees == null) await LoadEmployeePreviewAsync(scope);
                            exportService.ExportEmployeeListToExcel(sfd.FileName, _cachedEmployees!);
                            break;
                        case 1:
                            if (_cachedAttendances == null) await LoadAttendancePreviewAsync(scope);
                            exportService.ExportAttendanceReportToExcel(sfd.FileName, _cachedAttendances!, _dtpFrom.Value.Date, _dtpTo.Value.Date);
                            break;
                        case 2:
                            if (_cachedPayrollSummary == null || _cachedPayrollDetails == null)
                                await LoadPayrollPreviewAsync(scope);
                            if (_cachedPayrollSummary == null || _cachedPayrollDetails == null)
                                throw new InvalidOperationException("Vui lòng chọn kỳ bảng lương!");
                            exportService.ExportPayrollReportToExcel(sfd.FileName, _cachedPayrollSummary, _cachedPayrollDetails);
                            break;
                        case 3:
                            if (_cachedDashboard == null || _cachedTopProducts == null)
                                await LoadBusinessPreviewAsync(scope);
                            exportService.ExportBusinessReportToExcel(sfd.FileName, _cachedDashboard!, _cachedTopProducts!, _dtpFrom.Value.Date, _dtpTo.Value.Date);
                            break;
                    }

                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private async Task ExportPdfAsync()
        {
            int reportIndex = _cboReportType.SelectedIndex;

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                Title = "Xuất Báo Cáo Ra PDF"
            };

            switch (reportIndex)
            {
                case 0:
                    sfd.FileName = $"BaoCao_NhanSu_{DateTime.Now:yyyyMMdd}.pdf";
                    break;
                case 1:
                    sfd.FileName = $"BaoCao_ChamCong_{_dtpFrom.Value:yyyyMMdd}_{_dtpTo.Value:yyyyMMdd}.pdf";
                    break;
                case 2:
                    sfd.FileName = $"BaoCao_BangLuong_{DateTime.Now:yyyyMMdd}.pdf";
                    break;
                case 3:
                    sfd.FileName = $"BaoCao_KinhDoanh_{_dtpFrom.Value:yyyyMMdd}_{_dtpTo.Value:yyyyMMdd}.pdf";
                    break;
            }

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    using var scope = _scopeFactory.CreateScope();
                    var exportService = scope.ServiceProvider.GetRequiredService<IReportExportService>();

                    switch (reportIndex)
                    {
                        case 0:
                            if (_cachedEmployees == null) await LoadEmployeePreviewAsync(scope);
                            exportService.ExportEmployeeListToPdf(sfd.FileName, _cachedEmployees!);
                            break;
                        case 1:
                            if (_cachedAttendances == null) await LoadAttendancePreviewAsync(scope);
                            exportService.ExportAttendanceReportToPdf(sfd.FileName, _cachedAttendances!, _dtpFrom.Value.Date, _dtpTo.Value.Date);
                            break;
                        case 2:
                            if (_cachedPayrollSummary == null || _cachedPayrollDetails == null)
                                await LoadPayrollPreviewAsync(scope);
                            if (_cachedPayrollSummary == null || _cachedPayrollDetails == null)
                                throw new InvalidOperationException("Vui lòng chọn kỳ bảng lương!");
                            exportService.ExportPayrollReportToPdf(sfd.FileName, _cachedPayrollSummary, _cachedPayrollDetails);
                            break;
                        case 3:
                            if (_cachedDashboard == null || _cachedTopProducts == null)
                                await LoadBusinessPreviewAsync(scope);
                            exportService.ExportBusinessReportToPdf(sfd.FileName, _cachedDashboard!, _cachedTopProducts!, _dtpFrom.Value.Date, _dtpTo.Value.Date);
                            break;
                    }

                    MessageBox.Show("Xuất file PDF thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xuất PDF: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }
    }
}
