using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class TimetableSemesterEditorForm : Form
{
    private readonly TextBox _nameBox = new();
    private readonly HashSet<string> _existingNames;
    private readonly HashSet<string> _existingCodes;

    public TimetableSemester ResultSemester { get; private set; } = new();

    public TimetableSemesterEditorForm(IEnumerable<TimetableSemester> existingSemesters)
    {
        _existingNames = existingSemesters
            .Select(item => item.DisplayName.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _existingCodes = existingSemesters
            .Select(item => item.Code.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Text = "新增學期課表";
        StartPosition = FormStartPosition.CenterParent;
        // 使用 ClientSize 固定內容區尺寸，避免標題列與 DPI 縮放吃掉可用高度。
        ClientSize = new Size(620, 430);
        MinimumSize = new Size(620, 430);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = true;
        ShowInTaskbar = false;
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        Shown += (_, _) => _nameBox.Focus();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(28, 22, 28, 20),
            Margin = Padding.Empty,
            BackColor = UiTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        // 提示區固定高度，避免高 DPI 下被壓縮成只剩一行。
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));

        root.Controls.Add(UiTheme.StackedHeader(
            "新增學期課表",
            "輸入學期名稱後即可建立空白課表，也能接著匯入既有 CSV。",
            out _,
            22), 0, 0);

        var field = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(0, 8, 0, 8),
            BackColor = UiTheme.Background
        };
        field.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        field.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        field.Controls.Add(new Label
        {
            Text = "學期課表名稱 *",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9.5f, FontStyle.Bold)
        }, 0, 0);
        _nameBox.Dock = DockStyle.Fill;
        _nameBox.BorderStyle = BorderStyle.FixedSingle;
        _nameBox.Font = UiTheme.Font(10.5f);
        field.Controls.Add(_nameBox, 0, 1);
        root.Controls.Add(field, 0, 1);

        var hint = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.PrimarySoft,
            Padding = new Padding(16, 14, 16, 14),
            Margin = new Padding(0, 6, 0, 6)
        };
        hint.Controls.Add(new Label
        {
            Text = "命名範例：115 年第 1 學期、研究所上學期、暑期加強班。\n建立後系統會自動保存，並詢問是否立即匯入 CSV 課表。",
            Dock = DockStyle.Fill,
            AutoSize = false,
            Padding = new Padding(0, 2, 0, 0),
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = UiTheme.PrimaryDark,
            Font = UiTheme.Font(9.2f)
        });
        root.Controls.Add(hint, 0, 2);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(0, 10, 0, 0),
            BackColor = UiTheme.Background
        };
        Button save = UiTheme.PrimaryButton("建立學期");
        Button cancel = UiTheme.SecondaryButton("取消");
        foreach (Button button in new[] { save, cancel })
        {
            button.AutoSize = false;
            button.Size = new Size(116, 40);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Margin = new Padding(8, 0, 0, 0);
        }
        save.Click += (_, _) => SaveSemester();
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        root.Controls.Add(buttons, 0, 3);

        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(root);
    }

    private void SaveSemester()
    {
        string name = _nameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("請輸入學期課表名稱。", "資料不完整",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _nameBox.Focus();
            return;
        }

        if (_existingNames.Contains(name))
        {
            MessageBox.Show("已經存在相同名稱的學期課表。", "名稱重複",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _nameBox.SelectAll();
            _nameBox.Focus();
            return;
        }

        ResultSemester = new TimetableSemester
        {
            Code = TimetableCatalog.CreateSemesterCode(name, _existingCodes),
            DisplayName = name,
            IsBuiltIn = false,
            CreatedAt = DateTime.Now
        };
        DialogResult = DialogResult.OK;
    }
}
