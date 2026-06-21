using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public sealed class ScoreComponent
{
    public string Name { get; init; } = string.Empty;
    public int Score { get; init; }
    public int Maximum { get; init; }
    public string Explanation { get; init; } = string.Empty;
}

public sealed class PriorityAnalysis
{
    public Guid TaskId { get; init; }
    /// <summary>對外顯示的 0～100 標準化智慧分數。</summary>
    public int TotalScore { get; init; }
    /// <summary>各可解釋元件直接加總後的原始分數。</summary>
    public int RawScore { get; init; }
    public int MaximumRawScore { get; init; } = 149;
    public string Level { get; init; } = string.Empty;
    public string Recommendation { get; init; } = string.Empty;
    public IReadOnlyList<ScoreComponent> Components { get; init; } = Array.Empty<ScoreComponent>();
}

public static class SmartPlanner
{
    private const int RawScoreMaximum = 149;

    public static PriorityAnalysis Analyze(StudyTask task, DateTime? now = null)
    {
        DateTime current = now ?? DateTime.Now;

        if (task.IsCompleted)
        {
            return new PriorityAnalysis
            {
                TaskId = task.Id,
                TotalScore = 0,
                RawScore = 0,
                MaximumRawScore = RawScoreMaximum,
                Level = "已完成",
                Recommendation = "此任務已完成，不再列入優先排序。",
                Components = Array.Empty<ScoreComponent>()
            };
        }

        double hoursUntilDue = (task.DueDate - current).TotalHours;

        int priorityScore = task.Priority switch
        {
            TaskPriority.Low => 8,
            TaskPriority.Medium => 20,
            TaskPriority.High => 34,
            TaskPriority.Urgent => 45,
            _ => 20
        };

        int dueScore;
        string dueExplanation;
        if (hoursUntilDue < 0)
        {
            dueScore = Math.Min(45, 34 + (int)Math.Ceiling(Math.Abs(hoursUntilDue) / 12.0));
            dueExplanation = $"已逾期 {Humanize(current - task.DueDate)}，急迫度大幅提高。";
        }
        else if (hoursUntilDue <= 6)
        {
            dueScore = 38;
            dueExplanation = "六小時內到期，應立即處理。";
        }
        else if (hoursUntilDue <= 24)
        {
            dueScore = 31;
            dueExplanation = "二十四小時內到期。";
        }
        else if (hoursUntilDue <= 72)
        {
            dueScore = 22;
            dueExplanation = "三天內到期。";
        }
        else if (hoursUntilDue <= 168)
        {
            dueScore = 13;
            dueExplanation = "一週內到期。";
        }
        else
        {
            dueScore = 5;
            dueExplanation = "距離截止時間仍充裕。";
        }

        int workloadScore = task.RemainingMinutes switch
        {
            <= 20 => 4,
            <= 60 => 8,
            <= 180 => 13,
            <= 360 => 17,
            _ => 20
        };

        int difficultyScore = Math.Clamp(task.Difficulty, 1, 5) * 3;
        int energyScore = (Math.Clamp(task.EnergyRequired, 1, 5) - 1) * 2;
        int progressRelief = task.ProgressPercent / 8;
        int pinBonus = task.IsPinned ? 8 : 0;
        int staleBonus = Math.Min(8, Math.Max(0, (int)(current - task.UpdatedAt).TotalDays));

        var components = new List<ScoreComponent>
        {
            new()
            {
                Name = "人工優先級",
                Score = priorityScore,
                Maximum = 45,
                Explanation = $"使用者設定為「{PriorityText(task.Priority)}」。"
            },
            new()
            {
                Name = "截止時間急迫度",
                Score = dueScore,
                Maximum = 45,
                Explanation = dueExplanation
            },
            new()
            {
                Name = "剩餘工作量",
                Score = workloadScore,
                Maximum = 20,
                Explanation = $"估計仍需 {task.RemainingMinutes} 分鐘。"
            },
            new()
            {
                Name = "任務難度",
                Score = difficultyScore,
                Maximum = 15,
                Explanation = $"難度 {Math.Clamp(task.Difficulty, 1, 5)}/5。"
            },
            new()
            {
                Name = "精力需求",
                Score = energyScore,
                Maximum = 8,
                Explanation = $"精力需求 {Math.Clamp(task.EnergyRequired, 1, 5)}/5；高耗能任務應提早保留完整時段。"
            },
            new()
            {
                Name = "釘選加權",
                Score = pinBonus,
                Maximum = 8,
                Explanation = task.IsPinned ? "使用者已釘選此任務。" : "未釘選。"
            },
            new()
            {
                Name = "長期未更新",
                Score = staleBonus,
                Maximum = 8,
                Explanation = staleBonus > 0 ? $"已約 {staleBonus} 天未更新。" : "近期有更新。"
            },
            new()
            {
                Name = "進度抵銷",
                Score = -progressRelief,
                Maximum = 0,
                Explanation = $"目前進度 {task.ProgressPercent}%，降低部分急迫度。"
            }
        };

        int rawScore = Math.Max(0, components.Sum(component => component.Score));
        int score = Math.Clamp(
            (int)Math.Round(rawScore * 100.0 / RawScoreMaximum),
            0,
            100);

        string level = score switch
        {
            >= 80 => "立即處理",
            >= 60 => "高度優先",
            >= 40 => "中度優先",
            _ => "可排程"
        };

        string recommendation = score switch
        {
            >= 80 => "建議現在立刻開始一個專注時段。",
            >= 60 => "建議安排在今天的第一個可用時段。",
            >= 40 => "建議在三天內排入行程。",
            _ => "可先保留，待更高優先任務完成後處理。"
        };

        return new PriorityAnalysis
        {
            TaskId = task.Id,
            TotalScore = score,
            RawScore = rawScore,
            MaximumRawScore = RawScoreMaximum,
            Level = level,
            Recommendation = recommendation,
            Components = components
        };
    }

