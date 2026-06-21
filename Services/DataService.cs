using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public sealed class DataService
{
    private const int CurrentSchemaVersion = 9;
    private const int CurrentPreferencesSchemaVersion = 1;
    private readonly UserAccount _currentUser;
    private readonly AccountService _accountService;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public event EventHandler DataChanged;

    public DataService(UserAccount currentUser, AccountService accountService)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
    }

    public UserAccount CurrentUser => _currentUser;
    public AppData Data { get; private set; } = new();

    public string DataDirectory => _accountService.GetUserDirectory(_currentUser);

    public string DataPath => Path.Combine(DataDirectory, "studyflow-data.json");
    public string LastGoodBackupPath => Path.Combine(DataDirectory, "studyflow-data.lastgood.json");
    public string ProfileSettingsPath => Path.Combine(DataDirectory, "profile-settings.json");
    public string ProfileSettingsBackupPath => Path.Combine(DataDirectory, "profile-settings.lastgood.json");
    public string SnapshotDirectory => Path.Combine(DataDirectory, "Snapshots");
    public string BackupDirectory => Path.Combine(DataDirectory, "Backups");
    public string ExamLibraryDirectory => Path.Combine(DataDirectory, "ExamLibrary");
    public string ExamFilesDirectory => Path.Combine(ExamLibraryDirectory, "Files");
    public string ExamPreviewDirectory => Path.Combine(ExamLibraryDirectory, "PreviewCache");

    public void Load()
    {
        EnsureAccountDirectories();

        if (!File.Exists(DataPath))
        {
            Data = CreateNewUserData(_currentUser.DisplayName);
            ClaimDataForCurrentAccount(Data);
            ApplyStoredProfilePreferences();
            Save();
            return;
        }

        try
        {
            string json = File.ReadAllText(DataPath, Encoding.UTF8);
            AppData loaded = JsonSerializer.Deserialize<AppData>(json, _options) ?? new AppData();
            EnsureDataBelongsToCurrentAccount(loaded, DataPath);
            Data = loaded;
            NormalizeAndMigrate();
            ApplyStoredProfilePreferences();
            NormalizePersonalSettings();
            Save(); // 將舊版資料補上帳號所有權與獨立偏好檔。
        }
        catch (InvalidDataException)
        {
            PreserveForeignAccountFile(DataPath);
            Data = CreateNewUserData(_currentUser.DisplayName);
            ClaimDataForCurrentAccount(Data);
            ApplyStoredProfilePreferences();
            Save();
        }
        catch
        {
            PreserveBrokenFile();
            Data = TryReadOwnedData(LastGoodBackupPath)
                ?? CreateNewUserData(_currentUser.DisplayName);
            NormalizeAndMigrate();
            ApplyStoredProfilePreferences();
            NormalizePersonalSettings();
            Save();
        }
    }

    public void Save()
    {
        EnsureAccountDirectories();
        NormalizeAndMigrate();
        ClaimDataForCurrentAccount(Data);

        string json = JsonSerializer.Serialize(Data, _options);
        string tempPath = DataPath + ".tmp";

        File.WriteAllText(tempPath, json, new UTF8Encoding(false));

        if (File.Exists(DataPath))
            File.Copy(DataPath, LastGoodBackupPath, true);

        File.Move(tempPath, DataPath, true);
        SaveProfilePreferences();
        CreateRollingSnapshotIfNeeded();
    }

    public void SaveAndNotify()
    {
        Save();
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool UpdateDisplayName(string displayName, out string error)
    {
        if (!_accountService.UpdateDisplayName(_currentUser, displayName, out error))
            return false;

        Data.Settings.UserName = _currentUser.DisplayName;
        return true;
    }

    public void Log(ActivityType type, string entityType, Guid? entityId, string summary)
    {
        Data.Activities.Add(new ActivityLogEntry
        {
            Type = type,
            EntityType = entityType,
            EntityId = entityId,
            Summary = summary,
            OccurredAt = DateTime.Now
        });

        if (Data.Activities.Count > 1000)
        {
            Data.Activities = Data.Activities
                .OrderByDescending(item => item.OccurredAt)
                .Take(1000)
                .OrderBy(item => item.OccurredAt)
                .ToList();
        }
    }

    public void BackupTo(string path)
    {
        Save();
        File.Copy(DataPath, path, true);
        Data.Settings.LastBackupAt = DateTime.Now;
        Log(ActivityType.BackedUp, "System", null, $"建立備份：{Path.GetFileName(path)}");
        SaveAndNotify();
    }

    public void RestoreFrom(string path)
    {
        string json = File.ReadAllText(path, Encoding.UTF8);
        AppData restored = JsonSerializer.Deserialize<AppData>(json, _options)
            ?? throw new InvalidDataException("備份內容無法解析。");

        EnsureImportedDataCanBeUsed(restored);
        Data = restored;
        ClaimDataForCurrentAccount(Data);
        NormalizeAndMigrate();
        Log(ActivityType.Restored, "System", null, $"從目前帳號的備份還原：{Path.GetFileName(path)}");
        SaveAndNotify();
    }


    public void ImportDataFile(string path)
    {
        string json = File.ReadAllText(path, Encoding.UTF8);
        AppData imported = JsonSerializer.Deserialize<AppData>(json, _options)
            ?? throw new InvalidDataException("資料檔內容無法解析。");

        EnsureImportedDataCanBeUsed(imported);
        Save(); // 匯入前先保留目前帳號資料到 last-good 與 rolling snapshot。
        Data = imported;
        ClaimDataForCurrentAccount(Data);
        NormalizeAndMigrate();
        Log(ActivityType.Imported, "System", null, $"匯入目前帳號資料檔：{Path.GetFileName(path)}");
        SaveAndNotify();
    }

    public void ResetToDemo()
    {
        UserProfilePreferences preferences = CaptureCurrentPreferences();
        Data = CreateDemoData();
        ClaimDataForCurrentAccount(Data);
        ApplyPreferences(preferences);
        Log(ActivityType.System, "System", null, "重建目前帳號的展示資料");
        SaveAndNotify();
    }

    public void ClearAll()
    {
        UserProfilePreferences preferences = CaptureCurrentPreferences();
        Data = new AppData
        {
            SchemaVersion = CurrentSchemaVersion,
            OwnerAccountId = _currentUser.Id,
            OwnerUsername = _currentUser.Username,
            ProfileCreatedAt = DateTime.Now,
            Settings = new AppSettings(),
            TimetableInitialized = true
        };
        ApplyPreferences(preferences);
        try
        {
            if (Directory.Exists(ExamLibraryDirectory))
                Directory.Delete(ExamLibraryDirectory, true);
        }
        catch
        {
            // 若檔案被其他程式開啟，清除資料仍可繼續；題庫健檢會顯示殘留狀態。
        }
        Directory.CreateDirectory(ExamFilesDirectory);
        Directory.CreateDirectory(ExamPreviewDirectory);
        Log(ActivityType.System, "System", null, "清除目前帳號的全部資料（含考古題索引與本機題庫檔案）");
        SaveAndNotify();
    }

    public string ExportRawJson(string path)
    {
        Save();
        File.Copy(DataPath, path, true);
        return path;
    }

    private void NormalizeAndMigrate()
    {
        Data ??= new AppData();
        Data.Tasks ??= new List<StudyTask>();
        Data.Courses ??= new List<Course>();
        Data.Sessions ??= new List<FocusSession>();
        Data.Activities ??= new List<ActivityLogEntry>();
        Data.ExamSubjects ??= new List<ExamSubject>();
        Data.ExamPapers ??= new List<ExamPaper>();
        Data.TimetableSemesters ??= new List<TimetableSemester>();
        Data.TimetableEntries ??= new List<TimetableEntry>();
        Data.SmartSchedule ??= new SmartScheduleState();
        Data.SmartSchedule.Blocks ??= new List<SmartScheduleBlockSnapshot>();
        Data.Settings ??= new AppSettings();
        ClaimDataForCurrentAccount(Data);
        Data.ProfileCreatedAt = Data.ProfileCreatedAt == default ? DateTime.Now : Data.ProfileCreatedAt;

        if (!Data.TimetableInitialized)
        {
            if (Data.TimetableEntries.Count == 0)
                Data.TimetableEntries = TimetableSeedData.Create();
            if (Data.TimetableSemesters.Count == 0)
                Data.TimetableSemesters = TimetableCatalog.CreateDefaultSemesters();
            Data.TimetableInitialized = true;
        }

        // v5.3：舊版只有課程項目、沒有可編輯學期清單。首次升級時建立四個預設學期。
        if (Data.TimetableSemesters.Count == 0)
            Data.TimetableSemesters = TimetableCatalog.CreateDefaultSemesters();

        Data.Tasks = Data.Tasks
            .Where(item => item != null)
            .GroupBy(item => item.Id)
            .Select(group => group.First())
            .ToList();

        Data.Courses = Data.Courses
            .Where(item => item != null)
            .GroupBy(item => item.Id)
            .Select(group => group.First())
            .ToList();

        Data.Sessions = Data.Sessions
            .Where(item => item != null)
            .GroupBy(item => item.Id)
            .Select(group => group.First())
            .ToList();

        Data.Activities = Data.Activities
            .Where(item => item != null)
            .GroupBy(item => item.Id)
            .Select(group => group.First())
            .OrderBy(item => item.OccurredAt)
            .ToList();

        Data.ExamSubjects = Data.ExamSubjects
            .Where(item => item != null)
            .GroupBy(item => item.Id)
            .Select(group => group.First())
            .ToList();

        Data.ExamPapers = Data.ExamPapers
            .Where(item => item != null)
            .GroupBy(item => item.Id)
            .Select(group => group.First())
            .ToList();

        Data.TimetableSemesters = Data.TimetableSemesters
            .Where(item => item != null && !string.IsNullOrWhiteSpace(item.Code))
            .GroupBy(item => item.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
        if (Data.TimetableSemesters.Count == 0)
            Data.TimetableSemesters = TimetableCatalog.CreateDefaultSemesters();

        Data.TimetableEntries = Data.TimetableEntries
            .Where(item => item != null)
            .GroupBy(item => item.Id)
            .Select(group => group.First())
            .ToList();

        foreach (StudyTask task in Data.Tasks)
        {
            task.Title ??= string.Empty;
            task.Description ??= string.Empty;
            task.Tags ??= string.Empty;
            task.EstimatedMinutes = Math.Max(1, task.EstimatedMinutes);
            task.FocusedMinutes = Math.Max(0, task.FocusedMinutes);
            task.Difficulty = task.Difficulty is >= 1 and <= 5 ? task.Difficulty : 3;
            task.EnergyRequired = task.EnergyRequired is >= 1 and <= 5 ? task.EnergyRequired : 3;
            task.CreatedAt = task.CreatedAt == default ? DateTime.Now : task.CreatedAt;
            task.UpdatedAt = task.UpdatedAt == default ? task.CreatedAt : task.UpdatedAt;
        }

        foreach (FocusSession session in Data.Sessions)
        {
            session.TaskNameSnapshot ??= string.Empty;
            session.CourseNameSnapshot ??= string.Empty;
            session.Note ??= string.Empty;
            session.DurationMinutes = Math.Max(1, session.DurationMinutes);
            session.FocusQuality = Math.Clamp(session.FocusQuality, 0, 5);
            session.DistractionCount = Math.Max(0, session.DistractionCount);
            session.StartedAt = session.StartedAt == default ? DateTime.Now : session.StartedAt;
            session.EndedAt = session.EndedAt == default ? session.StartedAt.AddMinutes(session.DurationMinutes) : session.EndedAt;
        }

        foreach (ExamSubject subject in Data.ExamSubjects)
        {
            subject.Name = string.IsNullOrWhiteSpace(subject.Name) ? "未命名科目" : subject.Name.Trim();
            subject.Description ??= string.Empty;
            subject.ColorHex = string.IsNullOrWhiteSpace(subject.ColorHex) ? "#2563EB" : subject.ColorHex;
            subject.CreatedAt = subject.CreatedAt == default ? DateTime.Now : subject.CreatedAt;
        }

        HashSet<Guid> examSubjectIds = Data.ExamSubjects.Select(subject => subject.Id).ToHashSet();
        Data.ExamPapers = Data.ExamPapers
            .Where(paper => examSubjectIds.Contains(paper.SubjectId))
            .ToList();

        foreach (ExamPaper paper in Data.ExamPapers)
        {
            paper.Title = string.IsNullOrWhiteSpace(paper.Title)
                ? Path.GetFileNameWithoutExtension(paper.OriginalFileName)
                : paper.Title.Trim();
            paper.OriginalFileName ??= string.Empty;
            paper.StoredFileName ??= string.Empty;
            paper.FileExtension = string.IsNullOrWhiteSpace(paper.FileExtension)
                ? Path.GetExtension(paper.OriginalFileName).ToLowerInvariant()
                : paper.FileExtension.ToLowerInvariant();
            paper.Sha256 ??= string.Empty;
            paper.ExamYear ??= string.Empty;
            paper.Term ??= string.Empty;
            paper.Category ??= string.Empty;
            paper.Tags ??= string.Empty;
            paper.Notes ??= string.Empty;
            paper.FileSizeBytes = Math.Max(0, paper.FileSizeBytes);
            paper.OpenCount = Math.Max(0, paper.OpenCount);
            paper.ImportedAt = paper.ImportedAt == default ? DateTime.Now : paper.ImportedAt;
            paper.UpdatedAt = paper.UpdatedAt == default ? paper.ImportedAt : paper.UpdatedAt;
            paper.Status = Enum.IsDefined(paper.Status) ? paper.Status : ExamPaperStatus.NotStarted;
        }

        foreach (TimetableSemester semester in Data.TimetableSemesters)
        {
            semester.Code = semester.Code.Trim();
            semester.DisplayName = string.IsNullOrWhiteSpace(semester.DisplayName)
                ? TimetableCatalog.SemesterDisplay(semester.Code)
                : semester.DisplayName.Trim();
            semester.CreatedAt = semester.CreatedAt == default ? DateTime.Now : semester.CreatedAt;
            if (TimetableCatalog.IsBuiltInSemester(semester.Code))
                semester.IsBuiltIn = true;
        }

        // 若舊資料或外部 JSON 含有未登錄的學期代碼，保留資料並自動補上學期項目。
        foreach (string code in Data.TimetableEntries
                     .Select(entry => entry.SemesterCode?.Trim())
                     .Where(code => !string.IsNullOrWhiteSpace(code))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .ToList())
        {
            if (Data.TimetableSemesters.Any(item => string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)))
                continue;
            Data.TimetableSemesters.Add(new TimetableSemester
            {
                Code = code!,
                DisplayName = TimetableCatalog.SemesterDisplay(code!),
                IsBuiltIn = TimetableCatalog.IsBuiltInSemester(code!),
                CreatedAt = DateTime.Now
            });
        }

        string fallbackSemester = Data.TimetableSemesters.First().Code;
        HashSet<string> semesterCodes = Data.TimetableSemesters
            .Select(item => item.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (TimetableEntry entry in Data.TimetableEntries)
        {
            entry.SemesterCode = !string.IsNullOrWhiteSpace(entry.SemesterCode) && semesterCodes.Contains(entry.SemesterCode)
                ? entry.SemesterCode.Trim()
                : fallbackSemester;
            entry.CourseName = string.IsNullOrWhiteSpace(entry.CourseName)
                ? "未命名課程"
                : entry.CourseName.Trim();
            entry.DayIndex = Math.Clamp(entry.DayIndex, 1, 6);
            entry.StartPeriod = Math.Clamp(entry.StartPeriod, 1, 10);
            entry.EndPeriod = Math.Clamp(entry.EndPeriod, entry.StartPeriod, 10);
            entry.Location ??= string.Empty;
            entry.Instructor ??= string.Empty;
            entry.ColorHex = string.IsNullOrWhiteSpace(entry.ColorHex) ? "#2563EB" : entry.ColorHex;
            entry.Notes ??= string.Empty;
            entry.CreatedAt = entry.CreatedAt == default ? DateTime.Now : entry.CreatedAt;
            entry.UpdatedAt = entry.UpdatedAt == default ? entry.CreatedAt : entry.UpdatedAt;
        }

        HashSet<Guid> courseIds = Data.Courses.Select(course => course.Id).ToHashSet();
        foreach (StudyTask task in Data.Tasks.Where(task => task.CourseId.HasValue && !courseIds.Contains(task.CourseId.Value)))
            task.CourseId = null;

        HashSet<Guid> taskIds = Data.Tasks.Select(task => task.Id).ToHashSet();
        foreach (FocusSession session in Data.Sessions.Where(session => session.TaskId.HasValue && !taskIds.Contains(session.TaskId.Value)))
            session.TaskId = null;

        // 帳號的顯示名稱是唯一來源。即使匯入舊資料或承接 legacy JSON，
        // 主畫面、側邊欄與設定頁仍必須顯示目前登入帳號註冊的名稱。
        Data.Settings.UserName = string.IsNullOrWhiteSpace(_currentUser.DisplayName)
            ? "同學"
            : _currentUser.DisplayName.Trim();
        if (!Enum.IsDefined(Data.Settings.VisualStyle))
            Data.Settings.VisualStyle = VisualStyleKind.Facebook;
        Data.Settings.DefaultFocusMinutes = Math.Clamp(Data.Settings.DefaultFocusMinutes, 1, 180);
        Data.Settings.DailyGoalMinutes = Math.Clamp(Data.Settings.DailyGoalMinutes, 10, 1000);
        Data.Settings.DueSoonHours = Math.Clamp(Data.Settings.DueSoonHours, 1, 168);

        // v7.10：智慧排程屬於目前登入帳號，保存最後一次重新產生的設定與結果。
        Data.SmartSchedule.AvailableMinutes = Math.Clamp(Data.SmartSchedule.AvailableMinutes, 30, 720);
        Data.SmartSchedule.BreakMinutes = Math.Clamp(Data.SmartSchedule.BreakMinutes, 0, 30);
        Data.SmartSchedule.FocusMinutes = Math.Max(0, Data.SmartSchedule.FocusMinutes);
        Data.SmartSchedule.BreakMinutesUsed = Math.Max(0, Data.SmartSchedule.BreakMinutesUsed);
        Data.SmartSchedule.BufferMinutes = Math.Max(0, Data.SmartSchedule.BufferMinutes);
        Data.SmartSchedule.PlainText ??= string.Empty;
        foreach (SmartScheduleBlockSnapshot block in Data.SmartSchedule.Blocks)
        {
            block.TaskTitle ??= string.Empty;
            block.CourseName ??= string.Empty;
            block.PriorityLevel ??= string.Empty;
            block.Minutes = Math.Max(0, block.Minutes);
            block.BreakAfterMinutes = Math.Max(0, block.BreakAfterMinutes);
            block.SmartScore = Math.Clamp(block.SmartScore, 0, 100);
        }

        Data.Settings.LastTimetableSemester = Data.TimetableSemesters.Any(item =>
                string.Equals(item.Code, Data.Settings.LastTimetableSemester, StringComparison.OrdinalIgnoreCase))
            ? Data.Settings.LastTimetableSemester
            : Data.TimetableSemesters.First().Code;
        Data.SchemaVersion = CurrentSchemaVersion;
    }

    private void EnsureAccountDirectories()
    {
        _accountService.PrepareUserDataDirectory(_currentUser);
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(SnapshotDirectory);
        Directory.CreateDirectory(BackupDirectory);
        Directory.CreateDirectory(ExamFilesDirectory);
        Directory.CreateDirectory(ExamPreviewDirectory);
    }

    private void ClaimDataForCurrentAccount(AppData data)
    {
        data.OwnerAccountId = _currentUser.Id;
        data.OwnerUsername = _currentUser.Username;
        data.ProfileCreatedAt = data.ProfileCreatedAt == default ? DateTime.Now : data.ProfileCreatedAt;
    }

    private void EnsureDataBelongsToCurrentAccount(AppData data, string sourcePath)
    {
        // v7.2 以前沒有 OwnerAccountId，因此空值代表可由目前所在的帳號資料夾承接。
        if (data.OwnerAccountId == Guid.Empty)
            return;

        if (data.OwnerAccountId != _currentUser.Id)
        {
            throw new InvalidDataException(
                $"資料檔 {Path.GetFileName(sourcePath)} 屬於其他帳號，系統已阻止跨帳號載入。");
        }
    }

    private void EnsureImportedDataCanBeUsed(AppData data)
    {
        if (data.OwnerAccountId == Guid.Empty || data.OwnerAccountId == _currentUser.Id)
            return;

        // 換電腦後帳號 GUID 可能改變；只要登入帳號名稱相同，允許明確選取備份後重新綁定。
        if (!string.IsNullOrWhiteSpace(data.OwnerUsername) &&
            string.Equals(data.OwnerUsername, _currentUser.Username, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        throw new InvalidDataException(
            "這份資料檔屬於其他登入帳號，為避免不同使用者的任務、課表、考古題與設定混在一起，系統已取消匯入。請改用原帳號登入後操作。");
    }

    private AppData? TryReadOwnedData(string path)
    {
        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            AppData data = JsonSerializer.Deserialize<AppData>(json, _options) ?? new AppData();
            EnsureDataBelongsToCurrentAccount(data, path);
            return data;
        }
        catch
        {
            return null;
        }
    }

    private UserProfilePreferences CaptureCurrentPreferences() => new()
    {
        SchemaVersion = CurrentPreferencesSchemaVersion,
        OwnerAccountId = _currentUser.Id,
        OwnerUsername = _currentUser.Username,
        VisualStyle = Data.Settings.VisualStyle,
        DefaultFocusMinutes = Data.Settings.DefaultFocusMinutes,
        DailyGoalMinutes = Data.Settings.DailyGoalMinutes,
        ShowDueSoonReminder = Data.Settings.ShowDueSoonReminder,
        DueSoonHours = Data.Settings.DueSoonHours,
        LastTimetableSemester = Data.Settings.LastTimetableSemester,
        LastBackupAt = Data.Settings.LastBackupAt,
        UpdatedAt = DateTime.Now
    };

    private void ApplyStoredProfilePreferences()
    {
        UserProfilePreferences? preferences = ReadProfilePreferences(ProfileSettingsPath)
            ?? ReadProfilePreferences(ProfileSettingsBackupPath);

        if (preferences == null)
            return;

        ApplyPreferences(preferences);
    }

    private UserProfilePreferences? ReadProfilePreferences(string path)
    {
        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            UserProfilePreferences? preferences = JsonSerializer.Deserialize<UserProfilePreferences>(json, _options);
            if (preferences == null)
                return null;

            if (preferences.OwnerAccountId != Guid.Empty && preferences.OwnerAccountId != _currentUser.Id)
            {
                PreserveForeignAccountFile(path);
                return null;
            }

            return preferences;
        }
        catch
        {
            return null;
        }
    }

    private void ApplyPreferences(UserProfilePreferences preferences)
    {
        Data.Settings ??= new AppSettings();
        Data.Settings.UserName = string.IsNullOrWhiteSpace(_currentUser.DisplayName)
            ? "同學"
            : _currentUser.DisplayName.Trim();
        Data.Settings.VisualStyle = Enum.IsDefined(preferences.VisualStyle)
            ? preferences.VisualStyle
            : VisualStyleKind.Facebook;
        Data.Settings.DefaultFocusMinutes = Math.Clamp(preferences.DefaultFocusMinutes, 1, 180);
        Data.Settings.DailyGoalMinutes = Math.Clamp(preferences.DailyGoalMinutes, 10, 1000);
        Data.Settings.ShowDueSoonReminder = preferences.ShowDueSoonReminder;
        Data.Settings.DueSoonHours = Math.Clamp(preferences.DueSoonHours, 1, 168);
        Data.Settings.LastTimetableSemester = preferences.LastTimetableSemester ?? string.Empty;
        Data.Settings.LastBackupAt = preferences.LastBackupAt;
    }

    private void NormalizePersonalSettings()
    {
        Data.Settings.UserName = string.IsNullOrWhiteSpace(_currentUser.DisplayName)
            ? "同學"
            : _currentUser.DisplayName.Trim();
        if (!Enum.IsDefined(Data.Settings.VisualStyle))
            Data.Settings.VisualStyle = VisualStyleKind.Facebook;
        Data.Settings.DefaultFocusMinutes = Math.Clamp(Data.Settings.DefaultFocusMinutes, 1, 180);
        Data.Settings.DailyGoalMinutes = Math.Clamp(Data.Settings.DailyGoalMinutes, 10, 1000);
        Data.Settings.DueSoonHours = Math.Clamp(Data.Settings.DueSoonHours, 1, 168);
        Data.Settings.LastTimetableSemester = Data.TimetableSemesters.Any(item =>
                string.Equals(item.Code, Data.Settings.LastTimetableSemester, StringComparison.OrdinalIgnoreCase))
            ? Data.Settings.LastTimetableSemester
            : Data.TimetableSemesters.First().Code;
    }

    private void SaveProfilePreferences()
    {
        UserProfilePreferences preferences = CaptureCurrentPreferences();
        string json = JsonSerializer.Serialize(preferences, _options);
        string tempPath = ProfileSettingsPath + ".tmp";
        File.WriteAllText(tempPath, json, new UTF8Encoding(false));

        if (File.Exists(ProfileSettingsPath))
            File.Copy(ProfileSettingsPath, ProfileSettingsBackupPath, true);

        File.Move(tempPath, ProfileSettingsPath, true);
    }

    private void PreserveForeignAccountFile(string sourcePath)
    {
        try
        {
            if (!File.Exists(sourcePath))
                return;
            string extension = Path.GetExtension(sourcePath);
            string name = Path.GetFileNameWithoutExtension(sourcePath);
            string destination = Path.Combine(
                DataDirectory,
                $"{name}.foreign-account-{DateTime.Now:yyyyMMdd-HHmmss}{extension}");
            File.Move(sourcePath, destination, true);
        }
        catch
        {
            // 隔離備份失敗不應阻止建立目前帳號的新資料。
        }
    }

    private AppData ReadOrEmpty(string path)
    {
        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            return JsonSerializer.Deserialize<AppData>(json, _options) ?? new AppData();
        }
        catch
        {
            return new AppData();
        }
    }

    private void PreserveBrokenFile()
    {
        try
        {
            string path = Path.Combine(
                DataDirectory,
                $"studyflow-data.broken-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            File.Copy(DataPath, path, true);
        }
        catch
        {
            // 保留失敗不應阻止程式啟動。
        }
    }

    private void CreateRollingSnapshotIfNeeded()
    {
        try
        {
            string[] existing = Directory.GetFiles(SnapshotDirectory, "snapshot-*.json");
            DateTime newest = existing.Length == 0
                ? DateTime.MinValue
                : existing.Select(File.GetLastWriteTime).Max();

            if (DateTime.Now - newest < TimeSpan.FromMinutes(10))
                return;

            string snapshotPath = Path.Combine(
                SnapshotDirectory,
                $"snapshot-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            File.Copy(DataPath, snapshotPath, true);

            foreach (string oldPath in Directory
                         .GetFiles(SnapshotDirectory, "snapshot-*.json")
                         .OrderByDescending(File.GetLastWriteTime)
                         .Skip(12))
            {
                File.Delete(oldPath);
            }
        }
        catch
        {
            // 快照是額外保護，失敗時不影響主要存檔。
        }
    }

    private static AppData CreateNewUserData(string displayName)
    {
        string name = string.IsNullOrWhiteSpace(displayName) ? "同學" : displayName.Trim();
        List<TimetableSemester> semesters = TimetableCatalog.CreateDefaultSemesters();
        return new AppData
        {
            SchemaVersion = CurrentSchemaVersion,
            TimetableSemesters = semesters,
            TimetableEntries = new List<TimetableEntry>(),
            TimetableInitialized = true,
            Settings = new AppSettings
            {
                UserName = name,
                DefaultFocusMinutes = 25,
                DailyGoalMinutes = 120,
                ShowDueSoonReminder = true,
                DueSoonHours = 24,
                LastTimetableSemester = semesters.First().Code,
                VisualStyle = VisualStyleKind.Facebook
            },
            Activities = new List<ActivityLogEntry>
            {
                new()
                {
                    Type = ActivityType.System,
                    EntityType = "Account",
                    Summary = $"建立 {name} 的全新個人學習空間",
                    OccurredAt = DateTime.Now
                }
            }
        };
    }

    private static AppData CreateDemoData()
    {
        var windowsForms = new Course
        {
            Name = "Windows 程式設計",
            Instructor = "王老師",
            Location = "電腦教室 A",
            ColorHex = "#2563EB"
        };
        var algorithm = new Course
        {
            Name = "演算法",
            Instructor = "陳老師",
            Location = "R1203",
            ColorHex = "#7C3AED"
        };
        var research = new Course
        {
            Name = "專題研究",
            Instructor = "指導教授",
            Location = "研究室",
            ColorHex = "#059669"
        };

        var finalProject = new StudyTask
        {
            Title = "完成 WinForms 期末專題",
            Description = "完成主程式、測試、README、使用說明與五分鐘 DEMO。",
            CourseId = windowsForms.Id,
            Priority = TaskPriority.Urgent,
            DueDate = DateTime.Now.AddDays(3).Date.AddHours(23).AddMinutes(59),
            EstimatedMinutes = 360,
            FocusedMinutes = 120,
            Difficulty = 5,
            EnergyRequired = 4,
            IsPinned = true,
            Tags = "期末,WinForms,C#,作品集"
        };
        var graphReview = new StudyTask
        {
            Title = "複習圖論與最小生成樹",
            Description = "整理 Prim、Kruskal、時間複雜度與考古題。",
            CourseId = algorithm.Id,
            Priority = TaskPriority.High,
            DueDate = DateTime.Now.AddDays(1).Date.AddHours(21),
            EstimatedMinutes = 120,
            FocusedMinutes = 35,
            Difficulty = 4,
            EnergyRequired = 4,
            Tags = "考試,演算法"
        };
        var researchCharts = new StudyTask
        {
            Title = "整理研究資料與圖表",
            Description = "整理研究流程、輸出結果與報告圖表。",
            CourseId = research.Id,
            Priority = TaskPriority.Medium,
            DueDate = DateTime.Now.AddDays(7).Date.AddHours(18),
            EstimatedMinutes = 180,
            FocusedMinutes = 20,
            Difficulty = 4,
            EnergyRequired = 3,
            Tags = "研究,報告"
        };
        var requirements = new StudyTask
        {
            Title = "建立專題需求清單",
            Description = "確認基本功能、UI、讀寫檔、說明文件與 DEMO 要求。",
            CourseId = windowsForms.Id,
            Priority = TaskPriority.High,
            DueDate = DateTime.Now.AddDays(-1),
            EstimatedMinutes = 30,
            FocusedMinutes = 30,
            Difficulty = 2,
            EnergyRequired = 2,
            IsCompleted = true,
            CompletedAt = DateTime.Now.AddDays(-1),
            Tags = "規劃"
        };

        DateTime monday = ResearchMetricsService.StartOfWeek(DateTime.Today);
        var sessions = new List<FocusSession>
        {
            new()
            {
                TaskId = finalProject.Id,
                StartedAt = monday.AddHours(9),
                EndedAt = monday.AddHours(9).AddMinutes(45),
                DurationMinutes = 45,
                FocusQuality = 4,
                DistractionCount = 1,
                Note = "完成主畫面配置"
            },
            new()
            {
                TaskId = graphReview.Id,
                StartedAt = monday.AddDays(1).AddHours(20),
                EndedAt = monday.AddDays(1).AddHours(20).AddMinutes(35),
                DurationMinutes = 35,
                FocusQuality = 5,
                DistractionCount = 0,
                Note = "複習最小生成樹"
            },
            new()
            {
                TaskId = finalProject.Id,
                StartedAt = DateTime.Today.AddHours(14),
                EndedAt = DateTime.Today.AddHours(14).AddMinutes(50),
                DurationMinutes = 50,
                FocusQuality = 4,
                DistractionCount = 1,
                Note = "完成資料儲存與分析功能"
            }
        };

        return new AppData
        {
            SchemaVersion = CurrentSchemaVersion,
            Courses = new List<Course> { windowsForms, algorithm, research },
            Tasks = new List<StudyTask> { finalProject, graphReview, researchCharts, requirements },
            Sessions = sessions,
            TimetableSemesters = TimetableCatalog.CreateDefaultSemesters(),
            TimetableEntries = TimetableSeedData.Create(),
            TimetableInitialized = true,
            Activities = new List<ActivityLogEntry>
            {
                new()
                {
                    Type = ActivityType.System,
                    EntityType = "System",
                    Summary = "建立 StudyFlow Pro Research Edition 展示資料",
                    OccurredAt = DateTime.Now
                }
            },
            Settings = new AppSettings
            {
                UserName = "Howard",
                DefaultFocusMinutes = 25,
                DailyGoalMinutes = 120,
                ShowDueSoonReminder = true,
                DueSoonHours = 24
            }
        };
    }
}
