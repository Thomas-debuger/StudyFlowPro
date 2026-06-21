namespace StudyFlowPro.Services;

public static class ErrorLogService
{
    public static string Log(Exception exception, string context)
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StudyFlowPro",
            "Logs");
        Directory.CreateDirectory(directory);

        string path = Path.Combine(directory, $"error-{DateTime.Now:yyyyMMdd}.log");
        string message = $"""
[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]
Context: {context}
Type: {exception.GetType().FullName}
Message: {exception.Message}
StackTrace:
{exception.StackTrace}
------------------------------------------------------------
""";
        File.AppendAllText(path, message, new UTF8Encoding(false));
        return path;
    }
}
