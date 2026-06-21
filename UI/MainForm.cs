using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class MainForm : Form
{
    private readonly DataService _service;
    private readonly Panel _contentHost = new();
    private readonly Dictionary<string, Button> _navButtons = new();
    private readonly List<Button> _allSidebarButtons = new();

    // 側邊欄使用明確的控制項參考重新套色，避免深色主題之間的相近色
    // 在切回 Facebook 時被誤判成 Surface，造成下半部變淺色或白色。
    private Panel _sidebarPanel = null!;
    private TableLayoutPanel _sidebarLayout = null!;
    private Panel _sidebarLogoPanel = null!;
    private FlowLayoutPanel _sidebarNavigation = null!;
    private Panel _sidebarBottomPanel = null!;
    private TableLayoutPanel _sidebarBottomLayout = null!;
    private Label _sidebarLogoMark = null!;
    private Label _sidebarLogoTitle = null!;
    private Label _sidebarLogoSub = null!;
    private string _activePageKey = "dashboard";

    private Panel _dashboardPage = null!;
    private Panel _tasksPage = null!;
    private Panel _focusPage = null!;
    private Panel _coursesPage = null!;
    private TimetableControl _timetablePage = null!;
    private ExamLibraryControl _examLibraryPage = null!;
    private ProfessorMailControl _professorMailPage = null!;
    private VisualStyleControl _visualStylePage = null!;

    private Label _greetingLabel = null!;
    private Label _openTaskLabel = null!;
    private Label _overdueLabel = null!;
    private Label _completionLabel = null!;
    private Label _todayFocusLabel = null!;
    private Label _recommendationLabel = null!;
    private Label _dailyPlanLabel = null!;

    private DataGridView _dashboardGrid = null!;
    private DataGridView _taskGrid = null!;
    private DataGridView _courseGrid = null!;
    private DataGridView _courseTaskGrid = null!;
    private DataGridView _sessionGrid = null!;
    private Label _courseTaskTitle = null!;
    private Label _courseTaskHint = null!;

    private TextBox _searchBox = null!;
    private ComboBox _taskFilter = null!;

    private ComboBox _focusTaskCombo = null!;
    private NumericUpDown _focusMinutes = null!;
    private Label _timerLabel = null!;
    private Label _focusStateLabel = null!;
    private ProgressBar _focusProgress = null!;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
    private readonly System.Windows.Forms.Timer _identityTimer = new() { Interval = 60_000 };
    private int _totalSeconds;
    private int _remainingSeconds;
    private bool _isTimerRunning;
    private Guid? _activeTaskId;

    private ToolStripStatusLabel _statusLabel = null!;

    public bool LogoutRequested { get; private set; }

    public MainForm(DataService service)
    {
        _service = service;

        // 主視窗建立前再次以目前帳號的獨立偏好為準，避免上一位使用者的全域主題殘留。
        UiTheme.ApplyVisualStyle(_service.Data.Settings.VisualStyle, false);

        Text = $"StudyFlow Pro Research Edition｜{_service.Data.Settings.UserName} 的個人學習空間";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1180, 760);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        KeyPreview = true;
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;

        BuildInterface();
        WireEvents();
        RefreshAll();
        ShowPage("dashboard");
        Shown += (_, _) => ShowDueSoonReminder();
    }

    private void BuildInterface()
    {
        var statusStrip = new StatusStrip
        {
            Dock = DockStyle.Bottom,
            SizingGrip = false,
            BackColor = UiTheme.Surface,
            Font = UiTheme.Font(8.5f)
        };
        _statusLabel = new ToolStripStatusLabel("系統就緒");
        statusStrip.Items.Add(_statusLabel);

        var mainArea = new Panel { Dock = DockStyle.Fill };
        Panel sidebar = BuildSidebar();

        _contentHost.Dock = DockStyle.Fill;
        _contentHost.BackColor = UiTheme.Background;

        _dashboardPage = BuildDashboardPage();
        _tasksPage = BuildTasksPage();
        _focusPage = BuildFocusPage();
        _coursesPage = BuildCoursesPage();
        _timetablePage = new TimetableControl(_service);
        _examLibraryPage = new ExamLibraryControl(_service);
        _professorMailPage = new ProfessorMailControl(_service);
        _visualStylePage = new VisualStyleControl(_service, OnVisualStyleApplied);

        _contentHost.Controls.Add(_dashboardPage);
        _contentHost.Controls.Add(_tasksPage);
        _contentHost.Controls.Add(_focusPage);
        _contentHost.Controls.Add(_coursesPage);
        _contentHost.Controls.Add(_timetablePage);
        _contentHost.Controls.Add(_examLibraryPage);
        _contentHost.Controls.Add(_professorMailPage);
        _contentHost.Controls.Add(_visualStylePage);

        mainArea.Controls.Add(_contentHost);
        mainArea.Controls.Add(sidebar);

        Controls.Add(mainArea);
        Controls.Add(statusStrip);
    }

    private Panel BuildSidebar()
    {
        _sidebarPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 224,
            BackColor = UiTheme.Sidebar,
            Padding = new Padding(14, 18, 14, 14)
        };

        // 使用固定三列配置，避免 Dock.Fill 與 Dock.Top 的加入順序
        // 造成最上方的「主控台、任務管理」被 Logo 區塊覆蓋。
        _sidebarLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Sidebar,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        _sidebarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        _sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));

        _sidebarLogoPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Sidebar,
            Margin = Padding.Empty
        };
        _sidebarLogoMark = new Label
        {
            Text = "SF",
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(48, 48),
            Location = new Point(4, 8),
            BackColor = UiTheme.Primary,
            ForeColor = Color.White,
            Font = UiTheme.Font(15, FontStyle.Bold)
        };
        _sidebarLogoTitle = new Label
        {
            Text = "StudyFlow",
            AutoSize = true,
            Location = new Point(62, 10),
            ForeColor = Color.White,
            Font = UiTheme.Font(14, FontStyle.Bold)
        };
        _sidebarLogoSub = new Label
        {
            Text = $"PRO • {GetCurrentDisplayName()}",
            AutoSize = false,
            AutoEllipsis = true,
            Size = new Size(140, 24),
            Location = new Point(64, 38),
            ForeColor = UiTheme.SidebarAccentText,
            Font = UiTheme.Font(9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _sidebarLogoPanel.Controls.Add(_sidebarLogoMark);
        _sidebarLogoPanel.Controls.Add(_sidebarLogoTitle);
        _sidebarLogoPanel.Controls.Add(_sidebarLogoSub);

        _sidebarNavigation = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = UiTheme.Sidebar,
            Padding = new Padding(0, 4, 0, 0),
            Margin = Padding.Empty
        };

        AddNavigationButton(_sidebarNavigation, "dashboard", "▦  主控台");
        AddNavigationButton(_sidebarNavigation, "courses", "▤  課程 / 專案");
        AddNavigationButton(_sidebarNavigation, "tasks", "✓  任務管理");
        AddNavigationButton(_sidebarNavigation, "focus", "◷  專注計時");
        AddNavigationButton(_sidebarNavigation, "timetable", "▦  課表");
        AddNavigationButton(_sidebarNavigation, "exams", "▣  考古題庫");
        AddNavigationButton(_sidebarNavigation, "professors", "✉  寄信詢問教授");
        AddNavigationButton(_sidebarNavigation, "styles", "◐  視覺風格");

        Button scheduleButton = CreateNavigationButton("◇  智慧排程");
        scheduleButton.Click += (_, _) => OpenSmartSchedule();
        _allSidebarButtons.Add(scheduleButton);
        _sidebarNavigation.Controls.Add(scheduleButton);

        Button analyticsButton = CreateNavigationButton("▥  分析中心");
        analyticsButton.Click += (_, _) =>
        {
            using var form = new AnalyticsForm(_service);
            form.ShowDialog(this);
        };
        _allSidebarButtons.Add(analyticsButton);
        _sidebarNavigation.Controls.Add(analyticsButton);

        Button researchButton = CreateNavigationButton("◈  Research Center");
        researchButton.Click += (_, _) =>
        {
            using var form = new ResearchCenterForm(_service);
            form.ShowDialog(this);
            RefreshAll();
        };
        _allSidebarButtons.Add(researchButton);
        _sidebarNavigation.Controls.Add(researchButton);

        Button settingsButton = CreateNavigationButton("⚙  設定與備份");
        settingsButton.Click += (_, _) =>
        {
            using var form = new SettingsForm(_service);
            form.ShowDialog(this);
            RefreshAll();
        };
        _allSidebarButtons.Add(settingsButton);
        _sidebarNavigation.Controls.Add(settingsButton);

        _sidebarBottomPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Sidebar,
            Padding = new Padding(0, 4, 0, 0),
            Margin = Padding.Empty
        };
        _sidebarBottomLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Sidebar
        };
        _sidebarBottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _sidebarBottomLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        _sidebarBottomLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        Button helpButton = CreateNavigationButton("?  使用說明");
        helpButton.Dock = DockStyle.Fill;
        helpButton.Margin = new Padding(0, 1, 0, 1);
        helpButton.Click += (_, _) =>
        {
            using var form = new HelpForm(_service);
            form.ShowDialog(this);
        };
        _allSidebarButtons.Add(helpButton);

        Button logoutButton = CreateNavigationButton("↪  登出帳號");
        logoutButton.Dock = DockStyle.Fill;
        logoutButton.Margin = new Padding(0, 1, 0, 1);
        logoutButton.Click += (_, _) => RequestLogout();
        _allSidebarButtons.Add(logoutButton);

        _sidebarBottomLayout.Controls.Add(helpButton, 0, 0);
        _sidebarBottomLayout.Controls.Add(logoutButton, 0, 1);
        _sidebarBottomPanel.Controls.Add(_sidebarBottomLayout);

        _sidebarLayout.Controls.Add(_sidebarLogoPanel, 0, 0);
        _sidebarLayout.Controls.Add(_sidebarNavigation, 0, 1);
        _sidebarLayout.Controls.Add(_sidebarBottomPanel, 0, 2);
        _sidebarPanel.Controls.Add(_sidebarLayout);
        ApplySidebarTheme();
        return _sidebarPanel;
    }

    private void AddNavigationButton(FlowLayoutPanel panel, string key, string text)
    {
        Button button = CreateNavigationButton(text);
        button.Click += (_, _) => ShowPage(key);
        _navButtons[key] = button;
        _allSidebarButtons.Add(button);
        panel.Controls.Add(button);
    }

    private static Button CreateNavigationButton(string text)
    {
        // 導覽列不再直接把圖示字元和文字放在同一個 Text 裡。
        // 不同 Unicode 圖示（例如信封、齒輪）的字寬不一致，會造成文字起點左右飄移。
        // 改由 SidebarNavigationButton 使用固定圖示欄與固定文字起點繪製，
        // 讓全部 14 個按鈕在任何 DPI / 顯示縮放下都整排對齊。
        int separatorIndex = text.IndexOf("  ", StringComparison.Ordinal);
        string icon = separatorIndex >= 0 ? text[..separatorIndex].Trim() : string.Empty;
        string label = separatorIndex >= 0 ? text[(separatorIndex + 2)..].Trim() : text.Trim();

        var button = new SidebarNavigationButton(icon, label)
        {
            Width = 192,
            Height = 40,
            Margin = new Padding(0, 1, 0, 1),
            Font = UiTheme.Font(10, FontStyle.Bold),
            ForeColor = UiTheme.SidebarText,
            BackColor = UiTheme.Sidebar,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            AccessibleName = label
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = UiTheme.Palette.SidebarHover;
        return button;
    }

    private Panel BuildDashboardPage()
    {
        Panel page = CreateBasePage();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(28, 20, 28, 24)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 188));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        TableLayoutPanel header = UiTheme.StackedHeader(
            "早安",
            "以下是你今天的學習總覽與系統建議",
            out _greetingLabel,
            25);

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1
        };
        for (int i = 0; i < 4; i++)
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        var firstMetric = CreateMetricCard("未完成任務", UiTheme.Primary);
        Panel first = firstMetric.Card;
        _openTaskLabel = firstMetric.ValueLabel;

        var secondMetric = CreateMetricCard("逾期任務", UiTheme.Danger);
        Panel second = secondMetric.Card;
        _overdueLabel = secondMetric.ValueLabel;

        var thirdMetric = CreateMetricCard("任務完成率", UiTheme.Success);
        Panel third = thirdMetric.Card;
        _completionLabel = thirdMetric.ValueLabel;

        var fourthMetric = CreateMetricCard("今日專注", UiTheme.Purple);
        Panel fourth = fourthMetric.Card;
        _todayFocusLabel = fourthMetric.ValueLabel;

        metrics.Controls.Add(WrapWithPadding(first, new Padding(0, 0, 8, 0)), 0, 0);
        metrics.Controls.Add(WrapWithPadding(second, new Padding(8, 0, 8, 0)), 1, 0);
        metrics.Controls.Add(WrapWithPadding(third, new Padding(8, 0, 8, 0)), 2, 0);
        metrics.Controls.Add(WrapWithPadding(fourth, new Padding(8, 0, 0, 0)), 3, 0);

        var recommendationCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderColor = UiTheme.BorderAccent,
            Padding = new Padding(18)
        };
        var recommendationTitle = new Label
        {
            Text = "SMART PRIORITY｜智慧建議",
            Dock = DockStyle.Top,
            Height = 24,
            ForeColor = UiTheme.Primary,
            Font = UiTheme.Font(9, FontStyle.Bold)
        };
        _recommendationLabel = new Label
        {
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(11, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        recommendationCard.Controls.Add(_recommendationLabel);
        recommendationCard.Controls.Add(recommendationTitle);

        var dashboardInsightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        dashboardInsightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        dashboardInsightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        dashboardInsightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        dashboardInsightPanel.Controls.Add(WrapWithPadding(CreateScoreLegendCard(), new Padding(0, 0, 8, 0)), 0, 0);
        dashboardInsightPanel.Controls.Add(WrapWithPadding(CreateDailyPlanCard(), new Padding(8, 0, 0, 0)), 1, 0);

        var taskCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
            BackColor = UiTheme.Surface
        };
        var tableTitle = new Label
        {
            Text = "下一步任務",
            Dock = DockStyle.Top,
            Height = 54,
            Padding = new Padding(18, 14, 0, 0),
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(13, FontStyle.Bold)
        };
        _dashboardGrid = new DataGridView { Dock = DockStyle.Fill };
        UiTheme.StyleGrid(_dashboardGrid);
        _dashboardGrid.CellFormatting += FormatTaskGridCell;
        _dashboardGrid.CellDoubleClick += (_, _) => EditSelectedDashboardTask();
        taskCard.Controls.Add(_dashboardGrid);
        taskCard.Controls.Add(tableTitle);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(metrics, 0, 1);
        layout.Controls.Add(recommendationCard, 0, 2);
        layout.Controls.Add(dashboardInsightPanel, 0, 3);
        layout.Controls.Add(taskCard, 0, 4);
        page.Controls.Add(layout);
        return page;
    }


    private RoundedPanel CreateScoreLegendCard()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderColor = UiTheme.BorderAccent,
            Padding = new Padding(16)
        };

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        content.Controls.Add(new Label
        {
            Text = "智慧分數是什麼？",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 0);

        content.Controls.Add(new Label
        {
            Text = "以 0～100 表示任務目前的處理急迫度；分數越高，越應優先執行。",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(8.8f),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Margin = Padding.Empty
        }, 0, 1);

        content.Controls.Add(new Label
        {
            Text = "公式：[(優先級＋截止急迫度＋工作量＋難度＋精力＋釘選＋未更新)" +
                   Environment.NewLine +
                   "－進度抵銷] ÷ 149 × 100",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.PrimaryDark,
            Font = UiTheme.Font(8.4f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = false,
            AutoSize = false,
            Margin = Padding.Empty
        }, 0, 2);

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(0, 4, 0, 0),
            BackColor = UiTheme.Surface
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        grid.Controls.Add(CreateScoreBadge("80–100", "立即處理", UiTheme.Danger), 0, 0);
        grid.Controls.Add(CreateScoreBadge("60–79", "高度優先", UiTheme.Warning), 1, 0);
        grid.Controls.Add(CreateScoreBadge("40–59", "中度優先", UiTheme.Primary), 0, 1);
        grid.Controls.Add(CreateScoreBadge("0–39", "可排程", UiTheme.Success), 1, 1);
        content.Controls.Add(grid, 0, 3);

        card.Controls.Add(content);
        return card;
    }

    private Panel CreateScoreBadge(string range, string label, Color color)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 2, 8, 2),
            BackColor = UiTheme.Background
        };

        var bar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 5,
            BackColor = color
        };
        var text = new Label
        {
            Text = $"{range}  {label}",
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(8.8f, FontStyle.Bold)
        };
        panel.Controls.Add(text);
        panel.Controls.Add(bar);
        return panel;
    }

    private RoundedPanel CreateDailyPlanCard()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderColor = UiTheme.BorderAccent,
            Padding = new Padding(16)
        };

        var top = new Panel { Dock = DockStyle.Top, Height = 28 };
        var title = new Label
        {
            Text = "今日作戰計畫",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        var button = UiTheme.SecondaryButton("產生完整排程");
        button.Dock = DockStyle.Right;
        button.Width = 138;
        button.Click += (_, _) => OpenSmartSchedule();
        top.Controls.Add(title);
        top.Controls.Add(button);

        _dailyPlanLabel = new Label
        {
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9.2f),
            Padding = new Padding(0, 7, 0, 0),
            TextAlign = ContentAlignment.TopLeft
        };

        card.Controls.Add(_dailyPlanLabel);
        card.Controls.Add(top);
        return card;
    }

    private string BuildDailyPlanSummary(AppData data)
    {
        SmartScheduleState? schedule = data.SmartSchedule;
        if (schedule?.GeneratedAt?.Date == DateTime.Today)
        {
            string header = $"最後更新 {schedule.GeneratedAt:HH:mm}｜可用 {schedule.AvailableMinutes} 分｜" +
                $"專注 {schedule.FocusMinutes} 分｜休息 {schedule.BreakMinutesUsed} 分｜" +
                $"預計 {(schedule.ExpectedEnd?.ToString("HH:mm") ?? "--:--")} 結束";

            if (schedule.Blocks.Count == 0)
                return header + "\n目前沒有可排程的未完成任務。";

            string items = string.Join("   |   ", schedule.Blocks.Take(3).Select((block, index) =>
                $"{index + 1}. {block.Start:HH:mm}–{block.End:HH:mm} {block.TaskTitle}"));
            if (schedule.Blocks.Count > 3)
                items += $"   |   另有 {schedule.Blocks.Count - 3} 個區塊";

            return header + "\n" + items;
        }

        List<StudyTask> tasks = SmartPlanner.RankTasks(data.Tasks).Take(3).ToList();
        if (tasks.Count == 0)
            return "目前沒有未完成任務。可新增下一個學習目標，系統會自動排序。";

        return string.Join("   |   ", tasks.Select((task, index) =>
        {
            PriorityAnalysis analysis = SmartPlanner.Analyze(task);
            return $"{index + 1}. {task.Title}（{analysis.Level}，{analysis.TotalScore}/100）";
        })) + "\n尚未產生今日完整排程；可按右側按鈕設定可用時間與休息分鐘。";
    }

    private Button CreateMinuteShortcutButton(string text, int minutes)
    {
        Button button = UiTheme.SecondaryButton(text);
        button.Margin = new Padding(8, 4, 0, 0);
        button.Click += (_, _) =>
        {
            _focusMinutes.Value = Math.Clamp(minutes, (int)_focusMinutes.Minimum, (int)_focusMinutes.Maximum);
            if (!_isTimerRunning)
                ResetTimer(false);
        };
        return button;
    }

    private void OpenSmartSchedule()
    {
        using var form = new SmartScheduleForm(_service);
        form.ShowDialog(this);
        RefreshAll();
    }

    private Panel BuildTasksPage()
    {
        Panel page = CreateBasePage();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(28, 20, 28, 24)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(CreatePageHeader("任務管理", "搜尋、篩選、排序、編輯、完成，以及 CSV 匯入與匯出"), 0, 0);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoScroll = false,
            Padding = new Padding(0, 7, 0, 7)
        };
        Button addButton = UiTheme.PrimaryButton("＋ 新增任務");
        Button editButton = UiTheme.SecondaryButton("編輯");
        Button completeButton = UiTheme.SecondaryButton("完成 / 取消完成");
        Button pinButton = UiTheme.SecondaryButton("釘選 / 取消");
        Button insightButton = UiTheme.SecondaryButton("優先原因");
        Button scheduleButton = UiTheme.SecondaryButton("智慧排程");
        Button deleteButton = UiTheme.DangerButton("刪除");
        Button importButton = UiTheme.SecondaryButton("匯入任務 CSV");
        Button exportButton = UiTheme.SecondaryButton("匯出任務 CSV");

        _searchBox = new TextBox
        {
            Width = 220,
            Height = 38,
            Font = UiTheme.Font(10),
            PlaceholderText = "搜尋名稱、標籤或說明…",
            Margin = new Padding(12, 5, 4, 5)
        };
        _taskFilter = new ComboBox
        {
            Width = 140,
            Height = 38,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = UiTheme.Font(10),
            Margin = new Padding(4, 5, 4, 5)
        };
        _taskFilter.Items.AddRange(new object[]
        {
            "全部", "未完成", "已完成", "已逾期", "今日到期", "高優先", "已釘選"
        });
        _taskFilter.SelectedIndex = 0;

        addButton.Click += (_, _) => AddTask();
        editButton.Click += (_, _) => EditSelectedTask();
        completeButton.Click += (_, _) => ToggleSelectedTask();
        pinButton.Click += (_, _) => ToggleSelectedPin();
        insightButton.Click += (_, _) => ShowSelectedTaskInsight();
        scheduleButton.Click += (_, _) => OpenSmartSchedule();
        deleteButton.Click += (_, _) => DeleteSelectedTask();
        importButton.Click += (_, _) => ImportTasks();
        exportButton.Click += (_, _) => ExportTasks();

        toolbar.Controls.Add(addButton);
        toolbar.Controls.Add(editButton);
        toolbar.Controls.Add(completeButton);
        toolbar.Controls.Add(pinButton);
        toolbar.Controls.Add(insightButton);
        toolbar.Controls.Add(scheduleButton);
        toolbar.Controls.Add(deleteButton);
        toolbar.Controls.Add(_searchBox);
        toolbar.Controls.Add(_taskFilter);
        toolbar.Controls.Add(importButton);
        toolbar.Controls.Add(exportButton);

        // 表格使用方形 1px 邊框容器，避免 RoundedPanel 的 Region
        // 在左上角與右上角裁切 DataGridView 標題列邊框。
        var gridCard = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
            BackColor = UiTheme.Border,
            Margin = Padding.Empty
        };
        _taskGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };
        UiTheme.StyleGrid(_taskGrid);
        _taskGrid.CellFormatting += FormatTaskGridCell;
        _taskGrid.CellDoubleClick += (_, _) => EditSelectedTask();
        _taskGrid.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                EditSelectedTask();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                DeleteSelectedTask();
            }
        };
        gridCard.Controls.Add(_taskGrid);

        layout.Controls.Add(toolbar, 0, 1);
        layout.Controls.Add(gridCard, 0, 2);
        page.Controls.Add(layout);
        return page;
    }

    private Panel BuildFocusPage()
    {
        Panel page = CreateBasePage();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(28, 20, 28, 24)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 265));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(CreatePageHeader("專注計時", "將每次學習時間寫入紀錄，並回饋到任務進度"), 0, 0);

        var selectorCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(16)
        };
        var selectorFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        selectorFlow.Controls.Add(CreateInlineLabel("綁定任務"));
        _focusTaskCombo = new ComboBox
        {
            Width = 390,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = UiTheme.Font(10),
            Margin = new Padding(0, 4, 18, 0)
        };
        selectorFlow.Controls.Add(_focusTaskCombo);
        selectorFlow.Controls.Add(CreateInlineLabel("分鐘"));
        _focusMinutes = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 180,
            Value = 25,
            Width = 80,
            Font = UiTheme.Font(10),
            Margin = new Padding(0, 4, 0, 0)
        };
        _focusMinutes.ValueChanged += (_, _) =>
        {
            if (!_isTimerRunning)
                ResetTimer(false);
        };
        selectorFlow.Controls.Add(_focusMinutes);
        selectorFlow.Controls.Add(CreateMinuteShortcutButton("25 分", 25));
        selectorFlow.Controls.Add(CreateMinuteShortcutButton("50 分", 50));
        selectorFlow.Controls.Add(CreateMinuteShortcutButton("90 分", 90));
        selectorCard.Controls.Add(selectorFlow);

        var timerCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Sidebar,
            BorderColor = UiTheme.Sidebar,
            Padding = new Padding(24)
        };
        var timerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        timerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        timerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        timerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        timerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        _focusStateLabel = new Label
        {
            Text = "準備開始",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiTheme.SidebarAccentText,
            Font = UiTheme.Font(11, FontStyle.Bold)
        };
        _timerLabel = new Label
        {
            Text = "25:00",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            Font = UiTheme.Font(46, FontStyle.Bold)
        };
        _focusProgress = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Style = ProgressBarStyle.Continuous
        };
        var timerButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        Button startButton = UiTheme.PrimaryButton("開始 / 繼續");
        Button pauseButton = UiTheme.SecondaryButton("暫停");
        Button finishButton = UiTheme.SecondaryButton("完成並記錄");
        Button resetButton = UiTheme.DangerButton("重設");
        startButton.Click += (_, _) => StartTimer();
        pauseButton.Click += (_, _) => PauseTimer();
        finishButton.Click += (_, _) => CompleteFocusSession(false);
        resetButton.Click += (_, _) => ResetTimer(true);
        timerButtons.Controls.Add(startButton);
        timerButtons.Controls.Add(pauseButton);
        timerButtons.Controls.Add(finishButton);
        timerButtons.Controls.Add(resetButton);

        timerLayout.Controls.Add(_focusStateLabel, 0, 0);
        timerLayout.Controls.Add(_timerLabel, 0, 1);
        timerLayout.Controls.Add(_focusProgress, 0, 2);
        timerLayout.Controls.Add(timerButtons, 0, 3);
        timerCard.Controls.Add(timerLayout);

        var sessionCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
            BackColor = UiTheme.Surface
        };
        var sessionHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(16, 8, 12, 8)
        };
        var sessionTitle = new Label
        {
            Text = "最近專注紀錄",
            Dock = DockStyle.Left,
            Width = 180,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(12, FontStyle.Bold)
        };
        var sessionActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 310,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        Button importSessionsButton = UiTheme.SecondaryButton("匯入紀錄 CSV");
        Button exportSessionsButton = UiTheme.SecondaryButton("匯出紀錄 CSV");
        importSessionsButton.AutoSize = false;
        exportSessionsButton.AutoSize = false;
        importSessionsButton.Size = new Size(142, 36);
        exportSessionsButton.Size = new Size(142, 36);
        importSessionsButton.Margin = new Padding(0, 0, 8, 0);
        exportSessionsButton.Margin = Padding.Empty;
        importSessionsButton.Click += (_, _) => ImportSessions();
        exportSessionsButton.Click += (_, _) => ExportSessions();
        sessionActions.Controls.Add(importSessionsButton);
        sessionActions.Controls.Add(exportSessionsButton);
        sessionHeader.Controls.Add(sessionActions);
        sessionHeader.Controls.Add(sessionTitle);

        _sessionGrid = new DataGridView { Dock = DockStyle.Fill };
        UiTheme.StyleGrid(_sessionGrid);
        sessionCard.Controls.Add(_sessionGrid);
        sessionCard.Controls.Add(sessionHeader);

        layout.Controls.Add(selectorCard, 0, 1);
        layout.Controls.Add(timerCard, 0, 2);
        layout.Controls.Add(sessionCard, 0, 3);
        page.Controls.Add(layout);
        return page;
    }

    private Panel BuildCoursesPage()
    {
        Panel page = CreateBasePage();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(28, 20, 28, 24)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.Controls.Add(CreatePageHeader("課程 / 專案", "將任務分群；點選任一課程，即可在下方查看歸類於該課程的任務"), 0, 0);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 6)
        };
        Button addButton = UiTheme.PrimaryButton("＋ 新增課程");
        Button editButton = UiTheme.SecondaryButton("編輯");
        Button deleteButton = UiTheme.DangerButton("刪除");
        addButton.Click += (_, _) => AddCourse();
        editButton.Click += (_, _) => EditSelectedCourse();
        deleteButton.Click += (_, _) => DeleteSelectedCourse();
        toolbar.Controls.Add(addButton);
        toolbar.Controls.Add(editButton);
        toolbar.Controls.Add(deleteButton);

        // 上半部顯示課程／專案清單。
        var gridCard = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
            BackColor = UiTheme.Border,
            Margin = new Padding(0, 0, 0, 10)
        };
        _courseGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };
        UiTheme.StyleGrid(_courseGrid);
        _courseGrid.CellFormatting += FormatCourseGridCell;
        _courseGrid.SelectionChanged += (_, _) => RefreshSelectedCourseTasks();
        _courseGrid.CellDoubleClick += (_, _) => EditSelectedCourse();
        gridCard.Controls.Add(_courseGrid);

        // 下半部會跟著上方選取的課程，列出該課程底下的所有任務。
        Control courseTasksCard = BuildCourseTaskListCard();

        layout.Controls.Add(toolbar, 0, 1);
        layout.Controls.Add(gridCard, 0, 2);
        layout.Controls.Add(courseTasksCard, 0, 3);
        page.Controls.Add(layout);
        return page;
    }

    private Control BuildCourseTaskListCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
            BackColor = UiTheme.Border,
            Margin = Padding.Empty
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Surface,
            Padding = new Padding(14, 5, 14, 5),
            Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        _courseTaskTitle = new Label
        {
            Text = "請先選取一個課程 / 專案",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(12, FontStyle.Bold),
            Margin = Padding.Empty
        };
        _courseTaskHint = new Label
        {
            Text = "點選上方課程後，這裡會列出所屬任務",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.8f),
            Margin = Padding.Empty
        };
        header.Controls.Add(_courseTaskTitle, 0, 0);
        header.Controls.Add(_courseTaskHint, 1, 0);

        _courseTaskGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };
        UiTheme.StyleGrid(_courseTaskGrid);
        _courseTaskGrid.CellFormatting += FormatTaskGridCell;
        _courseTaskGrid.CellDoubleClick += (_, _) =>
        {
            Guid? taskId = GetSelectedTaskId(_courseTaskGrid);
            if (taskId != null)
                EditTask(taskId.Value);
        };
        EnsureCourseTaskGridColumns();

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_courseTaskGrid, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private void WireEvents()
    {
        _service.DataChanged += (_, _) => RefreshAll();
        _searchBox.TextChanged += (_, _) => RefreshTaskGrid();
        _taskFilter.SelectedIndexChanged += (_, _) => RefreshTaskGrid();

        _timer.Tick += (_, _) =>
        {
            if (!_isTimerRunning)
                return;

            _remainingSeconds = Math.Max(0, _remainingSeconds - 1);
            UpdateTimerDisplay();

            if (_remainingSeconds == 0)
                CompleteFocusSession(true);
        };

        // 每分鐘重新判斷早安／午安／晚安，即使程式長時間保持開啟，
        // 跨過中午或傍晚後也會自動更新，不必重開程式。
        _identityTimer.Tick += (_, _) => RefreshIdentityPresentation();
        _identityTimer.Start();

        FormClosing += (_, _) =>
        {
            _timer.Stop();
            _identityTimer.Stop();
            try
            {
                _service.Save();
            }
            catch
            {
                // 關閉階段不再顯示阻斷式錯誤。
            }
        };

        KeyDown += (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                e.SuppressKeyPress = true;
                AddTask();
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                e.SuppressKeyPress = true;
                ShowPage("tasks");
                _searchBox.Focus();
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                e.SuppressKeyPress = true;
                EditSelectedTask();
            }
            else if (e.Control && e.KeyCode == Keys.I)
            {
                e.SuppressKeyPress = true;
                ShowPage("tasks");
                ShowSelectedTaskInsight();
            }
            else if (e.Control && e.KeyCode == Keys.R)
            {
                e.SuppressKeyPress = true;
                using var form = new ResearchCenterForm(_service);
                form.ShowDialog(this);
                RefreshAll();
            }
            else if (e.Control && e.KeyCode == Keys.M)
            {
                e.SuppressKeyPress = true;
                using var form = new SmartMatrixForm(_service);
                form.ShowDialog(this);
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                e.SuppressKeyPress = true;
                OpenSmartSchedule();
            }
            else if (e.Control && e.KeyCode == Keys.T)
            {
                e.SuppressKeyPress = true;
                ShowPage("timetable");
            }
            else if (e.Control && e.KeyCode == Keys.L)
            {
                e.SuppressKeyPress = true;
                ShowPage("exams");
            }
            else if (e.Control && e.KeyCode == Keys.G)
            {
                e.SuppressKeyPress = true;
                ShowPage("professors");
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.V)
            {
                e.SuppressKeyPress = true;
                ShowPage("styles");
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                _service.SaveAndNotify();
                SetStatus("已手動儲存");
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                RefreshAll();
                SetStatus("畫面已重新整理");
            }
        };
    }

    private void RequestLogout()
    {
        if (!ConfirmDialog.Ask(
                this,
                "登出帳號",
                $"確定要登出「{_service.Data.Settings.UserName}」嗎？\n\n目前資料會先自動儲存，接著返回登入畫面。",
                "確認登出"))
            return;

        try
        {
            _service.Save();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                "登出前無法完成資料儲存：" + exception.Message,
                "無法登出",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        LogoutRequested = true;
        Close();
    }

    private void ApplySidebarTheme()
    {
        if (_sidebarPanel == null || _sidebarPanel.IsDisposed)
            return;

        // 容器一律使用目前風格的 Sidebar 色，不再依靠舊顏色推測語意角色。
        _sidebarPanel.BackColor = UiTheme.Sidebar;
        _sidebarLayout.BackColor = UiTheme.Sidebar;
        _sidebarLogoPanel.BackColor = UiTheme.Sidebar;
        _sidebarNavigation.BackColor = UiTheme.Sidebar;
        _sidebarBottomPanel.BackColor = UiTheme.Sidebar;
        _sidebarBottomLayout.BackColor = UiTheme.Sidebar;

        _sidebarLogoMark.BackColor = UiTheme.Primary;
        _sidebarLogoMark.ForeColor = UiTheme.OnPrimary;
        _sidebarLogoTitle.BackColor = UiTheme.Sidebar;
        _sidebarLogoTitle.ForeColor = UiTheme.OnPrimary;
        _sidebarLogoSub.BackColor = UiTheme.Sidebar;
        _sidebarLogoSub.ForeColor = UiTheme.SidebarAccentText;

        foreach (Button button in _allSidebarButtons)
        {
            button.UseVisualStyleBackColor = false;
            button.BackColor = UiTheme.Sidebar;
            button.ForeColor = UiTheme.SidebarText;
            button.FlatAppearance.BorderColor = UiTheme.Sidebar;
            button.FlatAppearance.MouseOverBackColor = UiTheme.Palette.SidebarHover;
            button.FlatAppearance.MouseDownBackColor = UiTheme.PrimaryDark;
        }

        if (_navButtons.TryGetValue(_activePageKey, out Button? activeButton))
        {
            activeButton.BackColor = UiTheme.PrimaryDark;
            activeButton.ForeColor = UiTheme.OnPrimary;
            activeButton.FlatAppearance.BorderColor = UiTheme.PrimaryDark;
        }

        _sidebarPanel.Invalidate(true);
    }

    private void ShowPage(string key)
    {
        _dashboardPage.Visible = key == "dashboard";
        _tasksPage.Visible = key == "tasks";
        _focusPage.Visible = key == "focus";
        _coursesPage.Visible = key == "courses";
        _timetablePage.Visible = key == "timetable";
        _examLibraryPage.Visible = key == "exams";
        _professorMailPage.Visible = key == "professors";
        _visualStylePage.Visible = key == "styles";

        _activePageKey = key;
        ApplySidebarTheme();

        Control selectedPage = key switch
        {
            "tasks" => _tasksPage,
            "focus" => _focusPage,
            "courses" => _coursesPage,
            "timetable" => _timetablePage,
            "exams" => _examLibraryPage,
            "professors" => _professorMailPage,
            "styles" => _visualStylePage,
            _ => _dashboardPage
        };
        selectedPage.BringToFront();
        selectedPage.Visible = true;
    }

    private void OnVisualStyleApplied(VisualStyleKind style)
    {
        BackColor = UiTheme.Background;
        _contentHost.BackColor = UiTheme.Background;
        _activePageKey = "styles";
        ApplySidebarTheme();
        ShowPage("styles");
        SetStatus($"已套用 {UiTheme.GetDisplayName(style)}，並自動儲存");
    }

    private void RefreshAll()
    {
        if (_service.Data.Settings.VisualStyle != UiTheme.CurrentStyle)
            UiTheme.ApplyVisualStyle(_service.Data.Settings.VisualStyle);

        ApplySidebarTheme();
        _visualStylePage?.RefreshStyleState();
        RefreshDashboard();
        RefreshTaskGrid();
        RefreshCourseGrid();
        RefreshFocusTaskChoices();
        RefreshSessionGrid();
        _timetablePage?.RefreshTimetable();
        _examLibraryPage?.RefreshLibrary();
        _professorMailPage?.RefreshDirectory();

        if (!_isTimerRunning && _remainingSeconds == 0)
            ResetTimer(false);

        SetStatus("資料已同步並自動儲存");
    }

    private void RefreshDashboard()
    {
        AppData data = _service.Data;
        RefreshIdentityPresentation();
        _openTaskLabel.Text = data.Tasks.Count(task => !task.IsCompleted).ToString();
        _overdueLabel.Text = data.Tasks.Count(task => !task.IsCompleted && task.DueDate < DateTime.Now).ToString();
        _completionLabel.Text = SmartPlanner.CompletionRate(data) + "%";
        _todayFocusLabel.Text = SmartPlanner.TodayFocusMinutes(data) + " 分";
        _recommendationLabel.Text = SmartPlanner.Recommendation(data);
        if (_dailyPlanLabel != null)
            _dailyPlanLabel.Text = BuildDailyPlanSummary(data);

        List<TaskRow> rows = SmartPlanner.RankTasks(data.Tasks)
            .Take(8)
            .Select(CreateTaskRow)
            .ToList();
        BindTaskGrid(_dashboardGrid, rows);
    }

    private string GetCurrentDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(_service.CurrentUser.DisplayName))
            return _service.CurrentUser.DisplayName.Trim();
        if (!string.IsNullOrWhiteSpace(_service.Data.Settings.UserName))
            return _service.Data.Settings.UserName.Trim();
        return "同學";
    }

    private void RefreshIdentityPresentation()
    {
        string userName = GetCurrentDisplayName();
        string greeting = DateTime.Now.Hour switch
        {
            < 6 => "晚安",
            < 12 => "早安",
            < 18 => "午安",
            _ => "晚安"
        };

        if (_greetingLabel != null && !_greetingLabel.IsDisposed)
            _greetingLabel.Text = $"{greeting}，{userName}";
        if (_sidebarLogoSub != null && !_sidebarLogoSub.IsDisposed)
            _sidebarLogoSub.Text = $"PRO • {userName}";

        Text = $"StudyFlow Pro Research Edition｜{userName} 的個人學習空間";
    }

    private void RefreshTaskGrid()
    {
        if (_taskGrid == null)
            return;

        IEnumerable<StudyTask> query = _service.Data.Tasks;
        string keyword = _searchBox?.Text.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(task =>
                task.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                task.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                task.Tags.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        string selectedFilter = _taskFilter?.SelectedItem?.ToString() ?? "全部";
        query = selectedFilter switch
        {
            "未完成" => query.Where(task => !task.IsCompleted),
            "已完成" => query.Where(task => task.IsCompleted),
            "已逾期" => query.Where(task => !task.IsCompleted && task.DueDate < DateTime.Now),
            "今日到期" => query.Where(task => task.DueDate.Date == DateTime.Today),
            "高優先" => query.Where(task => !task.IsCompleted &&
                (task.Priority == TaskPriority.High || task.Priority == TaskPriority.Urgent)),
            "已釘選" => query.Where(task => task.IsPinned),
            _ => query
        };

        List<TaskRow> rows = query
            .OrderBy(task => task.IsCompleted)
            .ThenByDescending(task => task.IsPinned)
            .ThenByDescending(task => SmartPlanner.CalculateScore(task))
            .ThenBy(task => task.DueDate)
            .Select(CreateTaskRow)
            .ToList();
        BindTaskGrid(_taskGrid, rows);
    }

    private TaskRow CreateTaskRow(StudyTask task)
    {
        Course? course = _service.Data.Courses
            .FirstOrDefault(item => item.Id == task.CourseId);
        string courseName = course?.Name ?? "未分類";
        string courseColorHex = course?.ColorHex ?? "#94A3B8";
        string status = task.IsCompleted
            ? "✓ 已完成"
            : task.DueDate < DateTime.Now
                ? "⚠ 已逾期"
                : "進行中";

        return new TaskRow
        {
            Id = task.Id,
            Pin = task.IsPinned ? "★" : string.Empty,
            Status = status,
            Title = task.Title,
            Course = courseName,
            CourseColorHex = courseColorHex,
            Priority = SmartPlanner.PriorityText(task.Priority),
            DueDate = task.DueDate.ToString("yyyy/MM/dd HH:mm"),
            Progress = $"{task.ProgressPercent}% ({task.FocusedMinutes}/{task.EstimatedMinutes}分)",
            SmartScore = task.IsCompleted ? 0 : SmartPlanner.CalculateScore(task)
        };
    }

    private static void BindTaskGrid(DataGridView grid, List<TaskRow> rows)
    {
        EnsureTaskGridColumns(grid);
        grid.DataSource = new BindingList<TaskRow>(rows);
    }

    private static void EnsureTaskGridColumns(DataGridView grid)
    {
        if (grid.Columns.Count > 0)
            return;

        grid.AutoGenerateColumns = false;
        grid.Columns.AddRange(
            UiTheme.TextColumn("Pin", "釘選", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("Status", "狀態", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells),
            UiTheme.TextColumn("Title", "任務名稱", 220),
            UiTheme.TextColumn("Course", "課程 / 專案", 140),
            UiTheme.TextColumn("Priority", "優先級", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("DueDate", "截止時間", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells),
            UiTheme.TextColumn("Progress", "進度", 150),
            UiTheme.TextColumn("SmartScore", "智慧分數/100", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter));
    }

    private void RefreshCourseGrid()
    {
        Guid? previouslySelectedCourseId = GetSelectedCourseId();

        List<CourseRow> rows = _service.Data.Courses
            .OrderBy(course => course.Name)
            .Select(course =>
            {
                List<StudyTask> tasks = _service.Data.Tasks
                    .Where(task => task.CourseId == course.Id)
                    .ToList();
                int completionRate = tasks.Count == 0
                    ? 0
                    : (int)Math.Round(tasks.Count(task => task.IsCompleted) * 100.0 / tasks.Count);

                return new CourseRow
                {
                    Id = course.Id,
                    Name = course.Name,
                    ColorHex = course.ColorHex,
                    Instructor = course.Instructor,
                    Location = course.Location,
                    OpenTasks = tasks.Count(task => !task.IsCompleted),
                    CompletionRate = completionRate + "%"
                };
            })
            .ToList();

        EnsureCourseGridColumns();
        _courseGrid.DataSource = new BindingList<CourseRow>(rows);

        if (rows.Count == 0)
        {
            RefreshSelectedCourseTasks();
            return;
        }

        // 資料更新後盡量保留原本選取的課程，避免新增／編輯任務後跳回第一列。
        int selectedIndex = previouslySelectedCourseId == null
            ? 0
            : rows.FindIndex(row => row.Id == previouslySelectedCourseId.Value);
        if (selectedIndex < 0)
            selectedIndex = 0;

        _courseGrid.ClearSelection();
        _courseGrid.Rows[selectedIndex].Selected = true;
        if (_courseGrid.Columns.Count > 0)
            _courseGrid.CurrentCell = _courseGrid.Rows[selectedIndex].Cells[0];

        RefreshSelectedCourseTasks();
    }

    private void RefreshSelectedCourseTasks()
    {
        if (_courseTaskGrid == null || _courseTaskGrid.IsDisposed ||
            _courseTaskTitle == null || _courseTaskHint == null)
            return;

        Guid? courseId = GetSelectedCourseId();
        Course? course = courseId == null
            ? null
            : _service.Data.Courses.FirstOrDefault(item => item.Id == courseId.Value);

        if (course == null)
        {
            _courseTaskTitle.Text = "請先選取一個課程 / 專案";
            _courseTaskHint.Text = "點選上方課程後，這裡會列出所屬任務";
            _courseTaskGrid.DataSource = new BindingList<TaskRow>();
            return;
        }

        List<StudyTask> tasks = _service.Data.Tasks
            .Where(task => task.CourseId == course.Id)
            .OrderBy(task => task.IsCompleted)
            .ThenByDescending(task => task.IsPinned)
            .ThenByDescending(task => SmartPlanner.CalculateScore(task))
            .ThenBy(task => task.DueDate)
            .ToList();

        List<TaskRow> rows = tasks.Select(CreateTaskRow).ToList();
        int openCount = tasks.Count(task => !task.IsCompleted);
        _courseTaskTitle.Text = $"「{course.Name}」底下的任務｜共 {tasks.Count} 項，未完成 {openCount} 項";
        _courseTaskHint.Text = tasks.Count == 0
            ? "目前沒有任務，可到「任務管理」新增並選擇此課程"
            : "雙擊下方任務可直接開啟編輯視窗";

        EnsureCourseTaskGridColumns();
        _courseTaskGrid.DataSource = new BindingList<TaskRow>(rows);
    }

    private void EnsureCourseGridColumns()
    {
        if (_courseGrid.Columns.Count > 0)
            return;

        _courseGrid.AutoGenerateColumns = false;
        _courseGrid.Columns.AddRange(
            UiTheme.TextColumn("ColorHex", "識別顏色", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("Name", "課程 / 專案", 180),
            UiTheme.TextColumn("Instructor", "老師 / 負責人", 130),
            UiTheme.TextColumn("Location", "地點", 110),
            UiTheme.TextColumn("OpenTasks", "未完成任務", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("CompletionRate", "完成率", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter));
    }

    private void EnsureCourseTaskGridColumns()
    {
        if (_courseTaskGrid == null || _courseTaskGrid.Columns.Count > 0)
            return;

        _courseTaskGrid.AutoGenerateColumns = false;
        _courseTaskGrid.Columns.AddRange(
            UiTheme.TextColumn("Pin", "釘選", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("Status", "狀態", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells),
            UiTheme.TextColumn("Title", "任務名稱", 230),
            UiTheme.TextColumn("Priority", "優先級", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("DueDate", "截止時間", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells),
            UiTheme.TextColumn("Progress", "進度", 150),
            UiTheme.TextColumn("SmartScore", "智慧分數/100", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter));
    }

    private void RefreshFocusTaskChoices()
    {
        Guid? selectedId = (_focusTaskCombo?.SelectedItem as TaskChoice)?.Id;
        var choices = new List<TaskChoice>
        {
            new() { Id = null, Text = "自由專注（不綁定任務）" }
        };
        choices.AddRange(SmartPlanner.RankTasks(_service.Data.Tasks).Select(task => new TaskChoice
        {
            Id = task.Id,
            Text = $"{task.Title}｜{task.ProgressPercent}%｜{task.DueDate:MM/dd HH:mm}"
        }));

        _focusTaskCombo.DataSource = choices;
        _focusTaskCombo.SelectedItem = choices.FirstOrDefault(choice => choice.Id == selectedId) ?? choices[0];

        if (!_isTimerRunning && _remainingSeconds == 0)
        {
            int defaultMinutes = Math.Clamp(
                _service.Data.Settings.DefaultFocusMinutes,
                (int)_focusMinutes.Minimum,
                (int)_focusMinutes.Maximum);
            _focusMinutes.Value = defaultMinutes;
        }
    }

    private void RefreshSessionGrid()
    {
        List<SessionRow> rows = _service.Data.Sessions
            .OrderByDescending(session => session.StartedAt)
            .Take(50)
            .Select(session =>
            {
                StudyTask? task = _service.Data.Tasks.FirstOrDefault(item => item.Id == session.TaskId);
                string taskName = task?.Title
                    ?? (string.IsNullOrWhiteSpace(session.TaskNameSnapshot) ? "自由專注" : session.TaskNameSnapshot);
                string courseName = task == null
                    ? (string.IsNullOrWhiteSpace(session.CourseNameSnapshot) ? "未分類" : session.CourseNameSnapshot)
                    : _service.Data.Courses.FirstOrDefault(course => course.Id == task.CourseId)?.Name ?? "未分類";

                return new SessionRow
                {
                    Id = session.Id,
                    Date = session.StartedAt.ToString("yyyy/MM/dd HH:mm"),
                    Task = taskName,
                    Course = courseName,
                    Minutes = session.DurationMinutes,
                    Quality = session.FocusQuality > 0 ? session.FocusQuality + "/5" : "未評分",
                    Distractions = session.DistractionCount,
                    Note = session.Note
                };
            })
            .ToList();

        EnsureSessionGridColumns();
        _sessionGrid.DataSource = new BindingList<SessionRow>(rows);
    }

    private void EnsureSessionGridColumns()
    {
        if (_sessionGrid.Columns.Count > 0)
            return;

        _sessionGrid.AutoGenerateColumns = false;
        _sessionGrid.Columns.AddRange(
            UiTheme.TextColumn("Date", "日期", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells),
            UiTheme.TextColumn("Task", "任務", 220),
            UiTheme.TextColumn("Course", "課程", 130),
            UiTheme.TextColumn("Minutes", "分鐘", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("Quality", "品質", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("Distractions", "分心", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("Note", "備註", 160));
    }

    private void AddTask()
    {
        using var form = new TaskEditorForm(_service);
        if (form.ShowDialog(this) != DialogResult.OK)
            return;

        _service.Data.Tasks.Add(form.ResultTask);
        _service.Log(ActivityType.Created, "Task", form.ResultTask.Id, $"新增任務：{form.ResultTask.Title}");
        _service.SaveAndNotify();
        ShowPage("tasks");
        SetStatus("已新增任務");
    }

    private void EditSelectedTask()
    {
        Guid? taskId = GetSelectedTaskId(_taskGrid);
        if (taskId == null)
        {
            ShowSelectionMessage("請先在任務表格選取一筆資料。");
            return;
        }
        EditTask(taskId.Value);
    }

    private void EditSelectedDashboardTask()
    {
        Guid? taskId = GetSelectedTaskId(_dashboardGrid);
        if (taskId != null)
            EditTask(taskId.Value);
    }

    private void EditTask(Guid taskId)
    {
        StudyTask? task = _service.Data.Tasks.FirstOrDefault(item => item.Id == taskId);
        if (task == null)
            return;

        using var form = new TaskEditorForm(_service, task);
        if (form.ShowDialog(this) != DialogResult.OK)
            return;

        int index = _service.Data.Tasks.FindIndex(item => item.Id == taskId);
        if (index >= 0)
            _service.Data.Tasks[index] = form.ResultTask;
        _service.Log(ActivityType.Updated, "Task", form.ResultTask.Id, $"修改任務：{form.ResultTask.Title}");
        _service.SaveAndNotify();
        SetStatus("任務已更新");
    }

    private void ToggleSelectedTask()
    {
        Guid? taskId = GetSelectedTaskId(_taskGrid);
        StudyTask? task = taskId == null
            ? null
            : _service.Data.Tasks.FirstOrDefault(item => item.Id == taskId.Value);

        if (task == null)
        {
            ShowSelectionMessage("請先選取任務。");
            return;
        }

        task.IsCompleted = !task.IsCompleted;
        task.CompletedAt = task.IsCompleted ? DateTime.Now : null;
        task.UpdatedAt = DateTime.Now;
        _service.Log(
            task.IsCompleted ? ActivityType.Completed : ActivityType.Reopened,
            "Task",
            task.Id,
            task.IsCompleted ? $"完成任務：{task.Title}" : $"重新開啟任務：{task.Title}");

        _service.SaveAndNotify();
        SetStatus(task.IsCompleted ? "任務已完成" : "任務已恢復為進行中");
    }


    private void ToggleSelectedPin()
    {
        Guid? taskId = GetSelectedTaskId(_taskGrid);
        StudyTask? task = taskId == null
            ? null
            : _service.Data.Tasks.FirstOrDefault(item => item.Id == taskId.Value);

        if (task == null)
        {
            ShowSelectionMessage("請先選取任務。");
            return;
        }

        task.IsPinned = !task.IsPinned;
        task.UpdatedAt = DateTime.Now;
        _service.Log(
            ActivityType.Updated,
            "Task",
            task.Id,
            task.IsPinned ? $"釘選任務：{task.Title}" : $"取消釘選：{task.Title}");
        _service.SaveAndNotify();
        SetStatus(task.IsPinned ? "任務已釘選" : "已取消釘選");
    }

    private void ShowSelectedTaskInsight()
    {
        Guid? taskId = GetSelectedTaskId(_taskGrid);
        StudyTask? task = taskId == null
            ? null
            : _service.Data.Tasks.FirstOrDefault(item => item.Id == taskId.Value);

        if (task == null)
        {
            ShowSelectionMessage("請先選取任務。");
            return;
        }

        using var form = new PriorityInsightForm(_service, task);
        form.ShowDialog(this);
    }

    private void DeleteSelectedTask()
    {
        Guid? taskId = GetSelectedTaskId(_taskGrid);
        StudyTask? task = taskId == null
            ? null
            : _service.Data.Tasks.FirstOrDefault(item => item.Id == taskId.Value);

        if (task == null)
        {
            ShowSelectionMessage("請先選取任務。");
            return;
        }

        if (!ConfirmDialog.Ask(
                this,
                "確認刪除任務",
                $"確定刪除「{task.Title}」？{Environment.NewLine}相關專注紀錄會保留。"))
            return;

        _service.Data.Tasks.Remove(task);
        _service.Log(ActivityType.Deleted, "Task", task.Id, $"刪除任務：{task.Title}");
        _service.SaveAndNotify();
        SetStatus("任務已刪除");
    }

    private void AddCourse()
    {
        using var form = new CourseEditorForm();
        if (form.ShowDialog(this) != DialogResult.OK)
            return;

        _service.Data.Courses.Add(form.ResultCourse);
        _service.Log(ActivityType.Created, "Course", form.ResultCourse.Id, $"新增課程：{form.ResultCourse.Name}");
        _service.SaveAndNotify();
        ShowPage("courses");
        SetStatus("已新增課程 / 專案");
    }

    private void EditSelectedCourse()
    {
        Guid? courseId = GetSelectedCourseId();
        Course? course = courseId == null
            ? null
            : _service.Data.Courses.FirstOrDefault(item => item.Id == courseId.Value);
        if (course == null)
        {
            ShowSelectionMessage("請先選取一筆課程。");
            return;
        }

        using var form = new CourseEditorForm(course);
        if (form.ShowDialog(this) != DialogResult.OK)
            return;

        int index = _service.Data.Courses.FindIndex(item => item.Id == course.Id);
        if (index >= 0)
            _service.Data.Courses[index] = form.ResultCourse;
        _service.Log(ActivityType.Updated, "Course", form.ResultCourse.Id, $"修改課程：{form.ResultCourse.Name}");
        _service.SaveAndNotify();
        SetStatus("課程已更新");
    }

    private void DeleteSelectedCourse()
    {
        Guid? courseId = GetSelectedCourseId();
        Course? course = courseId == null
            ? null
            : _service.Data.Courses.FirstOrDefault(item => item.Id == courseId.Value);
        if (course == null)
        {
            ShowSelectionMessage("請先選取一筆課程。");
            return;
        }

        if (!ConfirmDialog.Ask(
                this,
                "確認刪除課程 / 專案",
                $"確定刪除「{course.Name}」？{Environment.NewLine}任務不會被刪除，將改為未分類。"))
            return;

        foreach (StudyTask task in _service.Data.Tasks.Where(item => item.CourseId == course.Id))
            task.CourseId = null;
        _service.Data.Courses.Remove(course);
        _service.Log(ActivityType.Deleted, "Course", course.Id, $"刪除課程：{course.Name}");
        _service.SaveAndNotify();
        SetStatus("課程已刪除");
    }

    private void StartTimer()
    {
        if (_remainingSeconds <= 0)
            ResetTimer(false);

        _activeTaskId = (_focusTaskCombo.SelectedItem as TaskChoice)?.Id;
        _isTimerRunning = true;
        _timer.Start();
        string taskName = (_focusTaskCombo.SelectedItem as TaskChoice)?.Text ?? "自由專注";
        _focusStateLabel.Text = "專注中｜" + taskName;
        SetStatus("專注計時已開始");
    }

    private void PauseTimer()
    {
        _isTimerRunning = false;
        _timer.Stop();
        _focusStateLabel.Text = "已暫停";
        SetStatus("專注計時已暫停");
    }

    private void ResetTimer(bool askConfirmation)
    {
        bool hasProgress = _remainingSeconds > 0 && _remainingSeconds < _totalSeconds;
        if (askConfirmation && hasProgress)
        {
            // 使用自訂確認視窗，右上角 X 與「取消」都能正常關閉，避免
            // MessageBoxButtons.YesNo 因沒有 Cancel 結果而停用標題列關閉鍵。
            if (!ConfirmDialog.Ask(
                    this,
                    "確認重設",
                    "目前計時進度將被清除，是否重設？",
                    "重設"))
                return;
        }

        _timer.Stop();
        _isTimerRunning = false;
        _activeTaskId = null;
        _totalSeconds = (int)_focusMinutes.Value * 60;
        _remainingSeconds = _totalSeconds;
        _focusStateLabel.Text = "準備開始";
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = _remainingSeconds / 60;
        int seconds = _remainingSeconds % 60;
        _timerLabel.Text = $"{minutes:00}:{seconds:00}";

        int elapsed = Math.Max(0, _totalSeconds - _remainingSeconds);
        int progress = _totalSeconds <= 0
            ? 0
            : Math.Clamp((int)Math.Round(elapsed * 100.0 / _totalSeconds), 0, 100);
        _focusProgress.Value = progress;
    }

    private void CompleteFocusSession(bool automatic)
    {
        int elapsedSeconds = Math.Max(0, _totalSeconds - _remainingSeconds);
        if (elapsedSeconds < 10 && !automatic)
        {
            MessageBox.Show(
                "專注時間少於 10 秒，尚不建立紀錄。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        _timer.Stop();
        _isTimerRunning = false;

        int durationMinutes = Math.Max(1, (int)Math.Round(elapsedSeconds / 60.0));
        DateTime endedAt = DateTime.Now;
        DateTime startedAt = endedAt.AddSeconds(-elapsedSeconds);
        StudyTask? activeTask = _activeTaskId.HasValue
            ? _service.Data.Tasks.FirstOrDefault(item => item.Id == _activeTaskId.Value)
            : null;
        string taskName = activeTask?.Title ?? "自由專注";
        string defaultNote = automatic ? "完成預定專注時段" : "手動結束專注時段";

        int quality = 0;
        int distractions = 0;
        string note = defaultNote;

        using (var review = new FocusReviewForm(durationMinutes, taskName, defaultNote))
        {
            DialogResult reviewResult = review.ShowDialog(this);
            if (reviewResult == DialogResult.OK)
            {
                quality = review.FocusQuality;
                distractions = review.DistractionCount;
                note = string.IsNullOrWhiteSpace(review.SessionNote)
                    ? defaultNote
                    : review.SessionNote;
            }
            else if (reviewResult == DialogResult.Ignore)
            {
                note = defaultNote;
            }
        }

        string courseName = activeTask == null
            ? "未分類"
            : _service.Data.Courses.FirstOrDefault(course => course.Id == activeTask.CourseId)?.Name ?? "未分類";
        var session = new FocusSession
        {
            TaskId = _activeTaskId,
            TaskNameSnapshot = taskName,
            CourseNameSnapshot = courseName,
            StartedAt = startedAt,
            EndedAt = endedAt,
            DurationMinutes = durationMinutes,
            FocusQuality = quality,
            DistractionCount = distractions,
            Note = note
        };
        _service.Data.Sessions.Add(session);

        if (activeTask != null)
        {
            activeTask.FocusedMinutes += durationMinutes;
            activeTask.UpdatedAt = DateTime.Now;
        }

        _service.Log(
            ActivityType.Focused,
            "FocusSession",
            session.Id,
            $"專注 {durationMinutes} 分鐘：{taskName}" +
            (quality > 0 ? $"（品質 {quality}/5，分心 {distractions} 次）" : string.Empty));
        _service.SaveAndNotify();

        System.Media.SystemSounds.Asterisk.Play();
        MessageBox.Show(
            $"本次專注 {durationMinutes} 分鐘，已寫入紀錄與研究指標。",
            "專注完成",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        ResetTimer(false);
        SetStatus("專注紀錄已儲存");
    }

    private void ImportTasks()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "CSV 檔案 (*.csv)|*.csv|所有檔案 (*.*)|*.*",
            Title = "匯入任務 CSV"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            CsvImportResult result = CsvService.ImportTasks(dialog.FileName, _service.Data);
            if (result.ImportedCount > 0 || result.CreatedCourseCount > 0)
            {
                _service.Log(ActivityType.Imported, "Task", null,
                    $"匯入任務 CSV：{Path.GetFileName(dialog.FileName)}，新增 {result.ImportedCount} 筆");
                _service.SaveAndNotify();
                RefreshAll();
            }
            MessageBox.Show(result.BuildMessage("任務"), "任務 CSV 匯入結果",
                MessageBoxButtons.OK,
                result.Errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            SetStatus($"任務 CSV 匯入完成：新增 {result.ImportedCount} 筆");
        }
        catch (Exception ex)
        {
            MessageBox.Show("匯入失敗：\n" + ex.Message +
                            "\n\n請優先使用本系統『匯出任務 CSV』產生的欄位格式。",
                "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImportSessions()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "CSV 檔案 (*.csv)|*.csv|所有檔案 (*.*)|*.*",
            Title = "匯入專注紀錄 CSV"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            CsvImportResult result = CsvService.ImportSessions(dialog.FileName, _service.Data);
            if (result.ImportedCount > 0)
            {
                _service.Log(ActivityType.Imported, "FocusSession", null,
                    $"匯入專注紀錄 CSV：{Path.GetFileName(dialog.FileName)}，新增 {result.ImportedCount} 筆");
                _service.SaveAndNotify();
                RefreshAll();
            }
            MessageBox.Show(result.BuildMessage("專注紀錄"), "專注紀錄 CSV 匯入結果",
                MessageBoxButtons.OK,
                result.Errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            SetStatus($"專注紀錄 CSV 匯入完成：新增 {result.ImportedCount} 筆");
        }
        catch (Exception ex)
        {
            MessageBox.Show("匯入失敗：\n" + ex.Message +
                            "\n\n請優先使用本系統『匯出紀錄 CSV』產生的欄位格式。",
                "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportTasks()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "CSV 檔案 (*.csv)|*.csv",
            FileName = $"StudyFlow-Tasks-{DateTime.Now:yyyyMMdd}.csv"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            CsvService.ExportTasks(dialog.FileName, _service.Data);
            _service.Log(ActivityType.Exported, "Task", null, $"匯出任務 CSV：{Path.GetFileName(dialog.FileName)}");
            _service.SaveAndNotify();
            MessageBox.Show("匯出完成，可使用 Excel 開啟。", "完成",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            SetStatus("任務 CSV 匯出完成");
        }
        catch (Exception ex)
        {
            MessageBox.Show("匯出失敗：\n" + ex.Message, "錯誤",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportSessions()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "CSV 檔案 (*.csv)|*.csv",
            FileName = $"StudyFlow-Sessions-{DateTime.Now:yyyyMMdd}.csv"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            CsvService.ExportSessions(dialog.FileName, _service.Data);
            _service.Log(ActivityType.Exported, "FocusSession", null, $"匯出專注 CSV：{Path.GetFileName(dialog.FileName)}");
            _service.SaveAndNotify();
            MessageBox.Show("匯出完成。", "完成",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            SetStatus("專注紀錄 CSV 匯出完成");
        }
        catch (Exception ex)
        {
            MessageBox.Show("匯出失敗：\n" + ex.Message, "錯誤",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Guid? GetSelectedTaskId(DataGridView grid)
    {
        return grid.CurrentRow?.DataBoundItem is TaskRow row ? row.Id : null;
    }

    private Guid? GetSelectedCourseId()
    {
        return _courseGrid.CurrentRow?.DataBoundItem is CourseRow row ? row.Id : null;
    }


    private static void ShowSelectionMessage(string message)
    {
        MessageBox.Show(message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void FormatCourseGridCell(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        if (grid.Rows[e.RowIndex].DataBoundItem is not CourseRow row)
            return;

        string colorHex = string.IsNullOrWhiteSpace(row.ColorHex) ? "#2563EB" : row.ColorHex;
        Color courseColor;
        try
        {
            courseColor = ColorTranslator.FromHtml(colorHex);
        }
        catch
        {
            courseColor = UiTheme.Primary;
        }

        string property = grid.Columns[e.ColumnIndex].DataPropertyName;

        // 課程識別色必須原封不動套用到整個資料列。
        // 不論一般狀態或選取狀態，都不再加深、變亮或與主題色混合；
        // 只有文字會依原始背景色亮度自動切換黑／白，確保可讀性。
        double luminance = .299 * courseColor.R + .587 * courseColor.G + .114 * courseColor.B;
        Color textColor = luminance > 145 ? Color.Black : Color.White;

        e.CellStyle.BackColor = courseColor;
        e.CellStyle.ForeColor = textColor;
        e.CellStyle.SelectionBackColor = courseColor;
        e.CellStyle.SelectionForeColor = textColor;

        if (property == "ColorHex")
        {
            e.Value = colorHex.ToUpperInvariant();
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            e.FormattingApplied = true;
        }
    }

    private static Color BlendCourseColor(Color courseColor, Color surfaceColor, float courseRatio)
    {
        courseRatio = Math.Clamp(courseRatio, 0f, 1f);
        float surfaceRatio = 1f - courseRatio;
        return Color.FromArgb(
            (int)Math.Round(courseColor.R * courseRatio + surfaceColor.R * surfaceRatio),
            (int)Math.Round(courseColor.G * courseRatio + surfaceColor.G * surfaceRatio),
            (int)Math.Round(courseColor.B * courseRatio + surfaceColor.B * surfaceRatio));
    }

    private void FormatTaskGridCell(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        string property = grid.Columns[e.ColumnIndex].DataPropertyName;
        string text = e.Value?.ToString() ?? string.Empty;

        if (property == "Status")
        {
            if (text.Contains("逾期"))
            {
                e.CellStyle.ForeColor = UiTheme.Danger;
            }
            else if (text.Contains("完成"))
            {
                e.CellStyle.ForeColor = UiTheme.Success;
            }
        }
        else if (property == "Pin" && text == "★")
        {
            e.CellStyle.ForeColor = UiTheme.Warning;
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        else if (property == "Course" && grid.Rows[e.RowIndex].DataBoundItem is TaskRow row)
        {
            try
            {
                Color courseColor = ColorTranslator.FromHtml(row.CourseColorHex);
                e.CellStyle.BackColor = BlendCourseColor(courseColor, UiTheme.Surface, 0.18f);
                e.CellStyle.ForeColor = UiTheme.Navy;
            }
            catch
            {
                e.CellStyle.ForeColor = UiTheme.Slate;
            }
        }
        else if (property == "SmartScore" && int.TryParse(text, out int score))
        {
            e.CellStyle.ForeColor = score >= 80
                ? UiTheme.Danger
                : score >= 60
                    ? UiTheme.Warning
                    : UiTheme.Primary;
        }
    }

    private void ShowDueSoonReminder()
    {
        AppSettings settings = _service.Data.Settings;
        if (!settings.ShowDueSoonReminder)
            return;

        StudyTask? task = _service.Data.Tasks
            .Where(item => !item.IsCompleted)
            .OrderByDescending(item => SmartPlanner.CalculateScore(item))
            .ThenBy(item => item.DueDate)
            .FirstOrDefault();
        if (task == null)
            return;

        PriorityAnalysis analysis = SmartPlanner.Analyze(task);
        string course = _service.Data.Courses
            .FirstOrDefault(item => item.Id == task.CourseId)?.Name ?? "未分類";

        DateTime now = DateTime.Now;
        DateTime reminderLimit = now.AddHours(settings.DueSoonHours);
        string dueText;
        string statusText;

        if (task.DueDate < now)
        {
            dueText = $"已逾期 {SmartPlanner.Humanize(now - task.DueDate)}";
            statusText = "此任務已逾期，建議立即處理。";
        }
        else
        {
            dueText = $"剩餘 {SmartPlanner.Humanize(task.DueDate - now)}";
            statusText = task.DueDate <= reminderLimit
                ? $"已進入未來 {settings.DueSoonHours} 小時的提醒範圍。"
                : "目前是所有未完成任務中智慧分數最高的一項。";
        }

        string message =
            $"目前最緊急的任務{Environment.NewLine}{Environment.NewLine}" +
            $"{task.Title}{Environment.NewLine}" +
            $"課程／專案：{course}{Environment.NewLine}" +
            $"截止狀態：{dueText}{Environment.NewLine}" +
            $"智慧分數：{analysis.TotalScore}/100（{analysis.Level}）{Environment.NewLine}{Environment.NewLine}" +
            $"{statusText}{Environment.NewLine}" +
            $"建議：{analysis.Recommendation}";

        MessageBox.Show(
            message,
            "StudyFlow 今日首要任務",
            MessageBoxButtons.OK,
            task.DueDate < now ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = $"{DateTime.Now:HH:mm:ss}｜{message}";
    }

    private static Panel CreateBasePage()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Background,
            Visible = false
        };
    }

    private static Control CreatePageHeader(string title, string subtitle)
    {
        return UiTheme.StackedHeader(title, subtitle, out _, 25);
    }

    private static (Panel Card, Label ValueLabel) CreateMetricCard(string title, Color accentColor)
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(18)
        };
        var accentBar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 6,
            BackColor = accentColor
        };
        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9.5f)
        };
        var valueLabel = new Label
        {
            Text = "0",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(22, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        card.Controls.Add(accentBar);
        return (card, valueLabel);
    }

    private static Panel WrapWithPadding(Control control, Padding padding)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = padding
        };
        panel.Controls.Add(control);
        return panel;
    }

    private static Label CreateInlineLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(0, 10, 8, 0),
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9, FontStyle.Bold)
        };
    }
}
