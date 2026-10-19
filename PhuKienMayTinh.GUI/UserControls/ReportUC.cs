using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PhuKienMayTinh.BLL.DTOs;
using PhuKienMayTinh.BLL.Services.Interfaces;

namespace PhuKienMayTinh.GUI.UserControls
{
    public class ReportUC : UserControl
    {
        private readonly IReportService _reportService;

        // UI Controls
        private ComboBox cboReportType = null!;
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private ComboBox cboCategory = null!;
        private Button btnView = null!;
        private Button btnExportExcel = null!;
        private Button btnExportPdf = null!;
        private ProgressBar prgLoading = null!;

        private DataGridView dgvPreview = null!;
        private Label lblReportTitle = null!;
        private Label lblSummary = null!;

        private DataTable? _currentTable;

        public ReportUC(IReportService reportService)
        {
            _reportService = reportService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 9f);

            // 1. Top Bar (Slate 800)
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20, 12, 20, 12)
            };

            var lblHeader = new Label
            {
                Text = "📈 TRUNG TÂM BÁO CÁO THỐNG KÊ & XUẤT DỮ LIỆU",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 250, 252),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 18)
            };
            pnlTop.Controls.Add(lblHeader);

            // Actions bên phải
            var pnlActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 320,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            btnExportPdf = CreateBtn("📄 Xuất PDF", Color.FromArgb(239, 68, 68));
            btnExportPdf.Click += async (s, e) => await ExportPdfAsync();

            btnExportExcel = CreateBtn("📊 Xuất Excel", Color.FromArgb(16, 185, 129));
            btnExportExcel.Click += async (s, e) => await ExportExcelAsync();

            pnlActions.Controls.Add(btnExportPdf);
            pnlActions.Controls.Add(btnExportExcel);
            pnlTop.Controls.Add(pnlActions);

            Controls.Add(pnlTop);

            // 2. Filter Bar (Dùng FlowLayoutPanel để không bao giờ bị cắt chữ)
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(16, 8, 16, 8),
                Margin = new Padding(0, 1, 0, 0)
            };

            var pnlFilterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            var lblType = new Label { Text = "Loại báo cáo:", AutoSize = true, Margin = new Padding(0, 10, 4, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            cboReportType = new ComboBox { Width = 220, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 5, 12, 0) };
            cboReportType.Items.AddRange(new object[]
            {
                "1. Báo cáo Doanh thu & Bán hàng",
                "2. Báo cáo Tồn kho & Cảnh báo hàng",
                "3. Báo cáo Chấm công Nhân sự",
                "4. Báo cáo Bảng lương Nhân viên"
            });
            cboReportType.SelectedIndex = 0;
            cboReportType.SelectedIndexChanged += (s, e) => HandleReportTypeChanged();

            var lblFrom = new Label { Text = "Từ:", AutoSize = true, Margin = new Padding(0, 10, 4, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            dtpFrom = new DateTimePicker { Width = 100, Height = 32, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30), Margin = new Padding(0, 5, 10, 0) };

            var lblTo = new Label { Text = "Đến:", AutoSize = true, Margin = new Padding(0, 10, 4, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            dtpTo = new DateTimePicker { Width = 100, Height = 32, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(0, 5, 12, 0) };

            var lblCat = new Label { Text = "Danh mục:", AutoSize = true, Margin = new Padding(0, 10, 4, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            cboCategory = new ComboBox { Width = 110, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false, Margin = new Padding(0, 5, 12, 0) };
            cboCategory.Items.AddRange(new object[] { "Tất cả", "Bàn phím", "Chuột", "Tai nghe", "Lót chuột", "Cáp kết nối", "Webcam", "Micro" });
            cboCategory.SelectedIndex = 0;

            btnView = new Button
            {
                Text = "🔍 Xem dữ liệu",
                Width = 110,
                Height = 32,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Margin = new Padding(0, 4, 8, 0)
            };
            btnView.FlatAppearance.BorderSize = 0;
            btnView.Click += async (s, e) => await LoadReportDataAsync();

            prgLoading = new ProgressBar { Width = 80, Height = 22, Style = ProgressBarStyle.Marquee, Visible = false, Margin = new Padding(0, 9, 0, 0) };

            pnlFilterFlow.Controls.Add(lblType);
            pnlFilterFlow.Controls.Add(cboReportType);
            pnlFilterFlow.Controls.Add(lblFrom);
            pnlFilterFlow.Controls.Add(dtpFrom);
            pnlFilterFlow.Controls.Add(lblTo);
            pnlFilterFlow.Controls.Add(dtpTo);
            pnlFilterFlow.Controls.Add(lblCat);
            pnlFilterFlow.Controls.Add(cboCategory);
            pnlFilterFlow.Controls.Add(btnView);
            pnlFilterFlow.Controls.Add(prgLoading);

            pnlFilter.Controls.Add(pnlFilterFlow);
            Controls.Add(pnlFilter);

            // 3. Grid Container (Slate 900)
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.FromArgb(15, 23, 42)
            };

            lblReportTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = "BÁO CÁO DOANH THU BÁN HÀNG",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                UseMnemonic = false
            };
            pnlGrid.Controls.Add(lblReportTitle);

            dgvPreview = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(51, 65, 85),
                RowTemplate = { Height = 36 }
            };

            dgvPreview.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvPreview.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
            dgvPreview.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvPreview.ColumnHeadersHeight = 38;
            dgvPreview.DefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvPreview.DefaultCellStyle.ForeColor = Color.FromArgb(241, 245, 249);
            dgvPreview.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 58, 138);
            dgvPreview.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvPreview.CellFormatting += DgvPreview_CellFormatting;

            pnlGrid.Controls.Add(dgvPreview);

            lblSummary = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                Text = "Sẵn sàng trích xuất báo cáo.",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleRight,
                UseMnemonic = false
            };
            pnlGrid.Controls.Add(lblSummary);

            Controls.Add(pnlGrid);
            pnlGrid.BringToFront();
        }

        private void HandleReportTypeChanged()
        {
            // Enable category dropdown only when inventory report is chosen
            cboCategory.Enabled = (cboReportType.SelectedIndex == 1);
        }

        public async Task LoadReportDataAsync()
        {
            try
            {
                btnView.Enabled = false;
                prgLoading.Visible = true;

                var filter = GetCurrentFilter();
                lblReportTitle.Text = filter.ReportType switch
                {
                    "DoanhThu" => $"BÁO CÁO DOANH THU & ĐƠN HÀNG ({filter.FromDate:dd/MM/yyyy} - {filter.ToDate:dd/MM/yyyy})",
                    "TonKho" => "BÁO CÁO TỒN KHO & CẢNH BÁO NGUY CƠ HẾT HÀNG",
                    "ChamCong" => $"BÁO CÁO CHẤM CÔNG NHÂN SỰ ({filter.FromDate:dd/MM/yyyy} - {filter.ToDate:dd/MM/yyyy})",
                    "BangLuong" => "BÁO CÁO QUỸ LƯƠNG NHÂN VIÊN GẦN NHẤT",
                    _ => "BÁO CÁO TỔNG HỢP"
                };

                _currentTable = await _reportService.GenerateReportDataTableAsync(filter);
                dgvPreview.DataSource = _currentTable;

                lblSummary.Text = $"Tổng số: {_currentTable.Rows.Count} bản ghi dữ liệu được hiển thị";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lập báo cáo: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnView.Enabled = true;
                prgLoading.Visible = false;
            }
        }

        private async Task ExportExcelAsync()
        {
            if (_currentTable == null || _currentTable.Rows.Count == 0)
            {
                MessageBox.Show("Vui lòng nhấn 'Xem dữ liệu' trước khi xuất file.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var filter = GetCurrentFilter();
            using (var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"BaoCao_{filter.ReportType}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        prgLoading.Visible = true;
                        await _reportService.ExportReportToExcelAsync(filter, sfd.FileName);
                        MessageBox.Show("Xuất báo cáo Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    finally
                    {
                        prgLoading.Visible = false;
                    }
                }
            }
        }

        private async Task ExportPdfAsync()
        {
            if (_currentTable == null || _currentTable.Rows.Count == 0)
            {
                MessageBox.Show("Vui lòng nhấn 'Xem dữ liệu' trước khi xuất file.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var filter = GetCurrentFilter();
            using (var sfd = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"BaoCao_{filter.ReportType}_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        prgLoading.Visible = true;
                        await _reportService.ExportReportToPdfAsync(filter, sfd.FileName);
                        MessageBox.Show("Xuất báo cáo PDF thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    finally
                    {
                        prgLoading.Visible = false;
                    }
                }
            }
        }

        private ReportFilterDTO GetCurrentFilter()
        {
            string type = cboReportType.SelectedIndex switch
            {
                0 => "DoanhThu",
                1 => "TonKho",
                2 => "ChamCong",
                3 => "BangLuong",
                _ => "DoanhThu"
            };

            return new ReportFilterDTO
            {
                ReportType = type,
                FromDate = dtpFrom.Value.Date,
                ToDate = dtpTo.Value.Date,
                Category = cboCategory.SelectedItem?.ToString()
            };
        }

        private void DgvPreview_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Zebra
            if (e.RowIndex % 2 == 1)
            {
                e.CellStyle!.BackColor = Color.FromArgb(24, 34, 50); // Dark Slate alternate
            }

            // Định dạng số tiền
            if (e.Value is decimal dec)
            {
                e.Value = dec.ToString("N0") + " đ";
                e.FormattingApplied = true;
                e.CellStyle!.Alignment = DataGridViewContentAlignment.MiddleRight;
                e.CellStyle.ForeColor = Color.FromArgb(56, 189, 248); // Sky 400
                e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            }
        }

        private static Button CreateBtn(string text, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Height = 34,
                AutoSize = true,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Margin = new Padding(4, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
