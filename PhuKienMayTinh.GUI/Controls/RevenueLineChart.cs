using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using PhuKienMayTinh.BLL.DTOs;

namespace PhuKienMayTinh.GUI.Controls
{
    public class RevenueLineChart : Control
    {
        private List<DailyRevenuePointDTO> _points = new List<DailyRevenuePointDTO>();
        private int _hoveredIndex = -1;
        private Point _mouseLocation = Point.Empty;

        public Color ChartColor { get; set; } = Color.FromArgb(56, 189, 248); // Electric Cyan #38BDF8
        public Color GridColor { get; set; } = Color.FromArgb(51, 65, 85);    // Slate 700
        public Color AxisColor { get; set; } = Color.FromArgb(100, 116, 139);
        public Color TextColor { get; set; } = Color.FromArgb(148, 163, 184); // Slate 400

        public RevenueLineChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.FromArgb(30, 41, 59); // Dark Slate #1E293B
            Font = new Font("Segoe UI", 9f, FontStyle.Regular);
        }

        public void SetData(IEnumerable<DailyRevenuePointDTO> data)
        {
            _points = data?.ToList() ?? new List<DailyRevenuePointDTO>();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _mouseLocation = e.Location;

            int prevHover = _hoveredIndex;
            _hoveredIndex = -1;

            if (_points != null && _points.Count > 1)
            {
                int paddingLeft = 75;
                int paddingRight = 25;
                int paddingTop = 38;
                int paddingBottom = 42;

                float chartWidth = Width - paddingLeft - paddingRight;
                float chartHeight = Height - paddingTop - paddingBottom;
                float stepX = chartWidth / (_points.Count - 1);

                decimal maxVal = _points.Max(p => p.Revenue);
                if (maxVal == 0) maxVal = 1000000;

                for (int i = 0; i < _points.Count; i++)
                {
                    float x = paddingLeft + (i * stepX);
                    float y = paddingTop + chartHeight - (float)((_points[i].Revenue / maxVal) * (decimal)chartHeight);

                    double dist = Math.Sqrt(Math.Pow(e.X - x, 2) + Math.Pow(e.Y - y, 2));
                    if (dist <= 16)
                    {
                        _hoveredIndex = i;
                        break;
                    }
                }
            }

            if (prevHover != _hoveredIndex)
            {
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoveredIndex != -1)
            {
                _hoveredIndex = -1;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int paddingLeft = 75;
            int paddingRight = 25;
            int paddingTop = 38;
            int paddingBottom = 42;

            float chartWidth = Width - paddingLeft - paddingRight;
            float chartHeight = Height - paddingTop - paddingBottom;

            if (chartWidth <= 10 || chartHeight <= 10) return;

            // Nền thẻ Slate Dark bo góc 10px
            using (var path = GetRoundedRectPath(new Rectangle(0, 0, Width - 1, Height - 1), 10))
            using (var bgBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            using (var penBorder = new Pen(Color.FromArgb(51, 65, 85), 1))
            {
                g.FillPath(bgBrush, path);
                g.DrawPath(penBorder, path);
            }

            if (_points == null || _points.Count == 0)
            {
                using (var brush = new SolidBrush(TextColor))
                {
                    string noData = "Chưa có dữ liệu doanh thu trong khoảng thời gian này";
                    var sz = g.MeasureString(noData, Font);
                    g.DrawString(noData, Font, brush, (Width - sz.Width) / 2, (Height - sz.Height) / 2);
                }
                return;
            }

            decimal maxVal = _points.Max(p => p.Revenue);
            if (maxVal <= 0) maxVal = 5000000m;
            maxVal = Math.Ceiling(maxVal / 1000000m) * 1000000m;

            // 1. Vẽ các đường lưới ngang và nhãn trục Y
            int yLines = 4;
            using (var gridPen = new Pen(Color.FromArgb(40, 51, 65, 85), 1) { DashStyle = DashStyle.Dash })
            using (var textBrush = new SolidBrush(TextColor))
            using (var fontSmall = new Font("Segoe UI", 8f))
            {
                for (int i = 0; i <= yLines; i++)
                {
                    float y = paddingTop + (chartHeight * i / yLines);
                    g.DrawLine(gridPen, paddingLeft, y, Width - paddingRight, y);

                    decimal labelVal = maxVal - (maxVal * i / yLines);
                    string labelStr = labelVal >= 1000000
                        ? $"{(labelVal / 1000000m):N1}M"
                        : $"{labelVal:N0}";

                    var sz = g.MeasureString(labelStr, fontSmall);
                    g.DrawString(labelStr, fontSmall, textBrush, paddingLeft - sz.Width - 8, y - (sz.Height / 2));
                }
            }

            // 2. Tính tọa độ các điểm đồ thị
            float stepX = chartWidth / Math.Max(1, _points.Count - 1);
            var screenPoints = new List<PointF>();

            for (int i = 0; i < _points.Count; i++)
            {
                float x = paddingLeft + (i * stepX);
                float y = paddingTop + chartHeight - (float)((_points[i].Revenue / maxVal) * (decimal)chartHeight);
                screenPoints.Add(new PointF(x, y));
            }

            // 3. Tô màu Gradient bán trong suốt bên dưới đường cong
            if (screenPoints.Count > 1)
            {
                using (var fillPath = new GraphicsPath())
                {
                    fillPath.AddLine(screenPoints[0].X, paddingTop + chartHeight, screenPoints[0].X, screenPoints[0].Y);
                    fillPath.AddCurve(screenPoints.ToArray(), 0.35f);
                    fillPath.AddLine(screenPoints.Last().X, screenPoints.Last().Y, screenPoints.Last().X, paddingTop + chartHeight);
                    fillPath.CloseFigure();

                    using (var lgb = new LinearGradientBrush(
                        new PointF(0, paddingTop),
                        new PointF(0, paddingTop + chartHeight),
                        Color.FromArgb(90, 56, 189, 248),
                        Color.FromArgb(5, 56, 189, 248)))
                    {
                        g.FillPath(lgb, fillPath);
                    }
                }

                // 4. Vẽ đường cong Spline chính
                using (var linePen = new Pen(ChartColor, 2.8f))
                {
                    linePen.LineJoin = LineJoin.Round;
                    g.DrawCurve(linePen, screenPoints.ToArray(), 0.35f);
                }
            }

            // 5. Vẽ nhãn trục X (Ngày)
            using (var textBrush = new SolidBrush(TextColor))
            using (var fontSmall = new Font("Segoe UI", 8f))
            {
                int stepLabel = Math.Max(1, _points.Count / 7);
                for (int i = 0; i < _points.Count; i += stepLabel)
                {
                    float x = screenPoints[i].X;
                    string dateStr = _points[i].Label;
                    var sz = g.MeasureString(dateStr, fontSmall);
                    g.DrawString(dateStr, fontSmall, textBrush, x - (sz.Width / 2), paddingTop + chartHeight + 8);
                }
            }

            // 6. Vẽ các điểm nút (Dots) và Tooltip khi Hover
            for (int i = 0; i < screenPoints.Count; i++)
            {
                var pt = screenPoints[i];
                bool isHovered = (i == _hoveredIndex);

                float radius = isHovered ? 6.5f : 3.5f;

                using (var brushCore = new SolidBrush(isHovered ? Color.FromArgb(244, 63, 94) : ChartColor))
                using (var penRing = new Pen(Color.FromArgb(30, 41, 59), 2f))
                {
                    g.FillEllipse(brushCore, pt.X - radius, pt.Y - radius, radius * 2, radius * 2);
                    g.DrawEllipse(penRing, pt.X - radius, pt.Y - radius, radius * 2, radius * 2);
                }

                // Tooltip
                if (isHovered)
                {
                    string ttDate = $"Ngày {_points[i].Date:dd/MM/yyyy}";
                    string ttRev = $"{_points[i].Revenue:N0} đ";

                    using (var ttFontBold = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (var ttFontNormal = new Font("Segoe UI", 8f))
                    {
                        var sz1 = g.MeasureString(ttDate, ttFontNormal);
                        var sz2 = g.MeasureString(ttRev, ttFontBold);
                        float ttWidth = Math.Max(sz1.Width, sz2.Width) + 20;
                        float ttHeight = sz1.Height + sz2.Height + 14;

                        float ttX = pt.X - (ttWidth / 2);
                        float ttY = pt.Y - ttHeight - 12;

                        if (ttX < 10) ttX = 10;
                        if (ttX + ttWidth > Width - 10) ttX = Width - 10 - ttWidth;
                        if (ttY < 5) ttY = pt.Y + 16;

                        var ttRect = new RectangleF(ttX, ttY, ttWidth, ttHeight);
                        using (var ttPath = GetRoundedRectPath(Rectangle.Round(ttRect), 8))
                        using (var ttBg = new SolidBrush(Color.FromArgb(240, 15, 23, 42))) // Slate 900
                        using (var ttBorder = new Pen(Color.FromArgb(56, 189, 248), 1.2f))
                        using (var ttTextWhite = new SolidBrush(Color.FromArgb(226, 232, 240)))
                        using (var ttTextGreen = new SolidBrush(Color.FromArgb(52, 211, 153)))
                        {
                            g.FillPath(ttBg, ttPath);
                            g.DrawPath(ttBorder, ttPath);

                            g.DrawString(ttDate, ttFontNormal, ttTextWhite, ttX + 10, ttY + 5);
                            g.DrawString(ttRev, ttFontBold, ttTextGreen, ttX + 10, ttY + 5 + sz1.Height);
                        }
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
