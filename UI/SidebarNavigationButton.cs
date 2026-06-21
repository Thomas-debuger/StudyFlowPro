namespace StudyFlowPro.UI;

/// <summary>
/// 側邊導覽按鈕。圖示與文字分開繪製，確保每一列的圖示欄寬與文字起點一致。
/// </summary>
internal sealed class SidebarNavigationButton : Button
{
    private const int IconLeft = 12;
    private const int IconWidth = 22;
    private const int TextLeft = 44;
    private const int RightPadding = 8;

    public SidebarNavigationButton(string iconText, string labelText)
    {
        IconText = iconText;
        LabelText = labelText;

        Text = string.Empty;
        UseMnemonic = false;
        UseVisualStyleBackColor = false;
        TabStop = true;
    }

    public string IconText { get; }

    public string LabelText { get; }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Rectangle iconRectangle = new(IconLeft, 0, IconWidth, ClientSize.Height);
        Rectangle textRectangle = new(
            TextLeft,
            0,
            Math.Max(0, ClientSize.Width - TextLeft - RightPadding),
            ClientSize.Height);

        using var iconFont = new Font(
            "Segoe UI Symbol",
            9.5f,
            FontStyle.Regular,
            GraphicsUnit.Point);

        TextRenderer.DrawText(
            e.Graphics,
            IconText,
            iconFont,
            iconRectangle,
            ForeColor,
            TextFormatFlags.HorizontalCenter
            | TextFormatFlags.VerticalCenter
            | TextFormatFlags.SingleLine
            | TextFormatFlags.NoPadding
            | TextFormatFlags.NoPrefix);

        TextRenderer.DrawText(
            e.Graphics,
            LabelText,
            Font,
            textRectangle,
            ForeColor,
            TextFormatFlags.Left
            | TextFormatFlags.VerticalCenter
            | TextFormatFlags.SingleLine
            | TextFormatFlags.EndEllipsis
            | TextFormatFlags.NoPadding
            | TextFormatFlags.NoPrefix);
    }
}
