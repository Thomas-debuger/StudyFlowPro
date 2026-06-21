using StudyFlowPro.Models;
using StudyFlowPro.Services;
using StudyFlowPro.UI;

namespace StudyFlowPro;

internal sealed class StudyFlowApplicationContext : ApplicationContext
{
    private readonly AccountService _accountService = new();
    private Form? _activeForm;

    public StudyFlowApplicationContext()
    {
        ShowLoginForm();
    }

    private void ShowLoginForm()
    {
        UiTheme.ApplyVisualStyle(VisualStyleKind.Facebook, false);

        var loginForm = new LoginForm(_accountService);
        _activeForm = loginForm;
        loginForm.FormClosed += LoginFormClosed;
        loginForm.Show();
    }

    private void LoginFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (sender is not LoginForm loginForm)
        {
            ExitThread();
            return;
        }

        loginForm.FormClosed -= LoginFormClosed;
        UserAccount? account = loginForm.DialogResult == DialogResult.OK
            ? loginForm.AuthenticatedAccount
            : null;
        _activeForm = null;
        loginForm.Dispose();

        if (account == null)
        {
            ExitThread();
            return;
        }

        try
        {
            _accountService.PrepareUserDataDirectory(account);
            var service = new DataService(account, _accountService);
            service.Load();
            UiTheme.ApplyVisualStyle(service.Data.Settings.VisualStyle, false);

            var mainForm = new MainForm(service);
            _activeForm = mainForm;
            mainForm.FormClosed += MainFormClosed;
            mainForm.Show();
        }
        catch (Exception exception)
        {
            Program.HandleFatalError(exception, "User Session Startup");
            ExitThread();
        }
    }

    private void MainFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (sender is not MainForm mainForm)
        {
            ExitThread();
            return;
        }

        mainForm.FormClosed -= MainFormClosed;
        bool logoutRequested = mainForm.LogoutRequested;
        _activeForm = null;
        mainForm.Dispose();

        if (logoutRequested)
            ShowLoginForm();
        else
            ExitThread();
    }

    protected override void ExitThreadCore()
    {
        if (_activeForm != null && !_activeForm.IsDisposed)
        {
            _activeForm.FormClosed -= LoginFormClosed;
            _activeForm.FormClosed -= MainFormClosed;
            _activeForm.Close();
            _activeForm.Dispose();
            _activeForm = null;
        }

        base.ExitThreadCore();
    }
}
