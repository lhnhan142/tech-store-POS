using System.ComponentModel;

public class RoundedPanel : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue(10)]
    public int Radius { get; set; } = 10;

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        var r = new Rectangle(0, 0, Width - 1, Height - 1);
        int d = Radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        using var brush = new SolidBrush(BackColor);
        e.Graphics.FillPath(brush, path);
        Region = new Region(path);
    }
}