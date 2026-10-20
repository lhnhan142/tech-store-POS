using BLL.DTOs.Invoice;
using BLL.DTOs.Return;
using BLL.Services.Interfaces;
using Common.Enums;
using DAL.Repositories.Interfaces;
using GUI.Themes;
using GUI.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace GUI.Forms.InvoiceAndReturn
{
    /// <summary>
    /// Màn hình chi tiết hóa đơn: xem thông tin khách, thanh toán, hạn đổi trả,
    /// danh sách sản phẩm (chọn và nhập số lượng đổi trả), khu tạo yêu cầu đổi/trả hàng.
    /// </summary>
    [DesignerCategory("Code")]
    public class frmChiTietHoaDon : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int _invoiceId;
        private InvoiceDetailDto? _invoice;

        // Header
        private readonly Panel _pnlHeader = new();
        private readonly Label _lblTitle = new();
        private readonly Label _lblDaysRemaining = new();

        // Left Panel: Thông tin chung & Lưới dòng hàng
        private readonly Panel _pnlLeft = new();
        private readonly GroupBox _grpCustomer = new();
        private readonly Label _lblCustomerInfo = new();
        private readonly Label _lblPaymentInfo = new();
        private readonly DataGridView _dgvItems = new();

        // Right Panel: Khu vực tạo yêu cầu đổi/trả hàng
        private readonly Panel _pnlRight = new();
        private readonly Label _lblRightTitle = new();
        private readonly RadioButton _radReturn = new();
        private readonly RadioButton _radExchange = new();
        private readonly Label _lblChannel = new();
        private readonly ComboBox _cboChannel = new();
        private readonly Label _lblReason = new();
        private readonly ComboBox _cboReason = new();
        private readonly TextBox _txtReasonNote = new();
        private readonly Button _btnAttachImage = new();
        private readonly Label _lblAttachStatus = new();
        private readonly List<string> _attachmentPaths = new();

        // Tùy chọn đổi hàng (Exchange)
        private readonly Panel _pnlExchangeOptions = new();
        private readonly Label _lblNewProduct = new();
        private readonly ComboBox _cboNewProduct = new();
        private readonly Label _lblExchangeDiff = new();

        // Tổng kết tiền hoàn
        private readonly Label _lblTotalRefundPreview = new();
        private readonly Button _btnTaoYeuCau = new();
        private readonly Button _btnHuyHoaDon = new();

        public frmChiTietHoaDon(IServiceScopeFactory scopeFactory, int invoiceId)
        {
            _scopeFactory = scopeFactory;
            _invoiceId = invoiceId;

            Text = "Chi tiết Hóa đơn & Xử lý Đổi trả - Tech Store POS";
            Font = InvoiceTheme.BodyFont;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1260, 780);
            MinimumSize = new Size(1100, 680);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = InvoiceTheme.FormBg;

            BuildUi();

            Load += async (_, _) => await RunSafeAsync(LoadInvoiceAsync);
        }

        private void BuildUi()
        {
            // -------------------------------------------------------------
            // 1. TOP HEADER PANEL
            // -------------------------------------------------------------
            _pnlHeader.Dock = DockStyle.Top;
            _pnlHeader.Height = 65;
            _pnlHeader.BackColor = InvoiceTheme.SidebarDark;
            _pnlHeader.Padding = new Padding(20, 0, 20, 0);

            _lblTitle.Text = "CHI TIẾT HÓA ĐƠN";
            _lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            _lblTitle.ForeColor = Color.White;
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(20, 18);

            _lblDaysRemaining.Font = InvoiceTheme.BodyBoldFont;
            _lblDaysRemaining.ForeColor = Color.FromArgb(219, 234, 254);
            _lblDaysRemaining.AutoSize = true;
            _lblDaysRemaining.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _lblDaysRemaining.Location = new Point(950, 22);

            _pnlHeader.Controls.AddRange([_lblTitle, _lblDaysRemaining]);
            Controls.Add(_pnlHeader);

            // -------------------------------------------------------------
            // 2. MAIN SPLIT: LEFT (Grid dòng hàng) & RIGHT (Tạo yêu cầu)
            // -------------------------------------------------------------
            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(15)
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64)); // Bên trái: 64%
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36)); // Bên phải: 36%

            BuildLeftPanel();
            BuildRightPanel();

            split.Controls.Add(_pnlLeft, 0, 0);
            split.Controls.Add(_pnlRight, 1, 0);

            Controls.Add(split);
            split.BringToFront();
        }

        private void BuildLeftPanel()
        {
            _pnlLeft.Dock = DockStyle.Fill;
            _pnlLeft.BackColor = InvoiceTheme.CardBg;
            _pnlLeft.Padding = new Padding(15);

            // Thông tin khách hàng & thanh toán
            _grpCustomer.Text = "Thông tin hóa đơn & Khách hàng";
            _grpCustomer.Dock = DockStyle.Top;
            _grpCustomer.Height = 100;
            _grpCustomer.Font = InvoiceTheme.BodyBoldFont;

            _lblCustomerInfo.Font = InvoiceTheme.BodyFont;
            _lblCustomerInfo.ForeColor = Color.FromArgb(30, 41, 59);
            _lblCustomerInfo.Location = new Point(15, 25);
            _lblCustomerInfo.Size = new Size(400, 60);

            _lblPaymentInfo.Font = InvoiceTheme.BodyFont;
            _lblPaymentInfo.ForeColor = Color.FromArgb(30, 41, 59);
            _lblPaymentInfo.Location = new Point(420, 25);
            _lblPaymentInfo.Size = new Size(320, 60);

            _grpCustomer.Controls.AddRange([_lblCustomerInfo, _lblPaymentInfo]);
            _pnlLeft.Controls.Add(_grpCustomer);

            // Bảng sản phẩm
            var pnlGridHeader = new Panel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(0, 10, 0, 5) };
            var lblGridTitle = new Label
            {
                Text = "DANH SÁCH SẢN PHẨM MUA (Tick chọn và nhập số lượng muốn đổi/trả):",
                Font = InvoiceTheme.BodyBoldFont,
                ForeColor = InvoiceTheme.Primary,
                Dock = DockStyle.Fill
            };
            pnlGridHeader.Controls.Add(lblGridTitle);
            _pnlLeft.Controls.Add(pnlGridHeader);

            _dgvItems.Dock = DockStyle.Fill;
            UiHelper.ApplyFlatGridStyle(_dgvItems);
            SetupItemsGridColumns();

            _dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            _dgvItems.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_dgvItems.IsCurrentCellDirty)
                    _dgvItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _dgvItems.RowPrePaint += DgvItems_RowPrePaint;

            _pnlLeft.Controls.Add(_dgvItems);
            _dgvItems.BringToFront();

            // Nút Hủy hóa đơn ở góc dưới bên trái
            var pnlLeftBottom = new Panel { Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(0, 10, 0, 0) };
            _btnHuyHoaDon.Text = "Hủy toàn bộ hóa đơn này";
            _btnHuyHoaDon.Size = new Size(200, 44);
            _btnHuyHoaDon.BackColor = InvoiceTheme.Secondary;
            _btnHuyHoaDon.ForeColor = InvoiceTheme.Danger;
            _btnHuyHoaDon.FlatStyle = FlatStyle.Flat;
            _btnHuyHoaDon.FlatAppearance.BorderColor = InvoiceTheme.Danger;
            _btnHuyHoaDon.Font = InvoiceTheme.BodyBoldFont;
            _btnHuyHoaDon.Cursor = Cursors.Hand;
            _btnHuyHoaDon.Click += async (_, _) => await CancelInvoiceAsync();
            pnlLeftBottom.Controls.Add(_btnHuyHoaDon);
            _pnlLeft.Controls.Add(pnlLeftBottom);
        }

        private void SetupItemsGridColumns()
        {
            _dgvItems.Columns.Clear();

            // Cột checkbox Chọn
            var chkCol = new DataGridViewCheckBoxColumn
            {
                Name = "colSelect",
                HeaderText = "Chọn",
                DataPropertyName = "IsSelected",
                Width = 55
            };
            _dgvItems.Columns.Add(chkCol);

            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colProductName",
                HeaderText = "Sản phẩm",
                DataPropertyName = "ProductName",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colQty",
                HeaderText = "Đã mua",
                DataPropertyName = "Qty",
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Width = 75
            });

            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReturnedQty",
                HeaderText = "Đã trả",
                DataPropertyName = "ReturnedQty",
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Width = 75
            });

            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colAvailable",
                HeaderText = "Còn trả được",
                DataPropertyName = "Available",
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = InvoiceTheme.BodyBoldFont, ForeColor = InvoiceTheme.Primary },
                Width = 105
            });

            // Cột nhập số lượng trả
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReturnQty",
                HeaderText = "SL trả",
                DataPropertyName = "ReturnQty",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, BackColor = Color.FromArgb(254, 243, 199) },
                Width = 80
            });

            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRefundAmount",
                HeaderText = "Hoàn tạm tính",
                DataPropertyName = "RefundAmountText",
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = InvoiceTheme.BodyBoldFont },
                Width = 120
            });
        }

        private void BuildRightPanel()
        {
            _pnlRight.Dock = DockStyle.Fill;
            _pnlRight.BackColor = InvoiceTheme.CardBg;
            _pnlRight.Padding = new Padding(20, 15, 20, 15);
            _pnlRight.AutoScroll = true;

            int y = 15;

            _lblRightTitle.Text = "TẠO YÊU CẦU ĐỔI / TRẢ";
            _lblRightTitle.Font = InvoiceTheme.SubHeaderFont;
            _lblRightTitle.ForeColor = InvoiceTheme.Primary;
            _lblRightTitle.Location = new Point(20, y);
            _lblRightTitle.AutoSize = true;
            _pnlRight.Controls.Add(_lblRightTitle);
            y += 35;

            // Loại yêu cầu (Radio buttons)
            _radReturn.Text = "Trả hàng hoàn tiền";
            _radReturn.Checked = true;
            _radReturn.Location = new Point(25, y);
            _radReturn.AutoSize = true;
            _radReturn.Font = InvoiceTheme.BodyBoldFont;
            _radReturn.CheckedChanged += (_, _) => ToggleExchangeOptions();

            _radExchange.Text = "Đổi sản phẩm khác";
            _radExchange.Location = new Point(190, y);
            _radExchange.AutoSize = true;
            _radExchange.Font = InvoiceTheme.BodyBoldFont;
            _radExchange.CheckedChanged += (_, _) => ToggleExchangeOptions();

            _pnlRight.Controls.AddRange([_radReturn, _radExchange]);
            y += 40;

            // Kênh tiếp nhận
            _lblChannel.Text = "Kênh tiếp nhận:";
            _lblChannel.Location = new Point(20, y);
            _lblChannel.AutoSize = true;
            _lblChannel.ForeColor = Color.FromArgb(71, 85, 105);
            _pnlRight.Controls.Add(_lblChannel);
            y += 22;

            _cboChannel.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboChannel.Items.AddRange(["Tại quầy (Counter)", "Gửi hàng / Vận chuyển (Shipping)"]);
            _cboChannel.SelectedIndex = 0;
            _cboChannel.Location = new Point(20, y);
            _cboChannel.Size = new Size(350, 44);
            _cboChannel.Font = InvoiceTheme.BodyFont;
            _pnlRight.Controls.Add(_cboChannel);
            y += 50;

            // Lý do đổi trả
            _lblReason.Text = "Lý do đổi trả:";
            _lblReason.Location = new Point(20, y);
            _lblReason.AutoSize = true;
            _lblReason.ForeColor = Color.FromArgb(71, 85, 105);
            _pnlRight.Controls.Add(_lblReason);
            y += 22;

            _cboReason.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboReason.Items.AddRange(["Lỗi kỹ thuật / Hỏng hóc", "Đổi ý / Không vừa ý", "Giao sai sản phẩm", "Lý do khác"]);
            _cboReason.SelectedIndex = 0;
            _cboReason.Location = new Point(20, y);
            _cboReason.Size = new Size(350, 44);
            _cboReason.Font = InvoiceTheme.BodyFont;
            _pnlRight.Controls.Add(_cboReason);
            y += 50;

            // Ghi chú chi tiết
            _txtReasonNote.PlaceholderText = "Nhập mô tả chi tiết lý do của khách hàng...";
            _txtReasonNote.Multiline = true;
            _txtReasonNote.Location = new Point(20, y);
            _txtReasonNote.Size = new Size(350, 65);
            _txtReasonNote.Font = InvoiceTheme.BodyFont;
            _pnlRight.Controls.Add(_txtReasonNote);
            y += 75;

            // Đính kèm hình ảnh
            _btnAttachImage.Text = "📷 Đính kèm ảnh minh họa";
            _btnAttachImage.Size = new Size(200, 44);
            _btnAttachImage.Location = new Point(20, y);
            _btnAttachImage.BackColor = InvoiceTheme.Secondary;
            _btnAttachImage.ForeColor = InvoiceTheme.SecondaryText;
            _btnAttachImage.FlatStyle = FlatStyle.Flat;
            _btnAttachImage.FlatAppearance.BorderSize = 0;
            _btnAttachImage.Click += (_, _) => AttachImage();

            _lblAttachStatus.Text = "0 file đính kèm";
            _lblAttachStatus.Location = new Point(230, y + 12);
            _lblAttachStatus.AutoSize = true;
            _lblAttachStatus.ForeColor = Color.FromArgb(100, 116, 139);

            _pnlRight.Controls.AddRange([_btnAttachImage, _lblAttachStatus]);
            y += 55;

            // Khu vực đổi hàng (chọn món mới)
            _pnlExchangeOptions.Size = new Size(350, 90);
            _pnlExchangeOptions.Location = new Point(20, y);
            _pnlExchangeOptions.BackColor = Color.FromArgb(248, 250, 252);
            _pnlExchangeOptions.Visible = false;

            _lblNewProduct.Text = "Chọn sản phẩm đổi mới:";
            _lblNewProduct.Location = new Point(10, 8);
            _lblNewProduct.AutoSize = true;
            _lblNewProduct.Font = InvoiceTheme.BodyBoldFont;

            _cboNewProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboNewProduct.Location = new Point(10, 30);
            _cboNewProduct.Size = new Size(330, 35);
            _cboNewProduct.SelectedIndexChanged += (_, _) => UpdateExchangeDiff();

            _lblExchangeDiff.Text = "Chênh lệch bù/hoàn: 0đ";
            _lblExchangeDiff.Location = new Point(10, 68);
            _lblExchangeDiff.AutoSize = true;
            _lblExchangeDiff.ForeColor = InvoiceTheme.Primary;
            _lblExchangeDiff.Font = InvoiceTheme.BodyBoldFont;

            _pnlExchangeOptions.Controls.AddRange([_lblNewProduct, _cboNewProduct, _lblExchangeDiff]);
            _pnlRight.Controls.Add(_pnlExchangeOptions);
            y += 100;

            // Tổng tiền hoàn tạm tính
            _lblTotalRefundPreview.Text = "Tổng tiền hoàn: 0đ";
            _lblTotalRefundPreview.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            _lblTotalRefundPreview.ForeColor = InvoiceTheme.Danger;
            _lblTotalRefundPreview.Location = new Point(20, y);
            _lblTotalRefundPreview.AutoSize = true;
            _pnlRight.Controls.Add(_lblTotalRefundPreview);
            y += 45;

            // Nút Tạo yêu cầu
            _btnTaoYeuCau.Text = "TẠO YÊU CẦU ĐỔI TRẢ";
            _btnTaoYeuCau.Size = new Size(350, 48);
            _btnTaoYeuCau.Location = new Point(20, y);
            _btnTaoYeuCau.BackColor = InvoiceTheme.Primary;
            _btnTaoYeuCau.ForeColor = Color.White;
            _btnTaoYeuCau.FlatStyle = FlatStyle.Flat;
            _btnTaoYeuCau.FlatAppearance.BorderSize = 0;
            _btnTaoYeuCau.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _btnTaoYeuCau.Cursor = Cursors.Hand;
            _btnTaoYeuCau.Click += async (_, _) => await CreateReturnRequestAsync();
            _pnlRight.Controls.Add(_btnTaoYeuCau);
        }

        private async Task LoadInvoiceAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();

            _invoice = await invoiceService.GetDetailAsync(_invoiceId);

            _lblTitle.Text = $"HÓA ĐƠN {_invoice.InvoiceNo} - {GetStatusVietnamese(_invoice.Status)}";

            string daysNotice = _invoice.DaysRemainingForReturn >= 0
                ? $"Còn {_invoice.DaysRemainingForReturn} ngày đổi trả"
                : $"ĐÃ QUÁ HẠN {_invoice.DaysRemainingForReturn * -1} NGÀY (Cần duyệt đặc biệt)";
            _lblDaysRemaining.Text = daysNotice;
            _lblDaysRemaining.ForeColor = _invoice.DaysRemainingForReturn >= 0
                ? Color.FromArgb(219, 234, 254)
                : Color.FromArgb(254, 202, 202);

            _lblCustomerInfo.Text = $"Khách hàng: {_invoice.CustomerName ?? "Khách vãng lai"}\n" +
                                   $"SĐT: {_invoice.CustomerPhone ?? "—"}\n" +
                                   $"Ngày lập: {_invoice.CreatedAt:dd/MM/yyyy HH:mm}";

            string payments = string.Join(", ", _invoice.Payments.Select(p => $"{p.MethodDisplayName}: {p.Amount:N0}đ"));
            _lblPaymentInfo.Text = $"Tổng tiền: {_invoice.TotalAmount:N0}đ\n" +
                                  $"Thanh toán: {payments}\n" +
                                  $"Người lập: {_invoice.CreatedByUserName ?? "—"}";

            // Nạp sản phẩm vào grid
            var rows = _invoice.Items.Select(i => new InvoiceItemRow
            {
                InvoiceItemId = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Qty = i.Qty,
                ReturnedQty = i.ReturnedQty,
                Available = i.Available,
                IsReturnable = i.IsReturnable,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal,
                IsSelected = false,
                ReturnQty = 0,
                RefundAmount = 0
            }).ToList();

            _dgvItems.DataSource = rows;

            // Nạp sản phẩm cho dropdown đổi hàng
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var allProducts = (await uow.Products.GetAllAsync()).OrderBy(p => p.Name).ToList();
            _cboNewProduct.DataSource = allProducts;
            _cboNewProduct.DisplayMember = "Name";
            _cboNewProduct.ValueMember = "Id";

            // Vô hiệu hóa nút tạo yêu cầu nếu hóa đơn đã hủy hoặc đã trả hết
            bool canReturn = _invoice.Status is not (InvoiceStatus.Cancelled or InvoiceStatus.FullyReturned);
            _btnTaoYeuCau.Enabled = canReturn;
            _btnHuyHoaDon.Enabled = _invoice.Status == InvoiceStatus.Paid;
        }

        private void DgvItems_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = _dgvItems.Rows[e.RowIndex].DataBoundItem as InvoiceItemRow;
            if (row == null) return;

            // Nếu người dùng tick chọn nhưng chưa nhập SL trả -> mặc định điền Available
            if (_dgvItems.Columns["colSelect"] is { } colSelect && e.ColumnIndex == colSelect.Index)
            {
                if (row.IsSelected && row.ReturnQty == 0)
                {
                    row.ReturnQty = row.Available;
                }
                else if (!row.IsSelected)
                {
                    row.ReturnQty = 0;
                }
            }

            // Kiểm tra số lượng trả không vượt quá Available
            if (row.ReturnQty > row.Available)
            {
                MessageBox.Show(
                    $"Số lượng trả ({row.ReturnQty}) không được vượt quá số lượng còn khả dụng ({row.Available}).",
                    "Số lượng vượt mức", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                row.ReturnQty = row.Available;
            }
            if (row.ReturnQty < 0) row.ReturnQty = 0;

            // Tính tiền hoàn tạm tính
            if (row.Qty > 0)
            {
                decimal unitRefund = row.LineTotal / row.Qty;
                row.RefundAmount = Math.Round(unitRefund * row.ReturnQty, 2, MidpointRounding.ToEven);
            }
            else
            {
                row.RefundAmount = 0;
            }

            _dgvItems.InvalidateRow(e.RowIndex);
            RecalculateTotalRefund();
        }

        private void DgvItems_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _dgvItems.Rows[e.RowIndex].DataBoundItem as InvoiceItemRow;
            if (row == null) return;

            // Tô xám và khóa nếu IsReturnable = false hoặc Available = 0
            if (!row.IsReturnable || row.Available <= 0)
            {
                _dgvItems.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(243, 244, 246);
                _dgvItems.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(156, 163, 175);
                _dgvItems.Rows[e.RowIndex].Cells["colSelect"].ReadOnly = true;
                _dgvItems.Rows[e.RowIndex].Cells["colReturnQty"].ReadOnly = true;
            }
        }

        private void RecalculateTotalRefund()
        {
            if (_dgvItems.DataSource is not List<InvoiceItemRow> items) return;

            decimal total = items.Where(x => x.IsSelected).Sum(x => x.RefundAmount);
            _lblTotalRefundPreview.Text = $"Tổng tiền hoàn: {total:N0}đ";

            UpdateExchangeDiff();
        }

        private void ToggleExchangeOptions()
        {
            _pnlExchangeOptions.Visible = _radExchange.Checked;
            UpdateExchangeDiff();
        }

        private void UpdateExchangeDiff()
        {
            if (!_radExchange.Checked) return;

            if (_dgvItems.DataSource is not List<InvoiceItemRow> items) return;
            decimal totalRefund = items.Where(x => x.IsSelected).Sum(x => x.RefundAmount);

            if (_cboNewProduct.SelectedItem is DAL.Entities.Product newProd)
            {
                decimal diff = newProd.Price - totalRefund;
                if (diff > 0)
                    _lblExchangeDiff.Text = $"Khách cần bù thêm: +{diff:N0}đ";
                else if (diff < 0)
                    _lblExchangeDiff.Text = $"Hoàn lại khách: {Math.Abs(diff):N0}đ";
                else
                    _lblExchangeDiff.Text = "Đổi ngang giá (0đ)";
            }
        }

        private void AttachImage()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh chứng minh hàng lỗi/hỏng",
                Filter = "Hình ảnh (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Lưu file vào thư mục riêng của module: AppData hoặc Images\Returns
                    string returnDir = Path.Combine(Application.StartupPath, "Images", "Returns");
                    if (!Directory.Exists(returnDir)) Directory.CreateDirectory(returnDir);

                    string fileName = $"return_{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(ofd.FileName)}";
                    string destPath = Path.Combine(returnDir, fileName);
                    File.Copy(ofd.FileName, destPath, overwrite: true);

                    _attachmentPaths.Add(destPath);
                    _lblAttachStatus.Text = $"{_attachmentPaths.Count} file đính kèm";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể lưu ảnh đính kèm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task CreateReturnRequestAsync()
        {
            if (_dgvItems.DataSource is not List<InvoiceItemRow> items) return;

            var selectedItems = items.Where(x => x.IsSelected && x.ReturnQty > 0).ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng tick chọn ít nhất 1 sản phẩm và nhập số lượng trả > 0.",
                    "Chưa chọn sản phẩm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new CreateReturnRequestDto
            {
                InvoiceId = _invoiceId,
                Type = _radExchange.Checked ? ReturnType.Exchange : ReturnType.Return,
                Channel = _cboChannel.SelectedIndex == 1 ? ReturnChannel.Shipping : ReturnChannel.Counter,
                ReasonCode = _cboReason.SelectedItem?.ToString(),
                ReasonNote = _txtReasonNote.Text.Trim(),
                Items = selectedItems.Select(x => new CreateReturnItemDto
                {
                    InvoiceItemId = x.InvoiceItemId,
                    Qty = x.ReturnQty
                }).ToList(),
                AttachmentPaths = _attachmentPaths
            };

            await RunSafeAsync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();

                string requestNo = await returnService.CreateRequestAsync(dto);

                MessageBox.Show(
                    $"Tạo yêu cầu đổi trả thành công!\nMã yêu cầu: {requestNo}\n" +
                    "Yêu cầu đã được ghi nhận và chuyển tới bộ phận xử lý.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            });
        }

        private async Task CancelInvoiceAsync()
        {
            var confirm = MessageBox.Show(
                "Bạn có chắc chắn muốn hủy toàn bộ hóa đơn này không?\n" +
                "Thao tác này chỉ dành cho Quản lý / Admin và không thể hoàn tác.",
                "Xác nhận hủy hóa đơn", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            await RunSafeAsync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();

                await invoiceService.CancelAsync(_invoiceId, "Khách hàng hủy đơn / trả toàn bộ");

                MessageBox.Show("Đã hủy hóa đơn thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            });
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

        private static string GetStatusVietnamese(InvoiceStatus status) => status switch
        {
            InvoiceStatus.Paid => "ĐÃ THANH TOÁN",
            InvoiceStatus.PartiallyReturned => "TRẢ MỘT PHẦN",
            InvoiceStatus.FullyReturned => "ĐÃ TRẢ HẾT",
            InvoiceStatus.Cancelled => "ĐÃ HỦY",
            _ => status.ToString()
        };

        // Row model cho DataGridView dòng hàng
        private sealed class InvoiceItemRow
        {
            public int InvoiceItemId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public int Qty { get; set; }
            public int ReturnedQty { get; set; }
            public int Available { get; set; }
            public bool IsReturnable { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal LineTotal { get; set; }

            public bool IsSelected { get; set; }
            public int ReturnQty { get; set; }
            public decimal RefundAmount { get; set; }
            public string RefundAmountText => RefundAmount.ToString("N0") + "đ";
        }
    }
}
