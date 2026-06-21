using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public sealed class CsvImportResult
{
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public int CreatedCourseCount { get; set; }
    public List<string> Errors { get; } = new();

    public string BuildMessage(string itemName)
    {
        var lines = new List<string>
        {
            $"成功匯入 {ImportedCount} 筆{itemName}。"
        };
        if (CreatedCourseCount > 0)
            lines.Add($"同時建立 {CreatedCourseCount} 個 CSV 中尚未存在的課程／專案。");
        if (SkippedCount > 0)
            lines.Add($"略過 {SkippedCount} 筆重複或空白資料。");
        if (Errors.Count > 0)
        {
            lines.Add($"有 {Errors.Count} 筆格式無法匯入：");
            lines.AddRange(Errors.Take(6).Select(error => "• " + error));
            if (Errors.Count > 6)
                lines.Add($"• 其餘 {Errors.Count - 6} 筆未列出");
        }
        return string.Join(Environment.NewLine, lines);
    }
}

public static class CsvService
{
    private static readonly string[] CourseColors =
    {
        "#2563EB", "#7C3AED", "#059669", "#EA580C", "#DC2626", "#0891B2"
    };

    public static void ExportTasks(string path, AppData data)
    {
        var builder = new StringBuilder();
        builder.AppendLine("狀態,釘選,任務名稱,課程,優先級,截止時間,預估分鐘,已專注分鐘,進度,難度,精力需求,智慧分數(0-100),標籤,說明");

        foreach (StudyTask task in data.Tasks.OrderBy(task => task.DueDate))
        {
            string course = data.Courses.FirstOrDefault(item => item.Id == task.CourseId)?.Name ?? "未分類";
            builder.AppendLine(string.Join(",",
                Escape(task.IsCompleted ? "已完成" : "未完成"),
                Escape(task.IsPinned ? "是" : "否"),
                Escape(task.Title),
                Escape(course),
                Escape(SmartPlanner.PriorityText(task.Priority)),
                Escape(task.DueDate.ToString("yyyy-MM-dd HH:mm")),
                task.EstimatedMinutes.ToString(CultureInfo.InvariantCulture),
                task.FocusedMinutes.ToString(CultureInfo.InvariantCulture),
                Escape(task.ProgressPercent + "%"),
                task.Difficulty.ToString(CultureInfo.InvariantCulture),
                task.EnergyRequired.ToString(CultureInfo.InvariantCulture),
                SmartPlanner.CalculateScore(task).ToString(CultureInfo.InvariantCulture),
                Escape(task.Tags),
                Escape(task.Description)));
        }

        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(true));
    }

    public static CsvImportResult ImportTasks(string path, AppData data)
    {
        List<Dictionary<string, string>> rows = ReadRows(path, "任務名稱", "任務", "名稱");
        var result = new CsvImportResult();

        for (int index = 0; index < rows.Count; index++)
        {
            Dictionary<string, string> row = rows[index];
            int displayRow = index + 2;
            string title = Get(row, "任務名稱", "任務", "名稱").Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                result.SkippedCount++;
                continue;
            }

            try
            {
                string courseName = Get(row, "課程", "課程/專案", "課程／專案").Trim();
                Guid? courseId = ResolveOrCreateCourse(courseName, data, result);
                string dueText = Get(row, "截止時間", "期限");
                DateTime dueDate = string.IsNullOrWhiteSpace(dueText)
                    ? DateTime.Now.AddDays(1)
                    : ParseDate(dueText, "截止時間");

                bool duplicate = data.Tasks.Any(task =>
                    string.Equals(task.Title.Trim(), title, StringComparison.OrdinalIgnoreCase) &&
                    task.CourseId == courseId &&
                    Math.Abs((task.DueDate - dueDate).TotalMinutes) < 1);
                if (duplicate)
                {
                    result.SkippedCount++;
                    continue;
                }

                bool completed = ParseBool(Get(row, "狀態", "完成"), false, "已完成", "完成");
                int estimated = ParseInt(Get(row, "預估分鐘", "預估時間", "預估"), 60, 1, 100000);
                string focusedText = Get(row, "已專注分鐘", "專注分鐘", "已完成分鐘");
                int focused;
                if (!string.IsNullOrWhiteSpace(focusedText))
                {
                    focused = ParseInt(focusedText, 0, 0, 100000);
                }
                else
                {
                    int progress = ParseInt(Get(row, "進度", "進度百分比"), 0, 0, 100);
                    focused = (int)Math.Round(estimated * progress / 100.0);
                }
                DateTime now = DateTime.Now;
                var task = new StudyTask
                {
                    Title = title,
                    Description = Get(row, "說明", "描述"),
                    CourseId = courseId,
                    Priority = ParsePriority(Get(row, "優先級", "優先度")),
                    DueDate = dueDate,
                    EstimatedMinutes = estimated,
                    FocusedMinutes = focused,
                    Difficulty = ParseInt(Get(row, "難度"), 3, 1, 5),
                    EnergyRequired = ParseInt(Get(row, "精力需求", "精力"), 3, 1, 5),
                    IsPinned = ParseBool(Get(row, "釘選"), false, "是", "已釘選", "釘選"),
                    IsCompleted = completed,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CompletedAt = completed ? now : null,
                    Tags = Get(row, "標籤")
                };
                data.Tasks.Add(task);
                result.ImportedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"第 {displayRow} 列：{ex.Message}");
            }
        }

        return result;
    }

    public static void ExportSessions(string path, AppData data)
    {
        var builder = new StringBuilder();
        builder.AppendLine("開始時間,結束時間,任務,課程,分鐘,專注品質,分心次數,備註");

        foreach (FocusSession session in data.Sessions.OrderByDescending(session => session.StartedAt))
        {
            StudyTask task = data.Tasks.FirstOrDefault(item => item.Id == session.TaskId);
            string taskName = task?.Title
                ?? (string.IsNullOrWhiteSpace(session.TaskNameSnapshot) ? "自由專注" : session.TaskNameSnapshot);
            string course = task == null
                ? (string.IsNullOrWhiteSpace(session.CourseNameSnapshot) ? "未分類" : session.CourseNameSnapshot)
                : data.Courses.FirstOrDefault(item => item.Id == task.CourseId)?.Name ?? "未分類";

            builder.AppendLine(string.Join(",",
                Escape(session.StartedAt.ToString("yyyy-MM-dd HH:mm")),
                Escape(session.EndedAt.ToString("yyyy-MM-dd HH:mm")),
                Escape(taskName),
                Escape(course),
                session.DurationMinutes.ToString(CultureInfo.InvariantCulture),
                session.FocusQuality.ToString(CultureInfo.InvariantCulture),
                session.DistractionCount.ToString(CultureInfo.InvariantCulture),
                Escape(session.Note)));
        }

        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(true));
    }

    public static CsvImportResult ImportSessions(string path, AppData data)
    {
        List<Dictionary<string, string>> rows = ReadRows(path, "開始時間", "日期", "起始時間");
        var result = new CsvImportResult();
        var affectedTaskIds = new HashSet<Guid>();

        for (int index = 0; index < rows.Count; index++)
        {
            Dictionary<string, string> row = rows[index];
            int displayRow = index + 2;
            string startText = Get(row, "開始時間", "日期", "起始時間");
            if (string.IsNullOrWhiteSpace(startText))
            {
                result.SkippedCount++;
                continue;
            }

            try
            {
                DateTime startedAt = ParseDate(startText, "開始時間");
                string endText = Get(row, "結束時間", "結束");
                string minutesText = Get(row, "分鐘", "專注分鐘", "時長");
                DateTime endedAt;
                int durationMinutes;

                if (!string.IsNullOrWhiteSpace(minutesText))
                {
                    durationMinutes = ParseInt(minutesText, 0, 1, 100000);
                    endedAt = string.IsNullOrWhiteSpace(endText)
                        ? startedAt.AddMinutes(durationMinutes)
                        : ParseDate(endText, "結束時間");
                }
                else if (!string.IsNullOrWhiteSpace(endText))
                {
                    endedAt = ParseDate(endText, "結束時間");
                    durationMinutes = Math.Max(1, (int)Math.Round((endedAt - startedAt).TotalMinutes));
                }
                else
                {
                    throw new InvalidDataException("缺少「分鐘」或「結束時間」欄位值");
                }

                if (endedAt < startedAt)
                    throw new InvalidDataException("結束時間早於開始時間");

                string taskName = Get(row, "任務", "任務名稱").Trim();
                string courseName = Get(row, "課程", "課程/專案", "課程／專案").Trim();
                StudyTask matchedTask = FindMatchingTask(taskName, courseName, data);

                bool duplicate = data.Sessions.Any(session =>
                {
                    string existingTaskName = GetSessionTaskName(session, data);
                    return Math.Abs((session.StartedAt - startedAt).TotalMinutes) < 1 &&
                           session.DurationMinutes == durationMinutes &&
                           string.Equals(existingTaskName, NormalizeFreeFocusName(taskName), StringComparison.OrdinalIgnoreCase);
                });
                if (duplicate)
                {
                    result.SkippedCount++;
                    continue;
                }

                var session = new FocusSession
                {
                    TaskId = matchedTask?.Id,
                    TaskNameSnapshot = NormalizeFreeFocusName(taskName),
                    CourseNameSnapshot = NormalizeCourseName(courseName),
                    StartedAt = startedAt,
                    EndedAt = endedAt,
                    DurationMinutes = durationMinutes,
                    FocusQuality = ParseInt(Get(row, "專注品質", "品質"), 0, 0, 5),
                    DistractionCount = ParseInt(Get(row, "分心次數", "分心"), 0, 0, 100000),
                    Note = Get(row, "備註", "說明")
                };
                data.Sessions.Add(session);
                if (matchedTask != null)
                    affectedTaskIds.Add(matchedTask.Id);
                result.ImportedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"第 {displayRow} 列：{ex.Message}");
            }
        }

        // 以目前所有已綁定的專注紀錄重新計算最低應有的專注分鐘，
        // 避免先匯入任務 CSV、再匯入紀錄 CSV 時重複累加。
        foreach (Guid taskId in affectedTaskIds)
        {
            StudyTask task = data.Tasks.FirstOrDefault(item => item.Id == taskId);
            if (task == null)
                continue;
            int recordedMinutes = data.Sessions
                .Where(session => session.TaskId == taskId)
                .Sum(session => Math.Max(0, session.DurationMinutes));
            task.FocusedMinutes = Math.Max(task.FocusedMinutes, recordedMinutes);
            task.UpdatedAt = DateTime.Now;
        }

        return result;
    }

    private static StudyTask FindMatchingTask(string taskName, string courseName, AppData data)
    {
        taskName = NormalizeFreeFocusName(taskName);
        if (string.Equals(taskName, "自由專注", StringComparison.OrdinalIgnoreCase))
            return null;

        IEnumerable<StudyTask> candidates = data.Tasks.Where(task =>
            string.Equals(task.Title.Trim(), taskName, StringComparison.OrdinalIgnoreCase));
        string normalizedCourse = NormalizeCourseName(courseName);
        if (!string.Equals(normalizedCourse, "未分類", StringComparison.OrdinalIgnoreCase))
        {
            candidates = candidates.Where(task =>
            {
                string currentCourse = data.Courses.FirstOrDefault(course => course.Id == task.CourseId)?.Name ?? "未分類";
                return string.Equals(currentCourse.Trim(), normalizedCourse, StringComparison.OrdinalIgnoreCase);
            });
        }
        return candidates.FirstOrDefault();
    }

    private static string GetSessionTaskName(FocusSession session, AppData data)
    {
        StudyTask task = data.Tasks.FirstOrDefault(item => item.Id == session.TaskId);
        return task?.Title ?? NormalizeFreeFocusName(session.TaskNameSnapshot);
    }

    private static Guid? ResolveOrCreateCourse(string courseName, AppData data, CsvImportResult result)
    {
        courseName = NormalizeCourseName(courseName);
        if (string.Equals(courseName, "未分類", StringComparison.OrdinalIgnoreCase))
            return null;

        Course existing = data.Courses.FirstOrDefault(course =>
            string.Equals(course.Name.Trim(), courseName, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            return existing.Id;

        var course = new Course
        {
            Name = courseName,
            ColorHex = CourseColors[data.Courses.Count % CourseColors.Length]
        };
        data.Courses.Add(course);
        result.CreatedCourseCount++;
        return course.Id;
    }

    private static List<Dictionary<string, string>> ReadRows(string path, params string[] requiredHeaders)
    {
        string text = File.ReadAllText(path, Encoding.UTF8);
        List<List<string>> parsed = ParseCsv(text);
        if (parsed.Count == 0)
            throw new InvalidDataException("CSV 檔案是空的");

        List<string> headers = parsed[0]
            .Select(NormalizeHeader)
            .ToList();
        if (headers.All(string.IsNullOrWhiteSpace))
            throw new InvalidDataException("CSV 第一列找不到欄位名稱");
        if (requiredHeaders.Length > 0 && !requiredHeaders
                .Select(NormalizeHeader)
                .Any(required => headers.Contains(required, StringComparer.OrdinalIgnoreCase)))
        {
            throw new InvalidDataException(
                $"CSV 缺少必要欄位，至少需要其中一個：{string.Join("、", requiredHeaders)}");
        }

        var rows = new List<Dictionary<string, string>>();
        foreach (List<string> values in parsed.Skip(1))
        {
            if (values.All(string.IsNullOrWhiteSpace))
                continue;

            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int column = 0; column < headers.Count; column++)
            {
                string header = headers[column];
                if (string.IsNullOrWhiteSpace(header) || row.ContainsKey(header))
                    continue;
                string value = column < values.Count ? UnprotectFormula(values[column]).Trim() : string.Empty;
                row[header] = value;
            }
            rows.Add(row);
        }
        return rows;
    }

    private static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;

        for (int index = 0; index < text.Length; index++)
        {
            char current = text[index];
            if (inQuotes)
            {
                if (current == '"')
                {
                    if (index + 1 < text.Length && text[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(current);
                }
                continue;
            }

            if (current == '"' && field.Length == 0)
            {
                inQuotes = true;
            }
            else if (current == ',')
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if (current == '\r' || current == '\n')
            {
                row.Add(field.ToString());
                field.Clear();
                rows.Add(row);
                row = new List<string>();
                if (current == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                    index++;
            }
            else
            {
                field.Append(current);
            }
        }

        if (inQuotes)
            throw new InvalidDataException("CSV 中有未關閉的雙引號");

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }
        return rows;
    }

    private static string Get(Dictionary<string, string> row, params string[] names)
    {
        foreach (string name in names)
        {
            if (row.TryGetValue(NormalizeHeader(name), out string value))
                return value ?? string.Empty;
        }
        return string.Empty;
    }

    private static string NormalizeHeader(string value)
    {
        value ??= string.Empty;
        return value.Trim().TrimStart('\uFEFF')
            .Replace(" ", string.Empty)
            .Replace("　", string.Empty)
            .Replace("（", "(")
            .Replace("）", ")");
    }

    private static string UnprotectFormula(string value)
    {
        if (value?.Length >= 2 && value[0] == '\'' && "=+-@".Contains(value[1]))
            return value[1..];
        return value ?? string.Empty;
    }

    private static DateTime ParseDate(string value, string fieldName)
    {
        string[] formats =
        {
            "yyyy-MM-dd HH:mm", "yyyy/MM/dd HH:mm", "yyyy-MM-dd H:mm", "yyyy/MM/dd H:mm",
            "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm", "yyyy/M/d H:mm", "yyyy/M/d HH:mm",
            "M/d/yyyy H:mm", "M/d/yyyy HH:mm"
        };
        if (DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out DateTime parsed) ||
            DateTime.TryParse(value.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out parsed))
            return parsed;
        throw new InvalidDataException($"{fieldName}「{value}」不是可辨識的日期時間");
    }

    private static int ParseInt(string value, int defaultValue, int minimum, int maximum)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        string cleaned = value.Trim().TrimEnd('%').Trim();
        if (!int.TryParse(cleaned, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) &&
            !int.TryParse(cleaned, NumberStyles.Integer, CultureInfo.CurrentCulture, out parsed))
            throw new InvalidDataException($"數值「{value}」格式錯誤");
        return Math.Clamp(parsed, minimum, maximum);
    }

    private static bool ParseBool(string value, bool defaultValue, params string[] trueValues)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        string normalized = value.Trim();
        if (trueValues.Any(item => string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase)) ||
            normalized is "true" or "1" or "yes" or "y")
            return true;
        if (normalized is "false" or "0" or "no" or "n" || normalized.Contains("未完成"))
            return false;
        return defaultValue;
    }

    private static TaskPriority ParsePriority(string value)
    {
        string normalized = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
            return TaskPriority.Medium;
        if (normalized.Contains("緊急") || normalized.Equals("Urgent", StringComparison.OrdinalIgnoreCase) || normalized == "4")
            return TaskPriority.Urgent;
        if (normalized.Contains("高") || normalized.Equals("High", StringComparison.OrdinalIgnoreCase) || normalized == "3")
            return TaskPriority.High;
        if (normalized.Contains("低") || normalized.Equals("Low", StringComparison.OrdinalIgnoreCase) || normalized == "1")
            return TaskPriority.Low;
        return TaskPriority.Medium;
    }

    private static string NormalizeFreeFocusName(string value)
    {
        value = value?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(value) ? "自由專注" : value;
    }

    private static string NormalizeCourseName(string value)
    {
        value = value?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(value) ? "未分類" : value;
    }

    private static string Escape(string value)
    {
        value ??= string.Empty;

        if (value.StartsWith("=") || value.StartsWith("+") ||
            value.StartsWith("-") || value.StartsWith("@"))
        {
            value = "'" + value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
