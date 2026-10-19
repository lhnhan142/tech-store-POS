using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.BLL.Services.Interfaces;
using PhuKienMayTinh.Common.Enums;
using PhuKienMayTinh.GUI.Controls;
using PhuKienMayTinh.GUI.Forms;

namespace PhuKienMayTinh.GUI.UserControls
{
    public class PayrollUC : UserControl
    {
        private readonly IPayrollService _payrollService;

        // Current loaded Payroll
        private PayrollDTO? _currentPayroll;

        // Filter / Controls
        private ComboBox cboMonth = null!;
        private NumericUpDown numYear = null!;
        private Button btnCalculate = null!;
        private Button btnApprove = null!;
        private Button btnPay = null!;
        private Button btnExportExcel = null!;
        private Button btnViewPayslip = null!;

        // Stat Cards
        private StatCardControl cardBase = null!;
        private StatCardControl cardAllowance = null!;
        private StatCardControl cardBonus = null!;
        private StatCardControl cardNet = null!;

        // DataGridView
        private DataGridView dgvPayroll = null!;
        private Label lblStatusBadge = null!;
        private Label lblSummary = null!;

        public PayrollUC(IPayrollService payrollService)
        {
            _payrollService = payrollService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 9f);
            AutoScroll = true;

            // 1. Top Panel: Thao tác & Bộ chọn Tháng/Năm (Slate 800)
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20, 12, 20, 12)
            };

            var lblTitle = new Label
            {
                Text = "💰 QUẢN LÝ TÍNH LƯƠNG & BẢNG LƯƠNG NHÂN SỰ",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 250, 252),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 18)
            };
            pnlTop.Controls.Add(lblTitle);

            // Month / Year selector & Actions góc phải
            var pnlControls = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 920,
                Height = 45,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            var lblMonth = new Label { Text = "Tháng:", AutoSize = true, Margin = new Padding(4, 9, 2, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            cboMonth = new ComboBox { Width = 95, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f), Margin = new Padding(2, 4, 6, 0) };
            for (int m = 1; m <= 12; m++) cboMonth.Items.Add($"Tháng {m:D2}");
            cboMonth.SelectedIndex = DateTime.Today.Month - 1;
            cboMonth.SelectedIndexChanged += async (s, e) => await LoadPayrollDataAsync();

            var lblYear = new Label { Text = "Năm:", AutoSize = true, Margin = new Padding(4, 9, 2, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            numYear = new NumericUpDown { Width = 70, Height = 32, Minimum = 2020, Maximum = 2030, Value = DateTime.Today.Year, Font = new Font("Segoe UI", 9f), Margin = new Padding(2, 4, 10, 0) };
            numYear.ValueChanged += async (s, e) => await LoadPayrollDataAsync();

            btnCalculate = CreateBtn("⚡ Tính lương", Color.FromArgb(37, 99, 235));
            btnCalculate.Click += async (s, e) => await CalculatePayrollAsync();

            btnApprove = CreateBtn("✔️ Duyệt bảng lương", Color.FromArgb(217, 119, 6));
            btnApprove.Click += async (s, e) => await ApprovePayrollAsync();

            btnPay = CreateBtn("💵 Thanh toán", Color.FromArgb(13, 148, 136));
            btnPay.Click += async (s, e) => await MarkPaidAsync();

            btnViewPayslip = CreateBtn("📄 Phiếu lương", Color.FromArgb(99, 102, 241));
            btnViewPayslip.Click += (s, e) => ViewSelectedPayslip();

            btnExportExcel = CreateBtn("📊 Xuất Excel", Color.FromArgb(16, 185, 129));
            btnExportExcel.Click += async (s, e) => await ExportExcelAsync();

            pnlControls.Controls.Add(lblMonth);
            pnlControls.Controls.Add(cboMonth);
            pnlControls.Controls.Add(lblYear);
            pnlControls.Controls.Add(numYear);
            pnlControls.Controls.Add(btnCalculate);
            pnlControls.Controls.Add(btnApprove);
            pnlControls.Controls.Add(btnPay);
            pnlControls.Controls.Add(btnViewPayslip);
            pnlControls.Controls.Add(btnExportExcel);

            pnlTop.Controls.Add(pnlControls);
            Controls.Add(pnlTop);

            // 2. Body Container (Slate 900)
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.FromArgb(15, 23, 42),
                AutoScroll = true
            };
            Controls.Add(pnlBody);
            pnlBody.BringToFront();

            // 3. Status Bar & Stat Cards
            var pnlStatusBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                Margin = new Padding(0, 0, 0, 10)
            };

            lblStatusBadge = new Label
            {
                Text = "TRẠNG THÁI: CHƯA CÓ BẢNG LƯƠNG",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(0, 6)
            };
            pnlStatusBar.Controls.Add(lblStatusBadge);
            pnlBody.Controls.Add(pnlStatusBar);

            var pnlCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 120,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 15)
            };

            cardBase = new StatCardControl { Width = 265, Height = 110, CardTitle = "Tổng lương cơ bản", CardValue = "0 đ", CardSubtitle = "Theo ngày công thực", AccentColor = Color.FromArgb(56, 189, 248), IconSymbol = "💼" };
            cardAllowance = new StatCardControl { Width = 265, Height = 110, CardTitle = "Tổng phụ cấp", CardValue = "0 đ", CardSubtitle = "Ăn trưa, xăng xe, trách nhiệm", AccentColor = Color.FromArgb(52, 211, 153), IconSymbol = "🍱" };
            cardBonus = new StatCardControl { Width = 265, Height = 110, CardTitle = "Tổng thưởng", CardValue = "0 đ", CardSubtitle = "Chuyên cần & KPI", AccentColor = Color.FromArgb(251, 191, 36), IconSymbol = "⭐" };
            cardNet = new StatCardControl { Width = 265, Height = 110, CardTitle = "TỔNG THỰC LĨNH (NET)", CardValue = "0 đ", CardSubtitle = "Quỹ lương cần chi trả", AccentColor = Color.FromArgb(168, 85, 247), IconSymbol = "💳" };

            pnlCards.Controls.Add(cardBase);
            pnlCards.Controls.Add(cardAllowance);
            pnlCards.Controls.Add(cardBonus);
            pnlCards.Controls.Add(cardNet);
            pnlBody.Controls.Add(pnlCards);

            // 4. DataGridView Bảng lương (Slate 800)
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 15, 0, 0)
            };

            dgvPayroll = new DataGridView
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

            dgvPayroll.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvPayroll.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
            dgvPayroll.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvPayroll.ColumnHeadersHeight = 38;
            dgvPayroll.DefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvPayroll.DefaultCellStyle.ForeColor = Color.FromArgb(241, 245, 249);
            dgvPayroll.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 58, 138);
            dgvPayroll.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Visible = false });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã NV", FillWeight = 55 });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Họ và Tên", FillWeight = 110 });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "Pos", HeaderText = "Chức vụ", FillWeight = 85 });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "StdDays", HeaderText = "Công chuẩn", FillWeight = 55, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActDays", HeaderText = "Công thực", FillWeight = 55, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "BaseSal", HeaderText = "Lương CB", FillWeight = 75, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActSal", HeaderText = "Lương theo công", FillWeight = 85, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "Allowance", HeaderText = "Phụ cấp", FillWeight = 70, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "Bonus", HeaderText = "Thưởng", FillWeight = 65, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "Deduct", HeaderText = "Khấu trừ", FillWeight = 65, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvPayroll.Columns.Add(new DataGridViewTextBoxColumn { Name = "NetSal", HeaderText = "THỰC LĨNH", FillWeight = 95, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9f, FontStyle.Bold) } });

            dgvPayroll.CellFormatting += DgvPayroll_CellFormatting;
            dgvPayroll.CellDoubleClick += (s, e) => ViewSelectedPayslip();

            pnlGrid.Controls.Add(dgvPayroll);

            lblSummary = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                Text = "Tổng số nhân viên: 0",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlGrid.Controls.Add(lblSummary);

            pnlBody.Controls.Add(pnlGrid);
            pnlGrid.BringToFront();
        }

        public async Task LoadPayrollDataAsync()
        {
            try
            {
                int month = cboMonth.SelectedIndex + 1;
                int year = (int)numYear.Value;

                _currentPayroll = await _payrollService.GetPayrollByMonthYearAsync(month, year);

                dgvPayroll.Rows.Clear();

                if (_currentPayroll == null)
                {
                    lblStatusBadge.Text = $"TRẠNG THÁI: CHƯA TÍNH LƯƠNG CHO THÁNG {month:D2}/{year}";
                    lblStatusBadge.ForeColor = Color.FromArgb(100, 116, 139);
                    cardBase.SetValues("Lương cơ bản", "0 đ", "Chưa tính", Color.FromArgb(37, 99, 235), "💼");
                    cardAllowance.SetValues("Phụ cấp", "0 đ", "Chưa tính", Color.FromArgb(16, 185, 129), "🍱");
                    cardBonus.SetValues("Thưởng", "0 đ", "Chưa tính", Color.FromArgb(245, 158, 11), "⭐");
                    cardNet.SetValues("Thực lĩnh", "0 đ", "Chưa tính", Color.FromArgb(139, 92, 246), "💳");
                    lblSummary.Text = "Chưa có dữ liệu bảng lương";
                    btnApprove.Enabled = false;
                    btnPay.Enabled = false;
                    btnExportExcel.Enabled = false;
                    btnViewPayslip.Enabled = false;
                    return;
                }

                // Cập nhật trạng thái
                btnExportExcel.Enabled = true;
                btnViewPayslip.Enabled = true;

                if (_currentPayroll.Status == PayrollStatus.Draft)
                {
                    lblStatusBadge.Text = $"TRẠNG THÁI: BẢN NHÁP / DỰ THẢO (THÁNG {month:D2}/{year})";
                    lblStatusBadge.ForeColor = Color.FromArgb(245, 158, 11);
                    btnApprove.Enabled = true;
                    btnPay.Enabled = false;
                }
                else if (_currentPayroll.Status == PayrollStatus.Approved)
                {
                    lblStatusBadge.Text = $"TRẠNG THÁI: ĐÃ DUYỆT BỞI {_currentPayroll.ApprovedBy} (CHỜ THANH TOÁN)";
                    lblStatusBadge.ForeColor = Color.FromArgb(37, 99, 235);
                    btnApprove.Enabled = false;
                    btnPay.Enabled = true;
                }
                else if (_currentPayroll.Status == PayrollStatus.Paid)
                {
                    lblStatusBadge.Text = $"TRẠNG THÁI: ĐÃ THANH TOÁN HOÀN TẤT";
                    lblStatusBadge.ForeColor = Color.FromArgb(16, 185, 129);
                    btnApprove.Enabled = false;
                    btnPay.Enabled = false;
                }

                // Cập nhật thẻ KPI
                cardBase.SetValues("Lương cơ bản theo công", $"{_currentPayroll.TotalBaseSalary:N0} đ", $"{_currentPayroll.Details.Count} nhân viên", Color.FromArgb(37, 99, 235), "💼");
                cardAllowance.SetValues("Tổng phụ cấp", $"{_currentPayroll.TotalAllowance:N0} đ", "Ăn trưa, xăng xe, trách nhiệm", Color.FromArgb(16, 185, 129), "🍱");
                cardBonus.SetValues("Tổng thưởng", $"{_currentPayroll.TotalBonus:N0} đ", "Chuyên cần & KPI", Color.FromArgb(245, 158, 11), "⭐");
                cardNet.SetValues("TỔNG THỰC LĨNH", $"{_currentPayroll.TotalNetSalary:N0} đ", "Tổng chi phí lương", Color.FromArgb(139, 92, 246), "💳");

                // Đổ dữ liệu vào bảng
                foreach (var d in _currentPayroll.Details)
                {
                    dgvPayroll.Rows.Add(
                        d.Id,
                        d.EmployeeCode,
                        d.EmployeeName,
                        d.Position,
                        d.StandardDays,
                        d.ActualWorkDays,
                        $"{d.BaseSalary:N0} đ",
                        $"{d.ActualBaseSalary:N0} đ",
                        $"{d.TotalAllowance:N0} đ",
                        $"{d.TotalBonus:N0} đ",
                        $"{d.TotalDeductions:N0} đ",
                        $"{d.NetSalary:N0} đ"
                    );
                }

                lblSummary.Text = $"Tổng cộng: {_currentPayroll.Details.Count} nhân viên | Tổng quỹ lương: {_currentPayroll.TotalNetSalary:N0} đ";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp bảng lương: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CalculatePayrollAsync()
        {
            int month = cboMonth.SelectedIndex + 1;
            int year = (int)numYear.Value;

            var confirm = MessageBox.Show(
                $"Hệ thống sẽ tổng hợp toàn bộ dữ liệu chấm công và tính lương cho Tháng {month:D2}/{year}.\nBạn có muốn tiếp tục?",
                "Xác nhận tính lương",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            try
            {
                btnCalculate.Enabled = false;
                btnCalculate.Text = "Đang tính...";

                var req = new CalculatePayrollRequestDTO
                {
                    Month = month,
                    Year = year,
                    Note = $"Bảng tính lương tự động Tháng {month:D2}/{year}"
                };

                await _payrollService.CalculatePayrollAsync(req);
                MessageBox.Show($"Tính lương tự động Tháng {month:D2}/{year} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadPayrollDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tính lương: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCalculate.Enabled = true;
                btnCalculate.Text = "⚡ Tính lương tự động";
            }
        }

        private async Task ApprovePayrollAsync()
        {
            if (_currentPayroll == null) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn DUYỆT bảng lương Tháng {_currentPayroll.Month:D2}/{_currentPayroll.Year}?",
                "Xác nhận duyệt",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                await _payrollService.ApprovePayrollAsync(_currentPayroll.Id, "Quản lý Cửa hàng");
                MessageBox.Show("Duyệt bảng lương thành công! Bảng lương chuyển sang trạng thái sẵn sàng chi trả.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadPayrollDataAsync();
            }
        }

        private async Task MarkPaidAsync()
        {
            if (_currentPayroll == null) return;

            var confirm = MessageBox.Show(
                $"Xác nhận ĐÃ THANH TOÁN LƯƠNG cho toàn bộ nhân viên Tháng {_currentPayroll.Month:D2}/{_currentPayroll.Year}?",
                "Xác nhận thanh toán",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                await _payrollService.MarkAsPaidAsync(_currentPayroll.Id);
                MessageBox.Show("Đã xác nhận thanh toán lương hoàn tất!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadPayrollDataAsync();
            }
        }

        private void ViewSelectedPayslip()
        {
            if (dgvPayroll.SelectedRows.Count == 0 || _currentPayroll == null)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên trong bảng lương để xem phiếu lương.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int detailId = Convert.ToInt32(dgvPayroll.SelectedRows[0].Cells["Id"].Value);
            var detail = _currentPayroll.Details.FirstOrDefault(d => d.Id == detailId);
            if (detail == null) return;

            using (var form = new PayslipForm(detail, _currentPayroll.Month, _currentPayroll.Year, _payrollService))
            {
                form.ShowDialog();
            }
        }

        private async Task ExportExcelAsync()
        {
            if (_currentPayroll == null) return;

            using (var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"BangLuong_{_currentPayroll.Year}{_currentPayroll.Month:D2}.xlsx"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    await _payrollService.ExportPayrollToExcelAsync(_currentPayroll.Id, sfd.FileName);
                    MessageBox.Show("Xuất bảng lương ra Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void DgvPayroll_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvPayroll.Columns[e.ColumnIndex].Name == "NetSal")
            {
                e.CellStyle!.ForeColor = Color.FromArgb(56, 189, 248); // Sky 400
                e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            }

            if (e.RowIndex % 2 == 1 && dgvPayroll.Columns[e.ColumnIndex].Name != "NetSal")
            {
                e.CellStyle!.BackColor = Color.FromArgb(24, 34, 50); // Dark Slate alternate
            }
        }

        private static Button CreateBtn(string text, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Height = 32,
                AutoSize = true,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Margin = new Padding(3, 4, 3, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
