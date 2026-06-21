using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public sealed class AccountService
{
    private const int PasswordIterations = 120_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string RootDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "StudyFlowPro");

    public string AuthDirectory => Path.Combine(RootDirectory, "Auth");
    public string AccountsPath => Path.Combine(AuthDirectory, "accounts.json");
    public string AccountsBackupPath => Path.Combine(AuthDirectory, "accounts.lastgood.json");
    public string UsersDirectory => Path.Combine(RootDirectory, "Users");
    public string LegacyMigrationFlagPath => Path.Combine(RootDirectory, "legacy-data-migrated.flag");

    public UserAccount? Authenticate(string username, string password, out string error)
    {
        error = string.Empty;
        string normalized = NormalizeUsername(username);
        if (string.IsNullOrWhiteSpace(normalized) || string.IsNullOrEmpty(password))
        {
            error = "請輸入帳號與密碼。";
            return null;
        }

        AccountStore store = LoadStore();
        UserAccount? account = store.Accounts.FirstOrDefault(item =>
            string.Equals(item.Username, normalized, StringComparison.OrdinalIgnoreCase));

        if (account == null || !VerifyPassword(account, password))
        {
            error = "帳號或密碼錯誤。";
            return null;
        }

        account.LastLoginAt = DateTime.Now;
        SaveStore(store);
        PrepareUserDataDirectory(account);
        return ClonePublicAccount(account);
    }

    public UserAccount? Register(
        string displayName,
        string username,
        string password,
        string confirmPassword,
        out string error)
    {
        error = ValidateRegistration(displayName, username, password, confirmPassword);
        if (!string.IsNullOrEmpty(error))
            return null;

        string normalized = NormalizeUsername(username);
        AccountStore store = LoadStore();
        if (store.Accounts.Any(item =>
                string.Equals(item.Username, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            error = "此帳號已被使用，請更換其他帳號。";
            return null;
        }

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            PasswordIterations,
            HashAlgorithmName.SHA256,
            HashSize);

        var account = new UserAccount
        {
            Id = Guid.NewGuid(),
            Username = normalized,
            DisplayName = displayName.Trim(),
            PasswordSalt = Convert.ToBase64String(salt),
            PasswordHash = Convert.ToBase64String(hash),
            PasswordIterations = PasswordIterations,
            CreatedAt = DateTime.Now,
            LastLoginAt = DateTime.Now
        };

        store.Accounts.Add(account);
        SaveStore(store);
        PrepareUserDataDirectory(account);
        return ClonePublicAccount(account);
    }

    public bool UpdateDisplayName(UserAccount account, string displayName, out string error)
    {
        error = ValidateDisplayName(displayName);
        if (!string.IsNullOrEmpty(error))
            return false;

        AccountStore store = LoadStore();
        UserAccount? storedAccount = store.Accounts.FirstOrDefault(item => item.Id == account.Id);
        if (storedAccount == null)
        {
            error = "找不到目前登入的帳號，請登出後重新登入。";
            return false;
        }

        string normalizedDisplayName = displayName.Trim();
        storedAccount.DisplayName = normalizedDisplayName;
        account.DisplayName = normalizedDisplayName;
        SaveStore(store);
        return true;
    }

    public string GetUserDirectory(UserAccount account) =>
        Path.Combine(UsersDirectory, account.Id.ToString("N"));

    public void PrepareUserDataDirectory(UserAccount account)
    {
        Directory.CreateDirectory(RootDirectory);
        Directory.CreateDirectory(AuthDirectory);
        Directory.CreateDirectory(UsersDirectory);

        string userDirectory = GetUserDirectory(account);
        Directory.CreateDirectory(userDirectory);
        Directory.CreateDirectory(Path.Combine(userDirectory, "Backups"));
        Directory.CreateDirectory(Path.Combine(userDirectory, "Snapshots"));
        Directory.CreateDirectory(Path.Combine(userDirectory, "ExamLibrary", "Files"));
        Directory.CreateDirectory(Path.Combine(userDirectory, "ExamLibrary", "PreviewCache"));

        string userDataPath = Path.Combine(userDirectory, "studyflow-data.json");
        string legacyDataPath = Path.Combine(RootDirectory, "studyflow-data.json");

        if (File.Exists(userDataPath) || !File.Exists(legacyDataPath) || File.Exists(LegacyMigrationFlagPath))
            return;

        CopyFileIfExists(legacyDataPath, userDataPath);
        CopyFileIfExists(
            Path.Combine(RootDirectory, "studyflow-data.lastgood.json"),
            Path.Combine(userDirectory, "studyflow-data.lastgood.json"));
        CopyDirectoryIfExists(
            Path.Combine(RootDirectory, "Snapshots"),
            Path.Combine(userDirectory, "Snapshots"));
        CopyDirectoryIfExists(
            Path.Combine(RootDirectory, "ExamLibrary"),
            Path.Combine(userDirectory, "ExamLibrary"));

        File.WriteAllText(
            LegacyMigrationFlagPath,
            $"Legacy data copied to account {account.Username} ({account.Id:N}) at {DateTime.Now:O}.",
            new UTF8Encoding(false));
    }

    private AccountStore LoadStore()
    {
        Directory.CreateDirectory(AuthDirectory);
        if (!File.Exists(AccountsPath))
            return new AccountStore();

        try
        {
            string json = File.ReadAllText(AccountsPath, Encoding.UTF8);
            AccountStore store = JsonSerializer.Deserialize<AccountStore>(json, _options) ?? new AccountStore();
            store.Accounts ??= new List<UserAccount>();
            store.Accounts = store.Accounts
                .Where(item => item != null && item.Id != Guid.Empty && !string.IsNullOrWhiteSpace(item.Username))
                .GroupBy(item => item.Username.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
            return store;
        }
        catch
        {
            string brokenPath = Path.Combine(
                AuthDirectory,
                $"accounts.broken-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            try { File.Copy(AccountsPath, brokenPath, true); } catch { }

            if (File.Exists(AccountsBackupPath))
            {
                try
                {
                    string backupJson = File.ReadAllText(AccountsBackupPath, Encoding.UTF8);
                    AccountStore backup = JsonSerializer.Deserialize<AccountStore>(backupJson, _options)
                        ?? new AccountStore();
                    backup.Accounts ??= new List<UserAccount>();
                    return backup;
                }
                catch
                {
                    // 主檔與備份都無法解析時，才回傳空白帳號庫。
                }
            }

            return new AccountStore();
        }
    }

    private void SaveStore(AccountStore store)
    {
        Directory.CreateDirectory(AuthDirectory);
        string json = JsonSerializer.Serialize(store, _options);
        string tempPath = AccountsPath + ".tmp";
        File.WriteAllText(tempPath, json, new UTF8Encoding(false));

        if (File.Exists(AccountsPath))
            File.Copy(AccountsPath, AccountsBackupPath, true);

        File.Move(tempPath, AccountsPath, true);
    }

    private static bool VerifyPassword(UserAccount account, string password)
    {
        try
        {
            byte[] salt = Convert.FromBase64String(account.PasswordSalt);
            byte[] expected = Convert.FromBase64String(account.PasswordHash);
            int iterations = account.PasswordIterations > 0
                ? account.PasswordIterations
                : PasswordIterations;
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch
        {
            return false;
        }
    }

    private static string ValidateRegistration(
        string displayName,
        string username,
        string password,
        string confirmPassword)
    {
        string displayNameError = ValidateDisplayName(displayName);
        if (!string.IsNullOrEmpty(displayNameError))
            return displayNameError;

        string normalized = NormalizeUsername(username);
        if (!Regex.IsMatch(normalized, "^[a-z0-9._-]{4,30}$"))
            return "帳號需為 4～30 個英文字母、數字、句點、底線或連字號。";
        if (password.Length < 6)
            return "密碼至少需要 6 個字元。";
        if (password.Length > 128)
            return "密碼不可超過 128 個字元。";
        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            return "兩次輸入的密碼不一致。";
        return string.Empty;
    }

    private static string ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return "請輸入顯示名稱。";
        if (displayName.Trim().Length > 30)
            return "顯示名稱不可超過 30 個字元。";
        return string.Empty;
    }

    private static string NormalizeUsername(string username) =>
        (username ?? string.Empty).Trim().ToLowerInvariant();

    private static UserAccount ClonePublicAccount(UserAccount account) => new()
    {
        Id = account.Id,
        Username = account.Username,
        DisplayName = account.DisplayName,
        CreatedAt = account.CreatedAt,
        LastLoginAt = account.LastLoginAt
    };

    private static void CopyFileIfExists(string source, string destination)
    {
        if (!File.Exists(source))
            return;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination, true);
    }

    private static void CopyDirectoryIfExists(string source, string destination)
    {
        if (!Directory.Exists(source))
            return;

        Directory.CreateDirectory(destination);
        foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(source, directory);
            Directory.CreateDirectory(Path.Combine(destination, relative));
        }

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(source, file);
            string target = Path.Combine(destination, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, true);
        }
    }
}
