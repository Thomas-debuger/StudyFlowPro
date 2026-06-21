using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class TaskEditorForm : Form
{
    private readonly TextBox _titleBox = new();
    private readonly TextBox _descriptionBox = new();
    private readonly TextBox _tagsBox = new();
    private readonly ComboBox _courseCombo = new();
    private readonly ComboBox _priorityCombo = new();
    private readonly DateTimePicker _duePicker = new();
    private readonly NumericUpDown _estimatedMinutes = new();
    private readonly NumericUpDown _difficulty = new();
    private readonly NumericUpDown _energy = new();
    private readonly CheckBox _pinCheckBox = new();
    private readonly StudyTask? _original;

    public StudyTask ResultTask { get; private set; } = new();

    public TaskEditorForm(DataService service, StudyTask? task = null)
    {
        _original = task;

        Text = task == null ? "新增任務" : "編輯任務";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(820, 900);
        MinimumSize = new Size(760, 820);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        BuildInterface(service);
        LoadTask(task);
    }

    private void BuildInterface(DataService service)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 22, 26, 20),
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));

        Control header = UiTheme.StackedHeader(
            Text,
            "設定課程、優先級、截止時間與工作量；所有欄位都會自動儲存。",
            out _,
            23);
        root.Controls.Add(header, 0, 0);

        var bodyHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = UiTheme.Background,
            Padding = new Padding(0, 0, 8, 0),
            Margin = Padding.Empty
        };

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 9,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 172));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));

        ConfigureTextBox(_titleBox);
        content.Controls.Add(CreateField("任務名稱 *", _titleBox), 0, 0);

        _courseCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _courseCombo.BeginUpdate();
        _courseCombo.Items.Add(new CourseChoice { Id = null, Text = "未分類" });
        foreach (Course course in service.Data.Courses.OrderBy(course => course.Name))
            _courseCombo.Items.Add(new CourseChoice { Id = course.Id, Text = course.Name });
        _courseCombo.EndUpdate();
        content.Controls.Add(CreateField("課程 / 專案", _courseCombo), 0, 1);

        _priorityCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _priorityCombo.BeginUpdate();
        _priorityCombo.Items.AddRange(new object[]
        {
            new PriorityChoice { Value = TaskPriority.Low, Text = "低" },
            new PriorityChoice { Value = TaskPriority.Medium, Text = "中" },
            new PriorityChoice { Value = TaskPriority.High, Text = "高" },
            new PriorityChoice { Value = TaskPriority.Urgent, Text = "緊急" }
        });
        _priorityCombo.EndUpdate();
        content.Controls.Add(CreateField("優先級", _priorityCombo), 0, 2);

        _duePicker.Format = DateTimePickerFormat.Custom;
        _duePicker.CustomFormat = "yyyy/MM/dd HH:mm";
        content.Controls.Add(CreateField("截止時間", _duePicker), 0, 3);

        _estimatedMinutes.Minimum = 5;
        _estimatedMinutes.Maximum = 5000;
        _estimatedMinutes.Increment = 5;
        content.Controls.Add(CreateField("預估時間（分鐘）", _estimatedMinutes), 0, 4);

        var factorPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        factorPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        factorPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        _difficulty.Minimum = 1;
        _difficulty.Maximum = 5;
        _energy.Minimum = 1;
        _energy.Maximum = 5;
        factorPanel.Controls.Add(CreateField("任務難度（1–5）", _difficulty, new Padding(0, 0, 8, 0)), 0, 0);
        factorPanel.Controls.Add(CreateField("精力需求（1–5）", _energy, new Padding(8, 0, 0, 0)), 1, 0);
        content.Controls.Add(factorPanel, 0, 5);

        _pinCheckBox.Text = "釘選為重要任務（智慧排序時優先顯示）";
        _pinCheckBox.Dock = DockStyle.Fill;
        _pinCheckBox.AutoSize = false;
        _pinCheckBox.Font = UiTheme.Font(9.5f, FontStyle.Bold);
        _pinCheckBox.ForeColor = UiTheme.Slate;
        _pinCheckBox.Margin = new Padding(2, 4, 0, 4);
        _pinCheckBox.TextAlign = ContentAlignment.MiddleLeft;
        content.Controls.Add(_pinCheckBox, 0, 6);

        _descriptionBox.Multiline = true;
        _descriptionBox.ScrollBars = ScrollBars.Vertical;
        _descriptionBox.AcceptsReturn = true;
        ConfigureTextBox(_descriptionBox);
        content.Controls.Add(CreateField("詳細說明", _descriptionBox), 0, 7);

        ConfigureTextBox(_tagsBox);
        content.Controls.Add(CreateField("標籤（用逗號分隔）", _tagsBox), 0, 8);

        bodyHost.Controls.Add(content);
        root.Controls.Add(bodyHost, 0, 1);

        var buttonBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = UiTheme.Background,
            Padding = new Padding(0, 12, 0, 0),
            Margin = Padding.Empty
        };
        Button saveButton = UiTheme.PrimaryButton("儲存");
        Button cancelButton = UiTheme.SecondaryButton("取消");
        saveButton.AutoSize = false;
        saveButton.Size = new Size(96, 40);
        cancelButton.AutoSize = false;
        cancelButton.Size = new Size(96, 40);
        saveButton.Click += (_, _) => SaveTask();
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttonBar.Controls.Add(saveButton);
        buttonBar.Controls.Add(cancelButton);
        root.Controls.Add(buttonBar, 0, 2);

        Controls.Add(root);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static Panel CreateField(string labelText, Control control, Padding? margin = null)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 0, 8),
            Margin = margin ?? Padding.Empty,
            BackColor = UiTheme.Background
        };
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Top,
            Height = 27,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        };
        control.Dock = DockStyle.Fill;
        control.Font = UiTheme.Font(10);
        control.Margin = Padding.Empty;
        panel.Controls.Add(control);
        panel.Controls.Add(label);
        return panel;
    }

    private static void ConfigureTextBox(TextBox box)
    {
        box.BorderStyle = BorderStyle.FixedSingle;
        box.Font = UiTheme.Font(10);
    }

    private void LoadTask(StudyTask? task)
    {
        if (task == null)
        {
            SelectCourse(null);
            SelectPriority(TaskPriority.Medium);
            _duePicker.Value = DateTime.Now.AddDays(1);
            _estimatedMinutes.Value = 60;
            _difficulty.Value = 3;
            _energy.Value = 3;
            return;
        }

        _titleBox.Text = task.Title;
        _descriptionBox.Text = task.Description;
        _tagsBox.Text = task.Tags;
        _duePicker.Value = ClampDate(task.DueDate, _duePicker.MinDate, _duePicker.MaxDate);
        _estimatedMinutes.Value = Math.Clamp(task.EstimatedMinutes, (int)_estimatedMinutes.Minimum, (int)_estimatedMinutes.Maximum);
        _difficulty.Value = Math.Clamp(task.Difficulty, 1, 5);
        _energy.Value = Math.Clamp(task.EnergyRequired, 1, 5);
        _pinCheckBox.Checked = task.IsPinned;
        SelectCourse(task.CourseId);
        SelectPriority(task.Priority);
    }

    private void SelectCourse(Guid? courseId)
    {
        foreach (object item in _courseCombo.Items)
        {
            if (item is CourseChoice choice && choice.Id == courseId)
            {
                _courseCombo.SelectedItem = choice;
                return;
            }
        }

        _courseCombo.SelectedIndex = _courseCombo.Items.Count > 0 ? 0 : -1;
    }

    private void SelectPriority(TaskPriority priority)
    {
        foreach (object item in _priorityCombo.Items)
        {
            if (item is PriorityChoice choice && choice.Value == priority)
            {
                _priorityCombo.SelectedItem = choice;
                return;
            }
        }

        _priorityCombo.SelectedIndex = _priorityCombo.Items.Count > 0 ? 0 : -1;
    }

    private static DateTime ClampDate(DateTime value, DateTime minimum, DateTime maximum)
    {
        if (value < minimum) return minimum;
        if (value > maximum) return maximum;
        return value;
    }

    private void SaveTask()
    {
        string title = _titleBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("請輸入任務名稱。", "欄位檢查",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _titleBox.Focus();
            return;
        }

        ResultTask = new StudyTask
        {
            Id = _original?.Id ?? Guid.NewGuid(),
            Title = title,
            Description = _descriptionBox.Text.Trim(),
            Tags = _tagsBox.Text.Trim(),
            CourseId = (_courseCombo.SelectedItem as CourseChoice)?.Id,
            Priority = (_priorityCombo.SelectedItem as PriorityChoice)?.Value ?? TaskPriority.Medium,
            DueDate = _duePicker.Value,
            EstimatedMinutes = (int)_estimatedMinutes.Value,
            FocusedMinutes = _original?.FocusedMinutes ?? 0,
            Difficulty = (int)_difficulty.Value,
            EnergyRequired = (int)_energy.Value,
            IsPinned = _pinCheckBox.Checked,
            IsCompleted = _original?.IsCompleted ?? false,
            CreatedAt = _original?.CreatedAt ?? DateTime.Now,
            UpdatedAt = DateTime.Now,
            CompletedAt = _original?.CompletedAt
        };

        DialogResult = DialogResult.OK;
    }
}
