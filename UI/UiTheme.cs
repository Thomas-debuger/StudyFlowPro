using StudyFlowPro.Models;
using System.Runtime.CompilerServices;

namespace StudyFlowPro.UI;

public sealed record ThemePalette(
    Color Sidebar,
    Color SidebarHover,
    Color Background,
    Color Surface,
    Color SurfaceAlt,
    Color InputBackground,
    Color TextPrimary,
    Color TextSecondary,
    Color Muted,
    Color Border,
    Color Primary,
    Color PrimaryDark,
    Color PrimarySoft,
    Color BorderAccent,
    Color Selection,
    Color GridHeader,
    Color SidebarText,
    Color SidebarAccentText,
    Color Success,
    Color Warning,
    Color Danger,
    Color Purple,
    Color OnPrimary);

public static class UiTheme
{
    private static readonly IReadOnlyDictionary<VisualStyleKind, ThemePalette> Palettes =
        new Dictionary<VisualStyleKind, ThemePalette>
        {
            [VisualStyleKind.Facebook] = new(
                Html("#0F172A"), Html("#1E293B"), Html("#F8FAFC"), Color.White,
                Html("#F1F5F9"), Color.White, Html("#0F172A"), Html("#334155"),
                Html("#64748B"), Html("#E2E8F0"), Html("#2563EB"), Html("#1D4ED8"),
                Html("#EFF6FF"), Html("#BFDBFE"), Html("#DBEAFE"), Html("#E2E8F0"),
                Html("#CBD5E1"), Html("#93C5FD"), Html("#059669"), Html("#D97706"),
                Html("#DC2626"), Html("#7C3AED"), Color.White),

            [VisualStyleKind.Spotify] = new(
                Html("#000000"), Html("#282828"), Html("#121212"), Html("#181818"),
                Html("#242424"), Html("#242424"), Html("#FFFFFF"), Html("#E5E7EB"),
                Html("#A7A7A7"), Html("#333333"), Html("#1DB954"), Html("#169C46"),
                Html("#153D25"), Html("#2E6D42"), Html("#254B32"), Html("#242424"),
                Html("#B3B3B3"), Html("#1ED760"), Html("#1DB954"), Html("#F59E0B"),
                Html("#E91429"), Html("#B178FF"), Color.White),

            [VisualStyleKind.YouTube] = new(
                Html("#0F0F0F"), Html("#272727"), Html("#F9F9F9"), Color.White,
                Html("#F2F2F2"), Color.White, Html("#0F0F0F"), Html("#3F3F3F"),
                Html("#606060"), Html("#E5E5E5"), Html("#FF0000"), Html("#CC0000"),
                Html("#FFF0F0"), Html("#FFC7C7"), Html("#FFE3E3"), Html("#F2F2F2"),
                Html("#F1F1F1"), Html("#FFB3B3"), Html("#0F9D58"), Html("#F9AB00"),
                Html("#D93025"), Html("#7E57C2"), Color.White),

            // Netflix 官方品牌紅為 #E50914，深紅為 #B20710。
            // 其餘介面以黑色、炭黑與少量語意色構成，只更換顏色，不變更版面。
            [VisualStyleKind.Netflix] = new(
                Html("#000000"), Html("#181818"), Html("#0B0B0B"), Html("#141414"),
                Html("#1F1F1F"), Html("#232323"), Html("#FFFFFF"), Html("#E5E5E5"),
                Html("#B3B3B3"), Html("#333333"), Html("#E50914"), Html("#B20710"),
                Html("#351013"), Html("#6B171D"), Html("#421116"), Html("#1F1F1F"),
                Html("#D1D5DB"), Html("#FF8A92"), Html("#46D369"), Html("#F5A623"),
                Html("#E50914"), Html("#9B59B6"), Color.White),

            // Visual Studio-inspired dark palette. Microsoft documents a dark IDE theme and
            // uses #5649B0 as the current semantic AccentFillDefault starter value.
            // Only the shared theme colors change; control layout, typography, and data stay intact.
            [VisualStyleKind.VisualStudio] = new(
                Html("#15131A"), Html("#25212C"), Html("#1E1E22"), Html("#25242A"),
                Html("#2D2B33"), Html("#34313B"), Html("#F3F0F8"), Html("#D7D2DF"),
                Html("#A9A3B2"), Html("#44404C"), Html("#5649B0"), Html("#463B92"),
                Html("#2E294B"), Html("#7469C4"), Html("#3A345D"), Html("#302D37"),
                Html("#DDD8E6"), Html("#C8C0FF"), Html("#3FB950"), Html("#D29922"),
                Html("#F85149"), Html("#C586C0"), Color.White),

            // Visual Studio Code Dark Modern-inspired palette. The official built-in theme
            // uses near-black workbench surfaces with #0078D4 as its primary blue accent.
            // Only shared semantic colors are changed; layout, typography, data, and behavior stay intact.
            [VisualStyleKind.VisualStudioCode] = new(
                Html("#181818"), Html("#2B2B2B"), Html("#1F1F1F"), Html("#252526"),
                Html("#2B2B2B"), Html("#313131"), Html("#CCCCCC"), Html("#D7D7D7"),
                Html("#9D9D9D"), Html("#3C3C3C"), Html("#0078D4"), Html("#026EC1"),
                Html("#172B3A"), Html("#2488DB"), Html("#264F78"), Html("#181818"),
                Html("#CCCCCC"), Html("#85B6FF"), Html("#2EA043"), Html("#D29922"),
                Html("#F85149"), Html("#C586C0"), Color.White)
        };

