namespace StudyFlowPro.Models;

public sealed class TaskRow
{
    public Guid Id { get; init; }
    public string Pin { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Course { get; init; } = string.Empty;
    public string CourseColorHex { get; init; } = "#2563EB";
    public string Priority { get; init; } = string.Empty;
    public string DueDate { get; init; } = string.Empty;
    public string Progress { get; init; } = string.Empty;
    public int SmartScore { get; init; }
}

public sealed class CourseRow
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ColorHex { get; init; } = "#2563EB";
    public string Instructor { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public int OpenTasks { get; init; }
    public string CompletionRate { get; init; } = string.Empty;
}

public sealed class SessionRow
{
    public Guid Id { get; init; }
    public string Date { get; init; } = string.Empty;
    public string Task { get; init; } = string.Empty;
    public string Course { get; init; } = string.Empty;
    public int Minutes { get; init; }
    public string Quality { get; init; } = string.Empty;
    public int Distractions { get; init; }
    public string Note { get; init; } = string.Empty;
}

public sealed class ActivityRow
{
    public string Time { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

public sealed class CourseChoice
{
    public Guid? Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public override string ToString() => Text;
}

public sealed class TaskChoice
{
    public Guid? Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public override string ToString() => Text;
}

public sealed class PriorityChoice
{
    public TaskPriority Value { get; init; }
    public string Text { get; init; } = string.Empty;
    public override string ToString() => Text;
}


public sealed class ExamSubjectRow
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ColorHex { get; init; } = string.Empty;
    public int PaperCount { get; init; }
    public string Progress { get; init; } = string.Empty;
}

public sealed class ExamPaperRow
{
    public Guid Id { get; init; }
    public string Favorite { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Year { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Format { get; init; } = string.Empty;
    public string ImportedAt { get; init; } = string.Empty;
    public int OpenCount { get; init; }
    public string FileState { get; init; } = string.Empty;
}
