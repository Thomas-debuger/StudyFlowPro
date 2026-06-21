using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public sealed class DiagnosticItem
{
    public bool Passed { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
}

public static class DiagnosticsService
{
    public static IReadOnlyList<DiagnosticItem> Run(AppData data, Guid? expectedOwnerAccountId = null)
    {
        var results = new List<DiagnosticItem>();

        if (expectedOwnerAccountId.HasValue)
        {
            results.Add(Check(
                data.OwnerAccountId == expectedOwnerAccountId.Value,
                "帳號資料隔離",
                "目前載入的任務、課表、考古題、專注紀錄與偏好必須屬於目前登入帳號。"));
        }

        results.Add(Check(
            data.Tasks.Select(task => task.Id).Distinct().Count() == data.Tasks.Count,
            "任務 ID 唯一性",
            "每筆任務必須具有唯一識別碼。"));

        results.Add(Check(
            data.Courses.Select(course => course.Id).Distinct().Count() == data.Courses.Count,
            "課程 ID 唯一性",
            "每筆課程必須具有唯一識別碼。"));

        results.Add(Check(
            data.Sessions.Select(session => session.Id).Distinct().Count() == data.Sessions.Count,
            "專注紀錄 ID 唯一性",
            "每筆專注紀錄必須具有唯一識別碼。"));

        results.Add(Check(
            data.ExamSubjects.Select(subject => subject.Id).Distinct().Count() == data.ExamSubjects.Count,
            "考古題科目 ID 唯一性",
            "每個考古題科目必須具有唯一識別碼。"));

        results.Add(Check(
            data.ExamPapers.Select(paper => paper.Id).Distinct().Count() == data.ExamPapers.Count,
            "考古題文件 ID 唯一性",
            "每份考古題必須具有唯一識別碼。"));

        results.Add(Check(
            data.TimetableEntries.Select(entry => entry.Id).Distinct().Count() == data.TimetableEntries.Count,
            "課表項目 ID 唯一性",
            "每筆課表課程必須具有唯一識別碼。"));

        HashSet<Guid> courseIds = data.Courses.Select(course => course.Id).ToHashSet();
        results.Add(Check(
            data.Tasks.All(task => !task.CourseId.HasValue || courseIds.Contains(task.CourseId.Value)),
            "任務－課程參照完整性",
            "任務不可指向不存在的課程。"));

        HashSet<Guid> taskIds = data.Tasks.Select(task => task.Id).ToHashSet();
        results.Add(Check(
            data.Sessions.All(session => !session.TaskId.HasValue || taskIds.Contains(session.TaskId.Value)),
            "專注紀錄－任務參照完整性",
            "專注紀錄不可指向不存在的任務。"));

        HashSet<Guid> examSubjectIds = data.ExamSubjects.Select(subject => subject.Id).ToHashSet();
        results.Add(Check(
            data.ExamPapers.All(paper => examSubjectIds.Contains(paper.SubjectId)),
            "考古題－科目參照完整性",
            "考古題不可指向不存在的科目。"));

        results.Add(Check(
            data.ExamPapers.All(paper => paper.FileExtension is ".pdf" or ".docx" && !string.IsNullOrWhiteSpace(paper.StoredFileName)),
            "考古題檔案索引格式",
            "考古題只允許 PDF／DOCX，且必須具有內部保存檔名。"));

        results.Add(Check(
            data.TimetableSemesters.Count > 0 &&
            data.TimetableSemesters.Select(semester => semester.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() == data.TimetableSemesters.Count &&
            data.TimetableSemesters.All(semester => !string.IsNullOrWhiteSpace(semester.DisplayName)),
            "課表學期索引",
            "學期代碼必須唯一，且學期名稱不可空白。"));

        HashSet<string> timetableSemesterCodes = data.TimetableSemesters
            .Select(semester => semester.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        results.Add(Check(
            data.TimetableSemesters.Count > 0 &&
            data.TimetableEntries.All(entry =>
                timetableSemesterCodes.Contains(entry.SemesterCode) &&
                !string.IsNullOrWhiteSpace(entry.CourseName) &&
                entry.DayIndex is >= 1 and <= 6 &&
                entry.StartPeriod is >= 1 and <= 10 &&
                entry.EndPeriod >= entry.StartPeriod && entry.EndPeriod <= 10),
            "課表欄位範圍",
            "學期、星期、節次與課程名稱必須有效。"));

        bool timetableConflictFree = data.TimetableEntries
            .GroupBy(entry => new { entry.SemesterCode, entry.DayIndex })
            .All(group =>
            {
                TimetableEntry[] items = group.OrderBy(entry => entry.StartPeriod).ToArray();
                for (int index = 0; index < items.Length; index++)
                {
                    for (int other = index + 1; other < items.Length; other++)
                    {
                        if (items[other].StartPeriod <= items[index].EndPeriod &&
                            items[other].EndPeriod >= items[index].StartPeriod)
                            return false;
                    }
                }
                return true;
            });
        results.Add(Check(
            timetableConflictFree,
            "課表時段衝突",
            "同一學期、同一星期不可有重疊節次。"));

        results.Add(Check(
            data.Tasks.All(task => task.EstimatedMinutes > 0 && task.FocusedMinutes >= 0),
            "任務時間數值",
            "預估時間必須大於零，已專注時間不可為負數。"));

        results.Add(Check(
            data.Tasks.All(task => task.Difficulty is >= 1 and <= 5 && task.EnergyRequired is >= 1 and <= 5),
            "難度與精力範圍",
            "難度與精力需求必須介於 1 到 5。"));

        results.Add(Check(
            data.Sessions.All(session => session.DurationMinutes > 0 && session.EndedAt >= session.StartedAt),
            "專注紀錄時間合理性",
            "每次紀錄必須具有正分鐘數且結束時間不可早於開始時間。"));

        results.Add(Check(
            data.Settings.DailyGoalMinutes > 0 && data.Settings.DefaultFocusMinutes > 0,
            "設定值合理性",
            "每日目標與預設專注時間必須大於零。"));

        return results;
    }

    private static DiagnosticItem Check(bool condition, string name, string detail) => new()
    {
        Passed = condition,
        Name = name,
        Detail = detail
    };
}
