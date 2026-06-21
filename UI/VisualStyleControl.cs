using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class VisualStyleControl : UserControl
{
    private readonly DataService _service;
    private readonly Action<VisualStyleKind> _styleApplied;
    private readonly Dictionary<VisualStyleKind, RoundedPanel> _cards = new();
    private readonly Dictionary<VisualStyleKind, Label> _badges = new();
    private readonly Label _currentStyleLabel = new();
    private VisualStyleKind _selectedStyle;

    public VisualStyleControl(DataService service, Action<VisualStyleKind> styleApplied)
    {
        _service = service;
        _styleApplied = styleApplied;
        _selectedStyle = _service.Data.Settings.VisualStyle;

        Dock = DockStyle.Fill;
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScroll = true;

        BuildInterface();
        RefreshSelection();
    }

    private void BuildInterface()
    {
        // Six styles are arranged as a stable 3 x 2 card grid. The page scrolls only when
        // the window is shorter than the fixed content, avoiding narrow cards and clipped text.
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 1004,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(28, 20, 28, 24),
            Margin = Padding.Empty,
            BackColor = UiTheme.Background
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 684));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));

        Control header = UiTheme.StackedHeader(
            "視覺風格",
            "只切換全系統顏色，不改變字型、位置、功能與資料。",
            out _,
            25);

        var currentCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderColor = UiTheme.BorderAccent,
            Padding = new Padding(18, 12, 18, 12),
            Margin = new Padding(0, 0, 0, 10)
        };
        var currentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        currentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        currentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        _currentStyleLabel.Dock = DockStyle.Fill;
        _currentStyleLabel.TextAlign = ContentAlignment.MiddleLeft;
        _currentStyleLabel.Font = UiTheme.Font(11, FontStyle.Bold);
        _currentStyleLabel.ForeColor = UiTheme.TextPrimary;
        _currentStyleLabel.AutoEllipsis = true;

        var immediateLabel = new Label
        {
            Text = "✓ 立即套用並自動儲存",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiTheme.Success,
            Font = UiTheme.Font(9.5f, FontStyle.Bold)
        };
        currentLayout.Controls.Add(_currentStyleLabel, 0, 0);
        currentLayout.Controls.Add(immediateLabel, 1, 0);
        currentCard.Controls.Add(currentLayout);

        var styleGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Background
        };
        styleGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        styleGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334f));
        styleGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        styleGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        styleGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        VisualStyleKind[] styles =
        {
            VisualStyleKind.Facebook,
            VisualStyleKind.Spotify,
            VisualStyleKind.YouTube,
            VisualStyleKind.Netflix,
            VisualStyleKind.VisualStudio,
            VisualStyleKind.VisualStudioCode
        };

        for (int index = 0; index < styles.Length; index++)
        {
            int column = index % 3;
            int row = index / 3;
            Padding margin = new(
                column == 0 ? 0 : 6,
                row == 0 ? 0 : 8,
                column == 2 ? 0 : 6,
                row == 1 ? 0 : 8);
            styleGrid.Controls.Add(CreateStyleCard(styles[index], margin), column, row);
        }

        var noteCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderColor = UiTheme.Border,
            Padding = new Padding(18),
            Margin = new Padding(0, 12, 0, 0)
        };
        var note = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.TextSecondary,
            Font = UiTheme.Font(9.5f),
            Text = "風格會儲存在目前登入帳號專屬的偏好檔。切換帳號時會自動載入各自上次使用的風格，不會沿用其他人的選擇；課程顏色、考古題與學習資料也不會被修改。"
        };
        noteCard.Controls.Add(note);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(currentCard, 0, 1);
        root.Controls.Add(styleGrid, 0, 2);
        root.Controls.Add(noteCard, 0, 3);
        Controls.Add(root);
        AutoScrollMinSize = new Size(0, root.Height + 8);
    }

    private Control CreateStyleCard(VisualStyleKind style, Padding margin)
    {
        ThemePalette palette = UiTheme.GetPalette(style);
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderColor = UiTheme.Border,
            BorderThickness = 1,
            Padding = new Padding(18),
            Margin = margin,
            Cursor = Cursors.Hand,
            Tag = style
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Cursor = Cursors.Hand
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        var title = new Label
        {
            Text = UiTheme.GetDisplayName(style),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.TextPrimary,
            Font = UiTheme.Font(15, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        var badge = new Label
        {
            Text = string.Empty,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Primary,
            Font = UiTheme.Font(9, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _badges[style] = badge;

        var preview = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 8, 0, 8),
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface,
            Cursor = Cursors.Hand
        };
        for (int index = 0; index < 4; index++)
            preview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        preview.Controls.Add(ColorBlock(palette.Sidebar, new Padding(0, 0, 4, 0)), 0, 0);
        preview.Controls.Add(ColorBlock(palette.Primary, new Padding(4, 0, 4, 0)), 1, 0);
        preview.Controls.Add(ColorBlock(palette.Surface, new Padding(4, 0, 4, 0)), 2, 0);
        Color fourthPreviewColor = style is VisualStyleKind.Netflix
            or VisualStyleKind.VisualStudio
            or VisualStyleKind.VisualStudioCode
            ? palette.PrimaryDark
            : palette.Success;
        preview.Controls.Add(ColorBlock(fourthPreviewColor, new Padding(4, 0, 0, 0)), 3, 0);

        var description = new Label
        {
            Text = UiTheme.GetDescription(style),
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9.2f),
            Cursor = Cursors.Hand
        };

        var characteristics = new Label
        {
            Text = style switch
            {
                VisualStyleKind.Spotify => "深色背景　綠色操作重點　降低夜間亮度",
                VisualStyleKind.YouTube => "明亮背景　紅色操作重點　高辨識度",
                VisualStyleKind.Netflix => "黑色背景　Netflix 紅色重點　沉浸劇院感",
                VisualStyleKind.VisualStudio => "深黑灰背景　Visual Studio 紫色重點　專業開發感",
                VisualStyleKind.VisualStudioCode => "近黑工作區　VS Code 藍色重點　沉浸開發感",
                _ => "明亮背景　藍色操作重點　穩定預設"
            },
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = UiTheme.TextSecondary,
            Font = UiTheme.Font(8.8f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        Button apply = UiTheme.PrimaryButton("套用此風格");
        apply.Dock = DockStyle.Fill;
        apply.AutoSize = false;
        apply.Margin = Padding.Empty;
        apply.Click += (_, _) => ApplyStyle(style);

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(badge, 0, 1);
        layout.Controls.Add(preview, 0, 2);
        layout.Controls.Add(description, 0, 3);
        layout.Controls.Add(characteristics, 0, 4);
        layout.Controls.Add(apply, 0, 5);
        card.Controls.Add(layout);

        HookCardClick(card, style);
        _cards[style] = card;
        return card;
    }

    private static Control ColorBlock(Color color, Padding margin)
    {
        return new ThemePreviewPanel
        {
            Dock = DockStyle.Fill,
            BackColor = color,
            Margin = margin
        };
    }

    private void HookCardClick(Control root, VisualStyleKind style)
    {
        root.Click += (_, _) => SelectStyle(style);
        foreach (Control child in root.Controls)
            HookCardClick(child, style);
    }

    private void SelectStyle(VisualStyleKind style)
    {
        _selectedStyle = style;
        RefreshSelection();
    }

    private void ApplyStyle(VisualStyleKind style)
    {
        _selectedStyle = style;
        _service.Data.Settings.VisualStyle = style;
        UiTheme.ApplyVisualStyle(style);
        _service.Log(ActivityType.Updated, "System", null, $"切換視覺風格：{UiTheme.GetDisplayName(style)}");
        _service.SaveAndNotify();

        _styleApplied?.Invoke(style);
        RefreshSelection();
    }

    public void RefreshStyleState()
    {
        _selectedStyle = _service.Data.Settings.VisualStyle;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        VisualStyleKind active = _service.Data.Settings.VisualStyle;
        _currentStyleLabel.Text = $"目前使用：{UiTheme.GetDisplayName(active)}";

        foreach (KeyValuePair<VisualStyleKind, RoundedPanel> pair in _cards)
        {
            bool selected = pair.Key == _selectedStyle;
            bool applied = pair.Key == active;
            pair.Value.BorderThickness = selected ? 3 : 1;
            pair.Value.BorderColor = selected ? UiTheme.Primary : UiTheme.Border;
            _badges[pair.Key].Text = applied ? "● 目前使用中" : selected ? "○ 已選取，按下方按鈕套用" : "";
            pair.Value.Invalidate();
        }
    }
}