    private static ThemePalette _palette = Palettes[VisualStyleKind.Facebook];

    public static VisualStyleKind CurrentStyle { get; private set; } = VisualStyleKind.Facebook;
    public static int Version { get; private set; } = 1;
    public static ThemePalette Palette => _palette;

    // 舊程式原本使用的名稱全部保留，避免為了換色而改動版面與功能。
    public static Color Navy => _palette.TextPrimary;
    public static Color Sidebar => _palette.Sidebar;
    public static Color Slate => _palette.TextSecondary;
    public static Color Muted => _palette.Muted;
    public static Color Border => _palette.Border;
    public static Color Background => _palette.Background;
    public static Color Surface => _palette.Surface;
    public static Color SurfaceAlt => _palette.SurfaceAlt;
    public static Color InputBackground => _palette.InputBackground;
    public static Color Primary => _palette.Primary;
    public static Color PrimaryDark => _palette.PrimaryDark;
    public static Color PrimarySoft => _palette.PrimarySoft;
    public static Color BorderAccent => _palette.BorderAccent;
    public static Color Selection => _palette.Selection;
    public static Color GridHeader => _palette.GridHeader;
    public static Color SidebarText => _palette.SidebarText;
    public static Color SidebarAccentText => _palette.SidebarAccentText;
    public static Color Success => _palette.Success;
    public static Color Warning => _palette.Warning;
    public static Color Danger => _palette.Danger;
    public static Color Purple => _palette.Purple;
    public static Color TextPrimary => _palette.TextPrimary;
    public static Color TextSecondary => _palette.TextSecondary;
    public static Color OnPrimary => _palette.OnPrimary;

    public static ThemePalette GetPalette(VisualStyleKind style) => Palettes[style];

    public static string GetDisplayName(VisualStyleKind style) => style switch
    {
        VisualStyleKind.Spotify => "Spotify 顏色風格",
        VisualStyleKind.YouTube => "YouTube 顏色風格",
        VisualStyleKind.Netflix => "Netflix 顏色風格",
        VisualStyleKind.VisualStudio => "Visual Studio 顏色風格",
        VisualStyleKind.VisualStudioCode => "Visual Studio Code 顏色風格",
        _ => "Facebook 顏色風格"
    };

