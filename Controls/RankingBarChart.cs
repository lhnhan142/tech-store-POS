using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using PhuKienMayTinh.BLL.DTOs;

namespace PhuKienMayTinh.GUI.Controls
{
    public class RankingBarChart : Control
    {
        private List<TopEmployeeSalesDTO> _items = new List<TopEmployeeSalesDTO>();

        public RankingBarChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.FromArgb(30, 41, 59); // Dark Slate #1E293B
            Font = new Font("Segoe UI", 9f);
        }

        public void SetData(IEnumerable<TopEmployeeSalesDTO> data)
        {
            _items = data?.ToList() ?? new List<TopEmployeeSalesDTO>();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Border
            using (var path = GetRoundedRectPath(new Rectangle(0, 0, Width - 1, Height - 1), 10))
            using (var bgBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            using (var penBorder = new Pen(Color.FromArgb(51, 65, 85), 1))
            {
                g.FillPath(bgBrush, path);
                g.DrawPath(penBorder, path);
            }

            if (_items == null || !_items.Any())
            {
                using (var brush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                {
                    string noData = "Chưa có dữ liệu xếp hạng nhân viên";
                    var sz = g.MeasureString(noData, Font);
                    g.DrawString(noData, Font, brush, (Width - sz.Width) / 2, (Height - sz.Height) / 2);
                }
                return;
            }

            decimal maxSales = _items.Max(x => x.TotalSales);
            if (maxSales <= 0) maxSales = 1;

            float paddingLeft = 20f;
            float paddingRight = 24f;
            float paddingBottom = 16f;
            float availableHeight = Height - 24f - paddingBottom;
            float rowHeight = Math.Min(50f, availableHeight / Math.Max(1, _items.Count));
            if (rowHeight < 38f) rowHeight = 38f;

            float totalContentHeight = rowHeight * _items.Count;
            float startY = (Height - totalContentHeight) / 2f;
            if (startY < 16f) startY = 16f;

            using (var fontName = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var fontSub = new Font("Segoe UI", 8.2f, FontStyle.Regular))
            using (var fontBadge = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (var brushName = new SolidBrush(Color.FromArgb(248, 250, 252)))
            using (var brushSub = new SolidBrush(Color.FromArgb(148, 163, 184)))
            using (var brushSales = new SolidBrush(Color.FromArgb(56, 189, 248)))
            using (var brushWhite = new SolidBrush(Color.White))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    float y = startY + (i * rowHeight);

                    // 1. Huy hiệu Rank (#1, #2, #3, ...)
                    Color badgeColor = i switch
                    {
                        0 => Color.FromArgb(245, 158, 11),  // Vàng Gold
                        1 => Color.FromArgb(148, 163, 184), // Bạc Silver
                        2 => Color.FromArgb(217, 119, 6),   // Đồng Bronze
                        _ => Color.FromArgb(71, 85, 105)    // Slate
                    };

                    float badgeSize = 20f;
                    using (var badgeBrush = new SolidBrush(badgeColor))
                    {
                        g.FillEllipse(badgeBrush, paddingLeft, y, badgeSize, badgeSize);
                        string rankStr = (i + 1).ToString();
                        var rSz = g.MeasureString(rankStr, fontBadge);
                        g.DrawString(rankStr, fontBadge, brushWhite, paddingLeft + (badgeSize - rSz.Width) / 2f, y + (badgeSize - rSz.Height) / 2f);
                    }

                    // 2. Tên nhân viên
                    float textX = paddingLeft + badgeSize + 10f;
                    var nameSz = g.MeasureString(item.EmployeeName, fontName);
                    g.DrawString(item.EmployeeName, fontName, brushName, textX, y);

                    // 3. Số đơn hàng (Vẽ sau tên nếu đủ chỗ, tránh đè lên doanh số)
                    string countStr = $"({item.OrderCount} đơn)";
                    var countSz = g.MeasureString(countStr, fontSub);
                    float countX = textX + nameSz.Width + 8f;

                    // 4. Doanh số bên phải
                    string salesStr = $"{item.TotalSales:N0} đ";
                    var salesSz = g.MeasureString(salesStr, fontName);
                    float salesX = Width - paddingRight - salesSz.Width;
                    g.DrawString(salesStr, fontName, brushSales, salesX, y);

                    if (countX + countSz.Width < salesX - 12f)
                    {
                        g.DrawString(countStr, fontSub, brushSub, countX, y + 1f);
                    }

                    // 5. Thanh Bar tiến độ (Vẽ dưới dòng chữ 24px)
                    float barY = y + 24f;
                    float barWidthTotal = Width - paddingRight - textX;
                    float barFillWidth = (float)((item.TotalSales / maxSales) * (decimal)barWidthTotal);
                    if (barFillWidth < 10) barFillWidth = 10;

                    // Nền thanh bar tối #0F172A
                    var bgBarRect = new RectangleF(textX, barY, barWidthTotal, 7f);
                    using (var bgPath = GetRoundedRectPath(Rectangle.Round(bgBarRect), 3))
                    using (var bgBrush = new SolidBrush(Color.FromArgb(15, 23, 42)))
                    {
                        g.FillPath(bgBrush, bgPath);
                    }

                    // Phần đã đổ màu Neon Cyan -> Emerald
                    var fillBarRect = new RectangleF(textX, barY, barFillWidth, 7f);
                    using (var fillPath = GetRoundedRectPath(Rectangle.Round(fillBarRect), 3))
                    using (var gradBrush = new LinearGradientBrush(
                        fillBarRect,
                        Color.FromArgb(56, 189, 248),
                        Color.FromArgb(52, 211, 153),
                        LinearGradientMode.Horizontal))
                    {
                        g.FillPath(gradBrush, fillPath);
                    }
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
