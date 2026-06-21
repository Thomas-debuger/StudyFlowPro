using StudyFlowPro.Models;

namespace StudyFlowPro.UI;

public sealed class ExamSubjectPickerForm : Form
{
    private readonly ComboBox _combo = new();
    public Guid SelectedSubjectId { get; private set; }

    public ExamSubjectPickerForm(IEnumerable<ExamSubject> subjects)
    {
        Text = "選擇匯入科目";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(500, 270);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(24)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(UiTheme.StackedHeader("選擇科目", "匯入的 PDF／DOCX 將歸入此科目。", out _, 20), 0, 0);

        _combo.DropDownStyle = ComboBoxStyle.DropDownList;
        _combo.Dock = DockStyle.Fill;
        _combo.Font = UiTheme.Font(10);
        _combo.DataSource = subjects.OrderBy(item => item.Name)
            .Select(item => new Choice(item.Id, item.Name)).ToList();
        root.Controls.Add(_combo, 0, 1);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        Button ok = UiTheme.PrimaryButton("確定");
        Button cancel = UiTheme.SecondaryButton("取消");
        ok.AutoSize = false; ok.Size = new Size(100, 40);
        cancel.AutoSize = false; cancel.Size = new Size(100, 40);
        ok.Click += (_, _) =>
        {
            if (_combo.SelectedItem is Choice choice)
            {
                SelectedSubjectId = choice.Id;
                DialogResult = DialogResult.OK;
            }
        };
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        root.Controls.Add(buttons, 0, 2);
        Controls.Add(root);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private sealed record Choice(Guid Id, string Text)
    {
        public override string ToString() => Text;
    }
}