    public static string GetDescription(VisualStyleKind style) => style switch
    {
        VisualStyleKind.Spotify => "以黑色與深灰為主，搭配 Spotify 綠色重點，適合夜間與沉浸式使用。",
        VisualStyleKind.YouTube => "以白色、炭黑與紅色為主，對比清楚，操作重點醒目。",
        VisualStyleKind.Netflix => "以黑色與炭黑為主，搭配 Netflix 紅色操作重點，呈現沉浸式劇院質感。",
        VisualStyleKind.VisualStudio => "以 Visual Studio 深色介面為靈感，使用黑灰色表面與紫色操作重點。",
        VisualStyleKind.VisualStudioCode => "以 VS Code Dark Modern 為靈感，使用近黑工作區、深灰表面與藍色操作重點。",
        _ => "目前的預設風格，以深藍、Facebook 藍與明亮白色呈現。"
    };

    public static void ApplyVisualStyle(VisualStyleKind style, bool updateOpenWindows = true)
    {
        if (!Palettes.ContainsKey(style))
            style = VisualStyleKind.Facebook;

        CurrentStyle = style;
        _palette = Palettes[style];
        Version++;

        if (updateOpenWindows)
            ThemeRuntime.ApplyToOpenWindows();
    }

    public static Font Font(float size, FontStyle style = FontStyle.Regular) =>
        new("Microsoft JhengHei UI", size, style);

