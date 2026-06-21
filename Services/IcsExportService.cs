using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public static class IcsExportService
{
    public static void ExportTasks(string path, AppData data)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BEGIN:VCALENDAR");
        builder.AppendLine("VERSION:2.0");
        builder.AppendLine("PRODID:-//StudyFlow Pro//Research Edition//ZH-TW");
        builder.AppendLine("CALSCALE:GREGORIAN");
        builder.AppendLine("METHOD:PUBLISH");

        foreach (StudyTask task in data.Tasks.Where(task => !task.IsCompleted).OrderBy(task => task.DueDate))
        {
            string course = data.Courses.FirstOrDefault(item => item.Id == task.CourseId)?.Name ?? "未分類";
            DateTime start = task.DueDate.AddMinutes(-Math.Max(15, task.RemainingMinutes));
            DateTime end = task.DueDate;

            builder.AppendLine("BEGIN:VEVENT");
            builder.AppendLine($"UID:{task.Id}@studyflowpro.local");
            builder.AppendLine($"DTSTAMP:{ToUtcStamp(DateTime.UtcNow)}");
            builder.AppendLine($"DTSTART:{ToUtcStamp(start.ToUniversalTime())}");
            builder.AppendLine($"DTEND:{ToUtcStamp(end.ToUniversalTime())}");
            builder.AppendLine($"SUMMARY:{Escape(task.Title)}");
            string description = $"課程：{course}\n優先級：{SmartPlanner.PriorityText(task.Priority)}\n{task.Description}";
            builder.AppendLine($"DESCRIPTION:{Escape(description)}");
            builder.AppendLine("BEGIN:VALARM");
            builder.AppendLine("TRIGGER:-PT30M");
            builder.AppendLine("ACTION:DISPLAY");
            builder.AppendLine($"DESCRIPTION:{Escape(task.Title)} 即將到期");
            builder.AppendLine("END:VALARM");
            builder.AppendLine("END:VEVENT");
        }

        builder.AppendLine("END:VCALENDAR");
        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
    }

    private static string ToUtcStamp(DateTime value) => value.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);

    private static string Escape(string value) => (value ?? string.Empty)
        .Replace("\\", "\\\\")
        .Replace(";", "\\;")
        .Replace(",", "\\,")
        .Replace("\r\n", "\\n")
        .Replace("\n", "\\n");
}
