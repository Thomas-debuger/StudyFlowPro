using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class PriorityInsightForm : Form
{
    public PriorityInsightForm(DataService service, StudyTask task)
    {
        PriorityAnalysis analysis = SmartPlanner.Analyze(task);
        string course = service.Data.Courses.FirstOrDefault(item => item.Id == task.CourseId)?.Name ?? "未分類";

        Text = "智慧優先分析";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(920, 820);
        MinimumSize = new Size(820, 700);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;
        ShowInTaskbar = false;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 4,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 172));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));

        root.Controls.Add(UiTheme.StackedHeader(
            task.Title,
            $"{course}｜截止 {task.DueDate:yyyy/MM/dd HH:mm}｜進度 {task.ProgressPercent}%",
            out _,
            21,
            "以下分數由規則式可解釋引擎產生，每一項加分與扣分原因都能追蹤。"), 0, 0);

        root.Controls.Add(BuildSummaryCard(analysis), 0, 1);

        var listPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 12, 8, 12),
            BackColor = UiTheme.Background,
            Margin = Padding.Empty
        };

        foreach (ScoreComponent component in analysis.Components)
            listPanel.Controls.Add(CreateComponentCard(component));

        void ResizeCards(object? _, EventArgs __)
        {
            int width = Math.Max(420, listPanel.ClientSize.Width - 30);
            foreach (Control control in listPanel.Controls)
                control.Width = width;
        }
        listPanel.ClientSizeChanged += ResizeCards;
        root.Controls.Add(listPanel, 0, 2);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = UiTheme.Background,
            Padding = new Padding(0, 12, 0, 0),
            Margin = Padding.Empty
        };
        Button closeButton = UiTheme.PrimaryButton("關閉");
        closeButton.AutoSize = false;
        closeButton.Size = new Size(96, 40);
        closeButton.Click += (_, _) => Close();
        buttons.Controls.Add(closeButton);
        root.Controls.Add(buttons, 0, 3);

        Controls.Add(root);
        AcceptButton = closeButton;
        Shown += (_, _) => ResizeCards(this, EventArgs.Empty);
    }

    private static Control BuildSummaryCard(PriorityAnalysis analysis)
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(18),
            Radius = 14
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        layout.Controls.Add(CreateSummaryValue($"{analysis.TotalScore}/100", "標準化智慧分數"), 0, 0);
        layout.Controls.Add(CreateSummaryValue(analysis.Level, "建議層級"), 1, 0);

        var recommendation = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Margin = new Padding(12, 0, 0, 0),
            Padding = new Padding(14, 8, 8, 8)
        };
        recommendation.Controls.Add(new Label
        {
            Text = analysis.Recommendation,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(10, FontStyle.Bold),
            AutoEllipsis = true,
            Margin = Padding.Empty
        });
        layout.Controls.Add(recommendation, 2, 0);

        card.Controls.Add(layout);
        return card;
    }

    private static Control CreateSummaryValue(string value, string label)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(4)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 68));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
        layout.Controls.Add(new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomCenter,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(20, FontStyle.Bold),
            AutoEllipsis = true,
            Margin = Padding.Empty
        }, 0, 0);
        layout.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.7f),
            Margin = Padding.Empty
        }, 0, 1);
        return layout;
    }

    private static Control CreateComponentCard(ScoreComponent component)
    {
        var card = new RoundedPanel
        {
            Width = 760,
            Height = 98,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(14),
            BackColor = UiTheme.Surface,
            Radius = 14
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 168));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label
        {
            Text = component.Name,
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(9.5f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 0);

        int maximum = Math.Max(1, component.Maximum);
        int positive = Math.Max(0, component.Score);
        layout.Controls.Add(new ProgressBar
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = maximum,
            Value = Math.Min(maximum, positive),
            Margin = new Padding(6, 8, 6, 7)
        }, 1, 0);

        layout.Controls.Add(new Label
        {
            Text = component.Score >= 0 ? $"+{component.Score}" : component.Score.ToString(),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = component.Score < 0 ? UiTheme.Success : UiTheme.Primary,
            Font = UiTheme.Font(11, FontStyle.Bold),
            Margin = Padding.Empty
        }, 2, 0);

        var explanation = new Label
        {
            Text = component.Explanation,
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.8f),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        };
        layout.Controls.Add(explanation, 0, 1);
        layout.SetColumnSpan(explanation, 3);

        card.Controls.Add(layout);
        return card;
    }
}
