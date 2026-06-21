using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class ExamPaperEditorForm : Form
{
    private readonly DataService _dataService;
    private readonly ExamPaper _original;
    private readonly TextBox _titleBox = new();
    private readonly ComboBox _subjectCombo = new();
    private readonly TextBox _yearBox = new();
    private readonly ComboBox _termCombo = new();
    private readonly ComboBox _categoryCombo = new();
    private readonly TextBox _tagsBox = new();
    private readonly TextBox _notesBox = new();
    private readonly ComboBox _statusCombo = new();
    private readonly CheckBox _favoriteCheck = new();

    public ExamPaper ResultPaper { get; private set; }

    public ExamPaperEditorForm(DataService dataService, ExamPaper paper)
    {
        _dataService = dataService;
        _original = paper;
        ResultPaper = paper;

        Text = "編輯考古題資訊";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(820, 820);
        MinimumSize = new Size(760, 740);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildInterface();
        LoadPaper();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(26, 22, 26, 18),
            BackColor = UiTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        root.Controls.Add(UiTheme.StackedHeader(
            "編輯考古題資訊",
            _original.OriginalFileName,
            out _, 23,
            $"格式：{_original.FileExtension.TrimStart('.').ToUpperInvariant()}｜大小：{ExamLibraryService.FormatFileSize(_original.FileSizeBytes)}"), 0, 0);

        var scroll = new WheelScrollPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = UiTheme.Background,
            Padding = new Padding(0, 2, 0, 8)
        };

        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 6,
            BackColor = UiTheme.Background,
            Margin = new Padding(0)
        };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 156));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        _subjectCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _subjectCombo.DataSource = _dataService.Data.ExamSubjects
            .OrderBy(item => item.Name)
            .Select(item => new ExamSubjectChoice(item.Id, item.Name))
            .ToList();
        _termCombo.DropDownStyle = ComboBoxStyle.DropDown;
        _termCombo.Items.AddRange(new object[] { "", "上學期", "下學期", "暑期" });
        _categoryCombo.DropDownStyle = ComboBoxStyle.DropDown;
        _categoryCombo.Items.AddRange(new object[] { "考古題", "期中考", "期末考", "小考", "模擬考", "作業題" });
        _statusCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _statusCombo.DataSource = new[]
        {
            new ExamStatusChoice(ExamPaperStatus.NotStarted, "未開始"),
            new ExamStatusChoice(ExamPaperStatus.Reviewing, "複習中"),
            new ExamStatusChoice(ExamPaperStatus.Completed, "已完成")
        };
        _notesBox.Multiline = true;
        _notesBox.ScrollBars = ScrollBars.Vertical;
        _notesBox.AcceptsReturn = true;
        _favoriteCheck.Text = "收藏這份考古題";
        _favoriteCheck.Dock = DockStyle.Fill;
        _favoriteCheck.ForeColor = UiTheme.Slate;
        _favoriteCheck.Font = UiTheme.Font(10, FontStyle.Bold);
        _favoriteCheck.Margin = new Padding(2, 7, 8, 0);

        AddSpannedField(form, "標題 *", _titleBox, 0);
        form.Controls.Add(Field("科目", _subjectCombo), 0, 1);
        form.Controls.Add(Field("年份／學年度", _yearBox), 1, 1);
        form.Controls.Add(Field("學期", _termCombo), 0, 2);
        form.Controls.Add(Field("類型", _categoryCombo), 1, 2);
        form.Controls.Add(Field("複習狀態", _statusCombo), 0, 3);
        form.Controls.Add(Field("標籤（逗號分隔）", _tagsBox), 1, 3);
        AddSpannedField(form, "筆記與重點", _notesBox, 4);
        form.Controls.Add(_favoriteCheck, 0, 5);
        form.SetColumnSpan(_favoriteCheck, 2);

        scroll.Controls.Add(form);
        root.Controls.Add(scroll, 0, 1);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0),
            BackColor = UiTheme.Background
        };
        Button save = UiTheme.PrimaryButton("儲存");
        Button cancel = UiTheme.SecondaryButton("取消");
        save.AutoSize = false;
        cancel.AutoSize = false;
        save.Size = new Size(104, 40);
        cancel.Size = new Size(104, 40);
        save.Click += (_, _) => SavePaper();
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        root.Controls.Add(buttons, 0, 2);

        Controls.Add(root);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private static void AddSpannedField(TableLayoutPanel layout, string title, Control control, int row)
    {
        Control field = Field(title, control);
        layout.Controls.Add(field, 0, row);
        layout.SetColumnSpan(field, 2);
    }

    private static Control Field(string title, Control control)
    {
        var field = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Background,
            Padding = new Padding(0, 5, 12, 7),
            Margin = new Padding(0)
        };
        field.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        field.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var label = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9, FontStyle.Bold),
            Margin = new Padding(0)
        };

        control.Dock = DockStyle.Fill;
        control.Font = UiTheme.Font(10);
        control.Margin = new Padding(0, 2, 0, 3);

        field.Controls.Add(label, 0, 0);
        field.Controls.Add(control, 0, 1);
        return field;
    }

    private void LoadPaper()
    {
        _titleBox.Text = _original.Title;
        _yearBox.Text = _original.ExamYear;
        _termCombo.Text = _original.Term;
        _categoryCombo.Text = _original.Category;
        _tagsBox.Text = _original.Tags;
        _notesBox.Text = _original.Notes;
        _favoriteCheck.Checked = _original.IsFavorite;

        foreach (ExamSubjectChoice item in _subjectCombo.Items)
        {
            if (item.Id == _original.SubjectId)
            {
                _subjectCombo.SelectedItem = item;
                break;
            }
        }
        foreach (ExamStatusChoice item in _statusCombo.Items)
        {
            if (item.Status == _original.Status)
            {
                _statusCombo.SelectedItem = item;
                break;
            }
        }
        if (_subjectCombo.SelectedIndex < 0 && _subjectCombo.Items.Count > 0)
            _subjectCombo.SelectedIndex = 0;
        if (_statusCombo.SelectedIndex < 0 && _statusCombo.Items.Count > 0)
            _statusCombo.SelectedIndex = 0;
    }

    private void SavePaper()
    {
        string title = _titleBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("請輸入考古題標題。", "欄位檢查", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _titleBox.Focus();
            return;
        }
        if (_subjectCombo.SelectedItem is not ExamSubjectChoice subject)
        {
            MessageBox.Show("請選擇科目。", "欄位檢查", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ResultPaper = new ExamPaper
        {
            Id = _original.Id,
            SubjectId = subject.Id,
            Title = title,
            OriginalFileName = _original.OriginalFileName,
            StoredFileName = _original.StoredFileName,
            FileExtension = _original.FileExtension,
            FileSizeBytes = _original.FileSizeBytes,
            Sha256 = _original.Sha256,
            ExamYear = _yearBox.Text.Trim(),
            Term = _termCombo.Text.Trim(),
            Category = _categoryCombo.Text.Trim(),
            Tags = _tagsBox.Text.Trim(),
            Notes = _notesBox.Text.Trim(),
            IsFavorite = _favoriteCheck.Checked,
            Status = (_statusCombo.SelectedItem as ExamStatusChoice)?.Status ?? ExamPaperStatus.NotStarted,
            ImportedAt = _original.ImportedAt,
            UpdatedAt = DateTime.Now,
            LastOpenedAt = _original.LastOpenedAt,
            OpenCount = _original.OpenCount
        };
        DialogResult = DialogResult.OK;
    }

    private sealed record ExamSubjectChoice(Guid Id, string Text)
    {
        public override string ToString() => Text;
    }

    private sealed record ExamStatusChoice(ExamPaperStatus Status, string Text)
    {
        public override string ToString() => Text;
    }
}
