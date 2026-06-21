namespace StudyFlowPro.Models;

public sealed class ProfessorContact
{
    public string ChineseName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ResearchAreas { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }

    public string DisplayName => string.IsNullOrWhiteSpace(EnglishName)
        ? ChineseName
        : $"{ChineseName} / {EnglishName}";

    public string ContactSummary
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Office))
                parts.Add($"辦公室 {Office}");
            if (!string.IsNullOrWhiteSpace(Phone))
                parts.Add(Phone);
            return parts.Count == 0 ? "未提供辦公室與電話" : string.Join("｜", parts);
        }
    }
}
