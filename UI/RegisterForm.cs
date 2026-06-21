using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class RegisterForm : Form
{
    private readonly AccountService _accountService;
    private readonly TextBox _displayNameBox = new();
    private readonly TextBox _usernameBox = new();
    private readonly TextBox _passwordBox = new();
    private readonly TextBox _confirmPasswordBox = new();
    private readonly Label _errorLabel = new();

    public UserAccount? RegisteredAccount { get; private set; }

    public RegisterForm(AccountService accountService)
    {
        _accountService = accountService;

        Text = "StudyFlow Pro｜註冊";
        StartPosition = FormStartPosition.CenterParent;
        // 使用 ClientSize 鎖定「內容區」大小，避免 Windows 標題列與 DPI 縮放
        // 吃掉底部空間，造成註冊按鈕被視窗邊界裁切。
        ClientSize = new Size(590, 760);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        Shown += (_, _) => _displayNameBox.Focus();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(38, 28, 38, 26),
            BackColor = UiTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));

        root.Controls.Add(UiTheme.StackedHeader(
            "建立個人學習帳號",
            "註冊完成後會直接登入，並建立獨立的個人資料空間",
            out _,
            21), 0, 0);

        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(28),
            Margin = new Padding(0, 4, 0, 6)
        };
        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _displayNameBox.PlaceholderText = "例如：Howard";
        _usernameBox.PlaceholderText = "4～30 個英文、數字、._-";
        _passwordBox.PlaceholderText = "至少 6 個字元";
        _confirmPasswordBox.PlaceholderText = "請再次輸入密碼";
        _passwordBox.UseSystemPasswordChar = true;
        _confirmPasswordBox.UseSystemPasswordChar = true;

        fields.Controls.Add(CreateField("顯示名稱", _displayNameBox), 0, 0);
        fields.Controls.Add(CreateField("登入帳號", _usernameBox), 0, 1);
        fields.Controls.Add(CreateField("密碼", _passwordBox), 0, 2);
        fields.Controls.Add(CreateField("確認密碼", _confirmPasswordBox), 0, 3);

        var showPassword = new CheckBox
        {
            Text = "顯示密碼",
            AutoSize = true,
            Margin = new Padding(2, 6, 0, 0),
            ForeColor = UiTheme.TextSecondary,
            Font = UiTheme.Font(9)
        };
        showPassword.CheckedChanged += (_, _) =>
        {
            bool hide = !showPassword.Checked;
            _passwordBox.UseSystemPasswordChar = hide;
            _confirmPasswordBox.UseSystemPasswordChar = hide;
        };
        fields.Controls.Add(showPassword, 0, 4);

        _errorLabel.Dock = DockStyle.Fill;
        _errorLabel.ForeColor = UiTheme.Danger;
        _errorLabel.Font = UiTheme.Font(9, FontStyle.Bold);
        _errorLabel.TextAlign = ContentAlignment.MiddleLeft;
        fields.Controls.Add(_errorLabel, 0, 5);

        fields.Controls.Add(new Label
        {
            Text = "安全說明：程式不會儲存明碼密碼，而是使用隨機鹽值與 PBKDF2-SHA256 雜湊。帳號與學習資料僅保存在本機。",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.6f),
            TextAlign = ContentAlignment.TopLeft
        }, 0, 6);

        card.Controls.Add(fields);
        root.Controls.Add(card, 0, 1);

        var buttons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = new Padding(0, 8, 0, 4),
            BackColor = UiTheme.Background
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 188));

        Button cancelButton = UiTheme.SecondaryButton("返回登入");
        Button registerButton = UiTheme.PrimaryButton("建立帳號並登入");
        // UiTheme 的按鈕預設 AutoSize=true；在固定高度列中會受 DPI 影響。
        // 這裡改成固定填滿，並保留足夠高度，文字不再換行或被下緣裁掉。
        cancelButton.AutoSize = false;
        registerButton.AutoSize = false;
        cancelButton.Dock = DockStyle.Fill;
        registerButton.Dock = DockStyle.Fill;
        cancelButton.MinimumSize = new Size(0, 46);
        registerButton.MinimumSize = new Size(0, 46);
        cancelButton.Padding = new Padding(8, 0, 8, 0);
        registerButton.Padding = new Padding(8, 0, 8, 0);
        cancelButton.Margin = new Padding(0, 0, 8, 0);
        registerButton.Margin = Padding.Empty;
        cancelButton.Click += (_, _) => Close();
        registerButton.Click += (_, _) => Register();
        buttons.Controls.Add(cancelButton, 1, 0);
        buttons.Controls.Add(registerButton, 2, 0);
        root.Controls.Add(buttons, 0, 2);

        Controls.Add(root);
        AcceptButton = registerButton;
        CancelButton = cancelButton;
    }

    private void Register()
    {
        _errorLabel.Text = string.Empty;
        UserAccount? account;
        try
        {
            account = _accountService.Register(
                _displayNameBox.Text,
                _usernameBox.Text,
                _passwordBox.Text,
                _confirmPasswordBox.Text,
                out string error);

            if (account == null)
            {
                _errorLabel.Text = error;
                return;
            }
        }
        catch (Exception exception)
        {
            _errorLabel.Text = "無法建立帳號：" + exception.Message;
            return;
        }

        RegisteredAccount = account;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static Panel CreateField(string labelText, TextBox textBox)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        textBox.Dock = DockStyle.Bottom;
        textBox.Height = 34;
        textBox.Font = UiTheme.Font(10);
        panel.Controls.Add(textBox);
        panel.Controls.Add(new Label
        {
            Text = labelText,
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = UiTheme.TextSecondary,
            Font = UiTheme.Font(9, FontStyle.Bold),
            TextAlign = ContentAlignment.BottomLeft
        });
        return panel;
    }
}
