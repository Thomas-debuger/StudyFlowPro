using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class AnalyticsForm : Form
{
    private const int ContentMinimumWidth = 1080;
    private const int ContentHeight = 840;

    public AnalyticsForm(DataService service)
    {
        Text = "學習分析中心";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1180, 800);
        MinimumSize = new Size(860, 620);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface(service);
    }

    private void BuildInterface(DataService service)
    {
        var scrollHost = new WheelScrollPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = UiTheme.Background,
            TabStop = true,
            AutoScrollMinSize = new Size(ContentMinimumWidth, ContentHeight)
        };

        var contentSurface = new Panel
        {
            Location = Point.Empty,
            Size = new Size(ContentMinimumWidth, ContentHeight),
            BackColor = UiTheme.Background,
            Margin = Padding.Empty
        };

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(28, 8, 20, 8),
            Margin = Padding.Empty
        };
        var headerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116));

        Control headerText = UiTheme.StackedHeader(
            "學習分析中心",
            "以任務、課程、專注品質與估時資料產生視覺化洞察",
            out _,
            23);
        Button closeButton = UiTheme.SecondaryButton("關閉");
        closeButton.Dock = DockStyle.Fill;
        closeButton.Margin = new Padding(12, 22, 0, 22);
        closeButton.Click += (_, _) => Close();

        headerLayout.Controls.Add(headerText, 0, 0);
        headerLayout.Controls.Add(closeButton, 1, 0);
        header.Controls.Add(headerLayout);

        var canvas = new AnalyticsCanvas(service)
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(canvas, 0, 1);
        contentSurface.Controls.Add(root);
        scrollHost.Controls.Add(contentSurface);
        Controls.Add(scrollHost);

        void ResizeSurface()
        {
            int availableWidth = Math.Max(
                0,
                scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            contentSurface.Width = Math.Max(ContentMinimumWidth, availableWidth);
            contentSurface.Height = ContentHeight;
            canvas.Invalidate();
        }

        scrollHost.Resize += (_, _) => ResizeSurface();
        scrollHost.MouseEnter += (_, _) => scrollHost.Focus();
        contentSurface.MouseEnter += (_, _) => scrollHost.Focus();
        canvas.MouseEnter += (_, _) => scrollHost.Focus();
        ResizeSurface();
    }
}

public sealed class AnalyticsCanvas : Control
{
    private readonly DataService _service;

    public AnalyticsCanvas(DataService service)
    {
        _service = service;
        DoubleBuffered = true;
        BackColor = UiTheme.Background;
        Resize += (_, _) => Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        AppData data = _service.Data;
        ProductivityMetrics metrics = ResearchMetricsService.Calculate(data);
        int margin = 24;
        int width = Math.Max(920, ClientSize.Width - margin * 2);

        DrawMetricCards(graphics, metrics, margin, 24, width);

        int chartTop = 150;
        int gap = 18;
        int leftWidth = (width - gap) / 2;
        int rightWidth = width - gap - leftWidth;

        DrawFocusChart(graphics, data, new Rectangle(margin, chartTop, leftWidth, 280));
        DrawCourseChart(graphics, data, new Rectangle(margin + leftWidth + gap, chartTop, rightWidth, 280));
        DrawPriorityChart(graphics, data, new Rectangle(margin, chartTop + 300, width, 230));
    }

    private static void DrawMetricCards(
        Graphics graphics,
        ProductivityMetrics metrics,
        int x,
        int y,
        int totalWidth)
    {
        string[] labels = { "生產力指數", "本週專注", "專注品質", "估時準確度" };
        string[] values =
        {
            metrics.ProductivityIndex + "/100",
            metrics.WeeklyFocusMinutes + " 分",
            metrics.FocusQualityScore == 0 ? "待收集" : metrics.FocusQualityScore + "%",
            metrics.EstimationAccuracy == 0 ? "待收集" : metrics.EstimationAccuracy + "%"
        };
        Color[] colors = { UiTheme.Primary, UiTheme.Success, UiTheme.Purple, UiTheme.Warning };

        int gap = 16;
        int cardWidth = (totalWidth - gap * 3) / 4;

        for (int index = 0; index < 4; index++)
        {
            Rectangle rectangle = new(x + index * (cardWidth + gap), y, cardWidth, 102);
            FillRounded(graphics, rectangle, 14, UiTheme.Surface);
            DrawRoundedBorder(graphics, rectangle, 14, UiTheme.Border);

            using var accentBrush = new SolidBrush(colors[index]);
            graphics.FillRectangle(accentBrush, rectangle.X, rectangle.Y, 6, rectangle.Height);

            using Font valueFont = UiTheme.Font(20, FontStyle.Bold);
            using Font labelFont = UiTheme.Font(9.5f);
            using var valueBrush = new SolidBrush(UiTheme.Navy);
            using var labelBrush = new SolidBrush(UiTheme.Muted);

            graphics.DrawString(values[index], valueFont, valueBrush, rectangle.X + 20, rectangle.Y + 18);
            graphics.DrawString(labels[index], labelFont, labelBrush, rectangle.X + 20, rectangle.Y + 62);
        }
    }

