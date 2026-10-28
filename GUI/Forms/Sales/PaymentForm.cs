using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL.DTOs;
using Common.Enums;
using GUI.Services;

namespace GUI.Forms.Sales
{
    /// <summary>
    /// Màn hình thanh toán đơn hàng POS cỡ lớn với tông màu Trắng & Xanh nước biển hiện đại.
    /// </summary>
    [DesignerCategory("Code")]
    public partial class PaymentForm : Form
    {
        // ==================== BẢNG MÀU CHỦ ĐẠO (TRẮNG & XANH NƯỚC BIỂN) ====================
        private static readonly Color OceanPrimary = Color.FromArgb(2, 132, 199);     // Xanh nước biển tươi sáng (#0284c7)
        private static readonly Color OceanHover = Color.FromArgb(3, 105, 161);       // Xanh nước biển đậm hơn khi hover (#0369a1)
        private static readonly Color OceanDeep = Color.FromArgb(14, 85, 158);        // Xanh biển sâu (#0e559e)
        private static readonly Color OceanLight = Color.FromArgb(240, 249, 255);     // Xanh nước biển nhạt làm nền thẻ (#f0f9ff)
        private static readonly Color OceanBorder = Color.FromArgb(186, 230, 253);    // Viền xanh nước biển nhẹ (#bae6fd)
        private static readonly Color FormBg = Color.FromArgb(245, 249, 254);         // Nền form sáng dịu
        private static readonly Color CardBg = Color.White;                           // Nền thẻ trắng tinh
        private static readonly Color TextMain = Color.FromArgb(15, 23, 42);          // Chữ màu tối chuẩn (#0f172a)
        private static readonly Color TextMuted = Color.FromArgb(100, 116, 139);      // Chữ phụ xám nhạt (#64748b)
        private static readonly Color SuccessGreen = Color.FromArgb(16, 185, 129);    // Xanh lá tiền thừa (#10b981)
        private static readonly Color DangerRed = Color.FromArgb(239, 68, 68);        // Đỏ cảnh báo thiếu tiền (#ef4444)

        private static readonly CultureInfo VN = new("vi-VN");

        // Dữ liệu thanh toán
        private readonly decimal _total;
        private readonly decimal _subtotal;
        private readonly decimal _discount;
        private readonly decimal _tax;
        private readonly IReadOnlyList<CartItemDTO>? _cartItems;

        // Services & Real-time Auto Payment Detection
        private readonly VietQrService _qrService = new();
        private readonly SePayService _sePayService = new();
        private readonly System.Windows.Forms.Timer _sePayTimer = new() { Interval = 2000 };
        private readonly DateTime _sessionStartTime = DateTime.Now;
        private readonly string _transferMemo;
        private bool _isCheckingSePay;
        private bool _isPaymentCompleted;

        // Output properties
        public PaymentMethod SelectedMethod { get; private set; } = PaymentMethod.Cash;
        public decimal AmountPaid { get; private set; }
        public decimal Change => Math.Max(0, AmountPaid - _total);
        public bool PrintInvoice { get; private set; } = true;

        // Controls chính
        private Panel pnlHeader = null!;
        private Label lblHeaderTitle = null!;
        private Label lblHeaderSubtitle = null!;
        private Button btnCloseTop = null!;

        // Left Panel (Chi tiết đơn hàng)
        private Panel pnlLeft = null!;
        private DataGridView dgvItems = null!;
        private Label lblSubtotalVal = null!;
        private Label lblDiscountVal = null!;
        private Label lblTaxVal = null!;
        private Label lblTotalHighlight = null!;

        // Right Panel (Phương thức & thao tác)
        private Panel pnlRight = null!;
        private Button btnTabCash = null!;
        private Button btnTabTransfer = null!;
        private Panel pnlMethodContent = null!;

        // Cash View Controls
        private Panel pnlCashView = null!;
        private TextBox txtPaid = null!;
        private Button btnClearPaid = null!;
        private Button btnExactAmount = null!;
        private Panel pnlChangeCard = null!;
        private Label lblChangeTitle = null!;
        private Label lblChangeValue = null!;

        // Transfer View Controls
        private Panel pnlTransferView = null!;
        private PictureBox picQr = null!;
        private Label lblTransferStatus = null!;
        private Panel pnlLiveTransferStatus = null!;
        private Label lblLiveStatusTitle = null!;
        private Label lblLiveStatusDesc = null!;

        // Bottom Action Bar
        private Panel pnlBottomBar = null!;
        private CheckBox chkPrint = null!;
        private Button btnCancel = null!;
        private Button btnConfirm = null!;

        public PaymentForm(decimal total) : this(total, null, 0, 0)
        {
        }

