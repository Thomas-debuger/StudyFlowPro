using System.Net;
using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public static class HtmlReportService
{
    public static void ExportWeeklyReport(string path, AppData data)
    {
        ProductivityMetrics metrics = ResearchMetricsService.Calculate(data);
        DateTime weekStart = ResearchMetricsService.StartOfWeek(DateTime.Today);
        DateTime weekEnd = weekStart.AddDays(6);

        string focusChart = BuildFocusSvg(data, weekStart);
        string courseBars = BuildCourseBars(data);
        string topTasks = BuildTopTasks(data);

        // 使用「一般逐字字串 + 明確代換標記」建立 HTML。
        // 這樣 CSS 中大量的大括號不會被 C# 的插值字串解析器誤判。
        string template = @"<!DOCTYPE html>
<html lang='zh-Hant'>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'>
<title>StudyFlow Pro 週報</title>
<style>
:root{--navy:#0f172a;--slate:#334155;--muted:#64748b;--line:#e2e8f0;--bg:#f8fafc;--blue:#2563eb;--green:#059669;--purple:#7c3aed;--orange:#d97706;--red:#dc2626}
*{box-sizing:border-box}body{margin:0;background:var(--bg);font-family:'Microsoft JhengHei UI','Noto Sans TC',sans-serif;color:var(--slate)}
.container{max-width:1120px;margin:0 auto;padding:40px 24px 64px}.hero{background:linear-gradient(135deg,#0f172a,#1d4ed8);color:#fff;border-radius:24px;padding:32px;box-shadow:0 18px 50px rgba(15,23,42,.18)}
.hero h1{margin:0 0 8px;font-size:34px}.hero p{margin:0;color:#bfdbfe}.grid{display:grid;grid-template-columns:repeat(4,1fr);gap:16px;margin:20px 0}.card{background:#fff;border:1px solid var(--line);border-radius:18px;padding:20px;box-shadow:0 6px 20px rgba(15,23,42,.05)}
.metric{font-size:30px;font-weight:800;color:var(--navy)}.label{font-size:14px;color:var(--muted);margin-top:6px}.section{margin-top:20px}.section h2{margin:0 0 14px;color:var(--navy);font-size:20px}.two{display:grid;grid-template-columns:1.2fr .8fr;gap:18px}.summary{font-size:16px;line-height:1.8;border-left:5px solid var(--blue)}
.bar-row{display:grid;grid-template-columns:150px 1fr 54px;gap:12px;align-items:center;margin:12px 0}.track{height:12px;background:#e2e8f0;border-radius:999px;overflow:hidden}.fill{height:100%;background:linear-gradient(90deg,#2563eb,#7c3aed);border-radius:999px}
table{width:100%;border-collapse:collapse}th,td{text-align:left;padding:13px 10px;border-bottom:1px solid var(--line);font-size:14px}th{color:var(--muted);font-weight:700}.score{font-weight:800;color:var(--blue)}
.footer{margin-top:32px;color:var(--muted);font-size:13px;text-align:center}@media(max-width:800px){.grid{grid-template-columns:repeat(2,1fr)}.two{grid-template-columns:1fr}.bar-row{grid-template-columns:110px 1fr 45px}}
@media print{body{background:#fff}.container{padding:0}.hero,.card{box-shadow:none}}
</style>
</head>
<body>
<div class='container'>
<section class='hero'>
<h1>StudyFlow Pro｜每週學習報告</h1>
<p>@@USER@@・@@WEEK_START@@－@@WEEK_END@@・產生時間 @@GENERATED_AT@@</p>
</section>

<section class='grid'>
<div class='card'><div class='metric'>@@PRODUCTIVITY@@</div><div class='label'>生產力指數 / 100</div></div>
<div class='card'><div class='metric'>@@WEEKLY_FOCUS@@ 分</div><div class='label'>本週專注時間</div></div>
<div class='card'><div class='metric'>@@COMPLETION@@%</div><div class='label'>任務完成率</div></div>
<div class='card'><div class='metric'>@@CONSISTENCY@@%</div><div class='label'>學習一致性</div></div>
</section>

<section class='card summary'>
<strong>系統洞察：</strong>@@SUMMARY@@<br>
本週相較上週專注時間變化：<strong>@@WEEKLY_CHANGE@@%</strong>；深度工作時間：<strong>@@DEEP_WORK@@ 分鐘</strong>；估時準確度：<strong>@@ESTIMATION@@%</strong>。
</section>

<section class='section two'>
<div class='card'><h2>近七天專注趨勢</h2>@@FOCUS_CHART@@</div>
<div class='card'><h2>各課程任務完成率</h2>@@COURSE_BARS@@</div>
</section>

<section class='section card'>
<h2>智慧優先任務</h2>
@@TOP_TASKS@@
</section>

<div class='footer'>本報告由 StudyFlow Pro Research Edition 離線產生。指標用於自我追蹤，不等同醫療或心理評估。</div>
</div>
</body>
</html>";

        string html = template
            .Replace("@@USER@@", WebUtility.HtmlEncode(data.Settings.UserName))
            .Replace("@@WEEK_START@@", weekStart.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture))
            .Replace("@@WEEK_END@@", weekEnd.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture))
            .Replace("@@GENERATED_AT@@", DateTime.Now.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture))
            .Replace("@@PRODUCTIVITY@@", metrics.ProductivityIndex.ToString(CultureInfo.InvariantCulture))
            .Replace("@@WEEKLY_FOCUS@@", metrics.WeeklyFocusMinutes.ToString(CultureInfo.InvariantCulture))
            .Replace("@@COMPLETION@@", metrics.CompletionRate.ToString(CultureInfo.InvariantCulture))
            .Replace("@@CONSISTENCY@@", metrics.ConsistencyScore.ToString(CultureInfo.InvariantCulture))
            .Replace("@@SUMMARY@@", WebUtility.HtmlEncode(metrics.Summary))
            .Replace("@@WEEKLY_CHANGE@@", metrics.WeeklyChangePercent.ToString("+0.0;-0.0;0", CultureInfo.InvariantCulture))
            .Replace("@@DEEP_WORK@@", metrics.DeepWorkMinutes.ToString(CultureInfo.InvariantCulture))
            .Replace("@@ESTIMATION@@", metrics.EstimationAccuracy.ToString(CultureInfo.InvariantCulture))
            .Replace("@@FOCUS_CHART@@", focusChart)
            .Replace("@@COURSE_BARS@@", courseBars)
            .Replace("@@TOP_TASKS@@", topTasks);

        File.WriteAllText(path, html, new UTF8Encoding(false));
    }

    private static string BuildFocusSvg(AppData data, DateTime weekStart)
    {
        var values = Enumerable.Range(0, 7)
            .Select(index =>
            {
                DateTime day = weekStart.AddDays(index);
                int minutes = data.Sessions
                    .Where(session => session.StartedAt.Date == day)
                    .Sum(session => session.DurationMinutes);
                return new { Day = day, Minutes = minutes };
            })
            .ToList();

        int max = Math.Max(30, values.Max(item => item.Minutes));
        const int width = 620;
        const int height = 240;
        const int baseline = 190;
        const int barWidth = 48;
        const int gap = 34;

        var builder = new StringBuilder();
        builder.Append($"<svg viewBox='0 0 {width} {height}' role='img' aria-label='近七天專注分鐘長條圖' style='width:100%;height:auto'>");
        builder.Append("<line x1='30' y1='190' x2='600' y2='190' stroke='#cbd5e1' stroke-width='2'/>");

        for (int index = 0; index < values.Count; index++)
        {
            int x = 48 + index * (barWidth + gap);
            int barHeight = (int)Math.Round(values[index].Minutes * 140.0 / max);
            int y = baseline - barHeight;
            builder.Append($"<rect x='{x}' y='{y}' width='{barWidth}' height='{barHeight}' rx='8' fill='#2563eb'/>");
            builder.Append($"<text x='{x + barWidth / 2}' y='{Math.Max(18, y - 8)}' text-anchor='middle' font-size='13' fill='#334155'>{values[index].Minutes}</text>");
            builder.Append($"<text x='{x + barWidth / 2}' y='216' text-anchor='middle' font-size='12' fill='#64748b'>{values[index].Day:MM/dd}</text>");
        }

        builder.Append("</svg>");
        return builder.ToString();
    }

    private static string BuildCourseBars(AppData data)
    {
        var rows = data.Courses
            .Select(course =>
            {
                List<StudyTask> tasks = data.Tasks.Where(task => task.CourseId == course.Id).ToList();
                int rate = tasks.Count == 0
                    ? 0
                    : (int)Math.Round(tasks.Count(task => task.IsCompleted) * 100.0 / tasks.Count);
                return new { course.Name, Rate = rate, Count = tasks.Count };
            })
            .OrderByDescending(row => row.Count)
            .Take(6)
            .ToList();

        if (rows.Count == 0)
            return "<p>尚無課程資料。</p>";

        var builder = new StringBuilder();
        foreach (var row in rows)
        {
            builder.Append("<div class='bar-row'>");
            builder.Append($"<span>{WebUtility.HtmlEncode(row.Name)}</span>");
            builder.Append($"<div class='track'><div class='fill' style='width:{row.Rate}%'></div></div>");
            builder.Append($"<strong>{row.Rate}%</strong>");
            builder.Append("</div>");
        }

        return builder.ToString();
    }

    private static string BuildTopTasks(AppData data)
    {
        List<StudyTask> tasks = SmartPlanner.RankTasks(data.Tasks).Take(8).ToList();
        if (tasks.Count == 0)
            return "<p>目前沒有未完成任務。</p>";

        var builder = new StringBuilder();
        builder.Append("<table><thead><tr><th>任務</th><th>課程</th><th>截止時間</th><th>進度</th><th>智慧分數 / 100</th></tr></thead><tbody>");

        foreach (StudyTask task in tasks)
        {
            string course = data.Courses.FirstOrDefault(item => item.Id == task.CourseId)?.Name ?? "未分類";
            builder.Append("<tr>");
            builder.Append($"<td>{WebUtility.HtmlEncode(task.Title)}</td>");
            builder.Append($"<td>{WebUtility.HtmlEncode(course)}</td>");
            builder.Append($"<td>{task.DueDate:yyyy/MM/dd HH:mm}</td>");
            builder.Append($"<td>{task.ProgressPercent}%</td>");
            builder.Append($"<td class='score'>{SmartPlanner.CalculateScore(task)}</td>");
            builder.Append("</tr>");
        }

        builder.Append("</tbody></table>");
        return builder.ToString();
    }
}
