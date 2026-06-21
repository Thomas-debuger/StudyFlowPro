using StudyFlowPro.Services;
using StudyFlowPro.UI;

namespace StudyFlowPro;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, args) => HandleFatalError(args.Exception, "UI Thread");
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
                HandleFatalError(exception, "AppDomain");
        };

        try
        {
            ThemeRuntime.Install();
            Application.Run(new StudyFlowApplicationContext());
        }
        catch (Exception exception)
        {
            HandleFatalError(exception, "Application Startup");
        }
    }

    internal static void HandleFatalError(Exception exception, string context)
    {
        string logPath;
        try
        {
            logPath = ErrorLogService.Log(exception, context);
        }
        catch
        {
            logPath = "無法建立錯誤記錄檔";
        }

        MessageBox.Show(
            "程式遇到未預期錯誤，但已盡可能保留資料。\n\n" +
            exception.Message + "\n\n錯誤記錄：" + logPath,
            "StudyFlow Pro 錯誤處理",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
