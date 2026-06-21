namespace StudyFlowPro.Models;

/// <summary>
/// 每個帳號自己的介面與使用偏好。這份檔案和學習資料分開保存，
/// 避免匯入／還原其他資料時誤把別人的視覺風格或提醒設定帶進來。
/// </summary>
public sealed class UserProfilePreferences
{
    public int SchemaVersion { get; set; } = 1;
    public Guid OwnerAccountId { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public VisualStyleKind VisualStyle { get; set; } = VisualStyleKind.Facebook;
    public int DefaultFocusMinutes { get; set; } = 25;
    public int DailyGoalMinutes { get; set; } = 120;
    public bool ShowDueSoonReminder { get; set; } = true;
    public int DueSoonHours { get; set; } = 24;
    public string LastTimetableSemester { get; set; } = "114-2";
    public DateTime? LastBackupAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
