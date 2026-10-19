using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using PhuKienMayTinh.BLL.DTOs;

namespace PhuKienMayTinh.GUI.Controls
{
    public class CategoryDoughnutChart : Control
    {
        private List<CategoryRevenueSliceDTO> _slices = new List<CategoryRevenueSliceDTO>();

        private readonly Color[] _palette = new[]
        {
            Color.FromArgb(56, 189, 248),  // Neon Cyan
            Color.FromArgb(52, 211, 153),  // Emerald
            Color.FromArgb(251, 191, 36),  // Amber
            Color.FromArgb(168, 85, 247),  // Purple
            Color.FromArgb(244, 63, 94),   // Rose
            Color.FromArgb(129, 140, 248), // Indigo
            Color.FromArgb(251, 146, 60)   // Orange
        };

        public CategoryDoughnutChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.FromArgb(30, 41, 59); // Dark Slate #1E293B
            Font = new Font("Segoe UI", 9f);
        }

        public void SetData(IEnumerable<CategoryRevenueSliceDTO> slices)
        {
            _slices = slices?.ToList() ?? new List<CategoryRevenueSliceDTO>();
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

            if (_slices == null || !_slices.Any())
            {
                using (var brush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                {
                    string noData = "Chưa có dữ liệu danh mục";
                    var sz = g.MeasureString(noData, Font);
                    g.DrawString(noData, Font, brush, (Width - sz.Width) / 2, (Height - sz.Height) / 2);
                }
                return;
            }

            decimal totalRevenue = _slices.Sum(s => s.Revenue);
            if (totalRevenue == 0) totalRevenue = 1;

            // Vị trí bánh Donut
            float chartDiameter = Math.Min(Height - 36, Width * 0.38f);
            if (chartDiameter < 50) chartDiameter = 50;

            float chartX = 20f;
            float chartY = (Height - chartDiameter) / 2f;
            var chartRect = new RectangleF(chartX, chartY, chartDiameter, chartDiameter);

            // 1. Vẽ các lát bánh tròn
            float startAngle = -90f;
            for (int i = 0; i < _slices.Count; i++)
            {
                var slice = _slices[i];
                float sweepAngle = (float)((slice.Revenue / totalRevenue) * 360m);
                if (sweepAngle <= 0) continue;

                var color = _palette[i % _palette.Length];
                using (var brush = new SolidBrush(color))
                {
                    g.FillPie(brush, chartRect.X, chartRect.Y, chartRect.Width, chartRect.Height, startAngle, sweepAngle);
                }

                // Đường viền chia lát màu Slate Dark
                using (var penDark = new Pen(Color.FromArgb(30, 41, 59), 2f))
                {
                    g.DrawPie(penDark, chartRect.X, chartRect.Y, chartRect.Width, chartRect.Height, startAngle, sweepAngle);
                }

                startAngle += sweepAngle;
            }

            // 2. Lỗ tròn ở giữa (Donut hole)
            float holeDiameter = chartDiameter * 0.58f;
            float holeX = chartX + (chartDiameter - holeDiameter) / 2f;
            float holeY = chartY + (chartDiameter - holeDiameter) / 2f;
            var holeRect = new RectangleF(holeX, holeY, holeDiameter, holeDiameter);

            using (var holeBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            {
                g.FillEllipse(holeBrush, holeRect);
            }

            // Nhãn text ở tâm bánh
            using (var fontCenterTitle = new Font("Segoe UI", 7.5f, FontStyle.Regular))
            using (var fontCenterVal = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (var brushGray = new SolidBrush(Color.FromArgb(148, 163, 184)))
            using (var brushLight = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                string cTitle = "TỔNG THU";
                string cVal = totalRevenue >= 1000000 ? $"{(totalRevenue / 1000000m):N1}M" : $"{totalRevenue:N0}";

                var sz1 = g.MeasureString(cTitle, fontCenterTitle);
                var sz2 = g.MeasureString(cVal, fontCenterVal);

                float ty = holeY + (holeDiameter - (sz1.Height + sz2.Height)) / 2f;
                g.DrawString(cTitle, fontCenterTitle, brushGray, holeX + (holeDiameter - sz1.Width) / 2f, ty);
                g.DrawString(cVal, fontCenterVal, brushLight, holeX + (holeDiameter - sz2.Width) / 2f, ty + sz1.Height);
            }

            // 3. Vẽ Legend bên phải (ngăn chặn chèn chữ)
            float legendX = chartX + chartDiameter + 20f;
            float availableLegendHeight = Height - 36f;
            float rowHeight = Math.Min(42f, availableLegendHeight / Math.Max(1, _slices.Count));
            float legendY = (Height - (rowHeight * _slices.Count)) / 2f;
            if (legendY < 16f) legendY = 16f;

            using (var fontLegend = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (var fontBold = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var fontCompact = new Font("Segoe UI", 8.2f, FontStyle.Regular))
            using (var textBrush = new SolidBrush(Color.FromArgb(226, 232, 240)))
            using (var valBrush = new SolidBrush(Color.FromArgb(56, 189, 248)))
            {
                for (int i = 0; i < _slices.Count; i++)
                {
                    var slice = _slices[i];
                    var color = _palette[i % _palette.Length];

                    float cy = legendY + (i * rowHeight);

                    // Chấm màu
                    using (var colorBrush = new SolidBrush(color))
                    {
                        g.FillEllipse(colorBrush, legendX, cy + 4, 10, 10);
                    }

                    if (rowHeight >= 32f)
                    {
                        // Dòng 1: Tên danh mục + %
                        string catText = $"{slice.Category} ({slice.Percentage:N1}%)";
                        g.DrawString(catText, fontLegend, textBrush, legendX + 16, cy);

                        // Dòng 2: Doanh thu (cách dòng rõ ràng 16px, không bị đè chữ)
                        string revText = $"{slice.Revenue:N0} đ";
                        g.DrawString(revText, fontBold, valBrush, legendX + 16, cy + 16);
                    }
                    else
                    {
                        // Khi không gian hạn chế: hiển thị trên 1 dòng duy nhất để không bao giờ bị đè chữ
                        string fullLine = $"{slice.Category} ({slice.Percentage:N1}%): {slice.Revenue:N0} đ";
                        g.DrawString(fullLine, fontCompact, textBrush, legendX + 16, cy + 1);
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
