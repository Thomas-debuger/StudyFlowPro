using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class ResearchCenterForm : Form
{
    private readonly DataService _service;
    private readonly Label _productivity = new();
    private readonly Label _consistency = new();
    private readonly Label _quality = new();
    private readonly Label _accuracy = new();
    private readonly Label _summary = new();
    private readonly DataGridView _activityGrid = new();

    public ResearchCenterForm(DataService service)
    {
        _service = service;

        Text = "Research Center｜研究型分析與輸出";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1120, 760);
        MinimumSize = new Size(860, 620);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        RefreshData();
    }

    private void BuildInterface()
    {
        const int contentMinimumWidth = 1080;
        const int contentHeight = 710;

        var scrollHost = new WheelScrollPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = UiTheme.Background,
            TabStop = true,
            AutoScrollMinSize = new Size(contentMinimumWidth, contentHeight)
        };

        var contentSurface = new Panel
        {
            Location = Point.Empty,
            Size = new Size(contentMinimumWidth, contentHeight),
            BackColor = UiTheme.Background,
            Margin = Padding.Empty
        };

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 5,
            Margin = Padding.Empty,
            BackColor = UiTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));

        Control header = UiTheme.StackedHeader(
            "Research Center",
            "可解釋優先排序、品質指標、稽核軌跡、週報與資料健檢",
            out _,
            24);
        root.Controls.Add(header, 0, 0);

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = Padding.Empty
        };
        for (int index = 0; index < 4; index++)
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        metrics.Controls.Add(CreateMetricCard(_productivity, "生產力指數", UiTheme.Primary), 0, 0);
        metrics.Controls.Add(CreateMetricCard(_consistency, "學習一致性", UiTheme.Success), 1, 0);
        metrics.Controls.Add(CreateMetricCard(_quality, "專注品質", UiTheme.Purple), 2, 0);
        metrics.Controls.Add(CreateMetricCard(_accuracy, "估時準確度", UiTheme.Warning), 3, 0);
        root.Controls.Add(metrics, 0, 1);

        var summaryCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderColor = UiTheme.BorderAccent,
            Padding = new Padding(18)
        };
        _summary.Dock = DockStyle.Fill;
        _summary.ForeColor = UiTheme.Navy;
        _summary.Font = UiTheme.Font(10.5f, FontStyle.Bold);
        _summary.TextAlign = ContentAlignment.MiddleLeft;
        _summary.AutoEllipsis = true;
        summaryCard.Controls.Add(_summary);
        root.Controls.Add(summaryCard, 0, 2);

        var activityCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(1)
        };
        var activityLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        activityLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        activityLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var activityTitle = new Label
        {
            Text = "稽核軌跡｜最近系統操作",
            Dock = DockStyle.Fill,
            Padding = new Padding(17, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(12, FontStyle.Bold)
        };
        _activityGrid.Dock = DockStyle.Fill;
        _activityGrid.Margin = Padding.Empty;
        UiTheme.StyleGrid(_activityGrid);
        _activityGrid.Columns.AddRange(
            UiTheme.TextColumn("Time", "時間", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells),
            UiTheme.TextColumn("Type", "類型", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells),
            UiTheme.TextColumn("Summary", "內容", 420));
        activityLayout.Controls.Add(activityTitle, 0, 0);
        activityLayout.Controls.Add(_activityGrid, 0, 1);
        activityCard.Controls.Add(activityLayout);
        root.Controls.Add(activityCard, 0, 3);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoScroll = true,
            Padding = new Padding(0, 8, 0, 0)
        };
        Button reportButton = UiTheme.PrimaryButton("輸出 HTML 週報");
        Button matrixButton = UiTheme.SecondaryButton("智慧四象限");
        Button diagnosticButton = UiTheme.SecondaryButton("資料健檢");
        Button calendarButton = UiTheme.SecondaryButton("匯出 iCalendar");
        Button closeButton = UiTheme.SecondaryButton("關閉");

        reportButton.Click += (_, _) => ExportWeeklyReport();
        matrixButton.Click += (_, _) =>
        {
            using var form = new SmartMatrixForm(_service);
            form.ShowDialog(this);
        };
        diagnosticButton.Click += (_, _) =>
        {
            using var form = new DiagnosticsForm(_service);
            form.ShowDialog(this);
        };
        calendarButton.Click += (_, _) => ExportCalendar();
        closeButton.Click += (_, _) => Close();

        buttons.Controls.Add(reportButton);
        buttons.Controls.Add(matrixButton);
        buttons.Controls.Add(diagnosticButton);
        buttons.Controls.Add(calendarButton);
        buttons.Controls.Add(closeButton);
        root.Controls.Add(buttons, 0, 4);

        contentSurface.Controls.Add(root);
        scrollHost.Controls.Add(contentSurface);
        Controls.Add(scrollHost);

        void ResizeSurface()
        {
            int availableWidth = Math.Max(
                0,
                scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            contentSurface.Width = Math.Max(contentMinimumWidth, availableWidth);
            contentSurface.Height = contentHeight;
        }

        scrollHost.Resize += (_, _) => ResizeSurface();
        scrollHost.MouseEnter += (_, _) => scrollHost.Focus();
        contentSurface.MouseEnter += (_, _) => scrollHost.Focus();
        _activityGrid.MouseEnter += (_, _) => scrollHost.Focus();
        ResizeSurface();
    }

    private static Control CreateMetricCard(Label valueLabel, string title, Color accent)
    {
        var wrapper = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 10, 0),
            Margin = Padding.Empty
        };

        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(0),
            Margin = Padding.Empty
        };

        var cardLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 22));
        cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var accentHost = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = new Padding(0)
        };
        accentHost.Controls.Add(new Panel
        {
            Width = 6,
            Height = 48,
            BackColor = accent,
            Anchor = AnchorStyles.Left,
            Location = new Point(0, 18)
        });

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(0, 10, 10, 8),
            BackColor = UiTheme.Surface
        };
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 64));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 36));

        valueLabel.Dock = DockStyle.Fill;
        valueLabel.Margin = Padding.Empty;
        valueLabel.Padding = Padding.Empty;
        valueLabel.AutoSize = false;
        valueLabel.AutoEllipsis = true;
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;
        valueLabel.ForeColor = UiTheme.Navy;
        valueLabel.Font = UiTheme.Font(20, FontStyle.Bold);
        valueLabel.UseCompatibleTextRendering = true;

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = new Padding(0, 1, 0, 0),
            AutoSize = false,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9.2f),
            UseCompatibleTextRendering = true
        };

        content.Controls.Add(valueLabel, 0, 0);
        content.Controls.Add(titleLabel, 0, 1);
        cardLayout.Controls.Add(accentHost, 0, 0);
        cardLayout.Controls.Add(content, 1, 0);
        card.Controls.Add(cardLayout);
        wrapper.Controls.Add(card);
        return wrapper;
    }

    private void RefreshData()
    {
        ProductivityMetrics metrics = ResearchMetricsService.Calculate(_service.Data);
        _productivity.Text = metrics.ProductivityIndex + "/100";
        _consistency.Text = metrics.ConsistencyScore + "%";
        _quality.Text = metrics.FocusQualityScore == 0 ? "待收集" : metrics.FocusQualityScore + "%";
        _accuracy.Text = metrics.EstimationAccuracy == 0 ? "待收集" : metrics.EstimationAccuracy + "%";
        _summary.Text = $"研究型摘要｜{metrics.Summary} 本週專注 {metrics.WeeklyFocusMinutes} 分鐘，深度工作 {metrics.DeepWorkMinutes} 分鐘。";

        List<ActivityRow> rows = _service.Data.Activities
            .OrderByDescending(item => item.OccurredAt)
            .Take(100)
            .Select(item => new ActivityRow
            {
                Time = item.OccurredAt.ToString("yyyy/MM/dd HH:mm:ss"),
                Type = ActivityTypeText(item.Type),
                Summary = item.Summary
            })
            .ToList();

        _activityGrid.DataSource = new BindingList<ActivityRow>(rows);
    }

    private void ExportWeeklyReport()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "HTML 報告 (*.html)|*.html",
            FileName = $"StudyFlow-WeeklyReport-{DateTime.Now:yyyyMMdd}.html"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            HtmlReportService.ExportWeeklyReport(dialog.FileName, _service.Data);
            _service.Log(ActivityType.Exported, "Report", null, $"輸出週報：{Path.GetFileName(dialog.FileName)}");
            _service.SaveAndNotify();

            DialogResult open = MessageBox.Show(
                "HTML 週報已完成。是否立即用瀏覽器開啟？",
                "輸出完成",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (open == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = dialog.FileName,
                    UseShellExecute = true
                });
            }
            RefreshData();
        }
        catch (Exception ex)
        {
            MessageBox.Show("週報輸出失敗：\n" + ex.Message, "錯誤",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportCalendar()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "iCalendar 檔案 (*.ics)|*.ics",
            FileName = $"StudyFlow-Tasks-{DateTime.Now:yyyyMMdd}.ics"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            IcsExportService.ExportTasks(dialog.FileName, _service.Data);
            _service.Log(ActivityType.Exported, "Calendar", null, $"輸出 iCalendar：{Path.GetFileName(dialog.FileName)}");
            _service.SaveAndNotify();
            MessageBox.Show("已輸出 iCalendar，可匯入 Google Calendar 或 Outlook。", "完成",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshData();
        }
        catch (Exception ex)
        {
            MessageBox.Show("iCalendar 匯出失敗：\n" + ex.Message, "錯誤",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string ActivityTypeText(ActivityType type) => type switch
    {
        ActivityType.Created => "新增",
        ActivityType.Updated => "修改",
        ActivityType.Completed => "完成",
        ActivityType.Reopened => "重開",
        ActivityType.Deleted => "刪除",
        ActivityType.Focused => "專注",
        ActivityType.Exported => "匯出",
        ActivityType.BackedUp => "備份",
        ActivityType.Restored => "還原",
        ActivityType.Imported => "匯入",
        ActivityType.Viewed => "閱讀",
        _ => "系統"
    };
}
