using BLL.DTOs.Return;
using BLL.Services.Interfaces;
using GUI.Themes;
using GUI.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace GUI.Forms.InvoiceAndReturn
{
    /// <summary>
    /// Màn hình Báo cáo & Phân tích Đổi trả:
    /// Thống kê tổng quan kỳ (tổng số, hoàn thành, từ chối, tổng tiền hoàn, thời gian xử lý TB),
    /// Tỷ lệ trả theo sản phẩm, Phân bổ theo lý do, Biểu đồ hoàn tiền 12 tháng.
    /// </summary>
    [DesignerCategory("Code")]
    public class frmBaoCaoDoiTra : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Header
        private readonly Panel _pnlHeader = new();
        private readonly Label _lblTitle = new();

        // Bộ lọc kỳ báo cáo
        private readonly Panel _pnlFilter = new();
        private readonly DateTimePicker _dtpTuNgay = new();
        private readonly DateTimePicker _dtpDenNgay = new();
        private readonly Button _btnXemBaoCao = new();

        // 4 KPI Cards (Thẻ chỉ số tổng quan)
        private readonly FlowLayoutPanel _flpKpi = new();
        private readonly Panel _pnlKpiTotal = new();
        private readonly Panel _pnlKpiCompleted = new();
        private readonly Panel _pnlKpiRefund = new();
        private readonly Panel _pnlKpiAvgTime = new();

        // 2 DataGridViews: Top Sản phẩm & Lý do đổi trả
        private readonly DataGridView _dgvTopProducts = new();
        private readonly DataGridView _dgvByReason = new();

        public frmBaoCaoDoiTra(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Báo cáo Đổi trả Hàng hóa - Tech Store POS";
            Font = InvoiceTheme.BodyFont;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 750);
            MinimumSize = new Size(1000, 600);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = InvoiceTheme.FormBg;

            BuildUi();

            Load += async (_, _) => await RunSafeAsync(LoadReportAsync);
        }

        private void BuildUi()
        {
            // -------------------------------------------------------------
            // 1. TOP HEADER
            // -------------------------------------------------------------
            _pnlHeader.Dock = DockStyle.Top;
            _pnlHeader.Height = 65;
            _pnlHeader.BackColor = InvoiceTheme.SidebarDark;
            _pnlHeader.Padding = new Padding(20, 0, 20, 0);

            _lblTitle.Text = "BÁO CÁO & PHÂN TÍCH ĐỔI TRẢ HÀNG HÓA";
            _lblTitle.UseMnemonic = false;
            _lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            _lblTitle.ForeColor = Color.White;
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(20, 18);
            _pnlHeader.Controls.Add(_lblTitle);

            Controls.Add(_pnlHeader);

            // -------------------------------------------------------------
            // 2. TOOLBAR FILTER PANEL
            // -------------------------------------------------------------
            var pnlFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = InvoiceTheme.CardBg,
                Padding = new Padding(15, 10, 15, 8),
                WrapContents = false,
                AutoScroll = false
            };

            var lblTu = new Label { Text = "Kỳ báo cáo từ:", AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Margin = new Padding(0, 8, 4, 0) };
            _dtpTuNgay.Format = DateTimePickerFormat.Short;
            _dtpTuNgay.Size = new Size(125, 36);
            _dtpTuNgay.Margin = new Padding(0, 2, 12, 0);
            _dtpTuNgay.Value = DateTime.Today.AddDays(-30);

            var lblDen = new Label { Text = "đến ngày:", AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Margin = new Padding(0, 8, 4, 0) };
            _dtpDenNgay.Format = DateTimePickerFormat.Short;
            _dtpDenNgay.Size = new Size(125, 36);
            _dtpDenNgay.Margin = new Padding(0, 2, 12, 0);
            _dtpDenNgay.Value = DateTime.Today;

            _btnXemBaoCao.Text = "Xem báo cáo";
            _btnXemBaoCao.Size = new Size(125, 36);
            _btnXemBaoCao.Margin = new Padding(0, 1, 0, 0);
            _btnXemBaoCao.BackColor = InvoiceTheme.Primary;
            _btnXemBaoCao.ForeColor = Color.White;
            _btnXemBaoCao.FlatStyle = FlatStyle.Flat;
            _btnXemBaoCao.FlatAppearance.BorderSize = 0;
            _btnXemBaoCao.Font = InvoiceTheme.BodyBoldFont;
            _btnXemBaoCao.Cursor = Cursors.Hand;
            _btnXemBaoCao.Click += async (_, _) => await RunSafeAsync(LoadReportAsync);

            pnlFilter.Controls.AddRange([lblTu, _dtpTuNgay, lblDen, _dtpDenNgay, _btnXemBaoCao]);
            Controls.Add(pnlFilter);

            // -------------------------------------------------------------
            // 3. KPI METRIC CARDS
            // -------------------------------------------------------------
            _flpKpi.Dock = DockStyle.Top;
            _flpKpi.Height = 110;
            _flpKpi.BackColor = InvoiceTheme.FormBg;
            _flpKpi.Padding = new Padding(15, 10, 15, 10);

            CreateKpiCard(_pnlKpiTotal, "TỔNG YÊU CẦU", "0", Color.FromArgb(30, 64, 175), Color.FromArgb(219, 234, 254));
            CreateKpiCard(_pnlKpiCompleted, "TỶ LỆ THÀNH CÔNG", "0%", Color.FromArgb(6, 95, 70), Color.FromArgb(209, 250, 229));
            CreateKpiCard(_pnlKpiRefund, "TỔNG TIỀN HOÀN TRẢ", "0đ", Color.FromArgb(153, 27, 27), Color.FromArgb(254, 226, 226));
            CreateKpiCard(_pnlKpiAvgTime, "TG XỬ LÝ TRUNG BÌNH", "0.0h", Color.FromArgb(146, 64, 14), Color.FromArgb(254, 243, 199));

            _flpKpi.Controls.AddRange([_pnlKpiTotal, _pnlKpiCompleted, _pnlKpiRefund, _pnlKpiAvgTime]);
            Controls.Add(_flpKpi);

            // -------------------------------------------------------------
            // 4. MAIN SPLIT: BẢNG SẢN PHẨM TRẢ NHIỀU & BẢNG LÝ DO
            // -------------------------------------------------------------
            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(15)
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Trái 60%
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Phải 40%

            // Left: Top sản phẩm
            var pnlLeft = new Panel { Dock = DockStyle.Fill, BackColor = InvoiceTheme.CardBg, Padding = new Padding(15) };
            var lblTopProdTitle = new Label
            {
                Text = "TOP SẢN PHẨM BỊ HOÀN TRẢ NHIỀU NHẤT:",
                Font = InvoiceTheme.BodyBoldFont,
                ForeColor = InvoiceTheme.Primary,
                Dock = DockStyle.Top,
                Height = 35
            };
            pnlLeft.Controls.Add(lblTopProdTitle);
            UiHelper.ApplyFlatGridStyle(_dgvTopProducts);
            SetupTopProductsGrid();
            pnlLeft.Controls.Add(_dgvTopProducts);
            _dgvTopProducts.BringToFront();

            // Right: Phân bổ theo lý do
            var pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = InvoiceTheme.CardBg, Padding = new Padding(15) };
            var lblReasonTitle = new Label
            {
                Text = "PHÂN BỔ THEO LÝ DO ĐỔI TRẢ:",
                Font = InvoiceTheme.BodyBoldFont,
                ForeColor = InvoiceTheme.Primary,
                Dock = DockStyle.Top,
                Height = 35
            };
            pnlRight.Controls.Add(lblReasonTitle);
            UiHelper.ApplyFlatGridStyle(_dgvByReason);
            SetupReasonGrid();
            pnlRight.Controls.Add(_dgvByReason);
            _dgvByReason.BringToFront();

            split.Controls.Add(pnlLeft, 0, 0);
            split.Controls.Add(pnlRight, 1, 0);

            Controls.Add(split);
            split.BringToFront();
        }

        private static void CreateKpiCard(Panel card, string title, string val, Color textCol, Color bgCol)
        {
            card.Size = new Size(270, 85);
            card.BackColor = bgCol;
            card.Margin = new Padding(0, 0, 15, 0);

            var lblT = new Label
            {
                Text = title,
                Font = InvoiceTheme.SmallFont,
                ForeColor = textCol,
                Location = new Point(15, 12),
                AutoSize = true
            };

            var lblV = new Label
            {
                Name = "lblVal",
                Text = val,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = textCol,
                Location = new Point(15, 35),
                AutoSize = true
            };

            card.Controls.AddRange([lblT, lblV]);
        }

        private static void UpdateKpiValue(Panel card, string val)
        {
            if (card.Controls["lblVal"] is Label lbl)
            {
                lbl.Text = val;
            }
        }

        private void SetupTopProductsGrid()
        {
            _dgvTopProducts.Dock = DockStyle.Fill;
            _dgvTopProducts.Columns.Clear();

            _dgvTopProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "Tên sản phẩm",
                DataPropertyName = "ProductName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _dgvTopProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colQty",
                HeaderText = "SL trả",
                DataPropertyName = "TotalReturnedQty",
                Width = 80,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = InvoiceTheme.BodyBoldFont }
            });

            _dgvTopProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRefund",
                HeaderText = "Tổng tiền hoàn",
                DataPropertyName = "TotalRefundAmountText",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = InvoiceTheme.BodyBoldFont, ForeColor = InvoiceTheme.Danger },
                Width = 140
            });

            _dgvTopProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReqCount",
                HeaderText = "Số yêu cầu",
                DataPropertyName = "RequestCount",
                Width = 90,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
        }

        private void SetupReasonGrid()
        {
            _dgvByReason.Dock = DockStyle.Fill;
            _dgvByReason.Columns.Clear();

            _dgvByReason.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReason",
                HeaderText = "Mã lý do",
                DataPropertyName = "ReasonCode",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _dgvByReason.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReqCount",
                HeaderText = "Số vụ",
                DataPropertyName = "RequestCount",
                Width = 80,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = InvoiceTheme.BodyBoldFont }
            });

            _dgvByReason.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRefund",
                HeaderText = "Tiền hoàn",
                DataPropertyName = "TotalRefundAmountText",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Width = 120
            });
        }

        private async Task LoadReportAsync()
        {
            DateTime tuNgay = _dtpTuNgay.Value.Date;
            DateTime denNgay = _dtpDenNgay.Value.Date.AddDays(1).AddTicks(-1);

            using var scope = _scopeFactory.CreateScope();
            var reportService = scope.ServiceProvider.GetRequiredService<IReturnReportService>();

            var report = await reportService.GetReportAsync(tuNgay, denNgay);

            // Cập nhật 4 KPI Cards
            UpdateKpiValue(_pnlKpiTotal, report.TotalRequests.ToString("N0"));

            double rate = report.TotalRequests > 0
                ? Math.Round((double)report.CompletedRequests / report.TotalRequests * 100, 1)
                : 0;
            UpdateKpiValue(_pnlKpiCompleted, $"{rate}% ({report.CompletedRequests}/{report.TotalRequests})");

            UpdateKpiValue(_pnlKpiRefund, report.TotalRefundAmount.ToString("N0") + "đ");
            UpdateKpiValue(_pnlKpiAvgTime, $"{report.AvgProcessingHours} giờ");

            // Nạp bảng sản phẩm
            _dgvTopProducts.DataSource = report.TopReturnedProducts.Select(p => new
            {
                p.ProductName,
                p.TotalReturnedQty,
                TotalRefundAmountText = p.TotalRefundAmount.ToString("N0") + "đ",
                p.RequestCount
            }).ToList();

            // Nạp bảng lý do
            _dgvByReason.DataSource = report.ByReason.Select(r => new
            {
                r.ReasonCode,
                r.RequestCount,
                TotalRefundAmountText = r.TotalRefundAmount.ToString("N0") + "đ"
            }).ToList();
        }

        private async Task RunSafeAsync(Func<Task> action)
        {
            Cursor = Cursors.WaitCursor;
            Enabled = false;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
            finally
            {
                Enabled = true;
                Cursor = Cursors.Default;
            }
        }
    }
}