        public PaymentForm(decimal total, IReadOnlyList<CartItemDTO>? cartItems, decimal discount = 0, decimal tax = 0)
        {
            _total = Math.Max(0, total);
            _cartItems = cartItems;
            _discount = discount;
            _tax = tax;
            _subtotal = cartItems != null && cartItems.Count > 0 ? cartItems.Sum(c => c.LineTotal) : total;

            AmountPaid = _total;
            // VietinBank qua SePay bắt buộc nội dung bắt đầu bằng SEVQR
            _transferMemo = $"SEVQR POS{DateTime.Now:HHmmss}";

            InitializeCustomComponent();
            ApplyData();
            SelectPaymentMethod(PaymentMethod.Cash);

            _sePayTimer.Tick += SePayTimer_Tick;
            Disposed += (s, e) =>
            {
                _sePayTimer.Stop();
                _sePayTimer.Dispose();
            };
        }

        private static string FormatMoney(decimal amount) => amount.ToString("N0", VN) + "đ";

        private void InitializeCustomComponent()
        {
            // Thiết lập cửa sổ lớn hiện đại
            Text = "Thanh toán đơn hàng - Tech Store POS";
            Size = new Size(1200, 820);
            MinimumSize = new Size(1050, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = FormBg;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            KeyPreview = true;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;

            // Header Panel
            BuildHeader();

            // Bottom Action Bar
            BuildBottomBar();

            // Main Body Split (Left: Order Summary, Right: Payment Processing)
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 14, 18, 14),
                BackColor = FormBg
            };

