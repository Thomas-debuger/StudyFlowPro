using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class SmartMatrixForm : Form
{
    private readonly DataService _service;
    private readonly Dictionary<string, ListBox> _lists = new();

    public SmartMatrixForm(DataService service)
    {
        _service = service;

        Text = "智慧四象限矩陣";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1180, 780);
        MinimumSize = new Size(980, 680);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        RefreshMatrix();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 124));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        Control header = UiTheme.StackedHeader(
            "智慧四象限矩陣",
            "依重要度與截止時間自動分類；雙擊任務可查看智慧分數原因",
            out _,
            23,
            "重要：高 / 緊急優先級、釘選或高難度｜緊急：逾期或 72 小時內到期");
        root.Controls.Add(header, 0, 0);

        var matrix = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        matrix.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        matrix.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        matrix.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        matrix.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        matrix.Controls.Add(CreateQuadrant("importantUrgent", "① 重要且緊急｜立即執行", "逾期、三天內到期或高風險任務", UiTheme.Danger), 0, 0);
        matrix.Controls.Add(CreateQuadrant("importantNotUrgent", "② 重要但不緊急｜排入行程", "高價值任務，避免拖到變成危機", UiTheme.Primary), 1, 0);
        matrix.Controls.Add(CreateQuadrant("notImportantUrgent", "③ 不重要但緊急｜快速處理", "期限近但影響較小，可批次處理", UiTheme.Warning), 0, 1);
        matrix.Controls.Add(CreateQuadrant("notImportantNotUrgent", "④ 不重要且不緊急｜延後或刪除", "低價值工作，定期重新評估", UiTheme.Success), 1, 1);
        root.Controls.Add(matrix, 0, 1);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        Button closeButton = UiTheme.PrimaryButton("關閉");
        Button refreshButton = UiTheme.SecondaryButton("重新整理");
        closeButton.Click += (_, _) => Close();
        refreshButton.Click += (_, _) => RefreshMatrix();
        bottom.Controls.Add(closeButton);
        bottom.Controls.Add(refreshButton);
        root.Controls.Add(bottom, 0, 2);

        Controls.Add(root);
    }

    private Control CreateQuadrant(string key, string title, string subtitle, Color accent)
    {
        var wrapper = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(6)
        };
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(14)
        };
        var header = new Panel { Dock = DockStyle.Top, Height = 58 };
        header.Controls.Add(new Panel
        {
            Dock = DockStyle.Left,
            Width = 6,
            BackColor = accent
        });
        header.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 30,
            Padding = new Padding(12, 2, 0, 0),
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(11, FontStyle.Bold)
        });
        header.Controls.Add(new Label
        {
            Text = subtitle,
            Dock = DockStyle.Bottom,
            Height = 24,
            Padding = new Padding(12, 0, 0, 0),
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.5f)
        });

        var list = new ListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = UiTheme.Font(9.5f),
            ForeColor = UiTheme.Slate,
            IntegralHeight = false,
            HorizontalScrollbar = true
        };
        list.DoubleClick += (_, _) => ShowSelectedInsight(list);
        _lists[key] = list;

        card.Controls.Add(list);
        card.Controls.Add(header);
        wrapper.Controls.Add(card);
        return wrapper;
    }

    private void RefreshMatrix()
    {
        foreach (ListBox list in _lists.Values)
            list.Items.Clear();

        foreach (StudyTask task in SmartPlanner.RankTasks(_service.Data.Tasks))
        {
            bool important = task.IsPinned ||
                             task.Priority is TaskPriority.High or TaskPriority.Urgent ||
                             task.Difficulty >= 4;
            bool urgent = task.DueDate <= DateTime.Now.AddHours(72);

            string key = important
                ? urgent ? "importantUrgent" : "importantNotUrgent"
                : urgent ? "notImportantUrgent" : "notImportantNotUrgent";

            _lists[key].Items.Add(new MatrixTaskItem(task));
        }

        foreach (ListBox list in _lists.Values)
        {
            if (list.Items.Count == 0)
                list.Items.Add("目前沒有任務");
        }
    }

    private void ShowSelectedInsight(ListBox list)
    {
        if (list.SelectedItem is not MatrixTaskItem item)
            return;

        using var form = new PriorityInsightForm(_service, item.Task);
        form.ShowDialog(this);
    }

    private sealed class MatrixTaskItem
    {
        public StudyTask Task { get; }

        public MatrixTaskItem(StudyTask task)
        {
            Task = task;
        }

        public override string ToString()
        {
            PriorityAnalysis analysis = SmartPlanner.Analyze(Task);
            string pin = Task.IsPinned ? "★ " : string.Empty;
            return $"{pin}{Task.Title}｜{Task.DueDate:MM/dd HH:mm}｜{Task.ProgressPercent}%｜智慧分數 {analysis.TotalScore}/100";
        }
    }
}