    private static void DrawFocusChart(Graphics graphics, AppData data, Rectangle rectangle)
    {
        FillRounded(graphics, rectangle, 14, UiTheme.Surface);
        DrawRoundedBorder(graphics, rectangle, 14, UiTheme.Border);
        DrawChartTitle(graphics, "近 7 天專注分鐘與品質", rectangle);

        var values = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                DateTime day = DateTime.Today.AddDays(offset - 6);
                List<FocusSession> sessions = data.Sessions
                    .Where(session => session.StartedAt.Date == day)
                    .ToList();
                return new
                {
                    Day = day,
                    Minutes = sessions.Sum(session => session.DurationMinutes),
                    Quality = sessions.Where(session => session.FocusQuality > 0)
                        .Select(session => session.FocusQuality)
                        .DefaultIfEmpty(0)
                        .Average()
                };
            })
            .ToList();

        int max = Math.Max(30, values.Max(item => item.Minutes));

        // 保留固定的兩行底部標籤區：第一行日期、第二行品質。
        // 這樣在 125%～200% Windows 顯示縮放時，Q4.0 / Q5.0 不會貼到卡片邊界或被裁切。
        int footerHeight = 52;
        Rectangle area = new(
            rectangle.X + 44,
            rectangle.Y + 66,
            rectangle.Width - 64,
            rectangle.Height - 66 - footerHeight);

        int gap = 10;
        int barWidth = Math.Max(14, (area.Width - gap * 6) / 7);

        using var axisPen = new Pen(UiTheme.Border);
        graphics.DrawLine(axisPen, area.Left, area.Bottom, area.Right, area.Bottom);
        using Font dateFont = UiTheme.Font(8.5f);
        using Font qualityFont = UiTheme.Font(8.2f, FontStyle.Bold);
        using var dateBrush = new SolidBrush(UiTheme.Muted);
        using var qualityBrush = new SolidBrush(UiTheme.Purple);

        for (int index = 0; index < values.Count; index++)
        {
            int height = (int)Math.Round(values[index].Minutes * 1.0 / max * Math.Max(20, area.Height - 24));
            Rectangle bar = new(
                area.Left + index * (barWidth + gap),
                area.Bottom - height,
                barWidth,
                height);

            if (height > 0)
                FillRounded(graphics, bar, 6, UiTheme.Primary);

            string day = values[index].Day.ToString("MM/dd");
            SizeF daySize = graphics.MeasureString(day, dateFont);
            graphics.DrawString(
                day,
                dateFont,
                dateBrush,
                bar.X + (bar.Width - daySize.Width) / 2,
                area.Bottom + 5);

            if (values[index].Minutes > 0)
            {
                string value = values[index].Minutes.ToString();
                SizeF valueSize = graphics.MeasureString(value, dateFont);
                graphics.DrawString(
                    value,
                    dateFont,
                    dateBrush,
                    bar.X + (bar.Width - valueSize.Width) / 2,
                    Math.Max(area.Top, bar.Y - 18));
            }

            if (values[index].Quality > 0)
            {
                string quality = $"Q{values[index].Quality:0.0}";
                SizeF qualitySize = graphics.MeasureString(quality, qualityFont);
                graphics.DrawString(
                    quality,
                    qualityFont,
                    qualityBrush,
                    bar.X + (bar.Width - qualitySize.Width) / 2,
                    area.Bottom + 23);
            }
        }
    }

    private static void DrawCourseChart(Graphics graphics, AppData data, Rectangle rectangle)
    {
        FillRounded(graphics, rectangle, 14, UiTheme.Surface);
        DrawRoundedBorder(graphics, rectangle, 14, UiTheme.Border);
        DrawChartTitle(graphics, "各課程完成率", rectangle);

        var rows = data.Courses
            .Select(course =>
            {
                List<StudyTask> tasks = data.Tasks.Where(task => task.CourseId == course.Id).ToList();
                // 課程完成率應反映底下每一項任務目前的實際進度，
                // 而不是只有在任務勾選完成後才從 0% 跳到 100%。
                // 例如課程只有一項進度 13% 的任務，課程完成率就顯示 13%。
                int rate = tasks.Count == 0
                    ? 0
                    : (int)Math.Round(tasks.Average(task => task.ProgressPercent));
                return new { Course = course, Rate = rate, Count = tasks.Count };
            })
            .OrderByDescending(item => item.Count)
            .Take(5)
            .ToList();

        if (rows.Count == 0)
        {
            DrawEmpty(graphics, rectangle, "尚無課程資料");
            return;
        }

        int startY = rectangle.Y + 68;
        int rowHeight = 38;
        int labelWidth = Math.Min(145, rectangle.Width / 3);
        int barX = rectangle.X + 22 + labelWidth;
        int barWidth = rectangle.Width - labelWidth - 80;

        using Font labelFont = UiTheme.Font(9);
        using Font valueFont = UiTheme.Font(9, FontStyle.Bold);
        using var labelBrush = new SolidBrush(UiTheme.Slate);

        for (int index = 0; index < rows.Count; index++)
        {
            int y = startY + index * rowHeight;
            string name = rows[index].Course.Name.Length > 10
                ? rows[index].Course.Name[..10] + "…"
                : rows[index].Course.Name;
            graphics.DrawString(name, labelFont, labelBrush, rectangle.X + 22, y + 3);

            Rectangle track = new(barX, y + 7, barWidth, 13);
            FillRounded(graphics, track, 6, UiTheme.Border);

            Color color;
            try
            {
                color = ColorTranslator.FromHtml(rows[index].Course.ColorHex);
            }
            catch
            {
                color = UiTheme.Primary;
            }

            Rectangle fill = new(
                track.X,
                track.Y,
                Math.Max(0, (int)(track.Width * rows[index].Rate / 100.0)),
                track.Height);
            if (fill.Width > 0)
                FillRounded(graphics, fill, 6, color);

            graphics.DrawString(rows[index].Rate + "%", valueFont, labelBrush, track.Right + 8, y + 1);
        }
    }

    private static void DrawPriorityChart(Graphics graphics, AppData data, Rectangle rectangle)
    {
        FillRounded(graphics, rectangle, 14, UiTheme.Surface);
        DrawRoundedBorder(graphics, rectangle, 14, UiTheme.Border);
        DrawChartTitle(graphics, "未完成任務優先級分布", rectangle);

        var items = new[]
        {
            new { Name = "立即處理", Minimum = 80, Maximum = 100, Color = UiTheme.Danger },
            new { Name = "高度優先", Minimum = 60, Maximum = 79, Color = UiTheme.Warning },
            new { Name = "中度優先", Minimum = 40, Maximum = 59, Color = UiTheme.Primary },
            new { Name = "可排程", Minimum = 0, Maximum = 39, Color = UiTheme.Success }
        };

        List<int> scores = data.Tasks
            .Where(task => !task.IsCompleted)
            .Select(task => SmartPlanner.CalculateScore(task))
            .ToList();
        int total = scores.Count;
        int gap = 18;
        int width = (rectangle.Width - 56 - gap * 3) / 4;

        for (int index = 0; index < items.Length; index++)
        {
            int count = scores.Count(score => score >= items[index].Minimum && score <= items[index].Maximum);
            Rectangle card = new(
                rectangle.X + 28 + index * (width + gap),
                rectangle.Y + 78,
                width,
                106);
            FillRounded(graphics, card, 12, UiTheme.SurfaceAlt);

            using var circleBrush = new SolidBrush(items[index].Color);
            graphics.FillEllipse(circleBrush, card.X + 18, card.Y + 18, 16, 16);
            using Font nameFont = UiTheme.Font(10, FontStyle.Bold);
            using Font countFont = UiTheme.Font(19, FontStyle.Bold);
            using Font smallFont = UiTheme.Font(9);
            using var textBrush = new SolidBrush(UiTheme.Navy);
            using var mutedBrush = new SolidBrush(UiTheme.Muted);

            graphics.DrawString(items[index].Name, nameFont, textBrush, card.X + 42, card.Y + 15);
            graphics.DrawString(count.ToString(), countFont, textBrush, card.X + 18, card.Y + 49);
            string percent = total == 0 ? "0%" : $"{Math.Round(count * 100.0 / total):0}%";
            graphics.DrawString(percent, smallFont, mutedBrush, card.X + 60, card.Y + 60);
        }
    }

    private static void DrawChartTitle(Graphics graphics, string title, Rectangle rectangle)
    {
        using Font font = UiTheme.Font(12, FontStyle.Bold);
        using var brush = new SolidBrush(UiTheme.Navy);
        graphics.DrawString(title, font, brush, rectangle.X + 20, rectangle.Y + 18);
    }

    private static void DrawEmpty(Graphics graphics, Rectangle rectangle, string text)
    {
        using Font font = UiTheme.Font(10);
        using var brush = new SolidBrush(UiTheme.Muted);
        SizeF size = graphics.MeasureString(text, font);
        graphics.DrawString(
            text,
            font,
            brush,
            rectangle.X + (rectangle.Width - size.Width) / 2,
            rectangle.Y + (rectangle.Height - size.Height) / 2);
    }

    private static void FillRounded(Graphics graphics, Rectangle rectangle, int radius, Color color)
    {
        if (rectangle.Width <= 0 || rectangle.Height <= 0)
            return;

        using GraphicsPath path = CreateRoundedPath(rectangle, radius);
        using var brush = new SolidBrush(color);
        graphics.FillPath(brush, path);
    }

    private static void DrawRoundedBorder(Graphics graphics, Rectangle rectangle, int radius, Color color)
    {
        using GraphicsPath path = CreateRoundedPath(
            new Rectangle(rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1),
            radius);
        using var pen = new Pen(color);
        graphics.DrawPath(pen, path);
    }

    private static GraphicsPath CreateRoundedPath(Rectangle rectangle, int radius)
    {
        int diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
