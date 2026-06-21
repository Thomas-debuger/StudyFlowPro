namespace StudyFlowPro.UI;

public sealed class FocusReviewForm : Form
{
    private readonly NumericUpDown _quality = new();
    private readonly NumericUpDown _distractions = new();
    private readonly TextBox _note = new();

    public int FocusQuality => (int)_quality.Value;
    public int DistractionCount => (int)_distractions.Value;
    public string SessionNote => _note.Text.Trim();

    public FocusReviewForm(int durationMinutes, string taskName, string defaultNote)
    {
        Text = "專注回顧";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(600, 540);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);

        BuildInterface(durationMinutes, taskName, defaultNote);
    }

    private void BuildInterface(int durationMinutes, string taskName, string defaultNote)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 7
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        Control header = UiTheme.StackedHeader(
            "完成專注，做一分鐘回顧",
            $"{taskName}｜本次 {durationMinutes} 分鐘",
            out _,
            20);
        root.Controls.Add(header, 0, 0);

        var hint = new Label
        {
            Text = "這些自評資料會用於計算專注品質與每週報告，不會上傳到網路。",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9.5f),
            TextAlign = ContentAlignment.MiddleLeft
        };
        root.Controls.Add(hint, 0, 1);

        _quality.Minimum = 1;
        _quality.Maximum = 5;
        _quality.Value = 4;
        root.Controls.Add(CreateField("專注品質（1 很差，5 非常好）", _quality), 0, 2);

        _distractions.Minimum = 0;
        _distractions.Maximum = 99;
        root.Controls.Add(CreateField("分心 / 被打斷次數", _distractions), 0, 3);

        _note.Multiline = true;
        _note.ScrollBars = ScrollBars.Vertical;
        _note.Text = defaultNote;
        root.Controls.Add(CreateField("本次完成內容或反思", _note), 0, 4);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        Button saveButton = UiTheme.PrimaryButton("儲存紀錄");
        Button skipButton = UiTheme.SecondaryButton("略過評分");
        saveButton.Click += (_, _) => DialogResult = DialogResult.OK;
        skipButton.Click += (_, _) =>
        {
            _quality.Value = 1;
            _distractions.Value = 0;
            _note.Clear();
            DialogResult = DialogResult.Ignore;
        };
        buttons.Controls.Add(saveButton);
        buttons.Controls.Add(skipButton);
        root.Controls.Add(buttons, 0, 6);

        Controls.Add(root);
        AcceptButton = saveButton;
    }

    private static Panel CreateField(string labelText, Control control)
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        control.Dock = DockStyle.Fill;
        control.Font = UiTheme.Font(10);
        panel.Controls.Add(control);
        panel.Controls.Add(new Label
        {
            Text = labelText,
            Dock = DockStyle.Top,
            Height = 26,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9, FontStyle.Bold)
        });
        return panel;
    }
}
