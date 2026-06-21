using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class SmartScheduleForm : Form
{
    private readonly DataService _service;
    private readonly DateTimePicker _startTime = new();
    private readonly NumericUpDown _availableMinutes = new();
    private readonly NumericUpDown _breakMinutes = new();
    private readonly RichTextBox _planBox = new();
    private readonly Label _summaryLabel = new();
    private readonly Label _blockMetricLabel = new();
    private readonly Label _focusMetricLabel = new();
    private readonly Label _bufferMetricLabel = new();
    private readonly Label _endMetricLabel = new();
    private readonly Label _topTaskLabel = new();
    private string _plainPlanText = string.Empty;

    public SmartScheduleForm(DataService service)
    {
        _service = service;

        Text = "智慧排程｜今日最佳學習路線";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1120, 800);
        MinimumSize = new Size(920, 680);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        LoadAccountScheduleSettings();
        GeneratePlan();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(22),
            BackColor = UiTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildControls(), 0, 1);
        root.Controls.Add(BuildMetricStrip(), 0, 2);
        root.Controls.Add(BuildMainContent(), 0, 3);
        root.Controls.Add(BuildBottomBar(), 0, 4);

        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Sidebar,
            BorderColor = UiTheme.Sidebar,
            Radius = 18,
            Padding = new Padding(24, 10, 24, 10),
            Margin = Padding.Empty
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Sidebar,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Sidebar,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        left.Controls.Add(new Label
        {
            Text = "Smart Schedule",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = UiTheme.Font(24, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        }, 0, 0);
        left.Controls.Add(new Label
        {
            Text = "依截止時間、智慧分數與剩餘工作量，自動產生可執行的今日學習路線。",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.BorderAccent,
            Font = UiTheme.Font(9.3f),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Margin = Padding.Empty,
            Padding = new Padding(1, 0, 0, 0)
        }, 0, 1);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Sidebar,
            Margin = new Padding(20, 0, 0, 0),
            Padding = Padding.Empty
        };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        right.Controls.Add(new Label
        {
            Text = "可解釋規則式排程",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            BackColor = UiTheme.PrimaryDark,
            Font = UiTheme.Font(9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 4, 0, 5),
            Padding = Padding.Empty
        }, 0, 0);
        right.Controls.Add(new Label
        {
            Text = DateTime.Today.ToString("yyyy / MM / dd  dddd", CultureInfo.GetCultureInfo("zh-TW")),
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.SidebarAccentText,
            Font = UiTheme.Font(9),
            TextAlign = ContentAlignment.MiddleRight,
            Margin = Padding.Empty,
            Padding = new Padding(0, 2, 2, 0)
        }, 0, 1);

        layout.Controls.Add(left, 0, 0);
        layout.Controls.Add(right, 1, 0);
        card.Controls.Add(layout);
        return card;
    }

    private Control BuildControls()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Radius = 14,
            Padding = new Padding(16, 12, 16, 12)
        };

        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 9,
            RowCount = 1,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 146));
        // 「使用現在時間」在高 DPI / 125%～150% 顯示縮放時需要更寬的欄位，
        // 否則按鈕文字會被裁切成「使用現」之類的不完整內容。
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 176));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _startTime.Format = DateTimePickerFormat.Custom;
        _startTime.CustomFormat = "HH:mm";
        _startTime.ShowUpDown = true;
        _startTime.Value = DateTime.Now;
        StyleInput(_startTime);

        _availableMinutes.Minimum = 30;
        _availableMinutes.Maximum = 720;
        _availableMinutes.Increment = 15;
        _availableMinutes.Value = 180;
        StyleInput(_availableMinutes);

        _breakMinutes.Minimum = 0;
        _breakMinutes.Maximum = 30;
        _breakMinutes.Value = 5;
        StyleInput(_breakMinutes);

        Button regenerate = UiTheme.PrimaryButton("重新產生");
        regenerate.Dock = DockStyle.Fill;
        regenerate.AutoSize = false;
        regenerate.TextAlign = ContentAlignment.MiddleCenter;
        regenerate.Margin = new Padding(6, 7, 6, 7);
        regenerate.Click += (_, _) => GeneratePlan();

        Button useNow = UiTheme.SecondaryButton("使用現在時間");
        useNow.Dock = DockStyle.Fill;
        useNow.AutoSize = false;
        useNow.AutoEllipsis = false;
        useNow.Padding = new Padding(8, 0, 8, 0);
        useNow.Font = UiTheme.Font(9.5f, FontStyle.Bold);
        useNow.TextAlign = ContentAlignment.MiddleCenter;
        useNow.Margin = new Padding(6, 7, 6, 7);
        useNow.Click += (_, _) =>
        {
            _startTime.Value = DateTime.Now;
            GeneratePlan();
        };

        row.Controls.Add(CreateFieldLabel("開始"), 0, 0);
        row.Controls.Add(_startTime, 1, 0);
        row.Controls.Add(CreateFieldLabel("可用分鐘"), 2, 0);
        row.Controls.Add(_availableMinutes, 3, 0);
        row.Controls.Add(CreateFieldLabel("休息分鐘"), 4, 0);
        row.Controls.Add(_breakMinutes, 5, 0);
        row.Controls.Add(regenerate, 6, 0);
        row.Controls.Add(useNow, 7, 0);

        card.Controls.Add(row);
        return card;
    }

    private Control BuildMetricStrip()
    {
        var strip = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        for (int i = 0; i < 4; i++)
            strip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        strip.Controls.Add(WrapMetric(CreateMetricCard("專注區塊", _blockMetricLabel, UiTheme.Primary), new Padding(0, 0, 8, 0)), 0, 0);
        strip.Controls.Add(WrapMetric(CreateMetricCard("專注時間", _focusMetricLabel, UiTheme.Success), new Padding(8, 0, 8, 0)), 1, 0);
        strip.Controls.Add(WrapMetric(CreateMetricCard("保留緩衝", _bufferMetricLabel, UiTheme.Warning), new Padding(8, 0, 8, 0)), 2, 0);
        strip.Controls.Add(WrapMetric(CreateMetricCard("預計結束", _endMetricLabel, UiTheme.Purple), new Padding(8, 0, 0, 0)), 3, 0);
        return strip;
    }

    private Control BuildMainContent()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));

        var planHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 9, 0), BackColor = UiTheme.Background };
        planHost.Controls.Add(BuildPlanCard());

        var insightHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(9, 0, 0, 0), BackColor = UiTheme.Background };
        insightHost.Controls.Add(BuildInsightCard());

        layout.Controls.Add(planHost, 0, 0);
        layout.Controls.Add(insightHost, 1, 0);
        return layout;
    }

    private Control BuildPlanCard()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Radius = 14,
            Padding = new Padding(18)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label
        {
            Text = "今日最佳學習路線",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(13, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 0);

        _summaryLabel.Dock = DockStyle.Fill;
        _summaryLabel.ForeColor = UiTheme.Primary;
        _summaryLabel.Font = UiTheme.Font(9.5f, FontStyle.Bold);
        _summaryLabel.TextAlign = ContentAlignment.MiddleLeft;
        _summaryLabel.Margin = Padding.Empty;
        layout.Controls.Add(_summaryLabel, 0, 1);

        _planBox.Dock = DockStyle.Fill;
        _planBox.ReadOnly = true;
        _planBox.BorderStyle = BorderStyle.None;
        _planBox.BackColor = UiTheme.Surface;
        _planBox.ForeColor = UiTheme.Navy;
        _planBox.Font = UiTheme.Font(10.2f);
        _planBox.ScrollBars = RichTextBoxScrollBars.Vertical;
        _planBox.DetectUrls = false;
        layout.Controls.Add(_planBox, 0, 2);

        card.Controls.Add(layout);
        return card;
    }

    private Control BuildInsightCard()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Radius = 14,
            Padding = new Padding(18)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label
        {
            Text = "目前首要任務",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(12, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 0);

        _topTaskLabel.Dock = DockStyle.Fill;
        _topTaskLabel.ForeColor = UiTheme.Slate;
        _topTaskLabel.BackColor = UiTheme.PrimarySoft;
        _topTaskLabel.Font = UiTheme.Font(9.3f, FontStyle.Bold);
        _topTaskLabel.TextAlign = ContentAlignment.MiddleLeft;
        _topTaskLabel.Padding = new Padding(12, 8, 12, 8);
        _topTaskLabel.Margin = Padding.Empty;
        layout.Controls.Add(_topTaskLabel, 0, 1);

        layout.Controls.Add(new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0, 8, 0, 8)
        }, 0, 2);

        layout.Controls.Add(new Label
        {
            Text = "智慧分數圖例",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(12, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 3);

        var legend = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        for (int i = 0; i < 4; i++)
            legend.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        legend.Controls.Add(CreateLegendRow("80–100", "立即處理", UiTheme.Danger), 0, 0);
        legend.Controls.Add(CreateLegendRow("60–79", "高度優先", UiTheme.Warning), 0, 1);
        legend.Controls.Add(CreateLegendRow("40–59", "中度優先", UiTheme.Primary), 0, 2);
        legend.Controls.Add(CreateLegendRow("0–39", "可排程", UiTheme.Success), 0, 3);
        layout.Controls.Add(legend, 0, 4);

        layout.Controls.Add(new Label
        {
            Text = "排程原則\n先處理高分任務，再把剩餘工作量切成可完成的專注區塊；休息時間也會計入總可用時間。",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9),
            TextAlign = ContentAlignment.BottomLeft,
            Margin = Padding.Empty
        }, 0, 5);

        card.Controls.Add(layout);
        return card;
    }

    private Control BuildBottomBar()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0),
            BackColor = UiTheme.Background
        };

        Button close = UiTheme.SecondaryButton("關閉");
        Button copy = UiTheme.PrimaryButton("複製今日計畫");
        Button export = UiTheme.SecondaryButton("匯出 TXT");

        close.Click += (_, _) => Close();
        copy.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_plainPlanText))
                return;

            Clipboard.SetText(_plainPlanText);
            MessageBox.Show("今日計畫已複製到剪貼簿。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        export.Click += (_, _) => ExportPlan();

        bar.Controls.Add(close);
        bar.Controls.Add(copy);
        bar.Controls.Add(export);
        return bar;
    }

    private static void StyleInput(Control control)
    {
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(6, 8, 6, 8);
        control.Font = UiTheme.Font(10);
    }

    private static Label CreateFieldLabel(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleLeft,
        ForeColor = UiTheme.Slate,
        Font = UiTheme.Font(9, FontStyle.Bold),
        Margin = Padding.Empty
    };

    private static Control CreateMetricCard(string title, Label valueLabel, Color accent)
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Radius = 14,
            Padding = new Padding(16)
        };

        var accentBar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 5,
            BackColor = accent,
            Margin = Padding.Empty
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(10, 0, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));

        valueLabel.Dock = DockStyle.Fill;
        valueLabel.ForeColor = UiTheme.Navy;
        valueLabel.Font = UiTheme.Font(18, FontStyle.Bold);
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;
        valueLabel.Margin = Padding.Empty;

        layout.Controls.Add(valueLabel, 0, 0);
        layout.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9),
            TextAlign = ContentAlignment.TopLeft,
            Margin = Padding.Empty
        }, 0, 1);

        card.Controls.Add(layout);
        card.Controls.Add(accentBar);
        return card;
    }

    private static Control WrapMetric(Control metric, Padding padding)
    {
        var host = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = padding,
            BackColor = UiTheme.Background
        };
        host.Controls.Add(metric);
        return host;
    }

    private static Control CreateLegendRow(string range, string level, Color color)
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 66));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        row.Controls.Add(new Panel
        {
            Width = 8,
            Height = 8,
            BackColor = color,
            Anchor = AnchorStyles.None,
            Margin = Padding.Empty
        }, 0, 0);
        row.Controls.Add(new Label
        {
            Text = range,
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(8.8f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 1, 0);
        row.Controls.Add(new Label
        {
            Text = level,
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.8f),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 2, 0);
        return row;
    }

    private void LoadAccountScheduleSettings()
    {
        SmartScheduleState state = _service.Data.SmartSchedule ?? new SmartScheduleState();

        _availableMinutes.Value = Math.Clamp(
            state.AvailableMinutes,
            (int)_availableMinutes.Minimum,
            (int)_availableMinutes.Maximum);
        _breakMinutes.Value = Math.Clamp(
            state.BreakMinutes,
            (int)_breakMinutes.Minimum,
            (int)_breakMinutes.Maximum);

        // 同一天重新開啟時延續該帳號上次排程的開始時間；跨日則以現在時間開始。
        DateTime start = state.GeneratedAt?.Date == DateTime.Today && state.StartAt != default
            ? DateTime.Today.AddHours(state.StartAt.Hour).AddMinutes(state.StartAt.Minute)
            : DateTime.Now;
        _startTime.Value = start;
    }

    private void GeneratePlan()
    {
        int available = (int)_availableMinutes.Value;
        int breakTime = (int)_breakMinutes.Value;
        int defaultBlock = Math.Clamp(_service.Data.Settings.DefaultFocusMinutes, 10, 90);
        DateTime cursor = DateTime.Today
            .AddHours(_startTime.Value.Hour)
            .AddMinutes(_startTime.Value.Minute);

        List<StudyTask> ranked = SmartPlanner.RankTasks(_service.Data.Tasks).ToList();
        var remaining = ranked.ToDictionary(
            task => task.Id,
            task => Math.Max(10, task.RemainingMinutes));
        var blocks = new List<ScheduleBlock>();

        int elapsed = 0;
        int focusMinutes = 0;
        int breakMinutesUsed = 0;
        int taskCursor = 0;
        int safety = 0;

        while (elapsed < available && ranked.Count > 0 && safety < 50)
        {
            safety++;
            StudyTask? task = FindNextTask(ranked, remaining, ref taskCursor);
            if (task == null)
                break;

            int free = available - elapsed;
            int blockMinutes = Math.Min(defaultBlock, remaining[task.Id]);
            blockMinutes = Math.Min(blockMinutes, free);
            if (blockMinutes < 10)
                break;

            PriorityAnalysis analysis = SmartPlanner.Analyze(task);
            string course = _service.Data.Courses.FirstOrDefault(c => c.Id == task.CourseId)?.Name ?? "未分類";
            DateTime end = cursor.AddMinutes(blockMinutes);

            var scheduleBlock = new ScheduleBlock
            {
                Task = task,
                Course = course,
                Analysis = analysis,
                Start = cursor,
                End = end,
                Minutes = blockMinutes
            };
            blocks.Add(scheduleBlock);

            remaining[task.Id] = Math.Max(0, remaining[task.Id] - blockMinutes);
            focusMinutes += blockMinutes;
            elapsed += blockMinutes;
            cursor = end;

            bool hasMoreWork = remaining.Values.Any(value => value > 0);
            bool enoughForBreakAndWork = available - elapsed >= breakTime + 10;
            if (breakTime > 0 && hasMoreWork && enoughForBreakAndWork)
            {
                scheduleBlock.BreakAfterMinutes = breakTime;
                cursor = cursor.AddMinutes(breakTime);
                breakMinutesUsed += breakTime;
                elapsed += breakTime;
            }
        }

        int buffer = Math.Max(0, available - elapsed);
        DateTime expectedEnd = DateTime.Today
            .AddHours(_startTime.Value.Hour)
            .AddMinutes(_startTime.Value.Minute)
            .AddMinutes(elapsed);

        UpdateMetrics(blocks.Count, focusMinutes, buffer, expectedEnd);
        UpdateTopTask(ranked.FirstOrDefault());
        RenderPlan(blocks, available, focusMinutes, breakMinutesUsed, buffer, expectedEnd);
        PersistCurrentPlan(blocks, available, breakTime, focusMinutes, breakMinutesUsed, buffer, expectedEnd);
    }

    private void PersistCurrentPlan(
        IReadOnlyList<ScheduleBlock> blocks,
        int available,
        int breakTime,
        int focusMinutes,
        int breakMinutesUsed,
        int buffer,
        DateTime expectedEnd)
    {
        DateTime startAt = DateTime.Today
            .AddHours(_startTime.Value.Hour)
            .AddMinutes(_startTime.Value.Minute);

        _service.Data.SmartSchedule = new SmartScheduleState
        {
            GeneratedAt = DateTime.Now,
            StartAt = startAt,
            AvailableMinutes = available,
            BreakMinutes = breakTime,
            FocusMinutes = focusMinutes,
            BreakMinutesUsed = breakMinutesUsed,
            BufferMinutes = buffer,
            ExpectedEnd = expectedEnd,
            PlainText = _plainPlanText,
            Blocks = blocks.Select(block => new SmartScheduleBlockSnapshot
            {
                TaskId = block.Task.Id,
                TaskTitle = block.Task.Title,
                CourseName = block.Course,
                Start = block.Start,
                End = block.End,
                Minutes = block.Minutes,
                BreakAfterMinutes = block.BreakAfterMinutes,
                SmartScore = block.Analysis.TotalScore,
                PriorityLevel = block.Analysis.Level
            }).ToList()
        };

        _service.Log(
            ActivityType.Updated,
            "SmartSchedule",
            null,
            $"更新智慧排程：可用 {available} 分、休息 {breakTime} 分、{blocks.Count} 個專注區塊");
        _service.SaveAndNotify();
    }

    private static StudyTask? FindNextTask(
        IReadOnlyList<StudyTask> ranked,
        IReadOnlyDictionary<Guid, int> remaining,
        ref int cursor)
    {
        for (int attempt = 0; attempt < ranked.Count; attempt++)
        {
            StudyTask task = ranked[cursor % ranked.Count];
            cursor = (cursor + 1) % ranked.Count;
            if (remaining.TryGetValue(task.Id, out int value) && value > 0)
                return task;
        }

        return null;
    }

    private void UpdateMetrics(int blocks, int focus, int buffer, DateTime expectedEnd)
    {
        _blockMetricLabel.Text = blocks.ToString(CultureInfo.InvariantCulture);
        _focusMetricLabel.Text = focus + " 分";
        _bufferMetricLabel.Text = buffer + " 分";
        _endMetricLabel.Text = expectedEnd.ToString("HH:mm");
    }

    private void UpdateTopTask(StudyTask? task)
    {
        if (task == null)
        {
            _topTaskLabel.Text = "目前沒有未完成任務。\n新增任務後即可產生智慧排程。";
            return;
        }

        PriorityAnalysis analysis = SmartPlanner.Analyze(task);
        string due = task.DueDate < DateTime.Now
            ? "已逾期 " + SmartPlanner.Humanize(DateTime.Now - task.DueDate)
            : "剩餘 " + SmartPlanner.Humanize(task.DueDate - DateTime.Now);
        _topTaskLabel.Text = $"{task.Title}\n{analysis.Level}｜{analysis.TotalScore}/100｜{due}";
    }

    private void RenderPlan(
        IReadOnlyList<ScheduleBlock> blocks,
        int available,
        int focus,
        int rest,
        int buffer,
        DateTime expectedEnd)
    {
        _summaryLabel.Text = blocks.Count == 0
            ? "目前沒有可排程的未完成任務"
            : $"{blocks.Count} 個專注區塊｜專注 {focus} 分｜休息 {rest} 分｜緩衝 {buffer} 分";

        var plain = new StringBuilder();
        plain.AppendLine("今日智慧排程");
        plain.AppendLine(new string('═', 34));
        plain.AppendLine($"產生時間：{DateTime.Now:yyyy/MM/dd HH:mm}");
        plain.AppendLine($"可用時間：{available} 分鐘｜預計結束：{expectedEnd:HH:mm}");
        plain.AppendLine($"專注：{focus} 分鐘｜休息：{rest} 分鐘｜緩衝：{buffer} 分鐘");
        plain.AppendLine();

        _planBox.Clear();
        AppendPlanText("今日智慧排程\n", UiTheme.Navy, 14, FontStyle.Bold);
        AppendPlanText(
            $"可用 {available} 分鐘｜預計 {expectedEnd:HH:mm} 結束｜保留 {buffer} 分鐘緩衝\n\n",
            UiTheme.Muted,
            9.3f,
            FontStyle.Regular);

        if (blocks.Count == 0)
        {
            const string empty = "目前沒有未完成任務。可新增任務後重新產生排程。";
            plain.AppendLine(empty);
            AppendPlanText(empty, UiTheme.Muted, 10, FontStyle.Regular);
            _plainPlanText = plain.ToString();
            return;
        }

        for (int i = 0; i < blocks.Count; i++)
        {
            ScheduleBlock block = blocks[i];
            string number = (i + 1).ToString("00", CultureInfo.InvariantCulture);

            plain.AppendLine($"{number}. {block.Start:HH:mm}–{block.End:HH:mm}  {block.Task.Title}");
            plain.AppendLine($"    課程：{block.Course}｜智慧分數：{block.Analysis.TotalScore}/100｜{block.Analysis.Level}");
            plain.AppendLine($"    目標：專注推進 {block.Minutes} 分鐘，完成後記錄品質、分心次數與反思。");

            AppendPlanText($"{number}  {block.Start:HH:mm}–{block.End:HH:mm}\n", UiTheme.Primary, 10.5f, FontStyle.Bold);
            AppendPlanText(block.Task.Title + "\n", UiTheme.Navy, 11.2f, FontStyle.Bold);
            AppendPlanText(
                $"{block.Course}｜智慧分數 {block.Analysis.TotalScore}/100｜{block.Analysis.Level}\n",
                UiTheme.Slate,
                9.2f,
                FontStyle.Regular);
            AppendPlanText(
                $"本區塊目標：專注推進 {block.Minutes} 分鐘，完成後記錄品質與反思。\n",
                UiTheme.Success,
                9.2f,
                FontStyle.Regular);

            if (block.BreakAfterMinutes > 0)
            {
                DateTime restStart = block.End;
                DateTime restEnd = restStart.AddMinutes(block.BreakAfterMinutes);
                plain.AppendLine($"    休息：{restStart:HH:mm}–{restEnd:HH:mm}");
                AppendPlanText(
                    $"休息 {restStart:HH:mm}–{restEnd:HH:mm}\n",
                    UiTheme.Warning,
                    9.2f,
                    FontStyle.Bold);
            }

            plain.AppendLine();
            AppendPlanText("\n", UiTheme.Slate, 9, FontStyle.Regular);
        }

        plain.AppendLine($"保留緩衝：{buffer} 分鐘，可用於複習、整理筆記或處理突發事項。");
        plain.AppendLine("排程原則：優先處理高分任務，並把工作拆成可完成的專注區塊。休息時間已計入總可用時間。");
        AppendPlanText(
            $"保留緩衝 {buffer} 分鐘，可用於複習、整理筆記或處理突發事項。",
            UiTheme.Muted,
            9.2f,
            FontStyle.Italic);

        _plainPlanText = plain.ToString();
        _planBox.SelectionStart = 0;
        _planBox.ScrollToCaret();
    }

    private void AppendPlanText(string text, Color color, float size, FontStyle style)
    {
        _planBox.SelectionStart = _planBox.TextLength;
        _planBox.SelectionLength = 0;
        _planBox.SelectionColor = color;
        _planBox.SelectionFont = UiTheme.Font(size, style);
        _planBox.AppendText(text);
        _planBox.SelectionColor = _planBox.ForeColor;
    }

    private void ExportPlan()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "文字檔 (*.txt)|*.txt",
            FileName = $"StudyFlow-SmartSchedule-{DateTime.Now:yyyyMMdd-HHmm}.txt"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        File.WriteAllText(dialog.FileName, _plainPlanText, new UTF8Encoding(true));
        MessageBox.Show("匯出完成。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private sealed class ScheduleBlock
    {
        public StudyTask Task { get; init; } = new();
        public string Course { get; init; } = string.Empty;
        public PriorityAnalysis Analysis { get; init; } = new();
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public int Minutes { get; init; }
        public int BreakAfterMinutes { get; set; }
    }
}