    public static Label Heading(string text, float size = 24) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = TextPrimary,
        Font = Font(size, FontStyle.Bold)
    };

    public static Label SubText(string text) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = Muted,
        Font = Font(10)
    };

    public static TableLayoutPanel StackedHeader(
        string title,
        string subtitle,
        out Label titleLabel,
        float titleSize = 24,
        string tertiary = "")
    {
        bool hasTertiary = !string.IsNullOrWhiteSpace(tertiary);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = hasTertiary ? 3 : 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = Background
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        if (hasTertiary)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 27));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        }
        else
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        }

        titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            AutoSize = false,
            AutoEllipsis = true,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = TextPrimary,
            Font = Font(titleSize, FontStyle.Bold)
        };

        var subtitleLabel = new Label
        {
            Text = subtitle,
            Dock = DockStyle.Fill,
            AutoSize = false,
            AutoEllipsis = true,
            Margin = Padding.Empty,
            Padding = new Padding(2, 0, 0, 0),
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = Muted,
            Font = Font(10)
        };

        layout.Controls.Add(titleLabel, 0, 0);
        layout.Controls.Add(subtitleLabel, 0, 1);

        if (hasTertiary)
        {
            layout.Controls.Add(new Label
            {
                Text = tertiary,
                Dock = DockStyle.Fill,
                AutoSize = false,
                AutoEllipsis = true,
                Margin = Padding.Empty,
                Padding = new Padding(2, 0, 0, 0),
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = Muted,
                Font = Font(8.8f)
            }, 0, 2);
        }

        return layout;
    }

    private static Button BaseButton(string text) => new()
    {
        Text = text,
        Height = 38,
        AutoSize = true,
        Padding = new Padding(14, 0, 14, 0),
        Font = Font(10, FontStyle.Bold),
        Cursor = Cursors.Hand,
        FlatStyle = FlatStyle.Flat,
        TextAlign = ContentAlignment.MiddleCenter,
        UseVisualStyleBackColor = false
    };

    public static Button PrimaryButton(string text)
    {
        Button button = BaseButton(text);
        button.BackColor = Primary;
        button.ForeColor = OnPrimary;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = PrimaryDark;
        return button;
    }

    public static Button SecondaryButton(string text)
    {
        Button button = BaseButton(text);
        button.BackColor = Surface;
        button.ForeColor = TextSecondary;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Border;
        button.FlatAppearance.MouseOverBackColor = SurfaceAlt;
        return button;
    }

    public static Button DangerButton(string text)
    {
        Button button = BaseButton(text);
        button.BackColor = Danger;
        button.ForeColor = OnPrimary;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Danger;
        return button;
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.ReadOnly = true;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersHeight = 42;
        grid.RowTemplate.Height = 38;
        grid.GridColor = Border;
        grid.AutoGenerateColumns = false;

        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = GridHeader,
            ForeColor = TextPrimary,
            Font = Font(10, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            SelectionBackColor = GridHeader,
            SelectionForeColor = TextPrimary
        };

        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface,
            ForeColor = TextSecondary,
            Font = Font(9.5f),
            SelectionBackColor = Selection,
            SelectionForeColor = TextPrimary,
            Padding = new Padding(4)
        };

        grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceAlt;
    }

    public static DataGridViewTextBoxColumn TextColumn(
        string propertyName,
        string headerText,
        float fillWeight = 100,
        DataGridViewAutoSizeColumnMode autoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        DataGridViewContentAlignment alignment = DataGridViewContentAlignment.MiddleLeft)
    {
        var column = new DataGridViewTextBoxColumn
        {
            Name = propertyName,
            DataPropertyName = propertyName,
            HeaderText = headerText,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.Automatic,
            AutoSizeMode = autoSizeMode,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = alignment,
                NullValue = string.Empty
            }
        };

        if (autoSizeMode == DataGridViewAutoSizeColumnMode.Fill)
            column.FillWeight = Math.Max(1, fillWeight);

        return column;
    }

    internal static Color MapBackColor(Color color, Control control)
    {
        if (color.IsEmpty || color == Color.Transparent)
            return color;

        if (control is TextBoxBase or ComboBox or NumericUpDown or DateTimePicker or ListBox)
        {
            if (IsSurfaceFamily(color) || color == Color.White)
                return InputBackground;
        }

        if (color == Color.White || IsPaletteRole(color, p => p.Surface)) return Surface;
        if (IsPaletteRole(color, p => p.Background) || Same(color, "#F8FAFC")) return Background;
        if (IsPaletteRole(color, p => p.SurfaceAlt) || Same(color, "#F1F5F9")) return SurfaceAlt;
        if (IsPaletteRole(color, p => p.InputBackground)) return InputBackground;
        if (IsPaletteRole(color, p => p.Sidebar) || Same(color, "#0F172A")) return Sidebar;
        if (IsPaletteRole(color, p => p.SidebarHover) || Same(color, "#1E293B")) return _palette.SidebarHover;
        if (IsPaletteRole(color, p => p.Primary)) return Primary;
        if (IsPaletteRole(color, p => p.PrimaryDark) || Same(color, "#0F4C6E")) return PrimaryDark;
        if (IsPaletteRole(color, p => p.PrimarySoft) || Same(color, "#EFF6FF")) return PrimarySoft;
        if (IsPaletteRole(color, p => p.Selection) || Same(color, "#DBEAFE")) return Selection;
        if (IsPaletteRole(color, p => p.BorderAccent) || Same(color, "#BFDBFE")) return BorderAccent;
        if (IsPaletteRole(color, p => p.GridHeader)) return GridHeader;
        if (IsPaletteRole(color, p => p.Border) || Same(color, "#E2E8F0") || Same(color, "#E8EDF4")) return Border;
        if (IsPaletteRole(color, p => p.Success) || Same(color, "#0F9F78")) return Success;
        if (IsPaletteRole(color, p => p.Warning) || Same(color, "#F59E0B")) return Warning;
        if (IsPaletteRole(color, p => p.Danger)) return Danger;
        if (IsPaletteRole(color, p => p.Purple)) return Purple;
        return color;
    }

    internal static Color MapForeColor(Color color, Control control = null)
    {
        if (color.IsEmpty || color == Color.Transparent)
            return color;

        // 課程／科目的自訂色按鈕會自行選擇黑字或白字；不要把這種對比色誤當成系統文字色。
        if (control is Button && !IsKnownThemeBackColor(control.BackColor) &&
            (color == Color.Black || color == Color.White))
            return color;

        if (color == Color.Black) return TextPrimary;
        if (color == Color.White) return OnPrimary;
        if (IsPaletteRole(color, p => p.TextPrimary) || Same(color, "#0F172A")) return TextPrimary;
        if (IsPaletteRole(color, p => p.TextSecondary) || Same(color, "#334155")) return TextSecondary;
        if (IsPaletteRole(color, p => p.Muted) || Same(color, "#64748B")) return Muted;
        if (IsPaletteRole(color, p => p.SidebarText) || Same(color, "#CBD5E1")) return SidebarText;
        if (IsPaletteRole(color, p => p.SidebarAccentText) || Same(color, "#93C5FD")) return SidebarAccentText;
        if (IsPaletteRole(color, p => p.Primary)) return Primary;
        if (IsPaletteRole(color, p => p.PrimaryDark)) return PrimaryDark;
        if (IsPaletteRole(color, p => p.Success)) return Success;
        if (IsPaletteRole(color, p => p.Warning) || Same(color, "#F59E0B")) return Warning;
        if (IsPaletteRole(color, p => p.Danger)) return Danger;
        if (IsPaletteRole(color, p => p.Purple)) return Purple;
        return color;
    }

    private static bool IsKnownThemeBackColor(Color color) =>
        color == Color.White || IsPaletteRole(color, p => p.Surface) ||
        IsPaletteRole(color, p => p.Background) || IsPaletteRole(color, p => p.SurfaceAlt) ||
        IsPaletteRole(color, p => p.InputBackground) || IsPaletteRole(color, p => p.Sidebar) ||
        IsPaletteRole(color, p => p.SidebarHover) || IsPaletteRole(color, p => p.Primary) ||
        IsPaletteRole(color, p => p.PrimaryDark) || IsPaletteRole(color, p => p.PrimarySoft) ||
        IsPaletteRole(color, p => p.Selection) || IsPaletteRole(color, p => p.BorderAccent) ||
        IsPaletteRole(color, p => p.GridHeader) || IsPaletteRole(color, p => p.Border) ||
        IsPaletteRole(color, p => p.Success) || IsPaletteRole(color, p => p.Warning) ||
        IsPaletteRole(color, p => p.Danger) || IsPaletteRole(color, p => p.Purple);

    private static bool IsSurfaceFamily(Color color) =>
        IsPaletteRole(color, p => p.Surface) || IsPaletteRole(color, p => p.Background) ||
        IsPaletteRole(color, p => p.SurfaceAlt) || Same(color, "#F8FAFC") || Same(color, "#F1F5F9");

    private static bool IsPaletteRole(Color color, Func<ThemePalette, Color> selector) =>
        Palettes.Values.Any(palette => color.ToArgb() == selector(palette).ToArgb());

    private static bool Same(Color color, string html) => color.ToArgb() == Html(html).ToArgb();
    private static Color Html(string value) => ColorTranslator.FromHtml(value);
}