            var tableSplit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tableSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47F));
            tableSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53F));

            pnlLeft = BuildLeftOrderPanel();
            pnlRight = BuildRightPaymentPanel();

            tableSplit.Controls.Add(pnlLeft, 0, 0);
            tableSplit.Controls.Add(pnlRight, 1, 0);
            pnlBody.Controls.Add(tableSplit);

            Controls.Add(pnlBody);
            Controls.Add(pnlBottomBar);
            Controls.Add(pnlHeader);

            KeyDown += PaymentForm_KeyDown;
        }

        private void BuildHeader()
        {
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = OceanDeep,
                Padding = new Padding(24, 12, 20, 12)
            };

            // Vẽ dải gradient nhẹ biển xanh
            pnlHeader.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(
                    pnlHeader.ClientRectangle,
                    OceanDeep,
                    OceanPrimary,
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, pnlHeader.ClientRectangle);
            };

            var pnlTitleGroup = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Location = new Point(24, 14),
                BackColor = Color.Transparent
            };

            lblHeaderTitle = new Label
            {
                Text = "THANH TOÁN ĐƠN HÀNG",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4),
                BackColor = Color.Transparent
            };

            lblHeaderSubtitle = new Label
            {
                Text = "Tech Store POS • Quầy thu ngân bán lẻ trực tiếp",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(224, 242, 254),
                AutoSize = true,
                Margin = Padding.Empty,
                BackColor = Color.Transparent
            };

            pnlTitleGroup.Controls.Add(lblHeaderTitle);
            pnlTitleGroup.Controls.Add(lblHeaderSubtitle);

            btnCloseTop = new Button
            {
                Text = "✕ Đóng (Esc)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 255, 255, 255),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Size = new Size(110, 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlHeader.Width - 130, 23)
            };
            btnCloseTop.FlatAppearance.BorderSize = 0;
            btnCloseTop.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            pnlHeader.Resize += (s, e) =>
            {
                btnCloseTop.Location = new Point(pnlHeader.Width - 130, (pnlHeader.Height - btnCloseTop.Height) / 2);
            };

            pnlHeader.Controls.Add(pnlTitleGroup);
            pnlHeader.Controls.Add(btnCloseTop);
        }

        private Panel BuildLeftOrderPanel()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Margin = new Padding(0, 0, 10, 0),
                Padding = new Padding(16)
            };
            card.Paint += (s, e) => DrawCardBorder(e.Graphics, card.ClientRectangle);

            // Tiêu đề card trái
            var lblTitle = new Label
            {
                Text = "📦 CHI TIẾT ĐƠN HÀNG",
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = OceanDeep,
                Dock = DockStyle.Top,
                Height = 32
            };

            // Bảng danh sách món hàng
            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CardBg,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(241, 245, 249),
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            dgvItems.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvItems.ColumnHeadersHeight = 34;
            dgvItems.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = OceanLight,
                ForeColor = OceanDeep,
                Font = new Font("Segoe UI Semibold", 9.5F),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            dgvItems.RowTemplate.Height = 52;
            dgvItems.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = CardBg,
                ForeColor = TextMain,
                Font = new Font("Segoe UI", 9.5F),
                SelectionBackColor = OceanLight,
                SelectionForeColor = OceanDeep
            };

            var colImg = new DataGridViewImageColumn
            {
                HeaderText = "Ảnh",
                Width = 52,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Padding = new Padding(2)
                }
            };
            var colName = new DataGridViewTextBoxColumn
            {
                HeaderText = "Sản phẩm",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 110
            };
            var colQty = new DataGridViewTextBoxColumn
            {
                HeaderText = "SL",
                Width = 40,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            var colPrice = new DataGridViewTextBoxColumn
            {
                HeaderText = "Đơn giá",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            var colTotal = new DataGridViewTextBoxColumn
            {
                HeaderText = "Thành tiền",
                Width = 115,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI Semibold", 9.5F),
                    ForeColor = OceanPrimary
                }
            };

            dgvItems.Columns.AddRange(colImg, colName, colQty, colPrice, colTotal);

            // Khung tổng kết tài chính bên dưới card trái
            var pnlSummary = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 195,
                BackColor = CardBg,
                Padding = new Padding(0, 8, 0, 0)
            };

            var rowSubtotal = CreateSummaryRow("Tiền hàng (Tạm tính):", out lblSubtotalVal);
            var rowDiscount = CreateSummaryRow("Chiết khấu / Giảm giá:", out lblDiscountVal);
            var rowTax = CreateSummaryRow("Thuế VAT:", out lblTaxVal);

            // Card highlight tổng tiền to nổi bật
            var pnlTotalHighlight = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 84,
                BackColor = OceanLight,
                Padding = new Padding(14, 8, 14, 8)
            };
            pnlTotalHighlight.Paint += (s, e) =>
            {
                using var p = new Pen(OceanBorder, 1.5f);
                e.Graphics.DrawRectangle(p, 0, 0, pnlTotalHighlight.Width - 1, pnlTotalHighlight.Height - 1);
            };

            var lblTotalCap = new Label
            {
                Text = "TỔNG CẦN THANH TOÁN",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = OceanDeep,
                Dock = DockStyle.Top,
                Height = 22
            };

            lblTotalHighlight = new Label
            {
                Text = FormatMoney(_total),
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = OceanPrimary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            pnlTotalHighlight.Controls.Add(lblTotalHighlight);
            pnlTotalHighlight.Controls.Add(lblTotalCap);

            pnlSummary.Controls.Add(pnlTotalHighlight);
            pnlSummary.Controls.Add(rowTax);
            pnlSummary.Controls.Add(rowDiscount);
            pnlSummary.Controls.Add(rowSubtotal);

            card.Controls.Add(dgvItems);
            card.Controls.Add(pnlSummary);
            card.Controls.Add(lblTitle);

            return card;
        }

        private Panel CreateSummaryRow(string labelText, out Label valLabel)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 26,
                BackColor = Color.Transparent
            };
            var lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = TextMuted,
                Dock = DockStyle.Left,
                AutoSize = true
            };
            valLabel = new Label
            {
                Font = new Font("Segoe UI Semibold", 9.5F),
                ForeColor = TextMain,
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(valLabel);
            return pnl;
        }

        private Panel BuildRightPaymentPanel()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Margin = new Padding(10, 0, 0, 0),
                Padding = new Padding(20, 16, 20, 16)
            };
            card.Paint += (s, e) => DrawCardBorder(e.Graphics, card.ClientRectangle);

            // Tiêu đề card phải
            var lblTitle = new Label
            {
                Text = "HÌNH THỨC THANH TOÁN",
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = OceanDeep,
                Dock = DockStyle.Top,
                Height = 28
            };

            // Thanh tab chọn phương thức thanh toán
            var pnlTabs = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 14)
            };
            pnlTabs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlTabs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            btnTabCash = CreateTabButton("💵 Tiền mặt", PaymentMethod.Cash);
            btnTabTransfer = CreateTabButton("📲 Chuyển khoản", PaymentMethod.Transfer);

            pnlTabs.Controls.Add(btnTabCash, 0, 0);
            pnlTabs.Controls.Add(btnTabTransfer, 1, 0);

            // Vùng nội dung phương thức thanh toán
            pnlMethodContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Padding = new Padding(0, 12, 0, 0)
            };

            BuildCashView();
            BuildTransferView();

            pnlMethodContent.Controls.Add(pnlCashView);
            pnlMethodContent.Controls.Add(pnlTransferView);

            card.Controls.Add(pnlMethodContent);
            card.Controls.Add(pnlTabs);
            card.Controls.Add(lblTitle);

            return card;
        }

        private Button CreateTabButton(string text, PaymentMethod method)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 9F),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(2, 0, 2, 0),
                Tag = method
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = OceanBorder;
            btn.Click += (s, e) => SelectPaymentMethod(method);
            return btn;
        }

        private void BuildCashView()
        {
            pnlCashView = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg
            };

            // Row 1: Khung nhập tiền khách đưa
            var pnlInputRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = OceanLight,
                Padding = new Padding(12, 10, 12, 10)
            };
            pnlInputRow.Paint += (s, e) =>
            {
                using var p = new Pen(OceanPrimary, 1.5f);
                e.Graphics.DrawRectangle(p, 0, 0, pnlInputRow.Width - 1, pnlInputRow.Height - 1);
            };

            var lblPaidPrompt = new Label
            {
                Text = "Tiền khách đưa:",
                Font = new Font("Segoe UI Semibold", 10.5F),
                ForeColor = OceanDeep,
                Dock = DockStyle.Left,
                Width = 125,
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnClearPaid = new Button
            {
                Text = "✕ Xóa",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextMuted,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = 70
            };
            btnClearPaid.FlatAppearance.BorderColor = OceanBorder;
            btnClearPaid.Click += (s, e) =>
            {
                txtPaid.Text = "0";
                txtPaid.Focus();
                UpdateCashCalculations();
            };

            txtPaid = new TextBox
            {
                Text = _total.ToString("N0", VN),
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = OceanDeep,
                BackColor = OceanLight,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Right
            };
            txtPaid.TextChanged += TxtPaid_TextChanged;
            txtPaid.KeyPress += TxtPaid_KeyPress;

            pnlInputRow.Controls.Add(txtPaid);
            pnlInputRow.Controls.Add(btnClearPaid);
            pnlInputRow.Controls.Add(lblPaidPrompt);

            // Row 2: Bố cục 2 cột (Cột trái: Mệnh giá nhanh, Cột phải: Bàn phím số Numpad cảm ứng)
            var pnlMiddle = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 12, 0, 10),
                BackColor = CardBg
            };

            var tableMid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty
            };
            tableMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F)); // Mệnh giá gợi ý
            tableMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F)); // Numpad cảm ứng

            // --- Cột Trái: Mệnh giá tiện ích ---
            var pnlDenominations = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 10, 0)
            };

            btnExactAmount = new Button
            {
                Text = $"ĐÚNG SỐ TIỀN ({FormatMoney(_total)})",
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = OceanPrimary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExactAmount.FlatAppearance.BorderSize = 0;
            btnExactAmount.Click += (s, e) =>
            {
                txtPaid.Text = _total.ToString("N0", VN);
                txtPaid.Focus();
                UpdateCashCalculations();
            };

            var flpQuickBills = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 8, 0, 0)
            };

            decimal[] bills = { 50_000, 100_000, 200_000, 500_000, 1_000_000, 2_000_000 };
            foreach (var b in bills)
            {
                var btn = new Button
                {
                    Text = b.ToString("N0", VN),
                    Width = 120,
                    Height = 40,
                    Font = new Font("Segoe UI Semibold", 9.5F),
                    BackColor = OceanLight,
                    ForeColor = OceanDeep,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 6, 6)
                };
                btn.FlatAppearance.BorderColor = OceanBorder;
                btn.Click += (s, e) =>
                {
                    txtPaid.Text = b.ToString("N0", VN);
                    txtPaid.Focus();
                    UpdateCashCalculations();
                };
                flpQuickBills.Controls.Add(btn);
            }

            // Nút cộng thêm tiền (+10k, +20k, +50k, +100k, +500k)
            decimal[] addAmounts = { 10_000, 20_000, 50_000, 100_000, 500_000 };
            foreach (var a in addAmounts)
            {
                var btn = new Button
                {
                    Text = $"+{a.ToString("N0", VN)}",
                    Width = 120,
                    Height = 36,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    BackColor = Color.White,
                    ForeColor = OceanPrimary,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 6, 6)
                };
                btn.FlatAppearance.BorderColor = OceanBorder;
                btn.Click += (s, e) =>
                {
                    decimal cur = ParseInputAmount();
                    txtPaid.Text = (cur + a).ToString("N0", VN);
                    txtPaid.Focus();
                    UpdateCashCalculations();
                };
                flpQuickBills.Controls.Add(btn);
            }

            pnlDenominations.Controls.Add(flpQuickBills);
            pnlDenominations.Controls.Add(btnExactAmount);

            // --- Cột Phải: Bàn phím số Numpad cảm ứng (4 hàng x 3 cột) ---
            var pnlNumpad = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4,
                Padding = new Padding(6, 0, 0, 0)
            };
            for (int c = 0; c < 3; c++) pnlNumpad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            for (int r = 0; r < 4; r++) pnlNumpad.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "000", "0", "⌫" };
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                var btn = new Button
                {
                    Text = key,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                    BackColor = key == "⌫" ? Color.FromArgb(254, 242, 242) : Color.White,
                    ForeColor = key == "⌫" ? DangerRed : OceanDeep,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(2)
                };
                btn.FlatAppearance.BorderColor = OceanBorder;
                btn.Click += (s, e) => HandleNumpadKey(key);
                pnlNumpad.Controls.Add(btn, i % 3, i / 3);
            }

            tableMid.Controls.Add(pnlDenominations, 0, 0);
            tableMid.Controls.Add(pnlNumpad, 1, 0);
            pnlMiddle.Controls.Add(tableMid);

            // Row 3: Thẻ hiển thị Tiền thừa trả khách
            pnlChangeCard = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(240, 253, 244),
                Padding = new Padding(16, 8, 16, 8)
            };
            pnlChangeCard.Paint += (s, e) =>
            {
                Color borderClr = (AmountPaid >= _total) ? Color.FromArgb(134, 239, 172) : Color.FromArgb(254, 202, 202);
                using var p = new Pen(borderClr, 1.5f);
                e.Graphics.DrawRectangle(p, 0, 0, pnlChangeCard.Width - 1, pnlChangeCard.Height - 1);
            };

            lblChangeTitle = new Label
            {
                Text = "TIỀN THỪA TRẢ KHÁCH",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = SuccessGreen,
                Dock = DockStyle.Top,
                Height = 22
            };

            lblChangeValue = new Label
            {
                Text = "0đ",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(4, 120, 87),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            pnlChangeCard.Controls.Add(lblChangeValue);
            pnlChangeCard.Controls.Add(lblChangeTitle);

            pnlCashView.Controls.Add(pnlMiddle);
            pnlCashView.Controls.Add(pnlChangeCard);
            pnlCashView.Controls.Add(pnlInputRow);
        }

        private void BuildTransferView()
        {
            pnlTransferView = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Visible = false,
                Padding = new Padding(14, 4, 14, 4),
                AutoScroll = true
            };

            // Container chứa ảnh mã QR đặt ở trên (căn giữa)
            var pnlQrContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 186,
                BackColor = CardBg
            };

            picQr = new PictureBox
            {
                Size = new Size(180, 180),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            picQr.Paint += (s, e) =>
            {
                using var p = new Pen(OceanBorder, 1.5f);
                e.Graphics.DrawRectangle(p, 0, 0, picQr.Width - 1, picQr.Height - 1);
            };

            void CenterQr()
            {
                picQr.Location = new Point(Math.Max(0, (pnlQrContainer.Width - picQr.Width) / 2), 2);
            }
            pnlQrContainer.Controls.Add(picQr);
            pnlQrContainer.Resize += (s, e) => CenterQr();
            pnlQrContainer.HandleCreated += (s, e) => CenterQr();

            // Card thông tin chuyển khoản đặt ở dưới mã QR
            var pnlBankCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 224,
                BackColor = OceanLight,
                Padding = new Padding(16, 6, 16, 6),
                Margin = new Padding(0, 4, 0, 0)
            };
            pnlBankCard.Paint += (s, e) =>
            {
                using var p = new Pen(OceanBorder, 1.5f);
                e.Graphics.DrawRectangle(p, 0, 0, pnlBankCard.Width - 1, pnlBankCard.Height - 1);
            };

            var lblBankTitle = new Label
            {
                Text = "THÔNG TIN CHUYỂN KHOẢN (VIETQR)",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = OceanDeep,
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Bảng 2 cột hiển thị thông tin ngân hàng rõ ràng không bị đè chữ
            var tblInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 105,
                ColumnCount = 2,
                RowCount = 5,
                Margin = Padding.Empty,
                Padding = new Padding(0, 1, 0, 0)
            };
            tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165F));
            tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 5; i++)
                tblInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 20.5F));

            void AddBankRow(int row, string title, string val, Color valColor, bool bold = false)
            {
                var lTitle = new Label
                {
                    Text = title,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = TextMuted,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                var lVal = new Label
                {
                    Text = val,
                    Font = new Font("Segoe UI", 9.5F, bold ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = valColor,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };
                tblInfo.Controls.Add(lTitle, 0, row);
                tblInfo.Controls.Add(lVal, 1, row);
            }

            AddBankRow(0, "Ngân hàng:", _qrService.Config.BankName, OceanDeep, true);
            AddBankRow(1, "Số tài khoản:", _qrService.Config.AccountNumber, OceanPrimary, true);
            AddBankRow(2, "Chủ tài khoản:", _qrService.Config.AccountName, TextMain, true);
            AddBankRow(3, "Số tiền cần chuyển:", FormatMoney(_total), OceanPrimary, true);
            AddBankRow(4, "Nội dung CK:", _transferMemo, OceanDeep, true);

            // Khung trạng thái tự động nhận diện thanh toán thời gian thực qua SePay API
            pnlLiveTransferStatus = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(238, 242, 255),
                Padding = new Padding(6, 5, 6, 5)
            };
            pnlLiveTransferStatus.Paint += (s, e) =>
            {
                Color borderClr = _isPaymentCompleted ? Color.FromArgb(34, 197, 94) : Color.FromArgb(199, 210, 254);
                using var p = new Pen(borderClr, 1.5f);
                e.Graphics.DrawRectangle(p, 0, 0, pnlLiveTransferStatus.Width - 1, pnlLiveTransferStatus.Height - 1);
            };

            lblLiveStatusTitle = new Label
            {
                Text = "⏳ ĐANG CHỜ KHÁCH CHUYỂN TIỀN...",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(67, 56, 202),
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblLiveStatusDesc = new Label
            {
                Text = "Tự động phát hiện và hoàn tất hóa đơn ngay khi nhận được tiền từ VietinBank.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(79, 70, 229),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlLiveTransferStatus.Controls.Add(lblLiveStatusDesc);
            pnlLiveTransferStatus.Controls.Add(lblLiveStatusTitle);

            lblTransferStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = DangerRed,
                Dock = DockStyle.Bottom,
                Height = 16,
                Visible = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            pnlBankCard.Controls.Add(tblInfo);
            pnlBankCard.Controls.Add(lblTransferStatus);
            pnlBankCard.Controls.Add(pnlLiveTransferStatus);
            pnlBankCard.Controls.Add(lblBankTitle);

            // Thứ tự add: pnlBankCard trước rồi pnlQrContainer sau để pnlQrContainer nằm trên cùng
            pnlTransferView.Controls.Add(pnlBankCard);
            pnlTransferView.Controls.Add(pnlQrContainer);
        }


        private void BuildBottomBar()
        {
            pnlBottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = Color.White,
                Padding = new Padding(24, 10, 24, 10)
            };
            pnlBottomBar.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240), 1f);
                e.Graphics.DrawLine(p, 0, 0, pnlBottomBar.Width, 0);
            };

            chkPrint = new CheckBox
            {
                Text = "In hóa đơn ngay sau khi hoàn tất",
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = OceanDeep,
                Checked = true,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            btnConfirm = new Button
            {
                Text = "✓ XÁC NHẬN THANH TOÁN (F12)",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = OceanPrimary,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = 330
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += BtnConfirm_Click;
            btnConfirm.MouseEnter += (s, e) => btnConfirm.BackColor = OceanHover;
            btnConfirm.MouseLeave += (s, e) => btnConfirm.BackColor = OceanPrimary;

            btnCancel = new Button
            {
                Text = "✕ Hủy (Esc)",
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = TextMuted,
                BackColor = Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = 130,
                Margin = new Padding(0, 0, 12, 0)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            pnlBottomBar.Controls.Add(btnCancel);
            pnlBottomBar.Controls.Add(btnConfirm);
            pnlBottomBar.Controls.Add(chkPrint);
        }

        private void ApplyData()
        {
            lblSubtotalVal.Text = FormatMoney(_subtotal);
            lblDiscountVal.Text = _discount > 0 ? "-" + FormatMoney(_discount) : "0đ";
            lblTaxVal.Text = FormatMoney(_tax);
            lblTotalHighlight.Text = FormatMoney(_total);

            // Đổ danh sách sản phẩm kèm ảnh
            dgvItems.Rows.Clear();
            if (_cartItems != null && _cartItems.Count > 0)
            {
                foreach (var it in _cartItems)
                {
                    Image thumb = LoadProductThumbnail(it.ImagePath) ?? GetDefaultProductIcon();
                    dgvItems.Rows.Add(thumb, it.Name, it.Quantity, FormatMoney(it.UnitPrice), FormatMoney(it.LineTotal));
                }
            }
            else
            {
                dgvItems.Rows.Add(GetDefaultProductIcon(), "Đơn hàng POS", 1, FormatMoney(_total), FormatMoney(_total));
            }

            UpdateCashCalculations();
        }

        private static Image? LoadProductThumbnail(string? path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return null;
                if (!File.Exists(path))
                {
                    string fallback = Path.Combine(Application.StartupPath, "Images", "Products", Path.GetFileName(path));
                    if (File.Exists(fallback)) path = fallback;
                    else return null;
                }

                using var src = Image.FromFile(path);
                var thumb = new Bitmap(48, 48);
                using var g = Graphics.FromImage(thumb);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(src, 0, 0, 48, 48);
                return thumb;
            }
            catch
            {
                return null;
            }
        }

        private static Bitmap? _defaultProductIcon;
        private static Bitmap GetDefaultProductIcon()
        {
            if (_defaultProductIcon != null) return _defaultProductIcon;
            var bmp = new Bitmap(48, 48);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var bBg = new SolidBrush(Color.FromArgb(240, 249, 255));
            g.FillRectangle(bBg, 0, 0, 48, 48);
            using var pBorder = new Pen(Color.FromArgb(186, 230, 253), 1f);
            g.DrawRectangle(pBorder, 0, 0, 47, 47);
            using var f = new Font("Segoe UI", 15F);
            using var bText = new SolidBrush(Color.FromArgb(2, 132, 199));
            g.DrawString("📦", f, bText, new PointF(6, 4));
            _defaultProductIcon = bmp;
            return _defaultProductIcon;
        }

        public void SelectPaymentMethod(PaymentMethod method)
        {
            SelectedMethod = method;

            // Đổi giao diện các tab
            StyleTabButton(btnTabCash, method == PaymentMethod.Cash);
            StyleTabButton(btnTabTransfer, method == PaymentMethod.Transfer);

            // Chuyển panel nội dung
            pnlCashView.Visible = (method == PaymentMethod.Cash);
            pnlTransferView.Visible = (method == PaymentMethod.Transfer);

            if (method == PaymentMethod.Transfer)
            {
                AmountPaid = _total;
                btnConfirm.Visible = false; // Tự động phát hiện chuyển khoản qua SePay, không cần nút bấm thủ công
                LoadVietQrAsync();
                _sePayTimer.Start();
            }
            else
            {
                btnConfirm.Visible = true;
                _sePayTimer.Stop();

                UpdateCashCalculations();
                txtPaid.Focus();
                txtPaid.SelectAll();
            }

            btnConfirm.Enabled = (method != PaymentMethod.Cash) || (AmountPaid >= _total);
        }

        private void StyleTabButton(Button btn, bool isSelected)
        {
            if (isSelected)
            {
                btn.BackColor = OceanPrimary;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = OceanPrimary;
            }
            else
            {
                btn.BackColor = Color.White;
                btn.ForeColor = OceanDeep;
                btn.FlatAppearance.BorderColor = OceanBorder;
            }
        }

        private void HandleNumpadKey(string key)
        {
            if (key == "⌫")
            {
                string text = txtPaid.Text.Replace(".", "").Replace(",", "").Trim();
                if (text.Length > 1) text = text[..^1];
                else text = "0";
                if (decimal.TryParse(text, out decimal val))
                    txtPaid.Text = val.ToString("N0", VN);
            }
            else
            {
                string text = txtPaid.Text.Replace(".", "").Replace(",", "").Trim();
                if (text == "0") text = "";
                text += key;
                if (decimal.TryParse(text, out decimal val))
                {
                    if (val <= 1_000_000_000)
                        txtPaid.Text = val.ToString("N0", VN);
                }
            }
            txtPaid.Focus();
            txtPaid.SelectionStart = txtPaid.Text.Length;
            UpdateCashCalculations();
        }

        private decimal ParseInputAmount()
        {
            string clean = txtPaid.Text.Replace(".", "").Replace(",", "").Trim();
            return decimal.TryParse(clean, out decimal res) ? res : 0;
        }

        private void TxtPaid_TextChanged(object? sender, EventArgs e)
        {
            UpdateCashCalculations();
        }

        private void TxtPaid_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void UpdateCashCalculations()
        {
            AmountPaid = ParseInputAmount();
            decimal change = AmountPaid - _total;

            if (change >= 0)
            {
                pnlChangeCard.BackColor = Color.FromArgb(240, 253, 244);
                lblChangeTitle.Text = "TIỀN THỪA TRẢ KHÁCH";
                lblChangeTitle.ForeColor = Color.FromArgb(4, 120, 87);
                lblChangeValue.Text = FormatMoney(change);
                lblChangeValue.ForeColor = Color.FromArgb(4, 120, 87);
                btnConfirm.Enabled = true;
            }
            else
            {
                pnlChangeCard.BackColor = Color.FromArgb(254, 242, 242);
                lblChangeTitle.Text = "CÒN THIẾU CỦA KHÁCH";
                lblChangeTitle.ForeColor = DangerRed;
                lblChangeValue.Text = FormatMoney(Math.Abs(change));
                lblChangeValue.ForeColor = DangerRed;
                btnConfirm.Enabled = false;
            }
            pnlChangeCard.Invalidate();
        }

        private async void LoadVietQrAsync()
        {
            try
            {
                var (qrImg, statusMsg, isSuccess) = await _qrService.GenerateQrAsync(_total, _transferMemo);
                if (qrImg != null)
                {
                    picQr.Image?.Dispose();
                    picQr.Image = qrImg;
                }
                if (lblTransferStatus != null)
                {
                    if (isSuccess)
                    {
                        lblTransferStatus.Text = "";
                        lblTransferStatus.Visible = false;
                    }
                    else
                    {
                        lblTransferStatus.Text = statusMsg;
                        lblTransferStatus.ForeColor = DangerRed;
                        lblTransferStatus.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (lblTransferStatus != null)
                {
                    lblTransferStatus.Text = $"⚠️ Lỗi kết nối: {ex.Message}";
                    lblTransferStatus.ForeColor = DangerRed;
                    lblTransferStatus.Visible = true;
                }
            }
        }

        private async void SePayTimer_Tick(object? sender, EventArgs e)
        {
            if (_isCheckingSePay || _isPaymentCompleted || SelectedMethod != PaymentMethod.Transfer) return;
            _isCheckingSePay = true;

            try
            {
                var (isPaid, trans, msg) = await _sePayService.CheckPaymentAsync(_total, _transferMemo, _sessionStartTime);
                if (isPaid && trans != null)
                {
                    _isPaymentCompleted = true;
                    _sePayTimer.Stop();

                    pnlLiveTransferStatus.BackColor = Color.FromArgb(220, 252, 231);
                    lblLiveStatusTitle.Text = "🎉 ĐÃ THANH TOÁN THÀNH CÔNG!";
                    lblLiveStatusTitle.ForeColor = Color.FromArgb(21, 128, 61);
                    lblLiveStatusDesc.Text = $"Đã nhận +{trans.AmountIn:N0}đ vào VietinBank • Mã GD: {trans.ReferenceNumber}";
                    lblLiveStatusDesc.ForeColor = Color.FromArgb(22, 101, 52);
                    pnlLiveTransferStatus.Invalidate();

                    try { System.Media.SystemSounds.Asterisk.Play(); } catch { }

                    await Task.Delay(1600);

                    if (!IsDisposed)
                    {
                        PrintInvoice = chkPrint.Checked;
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
                else
                {
                    lblLiveStatusDesc.Text = msg;
                }
            }
            catch { }
            finally
            {
                _isCheckingSePay = false;
            }
        }

        private Bitmap GenerateFallbackQrPattern()
        {
            var bmp = new Bitmap(260, 260);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            // Vẽ khung viền QR
            using var pBorder = new Pen(OceanDeep, 2f);
            g.DrawRectangle(pBorder, 10, 10, 240, 240);

            // Vẽ 3 ô góc QR định vị
            DrawQrCorner(g, 22, 22);
            DrawQrCorner(g, 180, 22);
            DrawQrCorner(g, 22, 180);

            // Giả lập ma trận điểm ảnh QR
            using var bDot = new SolidBrush(Color.FromArgb(15, 23, 42));
            var rnd = new Random(42);
            for (int x = 24; x < 240; x += 9)
            {
                for (int y = 24; y < 240; y += 9)
                {
                    if ((x < 85 && y < 85) || (x > 165 && y < 85) || (x < 85 && y > 165)) continue;
                    if (rnd.Next(10) > 4)
                        g.FillRectangle(bDot, x, y, 7, 7);
                }
            }

            // Logo VietQR ở giữa
            using var bCenter = new SolidBrush(OceanPrimary);
            g.FillRectangle(bCenter, 90, 105, 80, 42);
            using var fLogo = new Font("Segoe UI", 11F, FontStyle.Bold);
            using var bWhite = new SolidBrush(Color.White);
            g.DrawString("VIETQR", fLogo, bWhite, new PointF(98, 115));

            return bmp;
        }

        private static void DrawQrCorner(Graphics g, int x, int y)
        {
            using var b = new SolidBrush(Color.FromArgb(15, 23, 42));
            g.FillRectangle(b, x, y, 48, 48);
            using var bWhite = new SolidBrush(Color.White);
            g.FillRectangle(bWhite, x + 8, y + 8, 32, 32);
            g.FillRectangle(b, x + 16, y + 16, 16, 16);
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            if (SelectedMethod == PaymentMethod.Cash && AmountPaid < _total)
            {
                MessageBox.Show("Số tiền khách đưa chưa đủ để thanh toán.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPaid.Focus();
                return;
            }

            PrintInvoice = chkPrint.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void PaymentForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else if (e.KeyCode == Keys.F12 || (e.KeyCode == Keys.Enter && btnConfirm.Enabled && !txtPaid.Focused))
            {
                BtnConfirm_Click(sender, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F1) SelectPaymentMethod(PaymentMethod.Cash);
            else if (e.KeyCode == Keys.F2) SelectPaymentMethod(PaymentMethod.Transfer);
        }

        private static void DrawCardBorder(Graphics g, Rectangle r)
        {
            using var p = new Pen(OceanBorder, 1.2f);
            g.DrawRectangle(p, 0, 0, r.Width - 1, r.Height - 1);
        }
    }
}
