using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace GUI.Forms.Sales
{
    /// <summary>
    /// Hộp thoại nhập và áp dụng mức giảm giá đơn hàng (theo % hoặc theo số tiền VNĐ).
    /// Thiết kế gọn gàng, chuẩn tỷ lệ, không bị che khuất hay đè chữ.
    /// </summary>
    [DesignerCategory("Code")]
    public partial class DiscountDialog : Form
    {
        private static readonly CultureInfo Vi = new("vi-VN");
        private static readonly Color Primary = Color.FromArgb(0, 112, 243);
        private static readonly Color PrimaryHover = Color.FromArgb(0, 89, 194);
        private static readonly Color PrimaryLight = Color.FromArgb(240, 249, 255);
        private static readonly Color BorderColor = Color.FromArgb(226, 232, 240);
        private static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        private static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
        private static readonly Color DangerRed = Color.FromArgb(220, 38, 38);
        private static readonly Color DangerLight = Color.FromArgb(254, 242, 242);

        private readonly decimal _subtotal;
        private bool _isPercent;

        // Output properties
        public decimal DiscountAmount { get; private set; }
        public decimal DiscountPercent { get; private set; }
        public bool IsPercent => _isPercent;
        public string Reason => string.Empty;

        // Controls
        private Panel pnlHeader = null!;
        private Label lblHeaderTitle = null!;
        private Label lblHeaderSubtitle = null!;
        private Button btnCloseTop = null!;

        private Label lblSubtotalVal = null!;
        private Button btnModePercent = null!;
        private Button btnModeAmount = null!;
        private TextBox txtInput = null!;
        private Label lblUnit = null!;
        private Button btnClear = null!;
        private FlowLayoutPanel flpPresets = null!;

        private Label lblPreviewDiscount = null!;
        private Label lblPreviewFinalTotal = null!;

        private Button btnRemoveDiscount = null!;
        private Button btnCancel = null!;
        private Button btnApply = null!;

        public DiscountDialog(decimal subtotal, decimal currentDiscount = 0, decimal currentPercent = 0, bool isPercent = false, string? currentReason = null)
        {
            _subtotal = Math.Max(0, subtotal);
            _isPercent = isPercent;
            DiscountAmount = Math.Min(currentDiscount, _subtotal);
            DiscountPercent = Math.Clamp(currentPercent, 0, 100);

            InitializeCustomComponent();

            // Thiết lập giá trị ban đầu vào input
            if (_isPercent)
            {
                SetMode(true);
                txtInput.Text = DiscountPercent > 0 ? DiscountPercent.ToString("0.##", Vi) : "";
            }
            else
            {
                SetMode(false);
                txtInput.Text = DiscountAmount > 0 ? DiscountAmount.ToString("N0", Vi) : "";
            }

            UpdateCalculations();
        }

        private static string FormatMoney(decimal amount) => amount.ToString("N0", Vi) + "đ";

        private void InitializeCustomComponent()
        {
            Text = "Áp dụng giảm giá - Tech Store POS";
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(520, 560);
            MinimumSize = new Size(480, 520);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            KeyPreview = true;

            // 1. Header (Cố định phía trên)
            BuildHeader();

            // 2. Footer (Cố định phía dưới)
            var pnlFooter = BuildFooter();

            // 3. Body: Bố cục dọc bằng TableLayoutPanel
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(22, 16, 22, 14),
                AutoScroll = true
            };

            var tblBody = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 5,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tblBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Dòng 1: Tạm tính tiền hàng
            var pnlSubtotalCard = CreateSubtotalCard();
            tblBody.Controls.Add(pnlSubtotalCard, 0, 0);

            // Dòng 2: Nút chọn chế độ (% / VNĐ)
            var pnlModeGroup = CreateModeSelector();
            tblBody.Controls.Add(pnlModeGroup, 0, 1);

            // Dòng 3: Nhóm ô nhập số mức giảm
            var pnlInputGroup = CreateInputRow();
            tblBody.Controls.Add(pnlInputGroup, 0, 2);

            // Dòng 4: Các nút gợi ý nhanh
            var pnlPresetsGroup = CreatePresetsGroup();
            tblBody.Controls.Add(pnlPresetsGroup, 0, 3);

            // Dòng 5: Khung xem trước kết quả
            var pnlPreviewCard = CreatePreviewCard();
            tblBody.Controls.Add(pnlPreviewCard, 0, 4);

            pnlBody.Controls.Add(tblBody);

            Controls.Add(pnlBody);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            };
        }

        private void BuildHeader()
        {
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(14, 85, 158),
                Padding = new Padding(20, 12, 16, 12)
            };

            pnlHeader.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(
                    pnlHeader.ClientRectangle,
                    Color.FromArgb(14, 85, 158),
                    Primary,
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, pnlHeader.ClientRectangle);
            };

            lblHeaderTitle = new Label
            {
                Text = "🏷️ ÁP DỤNG GIẢM GIÁ ĐƠN HÀNG",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 13),
                BackColor = Color.Transparent
            };

            lblHeaderSubtitle = new Label
            {
                Text = "Giảm theo phần trăm (%) hoặc trừ trực tiếp số tiền (VNĐ)",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(224, 242, 254),
                AutoSize = true,
                Location = new Point(22, 38),
                BackColor = Color.Transparent
            };

            btnCloseTop = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 255, 255, 255),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Size = new Size(34, 34),
                Location = new Point(pnlHeader.Width - 48, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCloseTop.FlatAppearance.BorderSize = 0;
            btnCloseTop.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(lblHeaderSubtitle);
            pnlHeader.Controls.Add(btnCloseTop);
        }

        private Panel CreateSubtotalCard()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 12),
                Padding = new Padding(14, 0, 14, 0)
            };
            pnl.Paint += (s, e) => DrawBorder(e.Graphics, pnl.ClientRectangle);

            var lblPrompt = new Label
            {
                Text = "Tạm tính tiền hàng:",
                Font = new Font("Segoe UI Semibold", 9.5F),
                ForeColor = TextMuted,
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblSubtotalVal = new Label
            {
                Text = FormatMoney(_subtotal),
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = TextDark,
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            pnl.Controls.Add(lblPrompt);
            pnl.Controls.Add(lblSubtotalVal);
            return pnl;
        }

        private Panel CreateModeSelector()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Margin = new Padding(0, 0, 0, 12),
                Padding = Padding.Empty
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            btnModePercent = new Button
            {
                Text = "% Giảm theo phần trăm",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 9.5F),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 4, 0)
            };
            btnModePercent.Click += (s, e) => SetMode(true);

            btnModeAmount = new Button
            {
                Text = "💵 Giảm theo số tiền",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 9.5F),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(4, 0, 0, 0)
            };
            btnModeAmount.Click += (s, e) => SetMode(false);

            table.Controls.Add(btnModePercent, 0, 0);
            table.Controls.Add(btnModeAmount, 1, 0);
            pnl.Controls.Add(table);

            return pnl;
        }

        private void SetMode(bool isPercent)
        {
            _isPercent = isPercent;

            btnModePercent.BackColor = _isPercent ? Primary : Color.White;
            btnModePercent.ForeColor = _isPercent ? Color.White : TextDark;
            btnModePercent.FlatAppearance.BorderColor = _isPercent ? Primary : BorderColor;

            btnModeAmount.BackColor = !_isPercent ? Primary : Color.White;
            btnModeAmount.ForeColor = !_isPercent ? Color.White : TextDark;
            btnModeAmount.FlatAppearance.BorderColor = !_isPercent ? Primary : BorderColor;

            lblUnit.Text = _isPercent ? "%" : "đ";

            // Tái tạo các nút chọn nhanh theo chế độ
            RenderPresets();

            // Format lại ô input khi đổi chế độ
            if (_isPercent)
            {
                if (DiscountPercent > 0)
                    txtInput.Text = DiscountPercent.ToString("0.##", Vi);
                else if (_subtotal > 0 && DiscountAmount > 0)
                    txtInput.Text = Math.Round((DiscountAmount / _subtotal) * 100m, 1).ToString("0.##", Vi);
                else
                    txtInput.Text = "";
            }
            else
            {
                if (DiscountAmount > 0)
                    txtInput.Text = DiscountAmount.ToString("N0", Vi);
                else
                    txtInput.Text = "";
            }

            txtInput.Focus();
            txtInput.SelectAll();
            UpdateCalculations();
        }

        private Panel CreateInputRow()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 12),
                Padding = new Padding(12, 6, 12, 6)
            };
            pnl.Paint += (s, e) =>
            {
                using var p = new Pen(Primary, 1.5f);
                e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));             // Cột 1: "Mức giảm:" tự co giãn
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));         // Cột 2: Ô nhập
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));         // Cột 3: Nút xóa X
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38F));         // Cột 4: Đơn vị % hoặc đ

            var lblPrompt = new Label
            {
                Text = "Mức giảm:",
                Font = new Font("Segoe UI Semibold", 10.5F),
                ForeColor = TextDark,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 8, 0)
            };

            txtInput = new TextBox
            {
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = TextDark,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Right
            };
            txtInput.TextChanged += (s, e) => UpdateCalculations();
            txtInput.KeyPress += TxtInput_KeyPress;
            txtInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    BtnApply_Click(s, e);
                }
            };

            btnClear = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = TextMuted,
                BackColor = Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 4, 3, 4)
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) =>
            {
                txtInput.Text = "";
                txtInput.Focus();
                UpdateCalculations();
            };

            lblUnit = new Label
            {
                Text = "%",
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = Primary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            tbl.Controls.Add(lblPrompt, 0, 0);
            tbl.Controls.Add(txtInput, 1, 0);
            tbl.Controls.Add(btnClear, 2, 0);
            tbl.Controls.Add(lblUnit, 3, 0);

            pnl.Controls.Add(tbl);
            return pnl;
        }

        private void TxtInput_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            if (_isPercent && (e.KeyChar == '.' || e.KeyChar == ','))
            {
                if (txtInput.Text.Contains('.') || txtInput.Text.Contains(','))
                    e.Handled = true;
                return;
            }

            if (!char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private Panel CreatePresetsGroup()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 14),
                Padding = Padding.Empty
            };

            var lblPresetTitle = new Label
            {
                Text = "Gợi ý chọn nhanh:",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = TextMuted,
                Dock = DockStyle.Top,
                Height = 18,
                TextAlign = ContentAlignment.BottomLeft
            };

            flpPresets = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,  // Xuống dòng tự nhiên nếu màn hình hẹp
                AutoScroll = false,   // Không hiện scrollbar gây che khuất
                Margin = new Padding(0, 4, 0, 0),
                Padding = Padding.Empty
            };

            pnl.Controls.Add(flpPresets);
            pnl.Controls.Add(lblPresetTitle);
            return pnl;
        }

        private void RenderPresets()
        {
            flpPresets.SuspendLayout();
            flpPresets.Controls.Clear();

            if (_isPercent)
            {
                decimal[] presets = { 5, 10, 15, 20, 30, 50 };
                foreach (var p in presets)
                {
                    var btn = CreateChipButton($"{p}%", () =>
                    {
                        txtInput.Text = p.ToString("0", Vi);
                        UpdateCalculations();
                    });
                    flpPresets.Controls.Add(btn);
                }
            }
            else
            {
                decimal[] presets = { 20_000, 50_000, 100_000, 200_000, 500_000 };
                foreach (var a in presets)
                {
                    var btn = CreateChipButton(FormatMoney(a), () =>
                    {
                        txtInput.Text = a.ToString("N0", Vi);
                        UpdateCalculations();
                    });
                    flpPresets.Controls.Add(btn);
                }
            }

            flpPresets.ResumeLayout(true);
        }

        private Button CreateChipButton(string text, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 32,
                Font = new Font("Segoe UI Semibold", 9F),
                BackColor = Color.White,
                ForeColor = Primary,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 6, 4),
                Padding = new Padding(10, 0, 10, 0)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(186, 230, 253);
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private Panel CreatePreviewCard()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = PrimaryLight,
                Margin = new Padding(0, 0, 0, 0),
                Padding = new Padding(16, 10, 16, 10)
            };
            pnl.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(186, 230, 253), 1.5f);
                e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));

            var lblDiscTitle = new Label
            {
                Text = "Số tiền được giảm:",
                Font = new Font("Segoe UI Semibold", 9.5F),
                ForeColor = DangerRed,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblPreviewDiscount = new Label
            {
                Text = "-0đ",
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = DangerRed,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            var lblTotTitle = new Label
            {
                Text = "Khách cần thanh toán:",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = TextDark,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblPreviewFinalTotal = new Label
            {
                Text = FormatMoney(_subtotal),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Primary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            tbl.Controls.Add(lblDiscTitle, 0, 0);
            tbl.Controls.Add(lblPreviewDiscount, 1, 0);
            tbl.Controls.Add(lblTotTitle, 0, 1);
            tbl.Controls.Add(lblPreviewFinalTotal, 1, 1);

            pnl.Controls.Add(tbl);
            return pnl;
        }

        private Panel BuildFooter()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 62,
                BackColor = Color.White,
                Padding = new Padding(18, 12, 18, 12)
            };
            pnl.Paint += (s, e) =>
            {
                using var p = new Pen(BorderColor, 1);
                e.Graphics.DrawLine(p, 0, 0, pnl.Width, 0);
            };

            btnRemoveDiscount = new Button
            {
                Text = "✕ Bỏ giảm giá",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = DangerRed,
                BackColor = DangerLight,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Left,
                Width = 125,
                Height = 38
            };
            btnRemoveDiscount.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            btnRemoveDiscount.Click += (s, e) =>
            {
                DiscountAmount = 0;
                DiscountPercent = 0;
                DialogResult = DialogResult.OK;
                Close();
            };

            var pnlRightButtons = new Panel
            {
                Dock = DockStyle.Right,
                Width = 230,
                Height = 38,
                Padding = Padding.Empty
            };

            btnApply = new Button
            {
                Text = "✔ Áp dụng",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Primary,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = 125,
                Height = 38
            };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += BtnApply_Click;

            btnCancel = new Button
            {
                Text = "Hủy bỏ",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = TextMuted,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = 85,
                Height = 38,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnCancel.FlatAppearance.BorderColor = BorderColor;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            pnlRightButtons.Controls.Add(btnCancel);
            pnlRightButtons.Controls.Add(btnApply);

            pnl.Controls.Add(btnRemoveDiscount);
            pnl.Controls.Add(pnlRightButtons);

            return pnl;
        }

        private void UpdateCalculations()
        {
            string raw = txtInput.Text.Trim().Replace(".", "").Replace(",", ".");
            decimal val = 0;
            decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out val);

            if (_isPercent)
            {
                if (val < 0) val = 0;
                if (val > 100) val = 100;

                DiscountPercent = val;
                DiscountAmount = Math.Round(_subtotal * (val / 100m), 0);
            }
            else
            {
                if (val < 0) val = 0;
                if (val > _subtotal) val = _subtotal;

                DiscountAmount = val;
                DiscountPercent = _subtotal > 0 ? Math.Round((val / _subtotal) * 100m, 2) : 0;
            }

            decimal finalTotal = Math.Max(0, _subtotal - DiscountAmount);

            lblPreviewDiscount.Text = DiscountAmount > 0 ? $"-{FormatMoney(DiscountAmount)}" : "0đ";
            lblPreviewFinalTotal.Text = FormatMoney(finalTotal);
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            UpdateCalculations();

            if (DiscountAmount > _subtotal)
            {
                MessageBox.Show("Mức giảm giá không được vượt quá tổng tiền đơn hàng.",
                    "Lỗi giảm giá", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private static void DrawBorder(Graphics g, Rectangle r)
        {
            using var p = new Pen(BorderColor, 1);
            g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
        }
    }
}