/// <summary>
/// 讓目前已開啟的頁面立即換色，也讓之後開啟的對話框自動套用同一風格。
/// 只處理顏色，不改變控制項大小、字型、位置或功能。
/// </summary>
public static class ThemeRuntime
{
    private sealed class ThemeStamp
    {
        public int Version;
        public bool Hooked;
    }

    private static readonly ConditionalWeakTable<Control, ThemeStamp> Stamps = new();
    private static bool _installed;

    public static void Install()
    {
        if (_installed)
            return;

        _installed = true;
        Application.Idle += (_, _) =>
        {
            foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
            {
                ThemeStamp stamp = Stamps.GetOrCreateValue(form);
                if (stamp.Version != UiTheme.Version)
                    ApplyToControlTree(form);
            }
        };
    }

    public static void ApplyToOpenWindows()
    {
        foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
            ApplyToControlTree(form);
    }

    public static void ApplyToControlTree(Control control)
    {
        if (control == null || control.IsDisposed)
            return;

        ThemeStamp stamp = Stamps.GetOrCreateValue(control);
        if (!stamp.Hooked)
        {
            control.ControlAdded += (_, eventArgs) => ApplyToControlTree(eventArgs.Control);
            stamp.Hooked = true;
        }

        if (control is not ThemePreviewPanel)
        {
            control.BackColor = UiTheme.MapBackColor(control.BackColor, control);
            control.ForeColor = UiTheme.MapForeColor(control.ForeColor, control);

            if (control is RoundedPanel rounded)
                rounded.BorderColor = UiTheme.MapBackColor(rounded.BorderColor, rounded);

            if (control is Button button)
            {
                button.UseVisualStyleBackColor = false;
                button.FlatAppearance.BorderColor = UiTheme.MapBackColor(button.FlatAppearance.BorderColor, button);
                button.FlatAppearance.MouseOverBackColor = UiTheme.MapBackColor(button.FlatAppearance.MouseOverBackColor, button);
                button.FlatAppearance.MouseDownBackColor = UiTheme.MapBackColor(button.FlatAppearance.MouseDownBackColor, button);
            }

            if (control is LinkLabel link)
            {
                link.LinkColor = UiTheme.MapForeColor(link.LinkColor);
                link.ActiveLinkColor = UiTheme.PrimaryDark;
                link.VisitedLinkColor = UiTheme.Purple;
            }

            if (control is DataGridView grid)
                ApplyGrid(grid);

            if (control is ToolStrip strip)
            {
                strip.BackColor = UiTheme.MapBackColor(strip.BackColor, strip);
                strip.ForeColor = UiTheme.MapForeColor(strip.ForeColor);
                foreach (ToolStripItem item in strip.Items)
                {
                    item.BackColor = UiTheme.MapBackColor(item.BackColor, strip);
                    item.ForeColor = UiTheme.MapForeColor(item.ForeColor);
                }
            }
        }

        foreach (Control child in control.Controls)
            ApplyToControlTree(child);

        stamp.Version = UiTheme.Version;
        control.Invalidate();
    }

