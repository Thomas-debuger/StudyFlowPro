using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class ExamDocumentViewerForm : Form
{
    private readonly DataService _dataService;
    private readonly ExamLibraryService _libraryService;
    private readonly ExamPaper _paper;
    private readonly WebView2 _webView = new();
    private readonly Panel _viewerHost = new();
    private readonly Label _stateLabel = new();
    private readonly Label _titleLabel = new();
    private readonly Label _metaLabel = new();
    private readonly Button _favoriteButton = UiTheme.SecondaryButton("☆ 收藏");
    private readonly ComboBox _statusCombo = new();
    private bool _loadingStatus;

    public ExamDocumentViewerForm(DataService dataService, ExamLibraryService libraryService, ExamPaper paper)
    {
        _dataService = dataService;
        _libraryService = libraryService;
        _paper = paper;

        Text = $"考古題閱讀器｜{paper.Title}";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1280, 900);
        MinimumSize = new Size(1040, 720);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        Shown += async (_, _) => await InitializeViewerAsync();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Surface,
            Padding = new Padding(24, 8, 20, 8),
            Margin = Padding.Empty
        };
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        _titleLabel.Text = _paper.Title;
        _titleLabel.Dock = DockStyle.Fill;
        _titleLabel.AutoEllipsis = true;
        _titleLabel.ForeColor = UiTheme.Navy;
        _titleLabel.Font = UiTheme.Font(18, FontStyle.Bold);
        _titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        _titleLabel.Margin = Padding.Empty;

        _metaLabel.Dock = DockStyle.Fill;
        _metaLabel.AutoEllipsis = true;
        _metaLabel.ForeColor = UiTheme.Muted;
        _metaLabel.Font = UiTheme.Font(9.2f);
        _metaLabel.TextAlign = ContentAlignment.MiddleLeft;
        _metaLabel.Margin = Padding.Empty;
        RefreshHeaderText();

        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 590));
        toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // 左側狀態列使用單列 FlowLayout，避免 Label 與 ComboBox 因 DPI 縮放上下錯位。
        var statusPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = false,
            BackColor = UiTheme.Surface,
            Padding = new Padding(0, 6, 0, 0),
            Margin = Padding.Empty
        };
        var statusLabel = new Label
        {
            Text = "複習狀態",
            AutoSize = true,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9.2f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 7, 10, 0)
        };
        _statusCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _statusCombo.Size = new Size(160, 30);
        _statusCombo.Margin = new Padding(0, 3, 0, 0);
        _statusCombo.Font = UiTheme.Font(9.2f);
        _statusCombo.DataSource = new[]
        {
            new StatusChoice(ExamPaperStatus.NotStarted, "未開始"),
            new StatusChoice(ExamPaperStatus.Reviewing, "複習中"),
            new StatusChoice(ExamPaperStatus.Completed, "已完成")
        };
        _statusCombo.SelectedIndexChanged += (_, _) => SaveStatus();
        statusPanel.Controls.Add(statusLabel);
        statusPanel.Controls.Add(_statusCombo);

        // 右側工具列改成固定高度、固定寬度的緊湊按鈕，不再填滿整個儲存格。
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoScroll = false,
            BackColor = UiTheme.Surface,
            Padding = new Padding(0, 5, 0, 0),
            Margin = Padding.Empty
        };

        Button close = UiTheme.SecondaryButton("關閉");
        Button export = UiTheme.SecondaryButton("匯出原檔");
        Button external = UiTheme.SecondaryButton("外部開啟");
        Button edit = UiTheme.PrimaryButton("編輯資訊");

        ConfigureToolbarButton(_favoriteButton, 98);
        ConfigureToolbarButton(edit, 104);
        ConfigureToolbarButton(external, 104);
        ConfigureToolbarButton(export, 104);
        ConfigureToolbarButton(close, 84);

        close.Click += (_, _) => Close();
        export.Click += (_, _) => ExportOriginal();
        external.Click += (_, _) => OpenExternal();
        edit.Click += (_, _) => EditPaper();
        _favoriteButton.Click += (_, _) =>
        {
            _libraryService.ToggleFavorite(_paper);
            ExamPaper? current = _dataService.Data.ExamPapers.FirstOrDefault(item => item.Id == _paper.Id);
            if (current != null)
                _paper.IsFavorite = current.IsFavorite;
            UpdateFavoriteButton();
        };

        // RightToLeft：先加入最右側按鈕，畫面由左至右仍為收藏、編輯、外部、匯出、關閉。
        actions.Controls.Add(close);
        actions.Controls.Add(export);
        actions.Controls.Add(external);
        actions.Controls.Add(edit);
        actions.Controls.Add(_favoriteButton);

        toolbar.Controls.Add(statusPanel, 0, 0);
        toolbar.Controls.Add(actions, 1, 0);
        header.Controls.Add(_titleLabel, 0, 0);
        header.Controls.Add(_metaLabel, 0, 1);
        header.Controls.Add(toolbar, 0, 2);
        root.Controls.Add(header, 0, 0);

        _viewerHost.Dock = DockStyle.Fill;
        _viewerHost.BackColor = UiTheme.Border;
        _viewerHost.Padding = new Padding(10);
        _webView.Dock = DockStyle.Fill;
        _webView.DefaultBackgroundColor = UiTheme.Surface;
        _viewerHost.Controls.Add(_webView);
        root.Controls.Add(_viewerHost, 0, 1);

        _stateLabel.Text = "正在準備文件預覽…";
        _stateLabel.Dock = DockStyle.Fill;
        _stateLabel.BackColor = UiTheme.Surface;
        _stateLabel.ForeColor = UiTheme.Muted;
        _stateLabel.Font = UiTheme.Font(8.5f);
        _stateLabel.Padding = new Padding(14, 0, 0, 0);
        _stateLabel.TextAlign = ContentAlignment.MiddleLeft;
        root.Controls.Add(_stateLabel, 0, 2);

        Controls.Add(root);
        UpdateFavoriteButton();
        LoadStatus();
    }

    private static void ConfigureToolbarButton(Button button, int width)
    {
        button.AutoSize = false;
        button.Size = new Size(width, 34);
        button.Margin = new Padding(4, 0, 4, 0);
        button.Padding = Padding.Empty;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.AutoEllipsis = false;
        button.Font = UiTheme.Font(9.0f, FontStyle.Bold);
        button.UseVisualStyleBackColor = false;
        button.UseCompatibleTextRendering = false;
    }

    private void RefreshHeaderText()
    {
        _titleLabel.Text = _paper.Title;
        string subject = _dataService.Data.ExamSubjects.FirstOrDefault(item => item.Id == _paper.SubjectId)?.Name ?? "未分類";
        string period = string.Join(" ", new[] { _paper.ExamYear, _paper.Term }.Where(value => !string.IsNullOrWhiteSpace(value)));
        if (string.IsNullOrWhiteSpace(period))
            period = "未設定年度";
        _metaLabel.Text = $"{subject}｜{period}｜{_paper.Category}｜{_paper.FileExtension.TrimStart('.').ToUpperInvariant()}｜{ExamLibraryService.FormatFileSize(_paper.FileSizeBytes)}";
    }

    private async Task InitializeViewerAsync()
    {
        string fullPath = _libraryService.GetFullPath(_paper);
        if (!File.Exists(fullPath))
        {
            ShowFallback("考古題原始檔遺失。請回到題庫刪除索引後重新匯入。", false);
            return;
        }

        try
        {
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

            if (_paper.FileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                _webView.Source = new Uri(fullPath);
                _stateLabel.Text = "PDF 內嵌預覽｜可使用 Ctrl+F 搜尋、滑鼠滾輪翻頁與 Ctrl+滾輪縮放。";
            }
            else
            {
                string html = DocxPreviewService.ConvertToHtml(fullPath);
                Directory.CreateDirectory(_dataService.ExamPreviewDirectory);
                string previewPath = Path.Combine(_dataService.ExamPreviewDirectory, $"{_paper.Id:N}.html");
                File.WriteAllText(previewPath, html, new UTF8Encoding(false));
                _webView.Source = new Uri(previewPath);
                _stateLabel.Text = "DOCX 內嵌閱讀預覽｜已支援自動題號與多層編號；複雜頁首頁尾仍可按「外部開啟」查看原始版面。";
            }

            _libraryService.MarkOpened(_paper);
        }
        catch (Exception ex)
        {
            ShowFallback("內嵌預覽無法啟動。可能尚未安裝 Microsoft Edge WebView2 Runtime。\n\n" + ex.Message, true);
        }
    }

    private void ShowFallback(string message, bool allowExternal)
    {
        _viewerHost.Controls.Clear();
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Surface,
            Padding = new Padding(50)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        panel.Controls.Add(new Label
        {
            Text = "文件預覽暫時不可用",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomCenter,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(22, FontStyle.Bold)
        }, 0, 0);
        panel.Controls.Add(new Label
        {
            Text = message,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(10)
        }, 0, 1);
        if (allowExternal)
        {
            Button open = UiTheme.PrimaryButton("使用系統預設程式開啟");
            open.AutoSize = false;
            open.Size = new Size(250, 44);
            open.Anchor = AnchorStyles.Top;
            open.Click += (_, _) => OpenExternal();
            panel.Controls.Add(open, 0, 2);
        }
        _viewerHost.Controls.Add(panel);
        _stateLabel.Text = "可使用上方「外部開啟」或「匯出原檔」。";
    }

    private void OpenExternal()
    {
        string path = _libraryService.GetFullPath(_paper);
        if (!File.Exists(path))
        {
            MessageBox.Show("原始檔不存在。", "無法開啟", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
    }

    private void ExportOriginal()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = _paper.FileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
                ? "PDF 檔案 (*.pdf)|*.pdf"
                : "Word 文件 (*.docx)|*.docx",
            FileName = string.IsNullOrWhiteSpace(_paper.OriginalFileName)
                ? _paper.Title + _paper.FileExtension
                : _paper.OriginalFileName
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        try
        {
            _libraryService.ExportOriginal(_paper, dialog.FileName);
            MessageBox.Show("考古題原檔已匯出。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("匯出失敗：\n" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EditPaper()
    {
        using var form = new ExamPaperEditorForm(_dataService, _paper);
        if (form.ShowDialog(this) != DialogResult.OK)
            return;
        _libraryService.UpdatePaper(form.ResultPaper);
        CopyPaper(form.ResultPaper, _paper);
        Text = $"考古題閱讀器｜{_paper.Title}";
        RefreshHeaderText();
        UpdateFavoriteButton();
        LoadStatus();
    }

    private void LoadStatus()
    {
        _loadingStatus = true;
        foreach (StatusChoice item in _statusCombo.Items)
        {
            if (item.Status == _paper.Status)
            {
                _statusCombo.SelectedItem = item;
                break;
            }
        }
        _loadingStatus = false;
    }

    private void SaveStatus()
    {
        if (_loadingStatus || _statusCombo.SelectedItem is not StatusChoice choice)
            return;
        ExamPaper? current = _dataService.Data.ExamPapers.FirstOrDefault(item => item.Id == _paper.Id);
        if (current == null || current.Status == choice.Status)
            return;
        current.Status = choice.Status;
        current.UpdatedAt = DateTime.Now;
        _paper.Status = choice.Status;
        _dataService.Log(ActivityType.Updated, "ExamPaper", current.Id,
            $"更新考古題狀態：{current.Title} → {ExamLibraryService.StatusText(choice.Status)}");
        _dataService.SaveAndNotify();
    }

    private void UpdateFavoriteButton()
    {
        _favoriteButton.Text = _paper.IsFavorite ? "★ 已收藏" : "☆ 收藏";
        _favoriteButton.ForeColor = _paper.IsFavorite ? UiTheme.Warning : UiTheme.Slate;
    }

    private static void CopyPaper(ExamPaper source, ExamPaper target)
    {
        target.SubjectId = source.SubjectId;
        target.Title = source.Title;
        target.ExamYear = source.ExamYear;
        target.Term = source.Term;
        target.Category = source.Category;
        target.Tags = source.Tags;
        target.Notes = source.Notes;
        target.IsFavorite = source.IsFavorite;
        target.Status = source.Status;
        target.UpdatedAt = source.UpdatedAt;
    }

    private sealed record StatusChoice(ExamPaperStatus Status, string Text)
    {
        public override string ToString() => Text;
    }
}