    public static int CalculateScore(StudyTask task, DateTime? now = null) =>
        Analyze(task, now).TotalScore;

    public static IReadOnlyList<StudyTask> RankTasks(IEnumerable<StudyTask> tasks) =>
        tasks
            .Where(task => !task.IsCompleted)
            .OrderByDescending(task => task.IsPinned)
            .ThenByDescending(task => CalculateScore(task))
            .ThenBy(task => task.DueDate)
            .ToList();

    public static string Recommendation(AppData data)
    {
        StudyTask task = RankTasks(data.Tasks).FirstOrDefault();
        if (task == null)
            return "目前沒有未完成任務，可以新增下一個目標。";

        PriorityAnalysis analysis = Analyze(task);
        string course = data.Courses.FirstOrDefault(item => item.Id == task.CourseId)?.Name ?? "未分類";
        string due = task.DueDate < DateTime.Now
            ? $"已逾期 {Humanize(DateTime.Now - task.DueDate)}"
            : $"剩餘 {Humanize(task.DueDate - DateTime.Now)}";

        return $"建議先做「{task.Title}」｜{course}｜{due}｜建議：{analysis.Level}｜智慧分數：{analysis.TotalScore}/100";
    }

    public static int CompletionRate(AppData data) => data.Tasks.Count == 0
        ? 0
        : (int)Math.Round(data.Tasks.Count(task => task.IsCompleted) * 100.0 / data.Tasks.Count);

    public static int TodayFocusMinutes(AppData data) => data.Sessions
        .Where(session => session.StartedAt.Date == DateTime.Today)
        .Sum(session => session.DurationMinutes);

    public static int CurrentStreak(AppData data)
    {
        HashSet<DateTime> days = data.Sessions
            .Where(session => session.DurationMinutes > 0)
            .Select(session => session.StartedAt.Date)
            .ToHashSet();

        DateTime cursor = DateTime.Today;
        if (!days.Contains(cursor))
            cursor = cursor.AddDays(-1);

        int streak = 0;
        while (days.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    public static string PriorityText(TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "低",
        TaskPriority.Medium => "中",
        TaskPriority.High => "高",
        TaskPriority.Urgent => "緊急",
        _ => "中"
    };

    public static string Humanize(TimeSpan span)
    {
        if (span.TotalDays >= 1)
            return $"{Math.Max(1, (int)Math.Ceiling(span.TotalDays))} 天";
        if (span.TotalHours >= 1)
            return $"{Math.Max(1, (int)Math.Ceiling(span.TotalHours))} 小時";
        return $"{Math.Max(1, (int)Math.Ceiling(span.TotalMinutes))} 分鐘";
    }
}
