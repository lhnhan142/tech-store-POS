using BLL.DTOs.Payroll;
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

namespace GUI.Forms.HumanResource
{
    [DesignerCategory("Code")]
    public class PayrollManagementForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // KPI Cards
        private readonly Label _lblTotalAnnualExpense = new();
        private readonly Label _lblLatestPayrollExpense = new();
        private readonly Label _lblTotalPayrolls = new();
        private readonly Label _lblDraftPayrolls = new();

        // Filters & Controls
        private readonly ComboBox _cboYear = new();
        private readonly ComboBox _cboStatus = new();
        private readonly DataGridView _grid = new();

        // Buttons
        private readonly HopeRoundButton _btnCreatePayroll = new();
        private readonly HopeRoundButton _btnRecalculate = new();
        private readonly HopeRoundButton _btnFinalize = new();
        private readonly HopeRoundButton _btnViewDetail = new();
        private readonly HopeRoundButton _btnExportExcel = new();
        private readonly HopeRoundButton _btnExportPdf = new();
        private readonly HopeRoundButton _btnDelete = new();
        private readonly HopeRoundButton _btnRefresh = new();

        private List<PayrollSummaryDto> _payrolls = new();

        public PayrollManagementForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Quản lý Bảng lương Nhân viên";
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.FromArgb(243, 244, 246);

