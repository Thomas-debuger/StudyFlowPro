using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public sealed class ProductivityMetrics
{
    public int ProductivityIndex { get; init; }
    public int CompletionRate { get; init; }
    public int ConsistencyScore { get; init; }
    public int FocusQualityScore { get; init; }
    public int EstimationAccuracy { get; init; }
    public int DeepWorkMinutes { get; init; }
    public int WeeklyFocusMinutes { get; init; }
    public int PreviousWeekFocusMinutes { get; init; }
    public double WeeklyChangePercent { get; init; }
    public string Summary { get; init; } = string.Empty;
}

public static class ResearchMetricsService
{
    public static ProductivityMetrics Calculate(AppData data, DateTime? reference = null)
    {
        DateTime today = (reference ?? DateTime.Today).Date;
        DateTime weekStart = StartOfWeek(today);
        DateTime previousWeekStart = weekStart.AddDays(-7);
        int elapsedDays = Math.Clamp((today - weekStart).Days + 1, 1, 7);
        DateTime currentPeriodEnd = weekStart.AddDays(elapsedDays);
        DateTime previousPeriodEnd = previousWeekStart.AddDays(elapsedDays);

        List<FocusSession> currentWeek = data.Sessions
            .Where(session => session.StartedAt >= weekStart && session.StartedAt < currentPeriodEnd)
            .ToList();

        List<FocusSession> previousWeek = data.Sessions
            .Where(session => session.StartedAt >= previousWeekStart && session.StartedAt < previousPeriodEnd)
            .ToList();

        int weeklyMinutes = currentWeek.Sum(session => session.DurationMinutes);
        int previousMinutes = previousWeek.Sum(session => session.DurationMinutes);
        double weeklyChange = previousMinutes == 0
            ? weeklyMinutes > 0 ? 100 : 0
            : (weeklyMinutes - previousMinutes) * 100.0 / previousMinutes;

        int activeDays = currentWeek
            .Where(session => session.DurationMinutes > 0)
            .Select(session => session.StartedAt.Date)
            .Distinct()
            .Count();
        int consistency = (int)Math.Round(activeDays * 100.0 / elapsedDays);

        List<FocusSession> ratedSessions = currentWeek
            .Where(session => session.FocusQuality > 0)
            .ToList();
        int focusQuality = ratedSessions.Count == 0
            ? 0
            : (int)Math.Round(ratedSessions.Average(session => session.FocusQuality) / 5.0 * 100);

        List<StudyTask> measurableTasks = data.Tasks
            .Where(task => task.IsCompleted && task.EstimatedMinutes > 0 && task.FocusedMinutes > 0)
            .ToList();
        int estimationAccuracy = measurableTasks.Count == 0
            ? 0
            : (int)Math.Round(measurableTasks.Average(task =>
            {
                double ratio = Math.Min(task.EstimatedMinutes, task.FocusedMinutes) * 1.0 /
                               Math.Max(task.EstimatedMinutes, task.FocusedMinutes);
                return ratio * 100;
            }));

        int completion = SmartPlanner.CompletionRate(data);
        int deepWork = currentWeek
            .Where(session => session.DurationMinutes >= 25 && session.DistractionCount <= 1)
            .Sum(session => session.DurationMinutes);

        int goal = Math.Max(1, data.Settings.DailyGoalMinutes * elapsedDays);
        int goalScore = Math.Clamp((int)Math.Round(weeklyMinutes * 100.0 / goal), 0, 100);

        int productivityIndex = (int)Math.Round(
            completion * 0.30 +
            consistency * 0.25 +
            goalScore * 0.25 +
            (focusQuality == 0 ? 50 : focusQuality) * 0.20);

        string summary = BuildSummary(productivityIndex, weeklyChange, consistency, focusQuality);

        return new ProductivityMetrics
        {
            ProductivityIndex = Math.Clamp(productivityIndex, 0, 100),
            CompletionRate = completion,
            ConsistencyScore = consistency,
            FocusQualityScore = focusQuality,
            EstimationAccuracy = estimationAccuracy,
            DeepWorkMinutes = deepWork,
            WeeklyFocusMinutes = weeklyMinutes,
            PreviousWeekFocusMinutes = previousMinutes,
            WeeklyChangePercent = weeklyChange,
            Summary = summary
        };
    }

    public static DateTime StartOfWeek(DateTime date)
    {
        int difference = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        return date.AddDays(-difference).Date;
    }

    private static string BuildSummary(
        int productivityIndex,
        double weeklyChange,
        int consistency,
        int focusQuality)
    {
        string level = productivityIndex switch
        {
            >= 85 => "本週整體表現非常穩定",
            >= 70 => "本週整體表現良好",
            >= 50 => "本週表現普通，仍有改善空間",
            _ => "本週執行狀況偏弱，建議降低同時進行的任務數"
        };

        string trend = weeklyChange switch
        {
            > 20 => "專注時間較上週明顯成長",
            < -20 => "專注時間較上週下降",
            _ => "專注時間與上週接近"
        };

        string consistencyText = consistency >= 70
            ? "學習節奏穩定"
            : "可嘗試增加有紀錄的學習天數";

        string qualityText = focusQuality == 0
            ? "尚無足夠的專注品質評分"
            : focusQuality >= 80
                ? "自評專注品質良好"
                : "可減少中斷與分心來源";

        return $"{level}；{trend}；{consistencyText}；{qualityText}。";
    }
}
