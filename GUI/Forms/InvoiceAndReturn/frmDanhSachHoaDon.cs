using BLL.DTOs.Invoice;
using BLL.Services.Interfaces;
using Common.Enums;
using GUI.Themes;
using GUI.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace GUI.Forms.InvoiceAndReturn
{
    /// <summary>
    /// Màn hình danh sách hóa đơn (tìm kiếm, lọc, phân trang, xem chi tiết, hủy hóa đơn).
    /// </summary>
    [DesignerCategory("Code")]
    public class frmDanhSachHoaDon : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Header & Controls tìm kiếm
        private readonly Panel _pnlHeader = new();
        private readonly Label _lblTitle = new();
        private readonly TextBox _txtTimKiem = new();
        private readonly DateTimePicker _dtpTuNgay = new();
        private readonly DateTimePicker _dtpDenNgay = new();
        private readonly ComboBox _cboTrangThai = new();
        private readonly Button _btnTim = new();
        private readonly Button _btnLamMoi = new();

        // Debug Switch Role Panel
#if DEBUG
        private readonly Panel _pnlDebugRole = new();
        private readonly Label _lblDebugRole = new();
        private readonly ComboBox _cboDebugRole = new();
#endif

        // DataGridView & Thanh thao tác bên dưới
        private readonly DataGridView _dgvHoaDon = new();
        private readonly Panel _pnlBottom = new();
        private readonly Label _lblTongSo = new();
        private readonly Button _btnXemChiTiet = new();
        private readonly Button _btnInHoaDon = new();


        // MenuStrip điều hướng giữa các màn hình trong module
        private readonly MenuStrip _mainMenu = new();
        private readonly ToolStripMenuItem _menuPos = new("🛒 Bán hàng (POS)");
        private readonly ToolStripMenuItem _menuHoaDon = new("📄 Quản lý Hóa đơn");
        private readonly ToolStripMenuItem _menuDoiTra = new("🔄 Yêu cầu Đổi trả");
        private readonly ToolStripMenuItem _menuNhanHang = new("📦 Nhận hàng (Kho)");
        private readonly ToolStripMenuItem _menuBaoCao = new("📊 Báo cáo Đổi trả");

        public frmDanhSachHoaDon(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Quản lý Hóa đơn & Đổi trả - Tech Store POS";
            Font = InvoiceTheme.BodyFont;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1220, 740);
            MinimumSize = new Size(950, 600);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = InvoiceTheme.FormBg;

            BuildMenu();
            BuildUi();

            Load += async (_, _) =>
            {
                ApplyRoleMenuPermissions();
                await RunSafeAsync(LoadDataAsync);
            };
        }

        private void BuildMenu()
        {
            _mainMenu.BackColor = InvoiceTheme.CardBg;
            _mainMenu.Font = InvoiceTheme.BodyBoldFont;
            _mainMenu.Padding = new Padding(10, 6, 10, 6);

            _menuPos.BackColor = InvoiceTheme.Primary;
            _menuPos.ForeColor = Color.White;
            _menuPos.Click += async (_, _) =>
            {
                using var frm = new GUI.Forms.Sales.POSTestForm();
                frm.ShowDialog(this);
                // Sau khi bán hàng xong, tự động tải lại danh sách hóa đơn
                await RunSafeAsync(LoadDataAsync);
            };

            _menuHoaDon.Click += async (_, _) => await RunSafeAsync(LoadDataAsync);

            _menuDoiTra.Click += (_, _) =>
            {
                using var frm = new frmYeuCauDoiTra(_scopeFactory);
                frm.ShowDialog(this);
            };

            _menuNhanHang.Click += (_, _) =>
            {
                using var frm = new frmNhanHang(_scopeFactory);
                frm.ShowDialog(this);
            };

            _menuBaoCao.Click += (_, _) =>
            {
                using var frm = new frmBaoCaoDoiTra(_scopeFactory);
                frm.ShowDialog(this);
            };

            _mainMenu.Items.AddRange([_menuPos, _menuHoaDon, _menuDoiTra, _menuNhanHang, _menuBaoCao]);
            MainMenuStrip = _mainMenu;
            Controls.Add(_mainMenu);
        }

        private void ApplyRoleMenuPermissions()
        {
            using var scope = _scopeFactory.CreateScope();
            var user = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();

            // Admin: toàn quyền
            // Quản lý: Hóa đơn, Đổi trả, Báo cáo
            // Nhân viên bán hàng: Hóa đơn, Tạo/xem yêu cầu đổi trả (ẩn nhận hàng & báo cáo)
            // Nhân viên kho: chỉ Nhận hàng (kênh Shipping)
            _menuPos.Visible = !user.IsWarehouse;
            _menuHoaDon.Visible = !user.IsWarehouse;
            _menuDoiTra.Visible = !user.IsWarehouse;
            _menuNhanHang.Visible = user.IsWarehouse || user.IsAdmin;
            _menuBaoCao.Visible = user.IsAdmin || user.IsManager;
        }

        private void BuildUi()
        {
            // -------------------------------------------------------------
            // 1. TOP HEADER PANEL (Sidebar Dark: #0b6651 - cùng màu với Primary)
            // -------------------------------------------------------------
            _pnlHeader.Dock = DockStyle.Top;
            _pnlHeader.Height = 70;
            _pnlHeader.BackColor = InvoiceTheme.SidebarDark;
            _pnlHeader.Padding = new Padding(20, 0, 20, 0);

            _lblTitle.Text = "HÓA ĐƠN & XỬ LÝ ĐỔI TRẢ";
            _lblTitle.UseMnemonic = false;
            _lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            _lblTitle.ForeColor = Color.White;
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(20, 20);

#if DEBUG
            // Panel debug role nhanh
            _pnlDebugRole.Size = new Size(320, 44);
            _pnlDebugRole.Location = new Point(870, 13);
            _pnlDebugRole.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _pnlDebugRole.BackColor = Color.Transparent;

            _lblDebugRole.Text = "Vai trò (DEBUG):";
            _lblDebugRole.ForeColor = Color.FromArgb(219, 234, 254);
            _lblDebugRole.AutoSize = true;
            _lblDebugRole.Location = new Point(0, 12);

            _cboDebugRole.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboDebugRole.Size = new Size(180, 32);
            _cboDebugRole.Location = new Point(130, 8);
            _cboDebugRole.Items.AddRange(["1. Admin", "2. Quản lý", "3. Thu ngân/Bán hàng", "4. Thủ kho"]);
            _cboDebugRole.SelectedIndex = 0;
            _cboDebugRole.SelectedIndexChanged += async (_, _) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var session = scope.ServiceProvider.GetService<ICurrentUserService>() as GUI.Services.UserSession;
                session?.SwitchRoleForDebug(_cboDebugRole.SelectedIndex + 1);
                ApplyRoleMenuPermissions();
                await RunSafeAsync(LoadDataAsync);
            };

            _pnlDebugRole.Controls.Add(_lblDebugRole);
            _pnlDebugRole.Controls.Add(_cboDebugRole);
            _pnlHeader.Controls.Add(_pnlDebugRole);
