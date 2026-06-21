using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class ExamLibraryControl : UserControl
{
    private readonly DataService _dataService;
    private readonly ExamLibraryService _libraryService;
    private readonly DataGridView _subjectGrid = new();
    private readonly DataGridView _paperGrid = new();
    private readonly TextBox _searchBox = new();
    private readonly ComboBox _formatFilter = new();
    private readonly ComboBox _statusFilter = new();
    private readonly Label _subjectCountLabel = new();
    private readonly Label _paperCountLabel = new();
    private readonly Label _completedCountLabel = new();
    private readonly Label _missingCountLabel = new();
    private readonly Label _detailTitle = new();
    private readonly Label _detailMeta = new();
    private readonly Label _detailNotes = new();
    private bool _refreshing;

    public ExamLibraryControl(DataService dataService)
    {
        _dataService = dataService;
        _libraryService = new ExamLibraryService(dataService);
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;
        AllowDrop = true;

        BuildInterface();
        WireEvents();
        _dataService.DataChanged += OnDataChanged;
        // 新帳號的考古題庫必須保持空白且獨立；不再從共用 DemoAssets 自動匯入。
        // 使用者仍可在考古題庫頁面手動匯入自己的 .sfexam 題庫包。
        HandleCreated += (_, _) => RefreshLibrary();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _dataService.DataChanged -= OnDataChanged;
        base.Dispose(disposing);
    }

    public void RefreshLibrary()
    {
        if (_refreshing || IsDisposed)
            return;
        _refreshing = true;
        try
        {
            Guid? selectedSubject = GetSelectedSubjectId();
            Guid? selectedPaper = GetSelectedPaper()?.Id;
            RefreshMetrics();
            RefreshSubjectGrid(selectedSubject);
            RefreshPaperGrid(selectedPaper);
            RefreshDetails();
        }
        finally
        {
            _refreshing = false;
        }
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(28, 20, 28, 24),
            BackColor = UiTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 124));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 166));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildMetrics(), 0, 1);
        root.Controls.Add(BuildToolbar(), 0, 2);
        root.Controls.Add(BuildContent(), 0, 3);
        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var header = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderColor = UiTheme.Border,
            Padding = new Padding(20, 14, 20, 14),
            Margin = new Padding(0, 0, 0, 8)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));

        var titleArea = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        titleArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        titleArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        titleArea.Controls.Add(new Label
        {
            Text = "考古題庫",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(25, FontStyle.Bold),
            Margin = Padding.Empty
        }, 0, 0);
        titleArea.Controls.Add(new Label
        {
            Text = "按科目管理 PDF／DOCX、內嵌預覽、標記複習進度，並以可攜題庫包完整備份。",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9.5f),
            Margin = Padding.Empty,
            Padding = new Padding(2, 0, 10, 0)
        }, 0, 1);

        var tip = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.PrimarySoft,
            Padding = new Padding(14, 8, 14, 8),
            Margin = new Padding(12, 2, 0, 2)
        };
        var tipLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.PrimarySoft,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        tipLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tipLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tipLayout.Controls.Add(new Label
        {
            Text = "快速匯入",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.PrimaryDark,
            Font = UiTheme.Font(10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 0);
        tipLayout.Controls.Add(new Label
        {
            Text = "可將 PDF 或 DOCX 直接拖曳到考古題列表",
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9),
            TextAlign = ContentAlignment.TopLeft,
            AutoSize = false,
            Margin = Padding.Empty
        }, 0, 1);
        tip.Controls.Add(tipLayout);

        layout.Controls.Add(titleArea, 0, 0);
        layout.Controls.Add(tip, 1, 0);
        header.Controls.Add(layout);
        return header;
    }

    private Control BuildMetrics()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty
        };
        for (int i = 0; i < 4; i++)
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        layout.Controls.Add(WrapMetric(CreateMetricCard("科目數", UiTheme.Primary, _subjectCountLabel), new Padding(0, 0, 8, 0)), 0, 0);
        layout.Controls.Add(WrapMetric(CreateMetricCard("考古題總數", UiTheme.Purple, _paperCountLabel), new Padding(8, 0, 8, 0)), 1, 0);
        layout.Controls.Add(WrapMetric(CreateMetricCard("已完成複習", UiTheme.Success, _completedCountLabel), new Padding(8, 0, 8, 0)), 2, 0);
        layout.Controls.Add(WrapMetric(CreateMetricCard("檔案完整性警示", UiTheme.Danger, _missingCountLabel), new Padding(8, 0, 0, 0)), 3, 0);
        return layout;
    }

    private Control BuildToolbar()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(14, 10, 14, 10),
            Margin = new Padding(0, 5, 0, 8)
        };

        var rows = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        rows.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
        rows.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
        rows.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34f));

        FlowLayoutPanel CreateRow() => new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = false,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        Button addSubject = UiTheme.PrimaryButton("＋ 新增科目");
        Button editSubject = UiTheme.SecondaryButton("編輯科目");
        Button deleteSubject = UiTheme.DangerButton("刪除科目");
        Button import = UiTheme.PrimaryButton("匯入 PDF／DOCX");
        Button preview = UiTheme.SecondaryButton("預覽");
        Button edit = UiTheme.SecondaryButton("編輯資訊");
        Button favorite = UiTheme.SecondaryButton("收藏／取消");
        Button status = UiTheme.SecondaryButton("切換狀態");
        Button random = UiTheme.SecondaryButton("智慧抽一份");
        Button export = UiTheme.SecondaryButton("匯出原檔");
        Button delete = UiTheme.DangerButton("刪除考古題");
        Button exportPackage = UiTheme.SecondaryButton("匯出題庫包");
        Button importPackage = UiTheme.SecondaryButton("匯入題庫包");
        Button openFolder = UiTheme.SecondaryButton("開啟題庫位置");

        foreach (Button button in new[]
        {
            addSubject, editSubject, deleteSubject, import, preview, edit,
            favorite, status, random, export, delete, exportPackage, importPackage, openFolder
        })
        {
            button.Height = 36;
            button.Margin = new Padding(3, 2, 3, 2);
        }

        addSubject.Click += (_, _) => AddSubject();
        editSubject.Click += (_, _) => EditSubject();
        deleteSubject.Click += (_, _) => DeleteSubject();
        import.Click += (_, _) => ImportFiles();
        preview.Click += (_, _) => PreviewSelected();
        edit.Click += (_, _) => EditSelectedPaper();
        favorite.Click += (_, _) => ToggleFavorite();
        status.Click += (_, _) => CycleStatus();
        random.Click += (_, _) => OpenRecommendedPaper();
        export.Click += (_, _) => ExportSelectedPaper();
        delete.Click += (_, _) => DeleteSelectedPaper();
        exportPackage.Click += (_, _) => ExportPackage();
        importPackage.Click += (_, _) => ImportPackage();
        openFolder.Click += (_, _) =>
        {
            Directory.CreateDirectory(_libraryService.LibraryDirectory);
            Process.Start(new ProcessStartInfo { FileName = _libraryService.LibraryDirectory, UseShellExecute = true });
        };

        _searchBox.Width = 205;
        _searchBox.Height = 36;
        _searchBox.Font = UiTheme.Font(10);
        _searchBox.PlaceholderText = "搜尋標題、年份、標籤或筆記…";
        _searchBox.Margin = new Padding(3, 3, 3, 3);

        _formatFilter.Width = 105;
        _formatFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _formatFilter.Items.AddRange(new object[] { "全部格式", "PDF", "DOCX" });
        _formatFilter.SelectedIndex = 0;
        _formatFilter.Margin = new Padding(3, 4, 3, 3);

        _statusFilter.Width = 120;
        _statusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _statusFilter.Items.AddRange(new object[] { "全部狀態", "未開始", "複習中", "已完成", "已收藏", "檔案遺失" });
        _statusFilter.SelectedIndex = 0;
        _statusFilter.Margin = new Padding(3, 4, 3, 3);

        FlowLayoutPanel first = CreateRow();
        first.Controls.Add(addSubject);
        first.Controls.Add(editSubject);
        first.Controls.Add(deleteSubject);
        first.Controls.Add(import);
        first.Controls.Add(preview);
        first.Controls.Add(edit);

        FlowLayoutPanel second = CreateRow();
        second.Controls.Add(favorite);
        second.Controls.Add(status);
        second.Controls.Add(random);
        second.Controls.Add(delete);
        second.Controls.Add(export);

        FlowLayoutPanel third = CreateRow();
        third.Controls.Add(_searchBox);
        third.Controls.Add(_formatFilter);
        third.Controls.Add(_statusFilter);
        third.Controls.Add(exportPackage);
        third.Controls.Add(importPackage);
        third.Controls.Add(openFolder);

        rows.Controls.Add(first, 0, 0);
        rows.Controls.Add(second, 0, 1);
        rows.Controls.Add(third, 0, 2);
        card.Controls.Add(rows);
        return card;
    }

    private Control BuildContent()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 275));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var left = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 0) };
        var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 0, 0) };
        left.Controls.Add(BuildSubjectCard());
        right.Controls.Add(BuildPaperArea());
        layout.Controls.Add(left, 0, 0);
        layout.Controls.Add(right, 1, 0);
        return layout;
    }

    private Control BuildSubjectCard()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(0)
        };
        var title = new Label
        {
            Text = "科目分類",
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(16, 13, 0, 0),
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(12, FontStyle.Bold)
        };
        _subjectGrid.Dock = DockStyle.Fill;
        UiTheme.StyleGrid(_subjectGrid);
        _subjectGrid.ColumnHeadersVisible = false;
        _subjectGrid.RowTemplate.Height = 48;
        _subjectGrid.CellFormatting += SubjectGridFormatting;
        card.Controls.Add(_subjectGrid);
        card.Controls.Add(title);
        return card;
    }

    private Control BuildPaperArea()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Background
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));

        var gridBorder = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Border,
            Padding = new Padding(1)
        };
        _paperGrid.Dock = DockStyle.Fill;
        UiTheme.StyleGrid(_paperGrid);
        _paperGrid.AllowDrop = true;
        _paperGrid.CellDoubleClick += (_, _) => PreviewSelected();
        _paperGrid.SelectionChanged += (_, _) => RefreshDetails();
        _paperGrid.CellFormatting += PaperGridFormatting;
        gridBorder.Controls.Add(_paperGrid);
        layout.Controls.Add(gridBorder, 0, 0);

        var details = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(18),
            Margin = new Padding(0, 10, 0, 0)
        };
        var detailsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Surface
        };
        detailsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        detailsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _detailTitle.Dock = DockStyle.Fill;
        _detailTitle.AutoEllipsis = true;
        _detailTitle.ForeColor = UiTheme.Navy;
        _detailTitle.Font = UiTheme.Font(12, FontStyle.Bold);
        _detailMeta.Dock = DockStyle.Fill;
        _detailMeta.AutoEllipsis = true;
        _detailMeta.ForeColor = UiTheme.PrimaryDark;
        _detailMeta.Font = UiTheme.Font(9);
        _detailNotes.Dock = DockStyle.Fill;
        _detailNotes.AutoEllipsis = true;
        _detailNotes.ForeColor = UiTheme.Muted;
        _detailNotes.Font = UiTheme.Font(9);
        detailsLayout.Controls.Add(_detailTitle, 0, 0);
        detailsLayout.Controls.Add(_detailMeta, 0, 1);
        detailsLayout.Controls.Add(_detailNotes, 0, 2);
        details.Controls.Add(detailsLayout);
        layout.Controls.Add(details, 0, 1);
        return layout;
    }

    private void WireEvents()
    {
        _subjectGrid.SelectionChanged += (_, _) =>
        {
            if (!_refreshing)
                RefreshPaperGrid(null);
        };
        _searchBox.TextChanged += (_, _) => RefreshPaperGrid(null);
        _formatFilter.SelectedIndexChanged += (_, _) => RefreshPaperGrid(null);
        _statusFilter.SelectedIndexChanged += (_, _) => RefreshPaperGrid(null);

        DragEnter += OnDragEnter;
        DragDrop += OnDragDrop;
        _paperGrid.DragEnter += OnDragEnter;
        _paperGrid.DragDrop += OnDragDrop;
    }

    private void OnDataChanged(object? sender, EventArgs e) => RefreshLibrary();

    private void RefreshMetrics()
    {
        _subjectCountLabel.Text = _dataService.Data.ExamSubjects.Count.ToString();
        _paperCountLabel.Text = _dataService.Data.ExamPapers.Count.ToString();
        _completedCountLabel.Text = _dataService.Data.ExamPapers.Count(item => item.Status == ExamPaperStatus.Completed).ToString();
        _missingCountLabel.Text = _dataService.Data.ExamPapers.Count(item => !_libraryService.FileExists(item)).ToString();
    }

    private void RefreshSubjectGrid(Guid? selectedId)
    {
        EnsureSubjectColumns();
        var rows = new List<ExamSubjectRow>
        {
            new()
            {
                Id = Guid.Empty,
                Name = "全部考古題",
                PaperCount = _dataService.Data.ExamPapers.Count,
                Progress = BuildProgress(_dataService.Data.ExamPapers)
            }
        };
        rows.AddRange(_dataService.Data.ExamSubjects
            .OrderBy(item => item.Name)
            .Select(subject =>
            {
                List<ExamPaper> papers = _dataService.Data.ExamPapers.Where(item => item.SubjectId == subject.Id).ToList();
                return new ExamSubjectRow
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    ColorHex = subject.ColorHex,
                    PaperCount = papers.Count,
                    Progress = BuildProgress(papers)
                };
            }));
        _subjectGrid.DataSource = new BindingList<ExamSubjectRow>(rows);

        Guid target = selectedId ?? Guid.Empty;
        foreach (DataGridViewRow row in _subjectGrid.Rows)
        {
            if (row.DataBoundItem is ExamSubjectRow subjectRow && subjectRow.Id == target)
            {
                row.Selected = true;
                _subjectGrid.CurrentCell = row.Cells[0];
                break;
            }
        }
    }

    private void RefreshPaperGrid(Guid? selectedPaperId)
    {
        if (_paperGrid.IsDisposed)
            return;
        EnsurePaperColumns();
        Guid? subjectId = GetSelectedSubjectId();
        IEnumerable<ExamPaper> query = _dataService.Data.ExamPapers;
        if (subjectId.HasValue)
            query = query.Where(item => item.SubjectId == subjectId.Value);

        string keyword = _searchBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(item =>
                item.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.ExamYear.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Tags.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Notes.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.OriginalFileName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        string format = _formatFilter.SelectedItem?.ToString() ?? "全部格式";
        query = format switch
        {
            "PDF" => query.Where(item => item.FileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)),
            "DOCX" => query.Where(item => item.FileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase)),
            _ => query
        };

        string status = _statusFilter.SelectedItem?.ToString() ?? "全部狀態";
        query = status switch
        {
            "未開始" => query.Where(item => item.Status == ExamPaperStatus.NotStarted),
            "複習中" => query.Where(item => item.Status == ExamPaperStatus.Reviewing),
            "已完成" => query.Where(item => item.Status == ExamPaperStatus.Completed),
            "已收藏" => query.Where(item => item.IsFavorite),
            "檔案遺失" => query.Where(item => !_libraryService.FileExists(item)),
            _ => query
        };

        List<ExamPaperRow> rows = query
            .OrderByDescending(item => item.IsFavorite)
            .ThenBy(item => item.Status == ExamPaperStatus.Completed)
            .ThenByDescending(item => item.ExamYear)
            .ThenByDescending(item => item.ImportedAt)
            .Select(item => new ExamPaperRow
            {
                Id = item.Id,
                Favorite = item.IsFavorite ? "★" : string.Empty,
                Status = ExamLibraryService.StatusText(item.Status),
                Title = item.Title,
                Year = string.IsNullOrWhiteSpace(item.ExamYear) ? "—" : item.ExamYear,
                Category = string.IsNullOrWhiteSpace(item.Category) ? "考古題" : item.Category,
                Format = item.FileExtension.TrimStart('.').ToUpperInvariant(),
                ImportedAt = item.ImportedAt.ToString("yyyy/MM/dd"),
                OpenCount = item.OpenCount,
                FileState = _libraryService.FileExists(item) ? "正常" : "遺失"
            }).ToList();
        _paperGrid.DataSource = new BindingList<ExamPaperRow>(rows);

        if (selectedPaperId.HasValue)
        {
            foreach (DataGridViewRow row in _paperGrid.Rows)
            {
                if (row.DataBoundItem is ExamPaperRow paperRow && paperRow.Id == selectedPaperId.Value)
                {
                    row.Selected = true;
                    _paperGrid.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }
        RefreshDetails();
    }

    private void RefreshDetails()
    {
        ExamPaper? paper = GetSelectedPaper();
        if (paper == null)
        {
            _detailTitle.Text = "選取一份考古題查看詳細資訊";
            _detailMeta.Text = "支援 PDF 與 DOCX；雙擊列表可直接預覽。";
            _detailNotes.Text = "匯入後檔案會複製到 StudyFlow Pro 的本機資料庫，下次執行會自動載入。";
            return;
        }
        ExamSubject? subject = _dataService.Data.ExamSubjects.FirstOrDefault(item => item.Id == paper.SubjectId);
        _detailTitle.Text = (paper.IsFavorite ? "★ " : string.Empty) + paper.Title;
        _detailMeta.Text = $"{subject?.Name ?? "未分類"}｜{paper.ExamYear} {paper.Term}｜{paper.Category}｜{paper.FileExtension.TrimStart('.').ToUpperInvariant()}｜{ExamLibraryService.FormatFileSize(paper.FileSizeBytes)}｜開啟 {paper.OpenCount} 次";
        string fileState = _libraryService.FileExists(paper) ? "檔案完整" : "原始檔遺失，請重新匯入";
        _detailNotes.Text = $"{fileState}｜狀態：{ExamLibraryService.StatusText(paper.Status)}" +
                            (string.IsNullOrWhiteSpace(paper.Notes) ? "｜尚未填寫筆記。" : "｜筆記：" + paper.Notes);
    }

    private void AddSubject()
    {
        using var form = new ExamSubjectEditorForm();
        if (form.ShowDialog(FindForm()) != DialogResult.OK)
            return;
        if (_dataService.Data.ExamSubjects.Any(item => string.Equals(item.Name, form.ResultSubject.Name, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("已有相同名稱的科目。", "無法新增", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _dataService.Data.ExamSubjects.Add(form.ResultSubject);
        _dataService.Log(ActivityType.Created, "ExamSubject", form.ResultSubject.Id, $"新增考古題科目：{form.ResultSubject.Name}");
        _dataService.SaveAndNotify();
        SelectSubject(form.ResultSubject.Id);
    }

    private void EditSubject()
    {
        ExamSubject? subject = GetSelectedSubject();
        if (subject == null)
        {
            MessageBox.Show("請先選取一個科目，不能編輯「全部考古題」。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new ExamSubjectEditorForm(subject);
        if (form.ShowDialog(FindForm()) != DialogResult.OK)
            return;
        if (_dataService.Data.ExamSubjects.Any(item => item.Id != subject.Id && string.Equals(item.Name, form.ResultSubject.Name, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("已有相同名稱的科目。", "無法儲存", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        int index = _dataService.Data.ExamSubjects.FindIndex(item => item.Id == subject.Id);
        if (index >= 0)
            _dataService.Data.ExamSubjects[index] = form.ResultSubject;
        _dataService.Log(ActivityType.Updated, "ExamSubject", subject.Id, $"編輯考古題科目：{form.ResultSubject.Name}");
        _dataService.SaveAndNotify();
        SelectSubject(subject.Id);
    }

    private void DeleteSubject()
    {
        ExamSubject? subject = GetSelectedSubject();
        if (subject == null)
        {
            MessageBox.Show("請先選取要刪除的科目。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        int count = _dataService.Data.ExamPapers.Count(item => item.SubjectId == subject.Id);
        if (!ConfirmDialog.Ask(FindForm(), "確認刪除科目",
                $"確定刪除「{subject.Name}」？{Environment.NewLine}此科目內的 {count} 份考古題原始檔也會一併刪除。"))
            return;
        _libraryService.DeleteSubject(subject);
    }

    private void ImportFiles()
    {
        Guid? subjectId = ResolveImportSubject();
        if (!subjectId.HasValue)
            return;
        using var dialog = new OpenFileDialog
        {
            Filter = "考古題文件 (*.pdf;*.docx)|*.pdf;*.docx|PDF (*.pdf)|*.pdf|Word 文件 (*.docx)|*.docx",
            Multiselect = true,
            Title = "選擇要匯入的考古題"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;
        ImportPaths(dialog.FileNames, subjectId.Value);
    }

    private void ImportPaths(IEnumerable<string> paths, Guid subjectId)
    {
        int imported = 0;
        var errors = new List<string>();
        ExamPaper? single = null;
        foreach (string path in paths.Where(path => Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path).Equals(".docx", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                single = _libraryService.ImportFile(path, subjectId);
                imported++;
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(path)}：{ex.Message}");
            }
        }
        if (imported == 1 && single != null)
        {
            using var editor = new ExamPaperEditorForm(_dataService, single);
            if (editor.ShowDialog(FindForm()) == DialogResult.OK)
                _libraryService.UpdatePaper(editor.ResultPaper);
        }
        string message = $"成功匯入 {imported} 份考古題。";
        if (errors.Count > 0)
            message += $"{Environment.NewLine}{Environment.NewLine}未匯入：{Environment.NewLine}" + string.Join(Environment.NewLine, errors.Take(8));
        MessageBox.Show(message, "匯入結果", MessageBoxButtons.OK,
            errors.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        RefreshLibrary();
    }

    private Guid? ResolveImportSubject()
    {
        ExamSubject? selected = GetSelectedSubject();
        if (selected != null)
            return selected.Id;
        if (_dataService.Data.ExamSubjects.Count == 0)
        {
            MessageBox.Show("請先建立至少一個科目。", "尚無科目", MessageBoxButtons.OK, MessageBoxIcon.Information);
            AddSubject();
            return GetSelectedSubject()?.Id;
        }
        using var picker = new ExamSubjectPickerForm(_dataService.Data.ExamSubjects);
        return picker.ShowDialog(FindForm()) == DialogResult.OK ? picker.SelectedSubjectId : null;
    }

    private void PreviewSelected()
    {
        ExamPaper? paper = GetSelectedPaper();
        if (paper == null)
        {
            MessageBox.Show("請先選取一份考古題。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!_libraryService.FileExists(paper))
        {
            MessageBox.Show("原始檔遺失，請重新匯入。", "無法預覽", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var viewer = new ExamDocumentViewerForm(_dataService, _libraryService, paper);
        viewer.ShowDialog(FindForm());
        RefreshLibrary();
    }

    private void EditSelectedPaper()
    {
        ExamPaper? paper = GetSelectedPaper();
        if (paper == null)
        {
            MessageBox.Show("請先選取一份考古題。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new ExamPaperEditorForm(_dataService, paper);
        if (form.ShowDialog(FindForm()) == DialogResult.OK)
            _libraryService.UpdatePaper(form.ResultPaper);
    }

    private void ToggleFavorite()
    {
        ExamPaper? paper = GetSelectedPaper();
        if (paper != null)
            _libraryService.ToggleFavorite(paper);
        else
            MessageBox.Show("請先選取一份考古題。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void CycleStatus()
    {
        ExamPaper? paper = GetSelectedPaper();
        if (paper != null)
            _libraryService.CycleStatus(paper);
        else
            MessageBox.Show("請先選取一份考古題。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OpenRecommendedPaper()
    {
        ExamPaper? paper = _libraryService.GetRecommendedPaper(GetSelectedSubjectId());
        if (paper == null)
        {
            MessageBox.Show("目前沒有可開啟的考古題。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var viewer = new ExamDocumentViewerForm(_dataService, _libraryService, paper);
        viewer.ShowDialog(FindForm());
        RefreshLibrary();
    }

    private void ExportSelectedPaper()
    {
        ExamPaper? paper = GetSelectedPaper();
        if (paper == null)
        {
            MessageBox.Show("請先選取一份考古題。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Filter = paper.FileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
                ? "PDF 檔案 (*.pdf)|*.pdf"
                : "Word 文件 (*.docx)|*.docx",
            FileName = string.IsNullOrWhiteSpace(paper.OriginalFileName) ? paper.Title + paper.FileExtension : paper.OriginalFileName
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;
        try
        {
            _libraryService.ExportOriginal(paper, dialog.FileName);
            MessageBox.Show("原始檔已匯出。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("匯出失敗：\n" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteSelectedPaper()
    {
        ExamPaper? paper = GetSelectedPaper();
        if (paper == null)
        {
            MessageBox.Show("請先選取一份考古題。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!ConfirmDialog.Ask(FindForm(), "確認刪除考古題",
                $"確定刪除「{paper.Title}」？{Environment.NewLine}題庫內保存的 PDF／DOCX 原始檔也會一併刪除。"))
            return;
        _libraryService.DeletePaper(paper);
    }

    private void ExportPackage()
    {
        if (_dataService.Data.ExamPapers.Count == 0)
        {
            MessageBox.Show("目前沒有可匯出的考古題。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Filter = "StudyFlow 考古題庫包 (*.sfexam)|*.sfexam",
            FileName = "ExamLibrary.sfexam"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;
        try
        {
            _libraryService.ExportPackage(dialog.FileName);
            MessageBox.Show(
                "完整題庫包已匯出。此檔包含科目、考古題資訊與 PDF／DOCX 原始檔。\n\n交作業時可把 .sfexam 一起放進專案 ZIP；老師匯入後即可看到相同題庫。",
                "題庫包建立完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("題庫包匯出失敗：\n" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImportPackage()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "StudyFlow 考古題庫包 (*.sfexam)|*.sfexam|ZIP 壓縮檔 (*.zip)|*.zip",
            Title = "匯入考古題庫包"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;
        try
        {
            ExamPackageImportResult result = _libraryService.ImportPackage(dialog.FileName);
            MessageBox.Show(
                $"匯入完成：新增 {result.SubjectsAdded} 個科目、{result.PapersAdded} 份考古題，略過 {result.DuplicatesSkipped} 份重複檔案。",
                "題庫包匯入完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("題庫包匯入失敗：\n" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            e.Effect = files.Any(path => Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path).Equals(".docx", StringComparison.OrdinalIgnoreCase))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] files)
            return;
        Guid? subjectId = ResolveImportSubject();
        if (subjectId.HasValue)
            ImportPaths(files, subjectId.Value);
    }

    private ExamSubject? GetSelectedSubject()
    {
        Guid? id = GetSelectedSubjectId();
        return id.HasValue ? _dataService.Data.ExamSubjects.FirstOrDefault(item => item.Id == id.Value) : null;
    }

    private Guid? GetSelectedSubjectId()
    {
        if (_subjectGrid.CurrentRow?.DataBoundItem is not ExamSubjectRow row || row.Id == Guid.Empty)
            return null;
        return row.Id;
    }

    private ExamPaper? GetSelectedPaper()
    {
        return _paperGrid.CurrentRow?.DataBoundItem is ExamPaperRow row
            ? _dataService.Data.ExamPapers.FirstOrDefault(item => item.Id == row.Id)
            : null;
    }

    private void SelectSubject(Guid id)
    {
        RefreshLibrary();
        foreach (DataGridViewRow row in _subjectGrid.Rows)
        {
            if (row.DataBoundItem is ExamSubjectRow subjectRow && subjectRow.Id == id)
            {
                row.Selected = true;
                _subjectGrid.CurrentCell = row.Cells[0];
                break;
            }
        }
    }

    private void EnsureSubjectColumns()
    {
        if (_subjectGrid.Columns.Count > 0)
            return;
        _subjectGrid.AutoGenerateColumns = false;
        _subjectGrid.Columns.AddRange(
            UiTheme.TextColumn("Name", "科目", 150),
            UiTheme.TextColumn("PaperCount", "份數", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("Progress", "進度", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter));
    }

    private void EnsurePaperColumns()
    {
        if (_paperGrid.Columns.Count > 0)
            return;
        _paperGrid.AutoGenerateColumns = false;
        _paperGrid.Columns.AddRange(
            UiTheme.TextColumn("Favorite", "收藏", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("Status", "狀態", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells),
            UiTheme.TextColumn("Title", "考古題名稱", 230),
            UiTheme.TextColumn("Year", "年份", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("Category", "類型", 80),
            UiTheme.TextColumn("Format", "格式", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("ImportedAt", "匯入日期", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells),
            UiTheme.TextColumn("OpenCount", "開啟", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter),
            UiTheme.TextColumn("FileState", "檔案", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells, alignment: DataGridViewContentAlignment.MiddleCenter));
    }

    private void SubjectGridFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || _subjectGrid.Rows[e.RowIndex].DataBoundItem is not ExamSubjectRow row)
            return;

        if (row.Id == Guid.Empty)
        {
            e.CellStyle.Font = UiTheme.Font(9.5f, FontStyle.Bold);
            return;
        }

        Color subjectColor;
        try
        {
            subjectColor = ColorTranslator.FromHtml(row.ColorHex);
        }
        catch
        {
            subjectColor = UiTheme.Primary;
        }

        // 精準使用使用者選取的原始色碼；選取列時也維持同一顏色，
        // 避免被 DataGridView 預設選取色覆蓋，看起來像沒有更新。
        Color textColor = GetReadableTextColor(subjectColor);
        e.CellStyle.BackColor = subjectColor;
        e.CellStyle.ForeColor = textColor;
        e.CellStyle.SelectionBackColor = subjectColor;
        e.CellStyle.SelectionForeColor = textColor;
    }

    private static Color GetReadableTextColor(Color background)
    {
        double luminance = 0.299 * background.R + 0.587 * background.G + 0.114 * background.B;
        return luminance > 160 ? Color.Black : Color.White;
    }

    private void PaperGridFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;
        string property = _paperGrid.Columns[e.ColumnIndex].DataPropertyName;
        string text = e.Value?.ToString() ?? string.Empty;
        if (property == "Favorite" && text == "★")
            e.CellStyle.ForeColor = UiTheme.Warning;
        else if (property == "Status")
        {
            e.CellStyle.ForeColor = text == "已完成" ? UiTheme.Success : text == "複習中" ? UiTheme.Primary : UiTheme.Muted;
        }
        else if (property == "FileState" && text == "遺失")
            e.CellStyle.ForeColor = UiTheme.Danger;
    }

    private static string BuildProgress(IEnumerable<ExamPaper> papers)
    {
        List<ExamPaper> list = papers.ToList();
        if (list.Count == 0)
            return "0%";
        int completed = list.Count(item => item.Status == ExamPaperStatus.Completed);
        return $"{Math.Round(completed * 100d / list.Count):0}%";
    }

    private static Control CreateMetricCard(string title, Color accent, Label valueLabel)
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(18)
        };
        var accentBar = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accent };
        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 27,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9.5f)
        };
        valueLabel.Text = "0";
        valueLabel.Dock = DockStyle.Fill;
        valueLabel.ForeColor = UiTheme.Navy;
        valueLabel.Font = UiTheme.Font(21, FontStyle.Bold);
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;
        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        card.Controls.Add(accentBar);
        return card;
    }

    private static Control WrapMetric(Control metric, Padding padding)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = padding };
        panel.Controls.Add(metric);
        return panel;
    }
}
