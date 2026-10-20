using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using ReaLTaiizor.Util;

namespace GUI
{
    /// <summary>
    /// Các hàm dựng giao diện dùng chung (cùng phong cách với SupplierForm):
    /// nhãn, ô nhập bo góc, bảng phẳng, nút Hope, bộ lọc phím.
    /// </summary>
    public static class UiKit
    {
        public static readonly Color InputBg = Color.FromArgb(243, 245, 249);
        public static readonly Color InputFocusBg = Color.FromArgb(231, 240, 255);
        public static readonly Color LabelGray = Color.FromArgb(95, 100, 120);

        // Đổi pixel thiết kế (96 DPI) sang pixel thật theo độ phóng to của màn hình
        public static int S(this Control c, int px) => (int)Math.Round(px * c.DeviceDpi / 96.0);

        public static Label MakeLabel(string text, Font font, Color color, ContentAlignment align) => new()
        {
            Text = text,
            Font = font,
            ForeColor = color,
            TextAlign = align,
            AutoSize = false,
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };

        /// <summary>Ô nhập bo góc: nền xám nhạt, xanh nhạt khi đang gõ, control luôn nằm giữa theo chiều dọc.</summary>
        public static RoundedPanel WrapInput(Control box, Control owner)
        {
            var wrap = new RoundedPanel
            {
                Radius = owner.S(10),
                BackColor = InputBg,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty
            };
            if (box is TextBox tb) tb.BorderStyle = BorderStyle.None;
            if (box is ComboBox cb)
            {
                cb.FlatStyle = FlatStyle.Flat;
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            box.BackColor = InputBg;
            box.ForeColor = AppTheme.TextDark;
            box.Dock = DockStyle.None;
            box.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            void Center()
            {
                int pad = owner.S(14);
                box.SetBounds(pad, Math.Max(0, (wrap.Height - box.Height) / 2),
                    Math.Max(10, wrap.Width - pad * 2), box.Height);
            }
            wrap.Controls.Add(box);
            wrap.Resize += (_, _) => Center();
            wrap.HandleCreated += (_, _) => Center();
            box.Enter += (_, _) => { wrap.BackColor = InputFocusBg; box.BackColor = InputFocusBg; };
            box.Leave += (_, _) => { wrap.BackColor = InputBg; box.BackColor = InputBg; };
            wrap.Click += (_, _) => box.Focus();
            return wrap;
        }

        /// <summary>Bảng phẳng: không lưới dọc, tiêu đề nhạt, dòng chọn xanh nhạt.</summary>
        public static void StyleGrid(DataGridView grid, Control owner, int rowHeight = 42)
        {
            grid.Dock = DockStyle.Fill;
            grid.Margin = Padding.Empty;
            grid.AutoGenerateColumns = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToResizeColumns = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;

            grid.BackgroundColor = AppTheme.Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(238, 240, 245);
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = owner.S(42);
            grid.ColumnHeadersDefaultCellStyle.BackColor = InputBg;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = LabelGray;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = InputBg;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = LabelGray;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False; // tiêu đề không xuống dòng
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(owner.S(8), 0, 0, 0);
            grid.RowTemplate.Height = owner.S(rowHeight);
            grid.DefaultCellStyle.ForeColor = AppTheme.TextDark;
            grid.DefaultCellStyle.Padding = new Padding(owner.S(8), 0, 0, 0);
            grid.DefaultCellStyle.SelectionBackColor = InputFocusBg;
            grid.DefaultCellStyle.SelectionForeColor = AppTheme.Primary;
        }

        public static void ConfigButton(HopeRoundButton btn, string text, HopeButtonType type,
            EventHandler onClick, Control owner)
        {
            btn.Text = text;
            btn.ButtonType = type;
            btn.Font = new Font("Segoe UI Semibold", 10F);
            btn.Dock = DockStyle.Fill;
            btn.Margin = new Padding(owner.S(3));
            btn.Click += onClick;
        }

        /// <summary>Cho gõ chữ, số, khoảng trắng và các ký tự trong extraAllowed.</summary>
        public static void FilterKey(KeyPressEventArgs e, string extraAllowed)
        {
            if (char.IsControl(e.KeyChar)) return; // Backspace, Ctrl+C/V...
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) && !extraAllowed.Contains(e.KeyChar))
                e.Handled = true;
        }

        /// <summary>Bỏ dấu tiếng Việt để tìm "chuot" ra "Chuột".</summary>
        public static string RemoveDiacritics(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var d = s.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(d.Length);
            foreach (var ch in d)
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString().Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant();
        }
    }

    /// <summary>
    /// ComboBox phẳng: tự vẽ nền cùng màu với khung bo góc (không còn ô trắng viền xám của Windows)
    /// và mũi tên nhỏ gọn. Chỉ dùng kiểu DropDownList.
    /// </summary>
    public sealed class FlatComboBox : ComboBox
    {
        private const int WM_PAINT = 0x000F;

        public FlatComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // Windows vẽ xong thì vẽ đè lên bằng màu của mình
            if (m.Msg == WM_PAINT && DropDownStyle == ComboBoxStyle.DropDownList && IsHandleCreated)
            {
                using var g = CreateGraphics();
                Draw(g);
            }
        }

        private void Draw(Graphics g)
        {
            var r = ClientRectangle;
            using (var bg = new SolidBrush(BackColor)) g.FillRectangle(bg, r);

            int arrowW = (int)Math.Round(26 * DeviceDpi / 96.0);
            string text = SelectedIndex >= 0 ? GetItemText(SelectedItem) : string.Empty;
            TextRenderer.DrawText(g, text, Font,
                new Rectangle(0, 0, Math.Max(1, r.Width - arrowW), r.Height), ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPadding);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            float cx = r.Width - arrowW / 2f, cy = r.Height / 2f, d = 4f * DeviceDpi / 96f;
            using var pen = new Pen(Color.FromArgb(120, 125, 145), 1.6f)
            { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
            g.DrawLines(pen, new[]
            {
                new PointF(cx - d, cy - d / 2), new PointF(cx, cy + d / 2), new PointF(cx + d, cy - d / 2)
            });
        }
    }
}