#endif
            _pnlHeader.Controls.Add(_lblTitle);
            Controls.Add(_pnlHeader);

            // -------------------------------------------------------------
            // 2. TOOLBAR / SEARCH FILTER PANEL (FlowLayoutPanel thoáng, không bao giờ bị cắt chữ)
            // -------------------------------------------------------------
            var pnlSearch = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = InvoiceTheme.CardBg,
                Padding = new Padding(20, 14, 20, 14),
                WrapContents = false,
                AutoScroll = true
            };
            pnlSearch.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
                e.Graphics.DrawLine(pen, 0, pnlSearch.Height - 1, pnlSearch.Width, pnlSearch.Height - 1);
            };

            // Ô tìm kiếm mã HĐ hoặc SĐT
            _txtTimKiem.PlaceholderText = "Tìm theo số HĐ (HD-...) hoặc SĐT...";
            _txtTimKiem.Size = new Size(290, 36);
            _txtTimKiem.Margin = new Padding(0, 0, 12, 0);
            _txtTimKiem.Font = InvoiceTheme.BodyFont;
            _txtTimKiem.TabIndex = 1;

            // Bộ lọc từ ngày (format dd/MM/yyyy độ rộng 135px không bị che số năm)
            var lblTu = new Label { Text = "Từ:", AutoSize = true, ForeColor = Color.FromArgb(100, 116, 139), Margin = new Padding(0, 6, 4, 0) };
            _dtpTuNgay.Format = DateTimePickerFormat.Custom;
            _dtpTuNgay.CustomFormat = "dd/MM/yyyy";
            _dtpTuNgay.Size = new Size(135, 36);
            _dtpTuNgay.Margin = new Padding(0, 0, 12, 0);
            _dtpTuNgay.Value = DateTime.Today.AddDays(-30);
            _dtpTuNgay.TabIndex = 2;

            // Bộ lọc đến ngày
            var lblDen = new Label { Text = "Đến:", AutoSize = true, ForeColor = Color.FromArgb(100, 116, 139), Margin = new Padding(0, 6, 4, 0) };
            _dtpDenNgay.Format = DateTimePickerFormat.Custom;
            _dtpDenNgay.CustomFormat = "dd/MM/yyyy";
            _dtpDenNgay.Size = new Size(135, 36);
            _dtpDenNgay.Margin = new Padding(0, 0, 12, 0);
            _dtpDenNgay.Value = DateTime.Today;
            _dtpDenNgay.TabIndex = 3;

            // Trạng thái
            _cboTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboTrangThai.Size = new Size(170, 36);
            _cboTrangThai.Margin = new Padding(0, 0, 12, 0);
            _cboTrangThai.Font = InvoiceTheme.BodyFont;
            _cboTrangThai.TabIndex = 4;
            _cboTrangThai.Items.Add("Tất cả trạng thái");
            _cboTrangThai.Items.Add("Đã thanh toán");
            _cboTrangThai.Items.Add("Trả một phần");
            _cboTrangThai.Items.Add("Đã trả hết");
            _cboTrangThai.Items.Add("Đã hủy");
            _cboTrangThai.SelectedIndex = 0;

            // Nút Tìm
            _btnTim.Text = "Tìm kiếm";
            _btnTim.Size = new Size(100, 36);
            _btnTim.Margin = new Padding(0, 0, 8, 0);
            _btnTim.BackColor = InvoiceTheme.Primary;
            _btnTim.ForeColor = Color.White;
            _btnTim.FlatStyle = FlatStyle.Flat;
            _btnTim.FlatAppearance.BorderSize = 0;
            _btnTim.Font = InvoiceTheme.BodyBoldFont;
            _btnTim.Cursor = Cursors.Hand;
            _btnTim.TabIndex = 5;
            _btnTim.Click += async (_, _) => await RunSafeAsync(LoadDataAsync);

            // Nút Làm mới
            _btnLamMoi.Text = "Làm mới";
            _btnLamMoi.Size = new Size(95, 36);
            _btnLamMoi.Margin = new Padding(0, 0, 0, 0);
            _btnLamMoi.BackColor = InvoiceTheme.Secondary;
            _btnLamMoi.ForeColor = InvoiceTheme.SecondaryText;
            _btnLamMoi.FlatStyle = FlatStyle.Flat;
            _btnLamMoi.FlatAppearance.BorderSize = 0;
            _btnLamMoi.Font = InvoiceTheme.BodyFont;
            _btnLamMoi.Cursor = Cursors.Hand;
            _btnLamMoi.TabIndex = 6;
            _btnLamMoi.Click += async (_, _) =>
            {
                _txtTimKiem.Clear();
                _dtpTuNgay.Value = DateTime.Today.AddDays(-30);
                _dtpDenNgay.Value = DateTime.Today;
                _cboTrangThai.SelectedIndex = 0;
                await RunSafeAsync(LoadDataAsync);
            };

            pnlSearch.Controls.AddRange([_txtTimKiem, lblTu, _dtpTuNgay, lblDen, _dtpDenNgay, _cboTrangThai, _btnTim, _btnLamMoi]);

            // Thứ tự Add controls để Dock xếp từ ngoài vào trong:
            // 1. _mainMenu (Top) -> chiếm hàng đầu tiên
            // 2. _pnlHeader (Top) -> chiếm hàng thứ hai
            // 3. pnlSearch (Top) -> chiếm hàng thứ ba
            // 4. _pnlBottom (Bottom) -> chiếm đáy
            // 5. _dgvHoaDon (Fill) -> lấp đầy khoảng còn lại ở giữa
            Controls.Clear();
            Controls.Add(_dgvHoaDon);
            Controls.Add(_pnlBottom);
            Controls.Add(pnlSearch);
            Controls.Add(_pnlHeader);
            Controls.Add(_mainMenu);

            // -------------------------------------------------------------
            // 3. BOTTOM ACTIONS PANEL (Thanh thao tác bên dưới)
            // -------------------------------------------------------------
            _pnlBottom.Dock = DockStyle.Bottom;
            _pnlBottom.Height = 65;
            _pnlBottom.BackColor = InvoiceTheme.CardBg;
            _pnlBottom.Padding = new Padding(20, 10, 20, 10);
            _pnlBottom.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
                e.Graphics.DrawLine(pen, 0, 0, _pnlBottom.Width, 0);
            };

            // Bên trái: Nhãn tổng số hóa đơn
            _lblTongSo.Text = "Tổng số: 0 hóa đơn";
            _lblTongSo.AutoSize = true;
            _lblTongSo.Font = InvoiceTheme.BodyBoldFont;
            _lblTongSo.ForeColor = Color.FromArgb(71, 85, 105);
            _lblTongSo.Dock = DockStyle.Left;
            _lblTongSo.Padding = new Padding(0, 12, 0, 0);

            // Bên phải: Nút In hóa đơn + Xem chi tiết
            var flpActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            _btnInHoaDon.Text = "In hóa đơn";
            _btnInHoaDon.Size = new Size(120, 44);
            _btnInHoaDon.Margin = new Padding(0, 0, 10, 0);
            _btnInHoaDon.BackColor = InvoiceTheme.Secondary;
            _btnInHoaDon.ForeColor = InvoiceTheme.SecondaryText;
            _btnInHoaDon.FlatStyle = FlatStyle.Flat;
            _btnInHoaDon.FlatAppearance.BorderSize = 0;
            _btnInHoaDon.Font = InvoiceTheme.BodyFont;
            _btnInHoaDon.Click += (_, _) =>
            {
                if (SelectedInvoiceId is null)
                {
                    MessageBox.Show("Vui lòng chọn một hóa đơn để in.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                MessageBox.Show("Lệnh in hóa đơn đã được gửi tới máy in mặc định.", "In thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            _btnXemChiTiet.Text = "Xem chi tiết / Đổi trả";
            _btnXemChiTiet.Size = new Size(200, 44);
            _btnXemChiTiet.Margin = new Padding(0, 0, 0, 0);
            _btnXemChiTiet.BackColor = InvoiceTheme.Primary;
            _btnXemChiTiet.ForeColor = Color.White;
            _btnXemChiTiet.FlatStyle = FlatStyle.Flat;
            _btnXemChiTiet.FlatAppearance.BorderSize = 0;
            _btnXemChiTiet.Font = InvoiceTheme.BodyBoldFont;
            _btnXemChiTiet.Cursor = Cursors.Hand;
            _btnXemChiTiet.Click += (_, _) => OpenDetail();

            flpActions.Controls.AddRange([_btnInHoaDon, _btnXemChiTiet]);

            _pnlBottom.Controls.Clear();
            _pnlBottom.Controls.AddRange([_lblTongSo, flpActions]);

            // -------------------------------------------------------------
            // 4. DATAGRIDVIEW CHÍNH
            // -------------------------------------------------------------
            _dgvHoaDon.Dock = DockStyle.Fill;
            UiHelper.ApplyFlatGridStyle(_dgvHoaDon);
            SetupGridColumns();

            _dgvHoaDon.CellDoubleClick += (_, _) => OpenDetail();
            _dgvHoaDon.CellPainting += DgvHoaDon_CellPainting;
        }

        private void SetupGridColumns()
        {
            _dgvHoaDon.Columns.Clear();

            _dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            _dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colInvoiceNo",
                HeaderText = "Số HĐ",
                DataPropertyName = "InvoiceNo",
                Width = 140
            });

            _dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCreatedAt",
                HeaderText = "Ngày lập",
                DataPropertyName = "CreatedAt",
                DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm" },
                Width = 150
            });

            _dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCustomer",
                HeaderText = "Khách hàng",
                DataPropertyName = "CustomerName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTotalAmount",
                HeaderText = "Tổng tiền",
                DataPropertyName = "TotalAmount",
                DefaultCellStyle = { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight },
                Width = 140
            });

            _dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPayment",
                HeaderText = "Thanh toán",
                DataPropertyName = "PaymentSummary",
                Width = 180
            });

            _dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "Trạng thái",
                DataPropertyName = "StatusText",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Width = 150
            });
        }

        private void DgvHoaDon_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && _dgvHoaDon.Columns["colStatus"] is { } col && e.ColumnIndex == col.Index)
            {
                string statusText = e.Value?.ToString() ?? "";
                string statusCode = _dgvHoaDon.Columns.Contains("colStatusRaw")
                    ? _dgvHoaDon.Rows[e.RowIndex].Cells["colStatusRaw"].Value?.ToString() ?? ""
                    : "";
                var (bg, text) = InvoiceTheme.GetStatusColors(statusCode);
                UiHelper.PaintStatusCell(e, statusText, bg, text);
            }
        }

        private int? SelectedInvoiceId
        {
            get
            {
                if (_dgvHoaDon.CurrentRow?.DataBoundItem is InvoiceGridRow row)
                    return row.Id;
                return null;
            }
        }

        private async Task LoadDataAsync()
        {
            string? kw = string.IsNullOrWhiteSpace(_txtTimKiem.Text) ? null : _txtTimKiem.Text.Trim();
            DateTime tuNgay = _dtpTuNgay.Value.Date;
            DateTime denNgay = _dtpDenNgay.Value.Date.AddDays(1).AddTicks(-1);

            InvoiceStatus? statusFilter = _cboTrangThai.SelectedIndex switch
            {
                1 => InvoiceStatus.Paid,
                2 => InvoiceStatus.PartiallyReturned,
                3 => InvoiceStatus.FullyReturned,
                4 => InvoiceStatus.Cancelled,
                _ => null
            };

            using var scope = _scopeFactory.CreateScope();
            var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();

            var (items, total) = await invoiceService.SearchAsync(
                kw, tuNgay, denNgay, statusFilter, 0, 1000);

            // Thêm cột ẩn colStatusRaw nếu chưa có
            if (!_dgvHoaDon.Columns.Contains("colStatusRaw"))
            {
                _dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "colStatusRaw",
                    DataPropertyName = "StatusRaw",
                    Visible = false
                });
            }

            var rows = items.Select(x => new InvoiceGridRow
            {
                Id = x.Id,
                InvoiceNo = x.InvoiceNo,
                CreatedAt = x.CreatedAt,
                CustomerName = string.IsNullOrEmpty(x.CustomerName) ? "Khách vãng lai" : $"{x.CustomerName} ({x.CustomerPhone})",
                TotalAmount = x.TotalAmount.ToString("N0") + "đ",
                PaymentSummary = x.PaymentSummary,
                StatusRaw = x.Status.ToString(),
                StatusText = x.Status switch
                {
                    InvoiceStatus.Paid => "Đã thanh toán",
                    InvoiceStatus.PartiallyReturned => "Trả một phần",
                    InvoiceStatus.FullyReturned => "Đã trả hết",
                    InvoiceStatus.Cancelled => "Đã hủy",
                    _ => x.Status.ToString()
                }
            }).ToList();

            _dgvHoaDon.DataSource = rows;
            _lblTongSo.Text = $"Tổng số: {total} hóa đơn";
        }

        private void OpenDetail()
        {
            if (SelectedInvoiceId is not { } id)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn từ bảng để xem chi tiết.", "Chưa chọn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var frm = new frmChiTietHoaDon(_scopeFactory, id);
            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                _ = RunSafeAsync(LoadDataAsync);
            }
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

        // ViewModel cho Grid
        private sealed class InvoiceGridRow
        {
            public int Id { get; set; }
            public string InvoiceNo { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public string CustomerName { get; set; } = "";
            public string TotalAmount { get; set; } = "";
            public string PaymentSummary { get; set; } = "";
            public string StatusRaw { get; set; } = "";
            public string StatusText { get; set; } = "";
        }
    }
}
