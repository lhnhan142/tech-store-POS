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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI.Forms.HumanResource
{
    [DesignerCategory("Code")]
    public class PayrollDetailForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int _payrollId;

        private readonly Label _lblTitle = new();
        private readonly Label _lblSummary = new();
        private readonly DataGridView _grid = new();
        private readonly HopeRoundButton _btnAdjust = new();
        private readonly HopeRoundButton _btnExportExcel = new();
        private readonly HopeRoundButton _btnExportPdf = new();
        private readonly HopeRoundButton _btnClose = new();

        private PayrollSummaryDto? _summary;
        private List<PayrollDetailDto> _details = new();

        public PayrollDetailForm(IServiceScopeFactory scopeFactory, int payrollId)
        {
            _scopeFactory = scopeFactory;
            _payrollId = payrollId;

            Text = "Chi tiết bảng lương nhân viên - Tech Store POS";
            Font = new Font("Segoe UI", 9.5F);
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(S(1280), S(720));
            MinimumSize = new Size(S(1050), S(600));
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
                Padding = new Padding(S(20), 0, S(20), 0)
            };

            _lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            _lblTitle.ForeColor = Color.FromArgb(15, 15, 60);
            _lblTitle.Dock = DockStyle.Left;
            _lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            _lblTitle.AutoSize = true;
            _lblTitle.UseMnemonic = false;

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

            pnlHeader.Controls.AddRange(new Control[] { pnlHeaderRight, _lblTitle });

            // 2. Summary Info Banner
            var pnlBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(45),
                BackColor = Color.FromArgb(235, 240, 255),
                Padding = new Padding(S(20), 0, S(20), 0)
            };

            _lblSummary.Font = new Font("Segoe UI Semibold", 9.5F);
            _lblSummary.ForeColor = Color.FromArgb(30, 64, 175);
            _lblSummary.Dock = DockStyle.Fill;
            _lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            _lblSummary.UseMnemonic = false;

            pnlBanner.Controls.Add(_lblSummary);

            // 3. Bottom Action Bar
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = S(58),
                BackColor = Color.White,
                Padding = new Padding(S(20), 0, S(20), 0)
            };
            pnlBottom.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(229, 231, 235), 1);
                e.Graphics.DrawLine(pen, 0, 0, pnlBottom.Width, 0);
            };

            var pnlBottomRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, S(11), 0, 0)
            };

            _btnClose.Text = "Đóng";
            _btnClose.Size = new Size(S(90), S(35));
            _btnClose.Margin = new Padding(S(6), 0, 0, 0);
            _btnClose.PrimaryColor = Color.FromArgb(229, 231, 235);
            _btnClose.TextColor = Color.FromArgb(55, 65, 81);
            _btnClose.Click += (_, _) => Close();

            _btnAdjust.Text = "Sửa Thưởng / Khấu trừ";
            _btnAdjust.Size = new Size(S(175), S(35));
            _btnAdjust.Margin = new Padding(S(6), 0, 0, 0);
            _btnAdjust.PrimaryColor = Color.FromArgb(0, 112, 243);
            _btnAdjust.TextColor = Color.White;
            _btnAdjust.Click += async (_, _) => await AdjustDetailAsync();

            pnlBottomRight.Controls.Add(_btnClose);
            pnlBottomRight.Controls.Add(_btnAdjust);

            var lblHelp = new Label
            {
                Text = "💡 Nhấp đúp chuột vào bất kỳ dòng nào để điều chỉnh Thưởng hoặc Khấu trừ",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(107, 114, 128),
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true,
                UseMnemonic = false
            };

            pnlBottom.Controls.AddRange(new Control[] { pnlBottomRight, lblHelp });

            // 4. DataGridView
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(S(20), S(12), S(20), S(12)),
                BackColor = Color.FromArgb(243, 244, 246)
            };

            UiKit.StyleGrid(_grid, this, 38);
            _grid.AllowUserToResizeColumns = true;
            _grid.ColumnHeadersHeight = S(40);
            ConfigureGridColumns();
            _grid.DoubleClick += async (_, _) => await AdjustDetailAsync();
            pnlGrid.Controls.Add(_grid);

            Controls.Add(pnlGrid);
            Controls.Add(pnlBottom);
            Controls.Add(pnlBanner);
            Controls.Add(pnlHeader);
        }

        private void ConfigureGridColumns()
        {
            _grid.Columns.Clear();

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColId",
                HeaderText = "ID",
                DataPropertyName = nameof(PayrollDetailDto.Id),
                Visible = false
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColCode",
                HeaderText = "MÃ NV",
                DataPropertyName = nameof(PayrollDetailDto.EmployeeCode),
                Width = S(75)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColName",
                HeaderText = "HỌ VÀ TÊN",
                DataPropertyName = nameof(PayrollDetailDto.EmployeeName),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = S(145)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColPosition",
                HeaderText = "CHỨC VỤ",
                DataPropertyName = nameof(PayrollDetailDto.Position),
                Width = S(135)
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColBaseSalary",
                HeaderText = "LƯƠNG CB",
                DataPropertyName = nameof(PayrollDetailDto.BaseSalary),
                Width = S(95),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColWorkDays",
                HeaderText = "CÔNG",
                DataPropertyName = nameof(PayrollDetailDto.ActualWorkDays),
                Width = S(55),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.#", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColOt",
                HeaderText = "TĂNG CA",
                DataPropertyName = nameof(PayrollDetailDto.OvertimeHours),
                Width = S(75),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.#", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColSalaryWork",
                HeaderText = "LƯƠNG CÔNG",
                DataPropertyName = nameof(PayrollDetailDto.SalaryByWorkDays),
                Width = S(95),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColOtPay",
                HeaderText = "TIỀN TC",
                DataPropertyName = nameof(PayrollDetailDto.OvertimePay),
                Width = S(80),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColAllowance",
                HeaderText = "PHỤ CẤP",
                DataPropertyName = nameof(PayrollDetailDto.Allowance),
                Width = S(80),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColBonus",
                HeaderText = "THƯỞNG",
                DataPropertyName = nameof(PayrollDetailDto.Bonus),
                Width = S(75),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight, ForeColor = Color.FromArgb(22, 101, 52) }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColDeductions",
                HeaderText = "KHẤU TRỪ",
                DataPropertyName = nameof(PayrollDetailDto.Deductions),
                Width = S(80),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight, ForeColor = Color.FromArgb(185, 28, 28) }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColNet",
                HeaderText = "THỰC LĨNH",
                DataPropertyName = nameof(PayrollDetailDto.NetSalary),
                Width = S(110),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(0, 112, 243) }
            });
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var paySvc = scope.ServiceProvider.GetRequiredService<IPayrollService>();

                _summary = await paySvc.GetPayrollByIdAsync(_payrollId);
                if (_summary == null)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu bảng lương.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                _lblTitle.Text = $"BẢNG THANH TOÁN LƯƠNG {_summary.PeriodText.ToUpper()} ({_summary.PayrollCode})";
                _lblSummary.Text = $"Trạng thái: {_summary.StatusText.ToUpper()}  |  Tổng nhân viên: {_summary.TotalEmployees}  |  Tổng chi phí lương: {_summary.TotalGrossAmount:N0} đ  |  TỔNG THỰC CHI: {_summary.TotalNetAmount:N0} đ";

                _btnAdjust.Enabled = _summary.Status == PayrollStatus.Draft;

                var details = await paySvc.GetPayrollDetailsAsync(_payrollId);
                _details = details.ToList();
                _grid.DataSource = _details;
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
        }

        private async Task AdjustDetailAsync()
        {
            if (_summary?.Status == PayrollStatus.Finalized)
            {
                MessageBox.Show("Bảng lương này đã chốt. Không thể thay đổi các khoản thưởng/khấu trừ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_grid.CurrentRow?.DataBoundItem is not PayrollDetailDto item)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần điều chỉnh thưởng/khấu trừ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new Form
            {
                Text = $"Điều chỉnh lương: {item.EmployeeName} ({item.EmployeeCode})",
                Font = new Font("Segoe UI", 9.5F),
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(S(400), S(270)),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            var lblBonus = new Label { Text = "Tiền thưởng (VNĐ):", Location = new Point(S(25), S(25)), AutoSize = true };
            var nudBonus = new NumericUpDown
            {
                Location = new Point(S(160), S(22)),
                Width = S(190),
                Maximum = 100_000_000,
                Increment = 100_000,
                ThousandsSeparator = true,
                Value = item.Bonus
            };

            var lblDeduct = new Label { Text = "Khấu trừ (VNĐ):", Location = new Point(S(25), S(68)), AutoSize = true };
            var nudDeduct = new NumericUpDown
            {
                Location = new Point(S(160), S(65)),
                Width = S(190),
                Maximum = 100_000_000,
                Increment = 100_000,
                ThousandsSeparator = true,
                Value = item.Deductions
            };

            var lblNote = new Label { Text = "Ghi chú điều chỉnh:", Location = new Point(S(25), S(112)), AutoSize = true };
            var txtNote = new TextBox { Text = item.Note ?? string.Empty, Location = new Point(S(160), S(110)), Width = S(190) };

            var btnOk = new HopeRoundButton { Text = "Cập nhật", PrimaryColor = Color.FromArgb(0, 112, 243), Location = new Point(S(160), S(170)), Size = new Size(S(105), S(35)) };
            var btnCancel = new HopeRoundButton { Text = "Hủy", PrimaryColor = Color.FromArgb(229, 231, 235), TextColor = Color.FromArgb(55, 65, 81), Location = new Point(S(275), S(170)), Size = new Size(S(75), S(35)) };

            btnOk.Click += async (_, _) =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var paySvc = scope.ServiceProvider.GetRequiredService<IPayrollService>();
                    await paySvc.UpdateDetailAdjustmentsAsync(new UpdateSalaryAdjustmentsDto
                    {
                        DetailId = item.Id,
                        Bonus = nudBonus.Value,
                        Deductions = nudDeduct.Value,
                        Note = txtNote.Text.Trim()
                    });
                    dlg.DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    UiHelper.HandleException(ex);
                }
            };
            btnCancel.Click += (_, _) => dlg.DialogResult = DialogResult.Cancel;

            dlg.Controls.Add(lblBonus);
            dlg.Controls.Add(nudBonus);
            dlg.Controls.Add(lblDeduct);
            dlg.Controls.Add(nudDeduct);
            dlg.Controls.Add(lblNote);
            dlg.Controls.Add(txtNote);
            dlg.Controls.Add(btnOk);
            dlg.Controls.Add(btnCancel);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                await LoadDataAsync();
            }
        }

        private void ExportExcel()
        {
            if (_summary == null || _details.Count == 0) return;

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"BangLuong_{_summary.Month:D2}_{_summary.Year}.xlsx"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var exp = scope.ServiceProvider.GetRequiredService<IReportExportService>();
                    exp.ExportPayrollReportToExcel(sfd.FileName, _summary, _details);
                    MessageBox.Show("Xuất bảng lương Excel thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    UiHelper.HandleException(ex);
                }
            }
        }

        private void ExportPdf()
        {
            if (_summary == null || _details.Count == 0) return;

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"BangLuong_{_summary.Month:D2}_{_summary.Year}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var exp = scope.ServiceProvider.GetRequiredService<IReportExportService>();
                    exp.ExportPayrollReportToPdf(sfd.FileName, _summary, _details);
                    MessageBox.Show("Xuất bảng lương PDF thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    UiHelper.HandleException(ex);
                }
            }
        }
    }
}
