using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace GUI.Controls
{
    public enum ChartType
    {
        Bar,
        Line
    }

    public class ChartItem
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Tooltip { get; set; }

        public ChartItem() { }
        public ChartItem(string label, decimal value, string? tooltip = null)
        {
            Label = label;
            Value = value;
            Tooltip = tooltip;
        }
    }

    /// <summary>
    /// UserControl vẽ biểu đồ chuyên nghiệp bằng GDI+ (Bar Chart & Line Chart)
    /// Hỗ trợ chống răng cưa, tự động định tỉ lệ, lưới mờ, nhãn và trạng thái không có dữ liệu.
    /// </summary>
    [DesignerCategory("Code")]
    public class ChartControl : UserControl
    {
        private readonly List<ChartItem> _items = new();
        private ChartType _chartType = ChartType.Bar;
        private string _title = "Biểu đồ";
        private string _valueFormat = "{0:N0}";
        private Color _primaryColor = Color.FromArgb(0, 112, 243);
        private Color _secondaryColor = Color.FromArgb(14, 85, 158);
        private Color _gridColor = Color.FromArgb(235, 238, 242);
        private Color _axisTextColor = Color.FromArgb(120, 125, 140);

        public ChartControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChartType ChartType
        {
            get => _chartType;
            set { _chartType = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color PrimaryColor
        {
            get => _primaryColor;
            set { _primaryColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ValueFormat
        {
            get => _valueFormat;
            set { _valueFormat = value; Invalidate(); }
        }

        public void SetData(IEnumerable<ChartItem> items)
        {
            _items.Clear();
            if (items != null)
            {
                _items.AddRange(items);
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int padLeft = 75;
            int padRight = 35;
            int padTop = 50;
            int padBottom = 45;

            // 1. Vẽ tiêu đề biểu đồ
            using (var titleFont = new Font("Segoe UI", 11.5F, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(20, 25, 45)))
            {
                g.DrawString(_title, titleFont, titleBrush, new PointF(18, 16));
            }

            int plotWidth = Width - padLeft - padRight;
            int plotHeight = Height - padTop - padBottom;

            if (plotWidth <= 20 || plotHeight <= 20) return;

            // 2. Kiểm tra nếu không có dữ liệu
            if (_items.Count == 0 || _items.All(x => x.Value == 0))
            {
                string emptyMsg = "Không có dữ liệu trong khoảng thời gian đã chọn";
                using var emptyFont = new Font("Segoe UI", 10.5F, FontStyle.Italic);
                using var emptyBrush = new SolidBrush(Color.FromArgb(160, 165, 175));
                var size = g.MeasureString(emptyMsg, emptyFont);
                g.DrawString(emptyMsg, emptyFont, emptyBrush,
                    padLeft + (plotWidth - size.Width) / 2,
                    padTop + (plotHeight - size.Height) / 2);
                return;
            }

            // 3. Tính toán Max Value cho trục Y
            decimal maxValue = _items.Max(x => x.Value);
            if (maxValue <= 0) maxValue = 100;
            // Làm tròn cận trên mượt mà (VD: 1.2M -> 1.5M, 78 -> 100)
            decimal scaleMax = GetNiceCeiling(maxValue);

            // 4. Vẽ các đường lưới ngang và nhãn trục Y
            int gridSteps = 4;
            using (var gridPen = new Pen(_gridColor, 1f) { DashStyle = DashStyle.Dash })
            using (var axisFont = new Font("Segoe UI", 8.5F, FontStyle.Regular))
            using (var axisBrush = new SolidBrush(_axisTextColor))
            {
                for (int i = 0; i <= gridSteps; i++)
                {
                    float y = padTop + plotHeight - (i * (float)plotHeight / gridSteps);
                    decimal val = scaleMax * i / gridSteps;

                    g.DrawLine(gridPen, padLeft, y, padLeft + plotWidth, y);

                    string yLabel = FormatNumberShort(val);
                    var strSize = g.MeasureString(yLabel, axisFont);
                    g.DrawString(yLabel, axisFont, axisBrush, padLeft - strSize.Width - 8, y - strSize.Height / 2);
                }
            }

            // 5. Vẽ Bar hoặc Line
            if (_chartType == ChartType.Bar)
            {
                DrawBarChart(g, padLeft, padTop, plotWidth, plotHeight, scaleMax);
            }
            else
            {
                DrawLineChart(g, padLeft, padTop, plotWidth, plotHeight, scaleMax);
            }
        }

        private static bool ShouldDrawAxisLabel(int index, int totalCount, int step)
        {
            if (step <= 1) return true;
            if (index == totalCount - 1) return true; // Luôn hiển thị nhãn cuối cùng
            if (index % step == 0 && (totalCount - 1 - index) >= Math.Max(2, step - 1))
                return true;
            return false;
        }

        private void DrawBarChart(Graphics g, int x0, int y0, int w, int h, decimal scaleMax)
        {
            int count = _items.Count;
            float slotWidth = (float)w / count;
            float barWidth = Math.Max(10, Math.Min(46, slotWidth * 0.55f));

            int labelStep = count switch
            {
                > 25 => 5,
                > 14 => 3,
                > 8 => 2,
                _ => 1
            };

            using var axisFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            using var valFont = new Font("Segoe UI", 8F, FontStyle.Bold);
            using var axisBrush = new SolidBrush(_axisTextColor);
            using var valBrush = new SolidBrush(_secondaryColor);
            using var tickPen = new Pen(_gridColor, 1f);

            for (int i = 0; i < count; i++)
            {
                var item = _items[i];
                float cx = x0 + i * slotWidth + slotWidth / 2;
                float barHeight = (float)(item.Value / scaleMax) * h;
                float barX = cx - barWidth / 2;
                float barY = y0 + h - barHeight;

                // Vẽ cột với góc bo nhẹ
                if (barHeight > 0)
                {
                    using var brush = new LinearGradientBrush(
                        new PointF(barX, barY),
                        new PointF(barX, y0 + h),
                        _primaryColor,
                        _secondaryColor);

                    var rect = new RectangleF(barX, barY, barWidth, barHeight);
                    using var path = GetRoundedRect(rect, 4);
                    g.FillPath(brush, path);

                    // Nhãn giá trị trên đầu cột
                    string valText = FormatNumberShort(item.Value);
                    var vSize = g.MeasureString(valText, valFont);
                    g.DrawString(valText, valFont, valBrush, cx - vSize.Width / 2, barY - vSize.Height - 3);
                }

                // Vẽ vạch chia nhẹ trục X
                g.DrawLine(tickPen, cx, y0 + h, cx, y0 + h + 4);

                // Nhãn trục X (tự động giãn cách tránh đè chữ)
                if (ShouldDrawAxisLabel(i, count, labelStep))
                {
                    var lblSize = g.MeasureString(item.Label, axisFont);
                    g.DrawString(item.Label, axisFont, axisBrush, cx - lblSize.Width / 2, y0 + h + 7);
                }
            }
        }

        private void DrawLineChart(Graphics g, int x0, int y0, int w, int h, decimal scaleMax)
        {
            int count = _items.Count;
            if (count < 1) return;

            float slotWidth = (float)w / (count > 1 ? count - 1 : 1);
            var points = new PointF[count];

            for (int i = 0; i < count; i++)
            {
                float x = count > 1 ? x0 + i * slotWidth : x0 + w / 2;
                float y = y0 + h - (float)(_items[i].Value / scaleMax) * h;
                points[i] = new PointF(x, y);
            }

            // Vùng đổ bóng mờ dưới đường
            if (count > 1)
            {
                using var fillPath = new GraphicsPath();
                fillPath.AddLine(points[0].X, y0 + h, points[0].X, points[0].Y);
                for (int i = 1; i < count; i++)
                {
                    fillPath.AddLine(points[i - 1], points[i]);
                }
                fillPath.AddLine(points[^1], new PointF(points[^1].X, y0 + h));
                fillPath.CloseFigure();

                using var gradBrush = new LinearGradientBrush(
                    new PointF(0, y0),
                    new PointF(0, y0 + h),
                    Color.FromArgb(60, _primaryColor),
                    Color.FromArgb(5, _primaryColor));
                g.FillPath(gradBrush, fillPath);
            }

            // Vẽ đường gấp khúc
            using (var linePen = new Pen(_primaryColor, 2.5f))
            {
                linePen.LineJoin = LineJoin.Round;
                if (count > 1)
                {
                    g.DrawLines(linePen, points);
                }
            }

            int labelStep = count switch
            {
                > 25 => 5,
                > 14 => 3,
                > 8 => 2,
                _ => 1
            };

            // Vẽ các điểm mốc tròn và nhãn
            using var pointBrush = new SolidBrush(Color.White);
            using var pointPen = new Pen(_primaryColor, 2.5f);
            using var axisFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            using var valFont = new Font("Segoe UI", 8F, FontStyle.Bold);
            using var axisBrush = new SolidBrush(_axisTextColor);
            using var valBrush = new SolidBrush(_secondaryColor);
            using var tickPen = new Pen(_gridColor, 1f);

            for (int i = 0; i < count; i++)
            {
                var pt = points[i];
                var item = _items[i];
                float radius = 4f;

                g.FillEllipse(pointBrush, pt.X - radius, pt.Y - radius, radius * 2, radius * 2);
                g.DrawEllipse(pointPen, pt.X - radius, pt.Y - radius, radius * 2, radius * 2);

                // Nhãn giá trị
                if (item.Value > 0)
                {
                    string valText = FormatNumberShort(item.Value);
                    var vSize = g.MeasureString(valText, valFont);
                    g.DrawString(valText, valFont, valBrush, pt.X - vSize.Width / 2, pt.Y - vSize.Height - 5);
                }

                // Vạch chia nhẹ trục X
                g.DrawLine(tickPen, pt.X, y0 + h, pt.X, y0 + h + 4);

                // Nhãn trục X (tự động giãn cách)
                if (ShouldDrawAxisLabel(i, count, labelStep))
                {
                    var lblSize = g.MeasureString(item.Label, axisFont);
                    g.DrawString(item.Label, axisFont, axisBrush, pt.X - lblSize.Width / 2, y0 + h + 7);
                }
            }
        }

        private static GraphicsPath GetRoundedRect(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.CloseFigure();
            return path;
        }

        private static decimal GetNiceCeiling(decimal val)
        {
            if (val <= 0) return 100;
            double v = (double)val;
            double exponent = Math.Floor(Math.Log10(v));
            double fraction = v / Math.Pow(10, exponent);

            double niceFraction;
            if (fraction <= 1.2) niceFraction = 1.5;
            else if (fraction <= 2) niceFraction = 2.5;
            else if (fraction <= 5) niceFraction = 6;
            else niceFraction = 10;

            return (decimal)(niceFraction * Math.Pow(10, exponent));
        }

        private static string FormatNumberShort(decimal number)
        {
            if (number >= 1_000_000_000)
                return (number / 1_000_000_000m).ToString("0.#") + "B";
            if (number >= 1_000_000)
                return (number / 1_000_000m).ToString("0.#") + "M";
            if (number >= 1_000)
                return (number / 1_000m).ToString("0.#") + "k";
            return number.ToString("0");
        }
    }
}
