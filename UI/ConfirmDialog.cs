namespace StudyFlowPro.UI;

public sealed class ConfirmDialog : Form
{
    private ConfirmDialog(string title, string message, string confirmText)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(540, 270);
        MinimumSize = new Size(500, 250);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = true;
        ShowIcon = false;
        ShowInTaskbar = false;
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 20, 24, 18),
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        root.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(16, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        root.Controls.Add(new Label
        {
            Text = message,
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(10),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 4, 0, 4)
        }, 0, 1);

        var buttons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = new Padding(0, 10, 0, 0),
            BackColor = UiTheme.Background
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 174));
        buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Button confirm = UiTheme.DangerButton(confirmText);
        Button cancel = UiTheme.SecondaryButton("取消");

        // 固定尺寸與零內距，確保中文字完整顯示且垂直置中。
        confirm.AutoSize = false;
        confirm.Dock = DockStyle.Fill;
        confirm.Margin = new Padding(8, 0, 0, 0);
        confirm.Padding = Padding.Empty;
        confirm.TextAlign = ContentAlignment.MiddleCenter;
        confirm.UseCompatibleTextRendering = false;
        confirm.AutoEllipsis = false;

        cancel.AutoSize = false;
        cancel.Dock = DockStyle.Fill;
        cancel.Margin = Padding.Empty;
        cancel.Padding = Padding.Empty;
        cancel.TextAlign = ContentAlignment.MiddleCenter;
        cancel.UseCompatibleTextRendering = false;
        cancel.AutoEllipsis = false;

        confirm.DialogResult = DialogResult.OK;
        cancel.DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(cancel, 1, 0);
        buttons.Controls.Add(confirm, 2, 0);
        root.Controls.Add(buttons, 0, 2);

        Controls.Add(root);
        AcceptButton = confirm;
        CancelButton = cancel;

        FormClosing += (_, _) =>
        {
            if (DialogResult == DialogResult.None)
                DialogResult = DialogResult.Cancel;
        };
    }

    public static bool Ask(IWin32Window owner, string title, string message, string confirmText = "刪除")
    {
        using var dialog = new ConfirmDialog(title, message, confirmText);
        return dialog.ShowDialog(owner) == DialogResult.OK;
    }
}
