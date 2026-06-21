namespace StudyFlowPro.UI;

/// <summary>
/// 可取得焦點並以滑鼠滾輪穩定捲動的 Panel。
/// WinForms 一般 Panel 在子控制項很多時，滑鼠滾輪訊息不一定會落到外層容器；
/// 此元件讓分析型長頁面在縮小視窗後仍能自然瀏覽。
/// </summary>
public sealed class WheelScrollPanel : Panel
{
    public int WheelStep { get; set; } = 56;

    public WheelScrollPanel()
    {
        AutoScroll = true;
        TabStop = true;
        SetStyle(ControlStyles.Selectable, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        Focus();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (!AutoScroll)
        {
            base.OnMouseWheel(e);
            return;
        }

        int currentX = Math.Max(0, -AutoScrollPosition.X);
        int currentY = Math.Max(0, -AutoScrollPosition.Y);
        int direction = Math.Sign(e.Delta);
        int notches = Math.Max(1, Math.Abs(e.Delta) / SystemInformation.MouseWheelScrollDelta);
        int maximumY = Math.Max(0, DisplayRectangle.Height - ClientSize.Height);
        int targetY = Math.Clamp(currentY - direction * WheelStep * notches, 0, maximumY);

        AutoScrollPosition = new Point(currentX, targetY);

        if (e is HandledMouseEventArgs handled)
            handled.Handled = true;
    }
}
