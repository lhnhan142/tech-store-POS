using System;
using System.Drawing;
using System.Windows.Forms;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.BLL.Services.Interfaces;
using PhuKienMayTinh.Common.Constants;

namespace PhuKienMayTinh.GUI.Forms
{
    public class PayslipForm : Form
    {
        private readonly PayrollDetailDTO _detail;
        private readonly int _month;
        private readonly int _year;
        private readonly IPayrollService _payrollService;

        public PayslipForm(PayrollDetailDTO detail, int month, int year, IPayrollService payrollService)
        {
            _detail = detail;
            _month = month;
            _year = year;
            _payrollService = payrollService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = $"PHIẾU LƯƠNG CHI TIẾT - {_detail.EmployeeCode} ({_detail.EmployeeName})";
            Size = new Size(700, 750);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            var pnlContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 20, 30, 20),
                AutoScroll = true
            };
            Controls.Add(pnlContainer);

            // 1. Tiêu đề công ty
            int currentY = 15;
            var lblStore = new Label
            {
                Text = AppConstants.StoreName,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(30, currentY)
            };
            pnlContainer.Controls.Add(lblStore);
            currentY += 24;

            var lblAddress = new Label
            {
                Text = $"{AppConstants.StoreAddress} | Hotline: {AppConstants.StorePhone}",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(30, currentY)
            };
            pnlContainer.Controls.Add(lblAddress);
            currentY += 35;

            // 2. Banner Phiếu Lương
            var pnlBanner = new Panel
            {
                Location = new Point(30, currentY),
                Width = 600,
                Height = 60,
                BackColor = Color.FromArgb(238, 242, 255)
            };
            var lblBanner = new Label
            {
                Text = $"PHIẾU THANH TOÁN LƯƠNG - THÁNG {_month:D2}/{_year}",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(110, 18)
            };
            pnlBanner.Controls.Add(lblBanner);
            pnlContainer.Controls.Add(pnlBanner);
            currentY += 75;

            // 3. Thông tin nhân sự
            var gbInfo = new GroupBox
            {
                Text = "THÔNG TIN NHÂN SỰ",
                Location = new Point(30, currentY),
                Width = 600,
                Height = 90,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            AddLabelToBox(gbInfo, "Mã nhân viên:", _detail.EmployeeCode, 20, 28);
            AddLabelToBox(gbInfo, "Họ và tên:", _detail.EmployeeName, 200, 28);
            AddLabelToBox(gbInfo, "Phòng ban:", _detail.Department, 400, 28);

            AddLabelToBox(gbInfo, "Chức vụ:", _detail.Position, 20, 56);
            AddLabelToBox(gbInfo, "Công chuẩn:", $"{_detail.StandardDays} ngày", 200, 56);
            AddLabelToBox(gbInfo, "Công thực tế:", $"{_detail.ActualWorkDays} ngày ({_detail.TotalHours}h)", 400, 56);

            pnlContainer.Controls.Add(gbInfo);
            currentY += 105;

            // 4. Bảng Thu nhập & Khấu trừ (2 cột song song)
            var gbEarnings = new GroupBox
            {
                Text = "I. CÁC KHOẢN THU NHẬP",
                Location = new Point(30, currentY),
                Width = 290,
                Height = 220,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129)
            };

            AddPayItem(gbEarnings, "Lương cơ bản:", $"{_detail.BaseSalary:N0} đ", 25);
            AddPayItem(gbEarnings, "Lương theo ngày công:", $"{_detail.ActualBaseSalary:N0} đ", 52);
            AddPayItem(gbEarnings, "Phụ cấp ăn trưa:", $"{_detail.LunchAllowance:N0} đ", 79);
            AddPayItem(gbEarnings, "Phụ cấp xăng xe:", $"{_detail.TravelAllowance:N0} đ", 106);
            AddPayItem(gbEarnings, "Phụ cấp trách nhiệm:", $"{_detail.ResponsibilityAllowance:N0} đ", 133);
            AddPayItem(gbEarnings, "Thưởng chuyên cần:", $"{_detail.DiligenceBonus:N0} đ", 160);
            AddPayItem(gbEarnings, "Thưởng hiệu suất KPI:", $"{_detail.PerformanceBonus:N0} đ", 187);

            var gbDeductions = new GroupBox
            {
                Text = "II. CÁC KHOẢN KHẤU TRỪ",
                Location = new Point(340, currentY),
                Width = 290,
                Height = 220,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(239, 68, 68)
            };

            AddPayItem(gbDeductions, $"Trừ đi trễ ({_detail.TotalLateMinutes}p):", $"{_detail.LatePenalty:N0} đ", 25);
            AddPayItem(gbDeductions, $"Trừ vắng ({_detail.TotalAbsenceCount}b):", $"{_detail.AbsencePenalty:N0} đ", 52);
            AddPayItem(gbDeductions, "Bảo hiểm XH (10.5%):", $"{_detail.InsuranceDeduction:N0} đ", 79);
            AddPayItem(gbDeductions, "Khấu trừ khác:", $"{_detail.OtherDeductions:N0} đ", 106);

            pnlContainer.Controls.Add(gbEarnings);
            pnlContainer.Controls.Add(gbDeductions);
            currentY += 235;

            // 5. Thực lĩnh (Banner to nổi bật)
            var pnlNet = new Panel
            {
                Location = new Point(30, currentY),
                Width = 600,
                Height = 55,
                BackColor = Color.FromArgb(236, 253, 245)
            };
            var lblNet = new Label
            {
                Text = $"THỰC LĨNH (NET): {_detail.NetSalary:N0} VNĐ",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(5, 150, 105),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(140, 16)
            };
            pnlNet.Controls.Add(lblNet);
            pnlContainer.Controls.Add(pnlNet);
            currentY += 70;

            // Bottom Actions: Xuất PDF & Đóng
            var btnExportPdf = new Button
            {
                Text = "📄 Xuất phiếu lương PDF",
                Location = new Point(30, currentY),
                Width = 190,
                Height = 38,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            btnExportPdf.FlatAppearance.BorderSize = 0;
            btnExportPdf.Click += async (s, e) => await ExportPdfAsync();

            var btnClose = new Button
            {
                Text = "Đóng",
                Location = new Point(530, currentY),
                Width = 100,
                Height = 38,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                DialogResult = DialogResult.OK
            };
            btnClose.FlatAppearance.BorderSize = 0;

            pnlContainer.Controls.Add(btnExportPdf);
            pnlContainer.Controls.Add(btnClose);
        }

        private async Task ExportPdfAsync()
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"PhieuLuong_{_detail.EmployeeCode}_{_year}{_month:D2}.pdf"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    await _payrollService.ExportPayslipToPdfAsync(_detail.Id, sfd.FileName);
                    MessageBox.Show("Xuất phiếu lương ra file PDF thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private static void AddLabelToBox(GroupBox gb, string title, string val, int x, int y)
        {
            var lblT = new Label { Text = title, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular), ForeColor = Color.FromArgb(100, 116, 139) };
            var lblV = new Label { Text = val, Location = new Point(x, y + 15), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59) };
            gb.Controls.Add(lblT);
            gb.Controls.Add(lblV);
        }

        private static void AddPayItem(GroupBox gb, string title, string val, int y)
        {
            var lblT = new Label { Text = title, Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular), ForeColor = Color.FromArgb(71, 85, 105) };
            var lblV = new Label { Text = val, Location = new Point(175, y), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42) };
            gb.Controls.Add(lblT);
            gb.Controls.Add(lblV);
        }
    }
}
