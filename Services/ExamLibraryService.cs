using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public sealed class ExamLibraryService
{
    private readonly DataService _dataService;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ExamLibraryService(DataService dataService)
    {
        _dataService = dataService;
        EnsureDirectories();
    }

    public string LibraryDirectory => _dataService.ExamLibraryDirectory;
    public string FilesDirectory => _dataService.ExamFilesDirectory;

    public string GetFullPath(ExamPaper paper) =>
        Path.Combine(FilesDirectory, paper.StoredFileName ?? string.Empty);

    public bool FileExists(ExamPaper paper) => File.Exists(GetFullPath(paper));

    public ExamPaper ImportFile(string sourcePath, Guid subjectId)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("找不到要匯入的檔案。", sourcePath);

        ExamSubject subject = _dataService.Data.ExamSubjects
            .FirstOrDefault(item => item.Id == subjectId)
            ?? throw new InvalidOperationException("請先選擇有效的考古題科目。");

        string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
        if (extension is not ".pdf" and not ".docx")
            throw new NotSupportedException("目前只支援 PDF 與 DOCX 考古題。");

        EnsureDirectories();
        string hash = ComputeSha256(sourcePath);
        ExamPaper? duplicate = _dataService.Data.ExamPapers
            .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.Sha256) &&
                                    string.Equals(item.Sha256, hash, StringComparison.OrdinalIgnoreCase));
        if (duplicate != null)
            throw new InvalidOperationException($"此檔案已存在於題庫：{duplicate.Title}");

        ExamPaperDraft draft = InferMetadata(sourcePath);
        Guid paperId = Guid.NewGuid();
        string storedFileName = $"{paperId:N}{extension}";
        string destination = Path.Combine(FilesDirectory, storedFileName);
        string temp = destination + ".tmp";

        File.Copy(sourcePath, temp, true);
        File.Move(temp, destination, true);

        var info = new FileInfo(destination);
        var paper = new ExamPaper
        {
            Id = paperId,
            SubjectId = subjectId,
            Title = draft.Title,
            OriginalFileName = Path.GetFileName(sourcePath),
            StoredFileName = storedFileName,
            FileExtension = extension,
            FileSizeBytes = info.Length,
            Sha256 = hash,
            ExamYear = draft.ExamYear,
            Term = draft.Term,
            Category = draft.Category,
            Tags = draft.Tags,
            ImportedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _dataService.Data.ExamPapers.Add(paper);
        _dataService.Log(ActivityType.Imported, "ExamPaper", paper.Id,
            $"匯入考古題：{paper.Title}（{subject.Name}）");
        _dataService.SaveAndNotify();
        return paper;
    }

    public void UpdatePaper(ExamPaper updated)
    {
        int index = _dataService.Data.ExamPapers.FindIndex(item => item.Id == updated.Id);
        if (index < 0)
            throw new InvalidOperationException("找不到要更新的考古題。");

        updated.UpdatedAt = DateTime.Now;
        _dataService.Data.ExamPapers[index] = updated;
        _dataService.Log(ActivityType.Updated, "ExamPaper", updated.Id, $"更新考古題資訊：{updated.Title}");
        _dataService.SaveAndNotify();
    }

    public void MarkOpened(ExamPaper paper)
    {
        ExamPaper? current = _dataService.Data.ExamPapers.FirstOrDefault(item => item.Id == paper.Id);
        if (current == null)
            return;

        current.OpenCount++;
        current.LastOpenedAt = DateTime.Now;
        current.UpdatedAt = DateTime.Now;
        if (current.Status == ExamPaperStatus.NotStarted)
            current.Status = ExamPaperStatus.Reviewing;

        _dataService.Log(ActivityType.Viewed, "ExamPaper", current.Id, $"開啟考古題：{current.Title}");
        _dataService.SaveAndNotify();
    }

    public void ToggleFavorite(ExamPaper paper)
    {
        ExamPaper? current = _dataService.Data.ExamPapers.FirstOrDefault(item => item.Id == paper.Id);
        if (current == null)
            return;

        current.IsFavorite = !current.IsFavorite;
        current.UpdatedAt = DateTime.Now;
        _dataService.Log(ActivityType.Updated, "ExamPaper", current.Id,
            current.IsFavorite ? $"收藏考古題：{current.Title}" : $"取消收藏：{current.Title}");
        _dataService.SaveAndNotify();
    }

    public void CycleStatus(ExamPaper paper)
    {
        ExamPaper? current = _dataService.Data.ExamPapers.FirstOrDefault(item => item.Id == paper.Id);
        if (current == null)
            return;

        current.Status = current.Status switch
        {
            ExamPaperStatus.NotStarted => ExamPaperStatus.Reviewing,
            ExamPaperStatus.Reviewing => ExamPaperStatus.Completed,
            _ => ExamPaperStatus.NotStarted
        };
        current.UpdatedAt = DateTime.Now;
        _dataService.Log(ActivityType.Updated, "ExamPaper", current.Id,
            $"更新考古題狀態：{current.Title} → {StatusText(current.Status)}");
        _dataService.SaveAndNotify();
    }

    public void DeletePaper(ExamPaper paper)
    {
        _dataService.Data.ExamPapers.RemoveAll(item => item.Id == paper.Id);
        string fullPath = GetFullPath(paper);
        try
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch
        {
            // 即使原始檔暫時被鎖定，仍保留操作紀錄並移除題庫索引。
        }

        _dataService.Log(ActivityType.Deleted, "ExamPaper", paper.Id, $"刪除考古題：{paper.Title}");
        _dataService.SaveAndNotify();
    }

    public void DeleteSubject(ExamSubject subject)
    {
        List<ExamPaper> papers = _dataService.Data.ExamPapers
            .Where(item => item.SubjectId == subject.Id)
            .ToList();
        foreach (ExamPaper paper in papers)
        {
            string fullPath = GetFullPath(paper);
            try
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch
            {
                // 檔案刪除失敗不阻止資料模型清理。
            }
        }

        _dataService.Data.ExamPapers.RemoveAll(item => item.SubjectId == subject.Id);
        _dataService.Data.ExamSubjects.RemoveAll(item => item.Id == subject.Id);
        _dataService.Log(ActivityType.Deleted, "ExamSubject", subject.Id,
            $"刪除考古題科目：{subject.Name}（含 {papers.Count} 份文件）");
        _dataService.SaveAndNotify();
    }

    public void ExportOriginal(ExamPaper paper, string destinationPath)
    {
        string source = GetFullPath(paper);
        if (!File.Exists(source))
            throw new FileNotFoundException("考古題原始檔遺失，請重新匯入。", source);

        File.Copy(source, destinationPath, true);
        _dataService.Log(ActivityType.Exported, "ExamPaper", paper.Id,
            $"匯出考古題原檔：{paper.Title}");
        _dataService.SaveAndNotify();
    }

    public void ExportPackage(string packagePath, Guid? subjectId = null)
    {
        EnsureDirectories();
        List<ExamSubject> subjects = subjectId.HasValue
            ? _dataService.Data.ExamSubjects.Where(item => item.Id == subjectId.Value).ToList()
            : _dataService.Data.ExamSubjects.ToList();
        HashSet<Guid> subjectIds = subjects.Select(item => item.Id).ToHashSet();
        List<ExamPaper> papers = _dataService.Data.ExamPapers
            .Where(item => subjectIds.Contains(item.SubjectId) && FileExists(item))
            .ToList();

        var manifest = new ExamLibraryPackage
        {
            SchemaVersion = 1,
            ExportedAt = DateTime.Now,
            Subjects = subjects.Select(CloneSubject).ToList(),
            Papers = papers.Select(item => new ExamPackagePaper
            {
                Paper = ClonePaper(item),
                PackageFileName = $"files/{item.Id:N}{item.FileExtension}"
            }).ToList()
        };

        if (File.Exists(packagePath))
            File.Delete(packagePath);

        using ZipArchive archive = ZipFile.Open(packagePath, ZipArchiveMode.Create);
        ZipArchiveEntry readmeEntry = archive.CreateEntry("README.txt", CompressionLevel.Optimal);
        using (Stream readmeStream = readmeEntry.Open())
        using (var readmeWriter = new StreamWriter(readmeStream, new UTF8Encoding(false)))
        {
            readmeWriter.WriteLine("StudyFlow Pro Exam Library Package");
            readmeWriter.WriteLine("此 .sfexam 檔包含科目、考古題索引與 PDF／DOCX 原始檔。");
            readmeWriter.WriteLine("請在 StudyFlow Pro 的考古題庫頁面按「匯入題庫包」載入。");
        }

        ZipArchiveEntry manifestEntry = archive.CreateEntry("manifest.json", CompressionLevel.Optimal);
        using (Stream stream = manifestEntry.Open())
        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
        {
            writer.Write(JsonSerializer.Serialize(manifest, _jsonOptions));
        }

        foreach (ExamPackagePaper packagePaper in manifest.Papers)
        {
            string source = GetFullPath(packagePaper.Paper);
            ZipArchiveEntry fileEntry = archive.CreateEntry(packagePaper.PackageFileName, CompressionLevel.Optimal);
            using Stream input = File.OpenRead(source);
            using Stream output = fileEntry.Open();
            input.CopyTo(output);
        }

        _dataService.Log(ActivityType.Exported, "ExamLibrary", subjectId,
            subjectId.HasValue ? "匯出單一科目考古題包" : "匯出完整考古題庫包");
        _dataService.SaveAndNotify();
    }

    public ExamPackageImportResult ImportPackage(string packagePath)
    {
        if (!File.Exists(packagePath))
            throw new FileNotFoundException("找不到考古題庫包。", packagePath);

        EnsureDirectories();
        int subjectAdded = 0;
        int paperAdded = 0;
        int duplicateSkipped = 0;

        using ZipArchive archive = ZipFile.OpenRead(packagePath);
        ZipArchiveEntry manifestEntry = archive.GetEntry("manifest.json")
            ?? throw new InvalidDataException("此檔案不是有效的 StudyFlow 考古題庫包。");

        ExamLibraryPackage package;
        using (Stream stream = manifestEntry.Open())
        {
            package = JsonSerializer.Deserialize<ExamLibraryPackage>(stream, _jsonOptions)
                ?? throw new InvalidDataException("考古題庫包的索引無法解析。");
        }

        var subjectMap = new Dictionary<Guid, Guid>();
        foreach (ExamSubject sourceSubject in package.Subjects ?? new List<ExamSubject>())
        {
            ExamSubject? existing = _dataService.Data.ExamSubjects.FirstOrDefault(item =>
                string.Equals(item.Name, sourceSubject.Name, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                existing = CloneSubject(sourceSubject);
                if (_dataService.Data.ExamSubjects.Any(item => item.Id == existing.Id))
                    existing.Id = Guid.NewGuid();
                existing.CreatedAt = DateTime.Now;
                _dataService.Data.ExamSubjects.Add(existing);
                subjectAdded++;
            }
            subjectMap[sourceSubject.Id] = existing.Id;
        }

        foreach (ExamPackagePaper source in package.Papers ?? new List<ExamPackagePaper>())
        {
            if (source.Paper == null || !subjectMap.TryGetValue(source.Paper.SubjectId, out Guid newSubjectId))
                continue;

            if (!string.IsNullOrWhiteSpace(source.Paper.Sha256) &&
                _dataService.Data.ExamPapers.Any(item =>
                    string.Equals(item.Sha256, source.Paper.Sha256, StringComparison.OrdinalIgnoreCase)))
            {
                duplicateSkipped++;
                continue;
            }

            string extension = source.Paper.FileExtension?.ToLowerInvariant() ?? string.Empty;
            if (extension is not ".pdf" and not ".docx")
                continue;

            ZipArchiveEntry? fileEntry = archive.GetEntry(source.PackageFileName ?? string.Empty);
            if (fileEntry == null)
                continue;

            Guid newId = Guid.NewGuid();
            string storedName = $"{newId:N}{extension}";
            string destination = Path.Combine(FilesDirectory, storedName);
            string temp = destination + ".tmp";
            using (Stream input = fileEntry.Open())
            using (Stream output = File.Create(temp))
                input.CopyTo(output);
            File.Move(temp, destination, true);

            ExamPaper paper = ClonePaper(source.Paper);
            paper.Id = newId;
            paper.SubjectId = newSubjectId;
            paper.StoredFileName = storedName;
            paper.FileSizeBytes = new FileInfo(destination).Length;
            paper.ImportedAt = DateTime.Now;
            paper.UpdatedAt = DateTime.Now;
            paper.OpenCount = 0;
            paper.LastOpenedAt = null;
            _dataService.Data.ExamPapers.Add(paper);
            paperAdded++;
        }

        _dataService.Log(ActivityType.Imported, "ExamLibrary", null,
            $"匯入考古題庫包：新增 {subjectAdded} 科、{paperAdded} 份，略過重複 {duplicateSkipped} 份");
        _dataService.SaveAndNotify();
        return new ExamPackageImportResult(subjectAdded, paperAdded, duplicateSkipped);
    }

    public bool TryImportBundledPackage()
    {
        if (_dataService.Data.ExamPapers.Count > 0)
            return false;

        string[] candidates =
        {
            Path.Combine(AppContext.BaseDirectory, "DemoAssets", "ExamLibrary.sfexam"),
            Path.Combine(AppContext.BaseDirectory, "ExamLibrary.sfexam")
        };
        string? package = candidates.FirstOrDefault(File.Exists);
        if (package == null)
            return false;

        try
        {
            ImportPackage(package);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public ExamPaper? GetRecommendedPaper(Guid? subjectId = null)
    {
        IEnumerable<ExamPaper> query = _dataService.Data.ExamPapers;
        if (subjectId.HasValue)
            query = query.Where(item => item.SubjectId == subjectId.Value);

        return query
            .Where(FileExists)
            .OrderBy(item => item.Status == ExamPaperStatus.Completed)
            .ThenBy(item => item.OpenCount)
            .ThenBy(item => item.LastOpenedAt ?? DateTime.MinValue)
            .ThenByDescending(item => item.IsFavorite)
            .ThenByDescending(item => item.ExamYear)
            .FirstOrDefault();
    }

    public static string StatusText(ExamPaperStatus status) => status switch
    {
        ExamPaperStatus.Reviewing => "複習中",
        ExamPaperStatus.Completed => "已完成",
        _ => "未開始"
    };

    public static string FormatFileSize(long bytes)
    {
        if (bytes >= 1024L * 1024L)
            return $"{bytes / 1024d / 1024d:0.0} MB";
        if (bytes >= 1024L)
            return $"{bytes / 1024d:0.0} KB";
        return $"{bytes} B";
    }

    private void EnsureDirectories()
    {
        Directory.CreateDirectory(LibraryDirectory);
        Directory.CreateDirectory(FilesDirectory);
        Directory.CreateDirectory(_dataService.ExamPreviewDirectory);
    }

    private static string ComputeSha256(string path)
    {
        using SHA256 sha = SHA256.Create();
        using Stream stream = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(stream));
    }

    private static ExamPaperDraft InferMetadata(string path)
    {
        string title = Path.GetFileNameWithoutExtension(path)
            .Replace('_', ' ')
            .Replace('-', ' ')
            .Trim();
        Match yearMatch = Regex.Match(title, @"(?<!\d)(20\d{2}|19\d{2}|1\d{2})(?!\d)");
        string year = yearMatch.Success ? yearMatch.Value : string.Empty;

        string category = title.Contains("期中", StringComparison.OrdinalIgnoreCase)
            ? "期中考"
            : title.Contains("期末", StringComparison.OrdinalIgnoreCase)
                ? "期末考"
                : title.Contains("quiz", StringComparison.OrdinalIgnoreCase) || title.Contains("小考")
                    ? "小考"
                    : "考古題";
        string term = title.Contains("上學期") || title.Contains("第一學期") ? "上學期"
            : title.Contains("下學期") ? "下學期"
            : string.Empty;

        return new ExamPaperDraft(title, year, term, category, string.Empty);
    }

    private static ExamSubject CloneSubject(ExamSubject source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Description = source.Description,
        ColorHex = source.ColorHex,
        CreatedAt = source.CreatedAt
    };

    private static ExamPaper ClonePaper(ExamPaper source) => new()
    {
        Id = source.Id,
        SubjectId = source.SubjectId,
        Title = source.Title,
        OriginalFileName = source.OriginalFileName,
        StoredFileName = source.StoredFileName,
        FileExtension = source.FileExtension,
        FileSizeBytes = source.FileSizeBytes,
        Sha256 = source.Sha256,
        ExamYear = source.ExamYear,
        Term = source.Term,
        Category = source.Category,
        Tags = source.Tags,
        Notes = source.Notes,
        IsFavorite = source.IsFavorite,
        Status = source.Status,
        ImportedAt = source.ImportedAt,
        UpdatedAt = source.UpdatedAt,
        LastOpenedAt = source.LastOpenedAt,
        OpenCount = source.OpenCount
    };
}

public sealed record ExamPaperDraft(
    string Title,
    string ExamYear,
    string Term,
    string Category,
    string Tags);

public sealed record ExamPackageImportResult(
    int SubjectsAdded,
    int PapersAdded,
    int DuplicatesSkipped);

public sealed class ExamLibraryPackage
{
    public int SchemaVersion { get; set; } = 1;
    public DateTime ExportedAt { get; set; }
    public List<ExamSubject> Subjects { get; set; } = new();
    public List<ExamPackagePaper> Papers { get; set; } = new();
}

public sealed class ExamPackagePaper
{
    public ExamPaper Paper { get; set; } = new();
    public string PackageFileName { get; set; } = string.Empty;
}
