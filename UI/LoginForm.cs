using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class LoginForm : Form
{
    private readonly AccountService _accountService;
    private readonly TextBox _usernameBox = new();
    private readonly TextBox _passwordBox = new();
    private readonly Label _errorLabel = new();

    public UserAccount? AuthenticatedAccount { get; private set; }

    public LoginForm(AccountService accountService)
    {
        _accountService = accountService;

        Text = "StudyFlow Pro｜登入";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(560, 720);
        MinimumSize = new Size(560, 720);
        MaximumSize = new Size(560, 720);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        Shown += (_, _) => _usernameBox.Focus();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(34, 28, 34, 28),
            BackColor = UiTheme.Background
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        var brand = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty,
            BackColor = UiTheme.Background
        };
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 66));
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        brand.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        brand.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var mark = new Label
        {
            Text = "SF",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 8),
            BackColor = UiTheme.Primary,
            ForeColor = UiTheme.OnPrimary,
            Font = UiTheme.Font(16, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        var title = new Label
        {
            Text = "StudyFlow Pro",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.TextPrimary,
            Font = UiTheme.Font(22, FontStyle.Bold),
            TextAlign = ContentAlignment.BottomLeft
        };
        var subtitle = new Label
        {
            Text = "登入你的個人學習帳號",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(10),
            TextAlign = ContentAlignment.TopLeft
        };
        brand.Controls.Add(mark, 0, 0);
        brand.SetRowSpan(mark, 2);
        brand.Controls.Add(title, 1, 0);
        brand.Controls.Add(subtitle, 1, 1);
        root.Controls.Add(brand, 1, 0);

        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(28),
            Margin = new Padding(0, 4, 0, 8)
        };
        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        form.Controls.Add(new Label
        {
            Text = "歡迎回來",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.TextPrimary,
            Font = UiTheme.Font(17, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        _usernameBox.PlaceholderText = "請輸入帳號";
        _passwordBox.PlaceholderText = "請輸入密碼";
        _passwordBox.UseSystemPasswordChar = true;
        form.Controls.Add(CreateField("帳號", _usernameBox), 0, 1);
        form.Controls.Add(CreateField("密碼", _passwordBox), 0, 2);

        var showPassword = new CheckBox
        {
            Text = "顯示密碼",
            AutoSize = true,
            Margin = new Padding(2, 6, 0, 0),
            ForeColor = UiTheme.TextSecondary,
            Font = UiTheme.Font(9)
        };
        showPassword.CheckedChanged += (_, _) =>
            _passwordBox.UseSystemPasswordChar = !showPassword.Checked;
        form.Controls.Add(showPassword, 0, 3);

        _errorLabel.Dock = DockStyle.Fill;
        _errorLabel.ForeColor = UiTheme.Danger;
        _errorLabel.Font = UiTheme.Font(9, FontStyle.Bold);
        _errorLabel.TextAlign = ContentAlignment.MiddleLeft;
        form.Controls.Add(_errorLabel, 0, 4);

        Button loginButton = UiTheme.PrimaryButton("登入");
        loginButton.Dock = DockStyle.Fill;
        loginButton.Margin = new Padding(0, 4, 0, 6);
        loginButton.Click += (_, _) => Login();
        form.Controls.Add(loginButton, 0, 5);

        Button registerButton = UiTheme.SecondaryButton("還沒有帳號？建立新帳號");
        registerButton.Dock = DockStyle.Fill;
        registerButton.Margin = new Padding(0, 4, 0, 6);
        registerButton.Click += (_, _) => OpenRegistration();
        form.Controls.Add(registerButton, 0, 6);

        form.Controls.Add(new Label
        {
            Text = "每個帳號都有獨立的任務、課表、專注紀錄、考古題與介面設定。\n帳號資料僅儲存在這台電腦，密碼會以雜湊方式保存。",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.6f),
            TextAlign = ContentAlignment.TopLeft
        }, 0, 7);

        card.Controls.Add(form);
        root.Controls.Add(card, 1, 1);

        root.Controls.Add(new Label
        {
            Text = "StudyFlow Pro Research Edition",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.5f),
            TextAlign = ContentAlignment.MiddleCenter
        }, 1, 2);

        Controls.Add(root);
        AcceptButton = loginButton;
    }

    private void Login()
    {
        _errorLabel.Text = string.Empty;
        UserAccount? account;
        try
        {
            account = _accountService.Authenticate(
                _usernameBox.Text,
                _passwordBox.Text,
                out string error);

            if (account == null)
            {
                _errorLabel.Text = error;
                _passwordBox.SelectAll();
                _passwordBox.Focus();
                return;
            }
        }
        catch (Exception exception)
        {
            _errorLabel.Text = "無法讀取帳號資料：" + exception.Message;
            return;
        }

        AuthenticatedAccount = account;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OpenRegistration()
    {
        using var form = new RegisterForm(_accountService);
        if (form.ShowDialog(this) != DialogResult.OK || form.RegisteredAccount == null)
            return;

        AuthenticatedAccount = form.RegisteredAccount;
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
