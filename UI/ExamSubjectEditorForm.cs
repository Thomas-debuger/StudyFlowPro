using StudyFlowPro.Models;

namespace StudyFlowPro.UI;

public sealed class ExamSubjectEditorForm : Form
{
    private readonly ExamSubject? _original;
    private readonly TextBox _nameBox = new();
    private readonly TextBox _descriptionBox = new();
    private readonly Button _colorButton = UiTheme.SecondaryButton("選擇識別顏色");
    private Color _selectedColor = UiTheme.Primary;

    public ExamSubject ResultSubject { get; private set; } = new();

    public ExamSubjectEditorForm(ExamSubject? subject = null)
    {
        _original = subject;
        Text = subject == null ? "新增考古題科目" : "編輯考古題科目";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(700, 640);
        MinimumSize = new Size(660, 600);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildInterface();
        LoadSubject();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(28, 24, 28, 22),
            BackColor = UiTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 102));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        root.Controls.Add(UiTheme.StackedHeader(
            _original == null ? "新增考古題科目" : "編輯考古題科目",
            "建立獨立科目後，即可匯入 PDF 或 DOCX 考古題。",
            out _, 23), 0, 0);

        var formCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderColor = UiTheme.Border,
            Padding = new Padding(20, 16, 20, 16),
            Margin = new Padding(0, 2, 0, 6)
        };
        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        _nameBox.Font = UiTheme.Font(10);
        _descriptionBox.Multiline = true;
        _descriptionBox.ScrollBars = ScrollBars.Vertical;
        _descriptionBox.Font = UiTheme.Font(10);

        _colorButton.AutoSize = false;
        _colorButton.Dock = DockStyle.Fill;
        _colorButton.Height = 42;
        _colorButton.Margin = Padding.Empty;
        _colorButton.TextAlign = ContentAlignment.MiddleCenter;
        _colorButton.Click += (_, _) => SelectColor();

        form.Controls.Add(BuildField("科目名稱 *", _nameBox), 0, 0);
        form.Controls.Add(BuildField("科目說明", _descriptionBox), 0, 1);
        form.Controls.Add(BuildField("識別顏色", _colorButton), 0, 2);
        form.Controls.Add(new Label
        {
            Text = "提示：科目可用於分類不同課程、學期或考試類型。",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 3);
        formCard.Controls.Add(form);
        root.Controls.Add(formCard, 0, 1);

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
        save.Size = new Size(105, 40);
        cancel.Size = new Size(105, 40);
        save.Click += (_, _) => SaveSubject();
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        root.Controls.Add(buttons, 0, 2);

        Controls.Add(root);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private static TableLayoutPanel BuildField(string title, Control control)
    {
        var field = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(0, 4, 0, 6)
        };
        field.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        field.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        field.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 0);
        control.Dock = DockStyle.Fill;
        control.Margin = Padding.Empty;
        field.Controls.Add(control, 0, 1);
        return field;
    }

    private void LoadSubject()
    {
        if (_original != null)
        {
            _nameBox.Text = _original.Name;
            _descriptionBox.Text = _original.Description;
            try { _selectedColor = ColorTranslator.FromHtml(_original.ColorHex); }
            catch { _selectedColor = UiTheme.Primary; }
        }
        UpdateColorButton();
    }

    private void SelectColor()
    {
        using var dialog = new ColorDialog { Color = _selectedColor, FullOpen = true };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _selectedColor = dialog.Color;
            UpdateColorButton();
        }
    }

    private void UpdateColorButton()
    {
        _colorButton.BackColor = _selectedColor;
        _colorButton.ForeColor = (0.299 * _selectedColor.R + 0.587 * _selectedColor.G + 0.114 * _selectedColor.B) > 160
            ? Color.Black
            : Color.White;
        _colorButton.Text = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
    }

    private void SaveSubject()
    {
        string name = _nameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("請輸入科目名稱。", "欄位檢查", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _nameBox.Focus();
            return;
        }

        ResultSubject = new ExamSubject
        {
            Id = _original?.Id ?? Guid.NewGuid(),
            Name = name,
            Description = _descriptionBox.Text.Trim(),
            ColorHex = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}",
            CreatedAt = _original?.CreatedAt ?? DateTime.Now
        };
        DialogResult = DialogResult.OK;
    }
}