    private static void ApplyGrid(DataGridView grid)
    {
        grid.BackgroundColor = UiTheme.MapBackColor(grid.BackgroundColor, grid);
        grid.GridColor = UiTheme.MapBackColor(grid.GridColor, grid);
        MapCellStyle(grid.ColumnHeadersDefaultCellStyle, grid, true);
        MapCellStyle(grid.RowHeadersDefaultCellStyle, grid, true);
        MapCellStyle(grid.DefaultCellStyle, grid, false);
        MapCellStyle(grid.AlternatingRowsDefaultCellStyle, grid, false);
        MapCellStyle(grid.RowsDefaultCellStyle, grid, false);

        foreach (DataGridViewColumn column in grid.Columns)
        {
            MapCellStyle(column.DefaultCellStyle, grid, false);
            MapCellStyle(column.HeaderCell.Style, grid, true);
        }
    }

    private static void MapCellStyle(DataGridViewCellStyle style, DataGridView grid, bool header)
    {
        if (style == null)
            return;

        style.BackColor = style.BackColor.IsEmpty
            ? (header ? UiTheme.GridHeader : UiTheme.Surface)
            : UiTheme.MapBackColor(style.BackColor, grid);
        style.ForeColor = style.ForeColor.IsEmpty
            ? UiTheme.TextPrimary
            : UiTheme.MapForeColor(style.ForeColor);
        style.SelectionBackColor = style.SelectionBackColor.IsEmpty
            ? (header ? style.BackColor : UiTheme.Selection)
            : UiTheme.MapBackColor(style.SelectionBackColor, grid);
        style.SelectionForeColor = style.SelectionForeColor.IsEmpty
            ? UiTheme.TextPrimary
            : UiTheme.MapForeColor(style.SelectionForeColor);
    }
}

/// <summary>主題預覽色塊不跟著目前主題改色，才能永遠顯示各套配色的真實樣貌。</summary>
public sealed class ThemePreviewPanel : Panel
{
}
