using StudyFlowPro.Models;

namespace StudyFlowPro.UI;

public sealed class CourseEditorForm : Form
{
    private readonly TextBox _nameBox = new();
    private readonly TextBox _teacherBox = new();
    private readonly TextBox _locationBox = new();
    private readonly Button _colorButton = UiTheme.SecondaryButton("選擇顏色");
    private readonly Course? _original;
    private Color _selectedColor = UiTheme.Primary;

    public Course ResultCourse { get; private set; } = new();

    public CourseEditorForm(Course? course = null)
    {
        _original = course;
        Text = course == null ? "新增課程 / 專案" : "編輯課程 / 專案";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(650, 620);
        MinimumSize = new Size(610, 570);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = true;
        ShowInTaskbar = false;
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        LoadCourse(course);
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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));

        root.Controls.Add(UiTheme.StackedHeader(
            Text,
            "建立課程分類，讓任務、進度與分析結果更容易管理。",
            out _,
            22), 0, 0);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

        ConfigureTextBox(_nameBox);
        ConfigureTextBox(_teacherBox);
        ConfigureTextBox(_locationBox);
        content.Controls.Add(CreateField("名稱 *", _nameBox), 0, 0);
        content.Controls.Add(CreateField("老師 / 負責人", _teacherBox), 0, 1);
        content.Controls.Add(CreateField("教室 / 地點", _locationBox), 0, 2);

        _colorButton.Dock = DockStyle.Fill;
        _colorButton.AutoSize = false;
        _colorButton.Height = 42;
        _colorButton.TextAlign = ContentAlignment.MiddleCenter;
        _colorButton.Click += (_, _) => ChooseColor();
        content.Controls.Add(CreateField("識別顏色", _colorButton), 0, 3);
        root.Controls.Add(content, 0, 1);

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
        saveButton.Click += (_, _) => SaveCourse();
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttonBar.Controls.Add(saveButton);
        buttonBar.Controls.Add(cancelButton);
        root.Controls.Add(buttonBar, 0, 2);

        Controls.Add(root);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static Panel CreateField(string labelText, Control control)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 0, 10),
            BackColor = UiTheme.Background,
            Margin = Padding.Empty
        };
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Top,
            Height = 28,
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

    private void ChooseColor()
    {
        using var dialog = new ColorDialog
        {
            Color = _selectedColor,
            FullOpen = true
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _selectedColor = dialog.Color;
            UpdateColorButton();
        }
    }

    private void LoadCourse(Course? course)
    {
        if (course != null)
        {
            _nameBox.Text = course.Name;
            _teacherBox.Text = course.Instructor;
            _locationBox.Text = course.Location;
            try
            {
                _selectedColor = ColorTranslator.FromHtml(course.ColorHex);
            }
            catch
            {
                _selectedColor = UiTheme.Primary;
            }
        }
        UpdateColorButton();
    }

    private void UpdateColorButton()
    {
        _colorButton.BackColor = _selectedColor;
        double luminance = .299 * _selectedColor.R + .587 * _selectedColor.G + .114 * _selectedColor.B;
        _colorButton.ForeColor = luminance > 160 ? Color.Black : Color.White;
        _colorButton.Text = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
    }

    private void SaveCourse()
    {
        if (string.IsNullOrWhiteSpace(_nameBox.Text))
        {
            MessageBox.Show("請輸入名稱。", "欄位檢查",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _nameBox.Focus();
            return;
        }

        ResultCourse = new Course
        {
            Id = _original?.Id ?? Guid.NewGuid(),
            Name = _nameBox.Text.Trim(),
            Instructor = _teacherBox.Text.Trim(),
            Location = _locationBox.Text.Trim(),
            ColorHex = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}"
        };
        DialogResult = DialogResult.OK;
    }
}
