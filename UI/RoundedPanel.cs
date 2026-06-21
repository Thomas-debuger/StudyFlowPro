namespace StudyFlowPro.UI;

public class RoundedPanel : Panel
{
    private int _radius = 16;

    public int Radius
    {
        get => _radius;
        set
        {
            _radius = Math.Max(0, value);
            UpdateRoundedRegion();
            Invalidate();
        }
    }

    public Color BorderColor { get; set; } = UiTheme.Border;
    public int BorderThickness { get; set; } = 1;

    public RoundedPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = UiTheme.Surface;
        Padding = new Padding(16);
    }

    protected override void OnResize(EventArgs eventArgs)
    {
        base.OnResize(eventArgs);
        UpdateRoundedRegion();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width <= 1 || Height <= 1)
            return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = CreatePath(
            new Rectangle(0, 0, Width - 1, Height - 1),
            Radius);
        using var pen = new Pen(BorderColor, Math.Max(1, BorderThickness));
        e.Graphics.DrawPath(pen, path);
    }

    private void UpdateRoundedRegion()
    {
        if (Width <= 1 || Height <= 1)
            return;

        using GraphicsPath path = CreatePath(new Rectangle(0, 0, Width, Height), Radius);
        Region oldRegion = Region;
        Region = new Region(path);
        oldRegion?.Dispose();
    }

    private static GraphicsPath CreatePath(Rectangle rectangle, int radius)
    {
        int safeRadius = Math.Min(radius, Math.Min(rectangle.Width, rectangle.Height) / 2);
        int diameter = safeRadius * 2;
        var path = new GraphicsPath();

        if (diameter <= 0)
        {
            path.AddRectangle(rectangle);
            return path;
        }

        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
