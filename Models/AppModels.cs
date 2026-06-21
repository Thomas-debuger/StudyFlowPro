namespace StudyFlowPro.Models;

public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum ActivityType
{
    Created,
    Updated,
    Completed,
    Reopened,
    Deleted,
    Focused,
    Exported,
    BackedUp,
    Restored,
    Imported,
    Viewed,
    System
}

public sealed class StudyTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? CourseId { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime DueDate { get; set; } = DateTime.Now.AddDays(1);
    public int EstimatedMinutes { get; set; } = 60;
    public int FocusedMinutes { get; set; }
    public int Difficulty { get; set; } = 3;
    public int EnergyRequired { get; set; } = 3;
    public bool IsPinned { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
    public string Tags { get; set; } = string.Empty;

    [JsonIgnore]
    public int RemainingMinutes => Math.Max(0, EstimatedMinutes - FocusedMinutes);

    [JsonIgnore]
    public int ProgressPercent => IsCompleted
        ? 100
        : EstimatedMinutes <= 0
            ? 0
            : Math.Clamp((int)Math.Round(FocusedMinutes * 100.0 / EstimatedMinutes), 0, 99);
}

public sealed class Course
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#2563EB";
}

public sealed class FocusSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? TaskId { get; set; }
    // 匯入 CSV 時，即使原任務尚未存在，也保留當時的任務與課程名稱。
    public string TaskNameSnapshot { get; set; } = string.Empty;
    public string CourseNameSnapshot { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime EndedAt { get; set; } = DateTime.Now;
    public int DurationMinutes { get; set; }
    public int FocusQuality { get; set; }
    public int DistractionCount { get; set; }
    public string Note { get; set; } = string.Empty;
}

public sealed class ActivityLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.Now;
    public ActivityType Type { get; set; } = ActivityType.System;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Summary { get; set; } = string.Empty;
}


public enum ExamPaperStatus
{
    NotStarted = 0,
    Reviewing = 1,
    Completed = 2
}

public sealed class ExamSubject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#2563EB";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public sealed class ExamPaper
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public string ExamYear { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public ExamPaperStatus Status { get; set; } = ExamPaperStatus.NotStarted;
    public DateTime ImportedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime? LastOpenedAt { get; set; }
    public int OpenCount { get; set; }
}


public sealed class TimetableSemester
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public sealed class TimetableEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SemesterCode { get; set; } = "114-2";
    public string CourseName { get; set; } = string.Empty;
    /// <summary>星期索引：1=週一，2=週二，…，6=週六。</summary>
    public int DayIndex { get; set; } = 1;
    public int StartPeriod { get; set; } = 1;
    public int EndPeriod { get; set; } = 1;
    public string Location { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#2563EB";
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}


public enum VisualStyleKind
{
    Facebook,
    Spotify,
    YouTube,
    Netflix,
    VisualStudio,
    VisualStudioCode
}

public sealed class AppSettings
{
    public string UserName { get; set; } = "同學";
    public int DefaultFocusMinutes { get; set; } = 25;
    public int DailyGoalMinutes { get; set; } = 120;
    public bool ShowDueSoonReminder { get; set; } = true;
    public int DueSoonHours { get; set; } = 24;
    public string LastTimetableSemester { get; set; } = "114-2";
    public VisualStyleKind VisualStyle { get; set; } = VisualStyleKind.Facebook;
    public DateTime? LastBackupAt { get; set; }
}

/// <summary>
/// 單一智慧排程區塊的保存快照。名稱採快照保存，避免日後任務改名時舊排程無法閱讀。
/// </summary>
public sealed class SmartScheduleBlockSnapshot
{
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int Minutes { get; set; }
    public int BreakAfterMinutes { get; set; }
    public int SmartScore { get; set; }
    public string PriorityLevel { get; set; } = string.Empty;
}

/// <summary>
/// 目前帳號最後一次按下「重新產生」後的智慧排程。
/// 此物件位於 AppData 內，因此會跟著帳號資料夾獨立保存，不會跨帳號共用。
/// </summary>
public sealed class SmartScheduleState
{
    public DateTime? GeneratedAt { get; set; }
    public DateTime StartAt { get; set; }
    public int AvailableMinutes { get; set; } = 180;
    public int BreakMinutes { get; set; } = 5;
    public int FocusMinutes { get; set; }
    public int BreakMinutesUsed { get; set; }
    public int BufferMinutes { get; set; }
    public DateTime? ExpectedEnd { get; set; }
    public string PlainText { get; set; } = string.Empty;
    public List<SmartScheduleBlockSnapshot> Blocks { get; set; } = new();
}

public sealed class AppData
{
    public int SchemaVersion { get; set; } = 9;
    public Guid OwnerAccountId { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public DateTime ProfileCreatedAt { get; set; } = DateTime.Now;
    public List<StudyTask> Tasks { get; set; } = new();
    public List<Course> Courses { get; set; } = new();
    public List<FocusSession> Sessions { get; set; } = new();
    public List<ActivityLogEntry> Activities { get; set; } = new();
    public List<ExamSubject> ExamSubjects { get; set; } = new();
    public List<ExamPaper> ExamPapers { get; set; } = new();
    public List<TimetableSemester> TimetableSemesters { get; set; } = new();
    public List<TimetableEntry> TimetableEntries { get; set; } = new();
    public bool TimetableInitialized { get; set; }
    public SmartScheduleState SmartSchedule { get; set; } = new();
    public AppSettings Settings { get; set; } = new();
}