            BuildUi();
            Load += async (_, _) => await LoadDataAsync();
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
                Text = "QUẢN LÝ BẢNG LƯƠNG NHÂN VIÊN",
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = AppTheme.Primary,
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(S(20), S(10))
            };

            var lblSub = new Label
            {
                Text = "Lập bảng lương hàng tháng, tính lương tự động từ dữ liệu chấm công, thưởng/phạt và chốt lương",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(107, 114, 128),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(S(20), S(38))
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

            // 2. KPI Cards Panel
            var pnlKpi = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = S(90),
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(S(20), S(10), S(20), S(8)),
                BackColor = Color.Transparent
            };
            pnlKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            pnlKpi.Controls.Add(CreateKpiCard("TỔNG CHI LƯƠNG NĂM", _lblTotalAnnualExpense, "0 đ", AppTheme.Primary), 0, 0);
            pnlKpi.Controls.Add(CreateKpiCard("KỲ LƯƠNG GẦN NHẤT", _lblLatestPayrollExpense, "0 đ", Color.FromArgb(16, 185, 129)), 1, 0);
            pnlKpi.Controls.Add(CreateKpiCard("TỔNG SỐ KỲ LƯƠNG", _lblTotalPayrolls, "0 kỳ", Color.FromArgb(99, 102, 241)), 2, 0);
            pnlKpi.Controls.Add(CreateKpiCard("BẢN NHÁP CHƯA CHỐT", _lblDraftPayrolls, "0 kỳ", Color.FromArgb(245, 158, 11)), 3, 0);

            // 3. Toolbar & Actions
            var pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(58),
                BackColor = Color.White,
                Padding = new Padding(S(20), S(10), S(20), S(10))
            };
            pnlToolbar.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(229, 231, 235), 1);
                e.Graphics.DrawLine(pen, 0, pnlToolbar.Height - 1, pnlToolbar.Width, pnlToolbar.Height - 1);
            };

            var lblYear = new Label
            {
                Text = "Năm:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(S(20), S(18)),
                ForeColor = Color.FromArgb(55, 65, 81)
            };

            _cboYear.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboYear.Font = new Font("Segoe UI", 9.5F);
            _cboYear.Size = new Size(S(90), S(32));
            _cboYear.Location = new Point(S(65), S(14));
            int currentYear = DateTime.Now.Year;
            for (int y = currentYear - 3; y <= currentYear + 1; y++)
            {
                _cboYear.Items.Add(y);
            }
            _cboYear.SelectedItem = currentYear;
            _cboYear.SelectedIndexChanged += async (_, _) => await LoadDataAsync();

            var lblStatus = new Label
            {
                Text = "Trạng thái:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(S(170), S(18)),
                ForeColor = Color.FromArgb(55, 65, 81)
            };

            _cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboStatus.Font = new Font("Segoe UI", 9.5F);
            _cboStatus.Size = new Size(S(130), S(32));
            _cboStatus.Location = new Point(S(245), S(14));
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Bản nháp", "Đã chốt" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (_, _) => FilterData();

            // Action Buttons
            ConfigureButton(_btnCreatePayroll, "+ Lập bảng lương", AppTheme.Primary, S(125), S(36));
            _btnCreatePayroll.Margin = new Padding(S(3), 0, 0, 0);
            _btnCreatePayroll.Click += async (_, _) => await PromptCreatePayrollAsync();

            ConfigureButton(_btnRecalculate, "Tính lại công & lương", Color.FromArgb(79, 70, 229), S(135), S(36));
            _btnRecalculate.Margin = new Padding(S(3), 0, 0, 0);
            _btnRecalculate.Click += async (_, _) => await RecalculatePayrollAsync();

            ConfigureButton(_btnFinalize, "Chốt bảng lương", Color.FromArgb(16, 185, 129), S(115), S(36));
            _btnFinalize.Margin = new Padding(S(3), 0, 0, 0);
            _btnFinalize.Click += async (_, _) => await FinalizePayrollAsync();

            ConfigureButton(_btnViewDetail, "Xem chi tiết", Color.FromArgb(59, 130, 246), S(95), S(36));
            _btnViewDetail.Margin = new Padding(S(3), 0, 0, 0);
            _btnViewDetail.Click += (_, _) => ViewPayrollDetail();

            ConfigureButton(_btnExportExcel, "Xuất Excel", Color.FromArgb(34, 197, 94), S(85), S(36));
            _btnExportExcel.Margin = new Padding(S(3), 0, 0, 0);
            _btnExportExcel.Click += async (_, _) => await ExportExcelAsync();

            ConfigureButton(_btnExportPdf, "Xuất PDF", Color.FromArgb(239, 68, 68), S(80), S(36));
            _btnExportPdf.Margin = new Padding(S(3), 0, 0, 0);
            _btnExportPdf.Click += async (_, _) => await ExportPdfAsync();

            ConfigureButton(_btnDelete, "Xóa nháp", Color.FromArgb(156, 163, 175), S(75), S(36));
            _btnDelete.Margin = new Padding(S(3), 0, 0, 0);
            _btnDelete.Click += async (_, _) => await DeletePayrollAsync();

            ConfigureButton(_btnRefresh, "Làm mới", Color.FromArgb(107, 114, 128), S(75), S(36));
            _btnRefresh.Margin = new Padding(S(3), 0, 0, 0);
            _btnRefresh.Click += async (_, _) => await LoadDataAsync();

            // Right-aligned flow for buttons
            var pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, S(8), 0, 0),
                BackColor = Color.Transparent
            };
            pnlButtons.Controls.AddRange(new Control[]
            {
                _btnCreatePayroll,
                _btnRecalculate,
                _btnFinalize,
                _btnViewDetail,
                _btnExportExcel,
                _btnExportPdf,
                _btnDelete,
                _btnRefresh
            });

            pnlToolbar.Controls.AddRange(new Control[] { pnlButtons, lblYear, _cboYear, lblStatus, _cboStatus });

            // 4. DataGridView
            BuildGrid();

            var pnlGridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(S(20), S(12), S(20), S(16)),
                BackColor = Color.FromArgb(243, 244, 246)
            };
            pnlGridContainer.Controls.Add(_grid);

            Controls.Add(pnlGridContainer);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlKpi);
            Controls.Add(pnlHeader);
        }

        private Panel CreateKpiCard(string title, Label lblValue, string defaultValue, Color accent)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(S(6)),
                Padding = new Padding(S(14), S(10), S(14), S(10))
            };
            card.Paint += (s, e) =>
            {
                using var path = GetRoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), S(8));
                using var pen = new Pen(Color.FromArgb(229, 231, 235), 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);

                using var accentBrush = new SolidBrush(accent);
                e.Graphics.FillRectangle(accentBrush, 0, 0, S(5), card.Height);
            };

            var lblT = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(107, 114, 128),
                Dock = DockStyle.Top,
                Height = S(18)
            };

            lblValue.Text = defaultValue;
            lblValue.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            lblValue.ForeColor = accent;
            lblValue.Dock = DockStyle.Fill;
            lblValue.TextAlign = ContentAlignment.MiddleLeft;

            card.Controls.Add(lblValue);
            card.Controls.Add(lblT);
            return card;
        }

        private void ConfigureButton(HopeRoundButton btn, string text, Color color, int width, int height)
        {
            btn.Text = text;
            btn.Size = new Size(width, height);
            btn.TextColor = Color.White;
            btn.PrimaryColor = color;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Margin = new Padding(S(3), 0, S(3), 0);
        }

        private void BuildGrid()
        {
            _grid.Dock = DockStyle.Fill;
            _grid.AutoGenerateColumns = false;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.ReadOnly = true;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.RowHeadersVisible = false;
            _grid.BackgroundColor = Color.White;
            _grid.BorderStyle = BorderStyle.None;
            _grid.RowTemplate.Height = S(40);
            _grid.ColumnHeadersHeight = S(42);
            _grid.EnableHeadersVisualStyles = false;

            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            _grid.DefaultCellStyle.ForeColor = Color.FromArgb(31, 41, 55);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);

            _grid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "#", Width = S(50), Visible = false },
                new DataGridViewTextBoxColumn { Name = "Period", HeaderText = "Kỳ lương", Width = S(130) },
                new DataGridViewTextBoxColumn { Name = "TotalEmployees", HeaderText = "Số nhân viên", Width = S(110) },
                new DataGridViewTextBoxColumn { Name = "TotalGrossSalary", HeaderText = "Tổng lương gộp", Width = S(140) },
                new DataGridViewTextBoxColumn { Name = "TotalDeductions", HeaderText = "Tổng giảm trừ", Width = S(130) },
                new DataGridViewTextBoxColumn { Name = "TotalNetSalary", HeaderText = "Thực lĩnh", Width = S(150) },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái", Width = S(120) },
                new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Ngày lập", Width = S(120) },
                new DataGridViewTextBoxColumn { Name = "FinalizedAt", HeaderText = "Ngày chốt", Width = S(120) },
                new DataGridViewTextBoxColumn { Name = "Note", HeaderText = "Ghi chú", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });

            _grid.Columns["TotalEmployees"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _grid.Columns["TotalGrossSalary"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _grid.Columns["TotalDeductions"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _grid.Columns["TotalNetSalary"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _grid.Columns["TotalNetSalary"].DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            _grid.CellPainting += Grid_CellPainting;
            _grid.CellDoubleClick += (_, _) => ViewPayrollDetail();
            _grid.SelectionChanged += (_, _) => UpdateButtonStates();
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_grid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);

                string statusText = e.Value.ToString() ?? "";
                bool isFinalized = statusText.Contains("Đã chốt");

                Color pillBg = isFinalized ? Color.FromArgb(209, 250, 229) : Color.FromArgb(254, 243, 199);
                Color pillText = isFinalized ? Color.FromArgb(6, 95, 70) : Color.FromArgb(146, 64, 14);

                var badgeRect = new Rectangle(e.CellBounds.X + S(8), e.CellBounds.Y + S(8), e.CellBounds.Width - S(16), e.CellBounds.Height - S(16));
                using var path = GetRoundedRect(badgeRect, S(6));
                using var bgBrush = new SolidBrush(pillBg);
                using var textBrush = new SolidBrush(pillText);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(bgBrush, path);

                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using var font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                e.Graphics.DrawString(statusText, font, textBrush, badgeRect, sf);

                e.Handled = true;
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                using var scope = _scopeFactory.CreateScope();
                var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();

                int selectedYear = _cboYear.SelectedItem is int y ? y : DateTime.Now.Year;
                _payrolls = (await payrollService.GetPayrollsAsync(selectedYear)).ToList();

                UpdateKpis();
                FilterData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách bảng lương: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateKpis()
        {
            decimal totalAnnual = _payrolls.Sum(p => p.TotalNetAmount);
            var latest = _payrolls.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).FirstOrDefault();
            int totalCount = _payrolls.Count;
            int draftCount = _payrolls.Count(p => p.Status == PayrollStatus.Draft);

            _lblTotalAnnualExpense.Text = $"{totalAnnual:N0} đ";
            _lblLatestPayrollExpense.Text = latest != null ? $"{latest.TotalNetAmount:N0} đ" : "0 đ";
            _lblTotalPayrolls.Text = $"{totalCount} kỳ";
            _lblDraftPayrolls.Text = $"{draftCount} kỳ";

            if (draftCount > 0)
            {
                _lblDraftPayrolls.ForeColor = Color.FromArgb(220, 38, 38);
            }
            else
            {
                _lblDraftPayrolls.ForeColor = Color.FromArgb(16, 185, 129);
            }
        }

        private void FilterData()
        {
            var list = _payrolls.AsEnumerable();

            if (_cboStatus.SelectedIndex == 1) // Bản nháp
                list = list.Where(p => p.Status == PayrollStatus.Draft);
            else if (_cboStatus.SelectedIndex == 2) // Đã chốt
                list = list.Where(p => p.Status == PayrollStatus.Finalized);

            var filtered = list.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToList();

            _grid.Rows.Clear();
            foreach (var item in filtered)
            {
                decimal totalDeduct = item.TotalGrossAmount - item.TotalNetAmount;
                int rowIndex = _grid.Rows.Add(
                    item.Id,
                    item.PeriodText,
                    $"{item.TotalEmployees} người",
                    $"{item.TotalGrossAmount:N0} đ",
                    $"{totalDeduct:N0} đ",
                    $"{item.TotalNetAmount:N0} đ",
                    item.Status == PayrollStatus.Finalized ? "Đã chốt" : "Bản nháp",
                    item.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    item.FinalizedAt?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                    item.Note ?? ""
                );

                _grid.Rows[rowIndex].Tag = item;
            }

            UpdateButtonStates();
        }

        private PayrollSummaryDto? GetSelectedPayroll()
        {
            if (_grid.CurrentRow?.Tag is PayrollSummaryDto payroll)
                return payroll;
            return null;
        }

        private void UpdateButtonStates()
        {
            var selected = GetSelectedPayroll();
            bool hasSelection = selected != null;
            bool isDraft = selected?.Status == PayrollStatus.Draft;

            _btnRecalculate.Enabled = hasSelection && isDraft;
            _btnFinalize.Enabled = hasSelection && isDraft;
            _btnDelete.Enabled = hasSelection && isDraft;
            _btnViewDetail.Enabled = hasSelection;
            _btnExportExcel.Enabled = hasSelection;
            _btnExportPdf.Enabled = hasSelection;
        }

        private async Task PromptCreatePayrollAsync()
        {
            using var dlg = new Form
            {
                Text = "Tạo Bảng Lương Mới",
                Font = new Font("Segoe UI", 9.5F),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(S(420), S(310)),
                BackColor = Color.White
            };

            var lblT = new Label
            {
                Text = "THIẾT LẬP KỲ LƯƠNG",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = AppTheme.Primary,
                Location = new Point(S(24), S(16)),
                AutoSize = true
            };

            var lblMonth = new Label { Text = "Tháng:", Location = new Point(S(24), S(58)), AutoSize = true };
            var numMonth = new NumericUpDown
            {
                Location = new Point(S(24), S(82)),
                Size = new Size(S(170), S(30)),
                Minimum = 1,
                Maximum = 12,
                Value = DateTime.Now.Month
            };

            var lblYear = new Label { Text = "Năm:", Location = new Point(S(210), S(58)), AutoSize = true };
            var numYear = new NumericUpDown
            {
                Location = new Point(S(210), S(82)),
                Size = new Size(S(170), S(30)),
                Minimum = 2020,
                Maximum = 2030,
                Value = DateTime.Now.Year
            };

            var lblStandardDays = new Label { Text = "Số ngày công chuẩn:", Location = new Point(S(24), S(125)), AutoSize = true };
            var numStandardDays = new NumericUpDown
            {
                Location = new Point(S(24), S(149)),
                Size = new Size(S(356), S(30)),
                Minimum = 20,
                Maximum = 31,
                Value = 26
            };

            var lblNote = new Label { Text = "Ghi chú kỳ lương:", Location = new Point(S(24), S(190)), AutoSize = true };
            var txtNote = new TextBox
            {
                Location = new Point(S(24), S(214)),
                Size = new Size(S(356), S(30)),
                Text = $"Bảng lương tháng {DateTime.Now.Month}/{DateTime.Now.Year}"
            };

            var btnSubmit = new HopeRoundButton
            {
                Text = "Tạo Bảng Lương",
                Size = new Size(S(150), S(38)),
                Location = new Point(S(230), S(258)),
                TextColor = Color.White,
                PrimaryColor = AppTheme.Primary,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            var btnCancel = new HopeRoundButton
            {
                Text = "Hủy bỏ",
                Size = new Size(S(100), S(38)),
                Location = new Point(S(120), S(258)),
                TextColor = Color.White,
                PrimaryColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnCancel.Click += (_, _) => dlg.DialogResult = DialogResult.Cancel;
            btnSubmit.Click += (_, _) =>
            {
                dlg.DialogResult = DialogResult.OK;
            };

            dlg.Controls.AddRange(new Control[] { lblT, lblMonth, numMonth, lblYear, numYear, lblStandardDays, numStandardDays, lblNote, txtNote, btnSubmit, btnCancel });

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    using var scope = _scopeFactory.CreateScope();
                    var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();

                    var request = new GeneratePayrollRequestDto
                    {
                        Month = (int)numMonth.Value,
                        Year = (int)numYear.Value,
                        StandardWorkDays = (int)numStandardDays.Value,
                        Note = txtNote.Text.Trim()
                    };

                    int currentUserId = AppSession.CurrentUserId ?? 1;
                    await payrollService.GeneratePayrollAsync(request, currentUserId);
                    MessageBox.Show($"Đã tạo thành công bảng lương Tháng {request.Month}/{request.Year}!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tạo bảng lương: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private async Task RecalculatePayrollAsync()
        {
            var selected = GetSelectedPayroll();
            if (selected == null) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn tính toán lại công và lương cho kỳ lương {selected.PeriodText}?\n(Dữ liệu ngày công và lương tạm tính sẽ được đồng bộ từ chấm công mới nhất)",
                "Xác nhận tính lại",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                using var scope = _scopeFactory.CreateScope();
                var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();

                await payrollService.RecalculatePayrollAsync(selected.Id);
                MessageBox.Show("Đã tính toán lại công và lương thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tính lại bảng lương: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task FinalizePayrollAsync()
        {
            var selected = GetSelectedPayroll();
            if (selected == null) return;

            var confirm = MessageBox.Show(
                $"CHÚ Ý: Sau khi chốt, bảng lương {selected.PeriodText} sẽ bị KHÓA hoàn toàn và không thể điều chỉnh hoặc xóa.\n\nBạn có chắc chắn muốn CHỐT kỳ lương này?",
                "Xác nhận chốt bảng lương",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                using var scope = _scopeFactory.CreateScope();
                var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();

                int currentUserId = AppSession.CurrentUserId ?? 1;
                await payrollService.FinalizePayrollAsync(selected.Id, currentUserId);
                MessageBox.Show("Bảng lương đã được chốt thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chốt bảng lương: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task DeletePayrollAsync()
        {
            var selected = GetSelectedPayroll();
            if (selected == null) return;

            if (selected.Status == PayrollStatus.Finalized)
            {
                MessageBox.Show("Không thể xóa bảng lương đã chốt!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa bản nháp bảng lương {selected.PeriodText}?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                using var scope = _scopeFactory.CreateScope();
                var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();

                await payrollService.DeletePayrollAsync(selected.Id);
                MessageBox.Show("Đã xóa bản nháp bảng lương thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa bảng lương: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ViewPayrollDetail()
        {
            var selected = GetSelectedPayroll();
            if (selected == null) return;

            using var dlg = new PayrollDetailForm(_scopeFactory, selected.Id);
            dlg.ShowDialog(this);
            _ = LoadDataAsync();
        }

        private async Task ExportExcelAsync()
        {
            var selected = GetSelectedPayroll();
            if (selected == null) return;

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"BangLuong_Thang_{selected.Month:D2}_{selected.Year}.xlsx",
                Title = "Xuất Bảng Lương Ra Excel"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    using var scope = _scopeFactory.CreateScope();
                    var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();
                    var exportService = scope.ServiceProvider.GetRequiredService<IReportExportService>();

                    var details = await payrollService.GetPayrollDetailsAsync(selected.Id);
                    exportService.ExportPayrollReportToExcel(sfd.FileName, selected, details);

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
            var selected = GetSelectedPayroll();
            if (selected == null) return;

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"BangLuong_Thang_{selected.Month:D2}_{selected.Year}.pdf",
                Title = "Xuất Bảng Lương Ra PDF"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    using var scope = _scopeFactory.CreateScope();
                    var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();
                    var exportService = scope.ServiceProvider.GetRequiredService<IReportExportService>();

                    var details = await payrollService.GetPayrollDetailsAsync(selected.Id);
                    exportService.ExportPayrollReportToPdf(sfd.FileName, selected, details);

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
