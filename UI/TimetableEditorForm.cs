using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class TimetableEditorForm : Form
{
    private readonly DataService _service;
    private readonly TimetableEntry? _original;

    private readonly ComboBox _semesterCombo = new();
    private readonly TextBox _courseBox = new();
    private readonly ComboBox _dayCombo = new();
    private readonly ComboBox _startCombo = new();
    private readonly ComboBox _endCombo = new();
    private readonly TextBox _locationBox = new();
    private readonly TextBox _instructorBox = new();
    private readonly Button _colorButton = UiTheme.SecondaryButton("選擇課程顏色");
    private readonly TextBox _notesBox = new();
    private Color _selectedColor = UiTheme.Primary;

    public TimetableEntry ResultEntry { get; private set; } = new();

    public TimetableEditorForm(
        DataService service,
        TimetableEntry? entry = null,
        string semesterCode = "114-2",
        int dayIndex = 1,
        int startPeriod = 1)
    {
        _service = service;
        _original = entry;

        Text = entry == null ? "新增課表課程" : "編輯課表課程";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 760);
        MinimumSize = new Size(720, 700);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = true;
        ShowInTaskbar = false;
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        LoadEntry(entry, semesterCode, dayIndex, startPeriod);
    }

    private void BuildInterface()
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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));

        root.Controls.Add(UiTheme.StackedHeader(
            Text,
            "設定學期、星期、節次與顏色；系統會自動檢查同時段衝突。",
            out _,
            22), 0, 0);

        ConfigureInputs();
        root.Controls.Add(BuildForm(), 0, 1);
        root.Controls.Add(BuildButtons(), 0, 2);
        Controls.Add(root);
    }

    private void ConfigureInputs()
    {
        _semesterCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        foreach (TimetableSemester semester in _service.Data.TimetableSemesters)
            _semesterCombo.Items.Add(new SemesterOption(semester.Code, semester.DisplayName));
        _semesterCombo.DisplayMember = nameof(SemesterOption.Display);

        _dayCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        for (int day = 1; day <= 6; day++)
            _dayCombo.Items.Add(TimetableCatalog.DayNames[day]);

        _startCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _endCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        for (int period = 1; period <= 10; period++)
        {
            string display = TimetableCatalog.PeriodDisplay(period);
            _startCombo.Items.Add(display);
            _endCombo.Items.Add(display);
        }
        _startCombo.SelectedIndexChanged += (_, _) =>
        {
            if (_startCombo.SelectedIndex >= 0 && _endCombo.SelectedIndex < _startCombo.SelectedIndex)
                _endCombo.SelectedIndex = _startCombo.SelectedIndex;
        };

        foreach (TextBox box in new[] { _courseBox, _locationBox, _instructorBox, _notesBox })
        {
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Font = UiTheme.Font(10);
        }
        _notesBox.Multiline = true;
        _notesBox.ScrollBars = ScrollBars.Vertical;

        _colorButton.AutoSize = false;
        _colorButton.Dock = DockStyle.Fill;
        _colorButton.TextAlign = ContentAlignment.MiddleCenter;
        _colorButton.Click += (_, _) => ChooseColor();
    }

    private Control BuildForm()
    {
        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Background
        };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        form.Controls.Add(CreateField("課程名稱 *", _courseBox), 0, 0);
        form.Controls.Add(CreateTwoFields(
            "學期 *", _semesterCombo,
            "星期 *", _dayCombo), 0, 1);
        form.Controls.Add(CreateTwoFields(
            "開始節次 *", _startCombo,
            "結束節次 *", _endCombo), 0, 2);
        form.Controls.Add(CreateTwoFields(
            "教室 / 地點", _locationBox,
            "老師 / 授課者", _instructorBox), 0, 3);
        form.Controls.Add(CreateField("課程顏色", _colorButton), 0, 4);
        form.Controls.Add(CreateField("備註", _notesBox), 0, 5);
        return form;
    }

    private Control BuildButtons()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 0),
            BackColor = UiTheme.Background
        };

        Button save = UiTheme.PrimaryButton("儲存");
        Button cancel = UiTheme.SecondaryButton("取消");
        save.AutoSize = false;
        cancel.AutoSize = false;
        save.Size = new Size(104, 40);
        cancel.Size = new Size(104, 40);
        save.TextAlign = ContentAlignment.MiddleCenter;
        cancel.TextAlign = ContentAlignment.MiddleCenter;
        save.Click += (_, _) => SaveEntry();
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        bar.Controls.Add(save);
        bar.Controls.Add(cancel);

        AcceptButton = save;
        CancelButton = cancel;
        return bar;
    }

    private static Control CreateTwoFields(
        string leftLabel,
        Control leftControl,
        string rightLabel,
        Control rightControl)
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Background
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        Control left = CreateField(leftLabel, leftControl);
        Control right = CreateField(rightLabel, rightControl);
        left.Margin = new Padding(0, 0, 8, 0);
        right.Margin = new Padding(8, 0, 0, 0);
        row.Controls.Add(left, 0, 0);
        row.Controls.Add(right, 1, 0);
        return row;
    }

    private static Panel CreateField(string labelText, Control control)
    {
        var field = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty,
            Padding = new Padding(0, 0, 0, 10)
        };
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Top,
            Height = 27,
            AutoSize = false,
            Margin = Padding.Empty,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        control.Dock = DockStyle.Fill;
        control.Margin = Padding.Empty;
        control.Font = UiTheme.Font(10);
        field.Controls.Add(control);
        field.Controls.Add(label);
        return field;
    }

    private void LoadEntry(TimetableEntry? entry, string semesterCode, int dayIndex, int startPeriod)
    {
        string semester = entry?.SemesterCode ?? semesterCode;
        int semesterIndex = _semesterCombo.Items
            .Cast<SemesterOption>()
            .Select((item, index) => new { item.Code, Index = index })
            .FirstOrDefault(item => string.Equals(item.Code, semester, StringComparison.OrdinalIgnoreCase))?.Index ?? 0;
        if (_semesterCombo.Items.Count > 0)
            _semesterCombo.SelectedIndex = semesterIndex;
        _dayCombo.SelectedIndex = Math.Clamp(entry?.DayIndex ?? dayIndex, 1, 6) - 1;
        _startCombo.SelectedIndex = Math.Clamp(entry?.StartPeriod ?? startPeriod, 1, 10) - 1;
        _endCombo.SelectedIndex = Math.Clamp(entry?.EndPeriod ?? startPeriod, 1, 10) - 1;

        if (entry != null)
        {
            _courseBox.Text = entry.CourseName;
            _locationBox.Text = entry.Location;
            _instructorBox.Text = entry.Instructor;
            _notesBox.Text = entry.Notes;
            _selectedColor = ParseColor(entry.ColorHex, UiTheme.Primary);
        }
        else
        {
            _selectedColor = SuggestColor(semester);
        }

        UpdateColorButton();
        Shown += (_, _) => _courseBox.Focus();
    }

    private Color SuggestColor(string semester)
    {
        string[] palette =
        {
            "#2563EB", "#7C3AED", "#059669", "#D97706", "#DB2777",
            "#0891B2", "#EA580C", "#0F766E", "#DC2626", "#4F46E5"
        };
        int distinctCourses = _service.Data.TimetableEntries
            .Where(item => item.SemesterCode == semester)
            .Select(item => item.CourseName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        return ParseColor(palette[distinctCourses % palette.Length], UiTheme.Primary);
    }

    private void ChooseColor()
    {
        using var dialog = new ColorDialog
        {
            Color = _selectedColor,
            FullOpen = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _selectedColor = dialog.Color;
        UpdateColorButton();
    }

    private void UpdateColorButton()
    {
        _colorButton.BackColor = _selectedColor;
        double luminance = .299 * _selectedColor.R + .587 * _selectedColor.G + .114 * _selectedColor.B;
        _colorButton.ForeColor = luminance > 160 ? Color.Black : Color.White;
        _colorButton.Text = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}　點擊選擇顏色";
    }

    private void SaveEntry()
    {
        if (string.IsNullOrWhiteSpace(_courseBox.Text))
        {
            MessageBox.Show("請輸入課程名稱。", "欄位檢查",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _courseBox.Focus();
            return;
        }

        int start = _startCombo.SelectedIndex + 1;
        int end = _endCombo.SelectedIndex + 1;
        if (start <= 0 || end < start)
        {
            MessageBox.Show("結束節次不可早於開始節次。", "欄位檢查",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string semester = ((SemesterOption)_semesterCombo.SelectedItem!).Code;
        int day = _dayCombo.SelectedIndex + 1;
        TimetableEntry? conflict = _service.Data.TimetableEntries.FirstOrDefault(item =>
            item.Id != (_original?.Id ?? Guid.Empty) &&
            item.SemesterCode == semester &&
            item.DayIndex == day &&
            start <= item.EndPeriod &&
            end >= item.StartPeriod);

        if (conflict != null)
        {
            MessageBox.Show(
                $"此時段與「{conflict.CourseName}」衝突。\n" +
                $"{TimetableCatalog.DayNames[day]}　{TimetableCatalog.PeriodRangeDisplay(conflict.StartPeriod, conflict.EndPeriod)}",
                "課表衝突",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        ResultEntry = new TimetableEntry
        {
            Id = _original?.Id ?? Guid.NewGuid(),
            SemesterCode = semester,
            CourseName = _courseBox.Text.Trim(),
            DayIndex = day,
            StartPeriod = start,
            EndPeriod = end,
            Location = _locationBox.Text.Trim(),
            Instructor = _instructorBox.Text.Trim(),
            ColorHex = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}",
            Notes = _notesBox.Text.Trim(),
            CreatedAt = _original?.CreatedAt ?? DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        DialogResult = DialogResult.OK;
    }

    private static Color ParseColor(string? value, Color fallback)
    {
        try
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : ColorTranslator.FromHtml(value);
        }
        catch
        {
            return fallback;
        }
    }

    private sealed record SemesterOption(string Code, string Display)
    {
        public override string ToString() => Display;
    }
}
