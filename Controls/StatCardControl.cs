using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PhuKienMayTinh.GUI.Controls
{
    public class StatCardControl : Control
    {
        public string CardTitle { get; set; } = "TIÊU ĐỀ";
        public string CardValue { get; set; } = "0";
        public string CardSubtitle { get; set; } = "";
        public Color AccentColor { get; set; } = Color.FromArgb(59, 130, 246);
        public string IconSymbol { get; set; } = "📊";

        private bool _isHovered = false;

        public StatCardControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.FromArgb(30, 41, 59); // Dark Slate #1E293B
            Font = new Font("Segoe UI", 9f);
            Size = new Size(250, 115);
            Cursor = Cursors.Hand;
        }

        public void SetValues(string title, string value, string subtitle, Color accent, string icon)
        {
            CardTitle = title;
            CardValue = value;
            CardSubtitle = subtitle;
            AccentColor = accent;
            IconSymbol = icon;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 1. Nền thẻ Slate Dark bo góc 12px
            var cardRect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = GetRoundedRectPath(cardRect, 12))
            using (var bgBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            using (var penBorder = new Pen(_isHovered ? AccentColor : Color.FromArgb(51, 65, 85), _isHovered ? 1.8f : 1f))
            {
                g.FillPath(bgBrush, path);
                g.DrawPath(penBorder, path);
            }

            // 2. Dải Gradient sáng ở đỉnh trên
            using (var topPath = new GraphicsPath())
            {
                topPath.AddArc(0, 0, 12, 12, 180, 90);
                topPath.AddLine(6, 0, Width - 6, 0);
                topPath.AddArc(Width - 13, 0, 12, 12, 270, 90);
                topPath.AddLine(Width - 1, 4, 0, 4);
                topPath.CloseFigure();

                using (var gradBrush = new LinearGradientBrush(
                    new Point(0, 0),
                    new Point(Width, 0),
                    AccentColor,
                    Color.FromArgb(30, AccentColor.R, AccentColor.G, AccentColor.B)))
                {
                    g.FillPath(gradBrush, topPath);
                }
            }

            // 3. Icon Box hình tròn / bo góc góc phải trên
            float iconBoxSize = 38f;
            float iconX = Width - iconBoxSize - 16f;
            float iconY = 16f;
            var iconRect = new RectangleF(iconX, iconY, iconBoxSize, iconBoxSize);

            using (var iconPath = GetRoundedRectPath(Rectangle.Round(iconRect), 10))
            using (var iconBg = new SolidBrush(Color.FromArgb(35, AccentColor.R, AccentColor.G, AccentColor.B)))
            using (var iconBorder = new Pen(Color.FromArgb(80, AccentColor.R, AccentColor.G, AccentColor.B), 1f))
            using (var iconFont = new Font("Segoe UI Emoji", 14f))
            using (var iconBrush = new SolidBrush(Color.White))
            {
                g.FillPath(iconBg, iconPath);
                g.DrawPath(iconBorder, iconPath);
                var isz = g.MeasureString(IconSymbol, iconFont);
                g.DrawString(IconSymbol, iconFont, iconBrush, iconX + (iconBoxSize - isz.Width) / 2f, iconY + (iconBoxSize - isz.Height) / 2f);
            }

            // 4. Tiêu đề thẻ (Màu xám nhạt #94A3B8)
            using (var fontTitle = new Font("Segoe UI", 8.2f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                g.DrawString(CardTitle.ToUpper(), fontTitle, titleBrush, 18, 16);
            }

            // 5. Giá trị số chính (Trắng tinh #F8FAFC, Lớn đậm)
            using (var fontValue = new Font("Segoe UI", 15f, FontStyle.Bold))
            using (var valBrush = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                g.DrawString(CardValue, fontValue, valBrush, 18, 38);
            }

            // 6. Nhãn phụ / Badge xu hướng
            if (!string.IsNullOrEmpty(CardSubtitle))
            {
                using (var fontSub = new Font("Segoe UI", 8f, FontStyle.Regular))
                using (var subBrush = new SolidBrush(Color.FromArgb(52, 211, 153))) // Emerald #34D399
                {
                    g.DrawString(CardSubtitle, fontSub, subBrush, 18, 80);
                }
            }
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
