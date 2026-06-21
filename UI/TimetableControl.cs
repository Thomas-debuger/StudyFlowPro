using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class TimetableControl : UserControl
{
    private readonly DataService _service;
    private readonly ComboBox _semesterCombo = new();
    private readonly DataGridView _grid = new();
    private readonly Label _semesterMetric = new();
    private readonly Label _courseMetric = new();
    private readonly Label _periodMetric = new();
    private readonly Label _todayMetric = new();
    private readonly Label _selectionTitle = new();
    private readonly Label _selectionDetail = new();
    private readonly Label _semesterBadgeTitle = new();
    private readonly Label _semesterBadgeDetail = new();
    private readonly Label _todayHintTitle = new();
    private readonly Label _todayHintDetail = new();

    private TimetableEntry? _selectedEntry;
    private bool _loadingSemester;
    private int _lastSelectedDay = 1;
    private int _lastSelectedPeriod = 1;

    public TimetableControl(DataService service)
    {
        _service = service;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Background;
        Margin = Padding.Empty;
        Padding = Padding.Empty;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;

        BuildInterface();
        InitializeSemester();
        RefreshTimetable();
    }

    public void RefreshTimetable()
    {
        if (!SemesterOptionsMatchData())
            InitializeSemester();
        if (_semesterCombo.SelectedItem is not SemesterOption option)
            return;

        string semester = option.Code;
        List<TimetableEntry> entries = _service.Data.TimetableEntries
            .Where(item => item.SemesterCode == semester)
            .OrderBy(item => item.DayIndex)
            .ThenBy(item => item.StartPeriod)
            .ThenBy(item => item.CourseName)
            .ToList();

        BuildGridRows(entries);
        RefreshMetrics(semester, entries);
        RefreshTodayHint(semester, entries);
        RefreshSelection();
        _grid.Invalidate();
    }


    private bool SemesterOptionsMatchData()
    {
        if (_semesterCombo.Items.Count != _service.Data.TimetableSemesters.Count)
            return false;
        HashSet<string> dataCodes = _service.Data.TimetableSemesters
            .Select(item => item.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return _semesterCombo.Items.Cast<SemesterOption>()
            .All(item => dataCodes.Contains(item.Code));
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(26, 18, 26, 22),
            Margin = Padding.Empty,
            BackColor = UiTheme.Background
        };
        // 課表標題卡在高 DPI / 顯示縮放下容易讓副標題被裁切，因此提高固定高度。
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildMetrics(), 0, 1);
        root.Controls.Add(BuildToolbar(), 0, 2);
        root.Controls.Add(BuildGridCard(), 0, 3);
        root.Controls.Add(BuildDetailsCard(), 0, 4);
        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(20, 14, 18, 14),
            Margin = new Padding(0, 0, 0, 10)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));

        var title = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        title.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        title.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        title.Controls.Add(new Label
        {
            Text = "課表",
            Dock = DockStyle.Fill,
            AutoSize = false,
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(23, FontStyle.Bold)
        }, 0, 0);
        title.Controls.Add(new Label
        {
            Text = "管理各學期上課時段、教室與顏色；雙擊課程格即可編輯。",
            Dock = DockStyle.Fill,
            AutoSize = false,
            Margin = Padding.Empty,
            Padding = new Padding(0, 2, 0, 0),
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9.5f)
        }, 0, 1);

        var badge = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.PrimarySoft,
            Padding = new Padding(16, 8, 12, 8),
            Margin = new Padding(12, 0, 0, 0)
        };
        var badgeLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = badge.BackColor
        };
        // 兩行資訊由卡片上方依序往下排列，避免整組文字落在區塊下半部。
        badgeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        badgeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        badgeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _semesterBadgeTitle.Dock = DockStyle.Fill;
        _semesterBadgeTitle.AutoSize = false;
        _semesterBadgeTitle.Margin = Padding.Empty;
        _semesterBadgeTitle.TextAlign = ContentAlignment.MiddleLeft;
        _semesterBadgeTitle.ForeColor = UiTheme.PrimaryDark;
        _semesterBadgeTitle.Font = UiTheme.Font(9.4f, FontStyle.Bold);
        _semesterBadgeDetail.Dock = DockStyle.Fill;
        _semesterBadgeDetail.AutoSize = false;
        _semesterBadgeDetail.Margin = Padding.Empty;
        _semesterBadgeDetail.TextAlign = ContentAlignment.TopLeft;
        _semesterBadgeDetail.ForeColor = UiTheme.PrimaryDark;
        _semesterBadgeDetail.Font = UiTheme.Font(8.8f);
        badgeLayout.Controls.Add(_semesterBadgeTitle, 0, 0);
        badgeLayout.Controls.Add(_semesterBadgeDetail, 0, 1);
        badge.Controls.Add(badgeLayout);

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(badge, 1, 0);
        card.Controls.Add(layout);
        return card;
    }

    private Control BuildMetrics()
    {
        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        for (int index = 0; index < 4; index++)
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        metrics.Controls.Add(CreateMetricCard("目前學期", _semesterMetric, UiTheme.Primary, 0), 0, 0);
        metrics.Controls.Add(CreateMetricCard("不重複課程", _courseMetric, UiTheme.Purple, 1), 1, 0);
        metrics.Controls.Add(CreateMetricCard("每週上課節數", _periodMetric, UiTheme.Success, 1), 2, 0);
        metrics.Controls.Add(CreateMetricCard("今天課程", _todayMetric, UiTheme.Warning, 1), 3, 0);
        return metrics;
    }

    private static Control CreateMetricCard(string title, Label valueLabel, Color accent, int leftMargin)
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(18, 14, 14, 12),
            Margin = new Padding(leftMargin == 0 ? 0 : 8, 0, 0, 10)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 7));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var accentBar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = accent,
            Margin = Padding.Empty
        };
        layout.Controls.Add(accentBar, 0, 0);
        layout.SetRowSpan(accentBar, 2);

        layout.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            AutoSize = false,
            Margin = new Padding(10, 0, 0, 0),
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9),
            TextAlign = ContentAlignment.MiddleLeft
        }, 1, 0);

        valueLabel.Dock = DockStyle.Fill;
        valueLabel.AutoSize = false;
        valueLabel.AutoEllipsis = true;
        valueLabel.Margin = new Padding(10, 0, 0, 0);
        valueLabel.ForeColor = UiTheme.Navy;
        valueLabel.Font = UiTheme.Font(20, FontStyle.Bold);
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(valueLabel, 1, 1);
        card.Controls.Add(layout);
        return card;
    }

    private Control BuildToolbar()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(14, 10, 14, 10),
            Margin = new Padding(0, 0, 0, 10)
        };

        var rows = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        rows.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        rows.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        rows.Controls.Add(BuildPrimaryToolbar(), 0, 0);
        rows.Controls.Add(BuildSecondaryToolbar(), 0, 1);
        card.Controls.Add(rows);
        return card;
    }

    private Control BuildPrimaryToolbar()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 8,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 224));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 162));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 14));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 98));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        row.Controls.Add(new Label
        {
            Text = "學期",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9.5f, FontStyle.Bold)
        }, 0, 0);

        _semesterCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _semesterCombo.Dock = DockStyle.Fill;
        _semesterCombo.Margin = new Padding(0, 4, 8, 4);
        _semesterCombo.Font = UiTheme.Font(10);
        _semesterCombo.SelectedIndexChanged += (_, _) => SemesterChanged();
        row.Controls.Add(_semesterCombo, 1, 0);

        Button deleteSemester = FixedButton(UiTheme.DangerButton("刪除學期"), 112);
        deleteSemester.Click += (_, _) => DeleteCurrentSemester();
        row.Controls.Add(deleteSemester, 2, 0);

        Button add = FixedButton(UiTheme.PrimaryButton("新增課程"), 122);
        Button edit = FixedButton(UiTheme.SecondaryButton("編輯"), 88);
        Button delete = FixedButton(UiTheme.DangerButton("刪除課程"), 102);
        add.Click += (_, _) => AddEntry();
        edit.Click += (_, _) => EditSelectedEntry();
        delete.Click += (_, _) => DeleteSelectedEntry();
        row.Controls.Add(add, 4, 0);
        row.Controls.Add(edit, 5, 0);
        row.Controls.Add(delete, 6, 0);
        row.Controls.Add(new Label
        {
            Text = "點選課程格查看資訊；雙擊可直接編輯。",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.8f)
        }, 7, 0);
        return row;
    }

    private Control BuildSecondaryToolbar()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 162));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 156));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        Button addSemester = FixedButton(UiTheme.PrimaryButton("新增學期課表"), 140);
        addSemester.BackColor = UiTheme.Success;
        addSemester.FlatAppearance.BorderColor = UiTheme.Success;
        Button importCsv = FixedButton(UiTheme.SecondaryButton("匯入課表 CSV"), 132);
        Button exportCsv = FixedButton(UiTheme.SecondaryButton("匯出課表 CSV"), 152);
        Button exportPng = FixedButton(UiTheme.SecondaryButton("匯出課表 PNG"), 132);
        Button restore = FixedButton(UiTheme.SecondaryButton("還原此學期範例"), 146);

        addSemester.Click += (_, _) => AddSemester();
        importCsv.Click += (_, _) => ImportCsvIntoCurrentSemester();
        exportCsv.Click += (_, _) => ExportCsv();
        exportPng.Click += (_, _) => ExportPng();
        restore.Click += (_, _) => RestoreSemester();

        row.Controls.Add(addSemester, 0, 0);
        row.Controls.Add(importCsv, 1, 0);
        row.Controls.Add(exportCsv, 2, 0);
        row.Controls.Add(exportPng, 3, 0);
        row.Controls.Add(restore, 4, 0);
        row.Controls.Add(new Label
        {
            Text = "新增學期後可立即匯入 CSV；同一時段會自動檢查衝突。",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.7f)
        }, 5, 0);
        return row;
    }

    private static Button FixedButton(Button button, int width)
    {
        button.AutoSize = false;
        button.Dock = DockStyle.None;
        button.Size = new Size(width, 36);
        button.Anchor = AnchorStyles.Left;
        button.Margin = new Padding(0, 3, 8, 3);
        button.Padding = Padding.Empty;
        button.TextAlign = ContentAlignment.MiddleCenter;
        return button;
    }

    private Control BuildGridCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Border,
            Padding = new Padding(1),
            Margin = Padding.Empty
        };

        ConfigureGrid();
        card.Controls.Add(_grid);
        return card;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = UiTheme.Surface;
        _grid.BorderStyle = BorderStyle.None;
        _grid.RowHeadersVisible = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.AllowUserToResizeColumns = false;
        _grid.ReadOnly = true;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersHeight = 42;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.GridColor = UiTheme.Border;
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.Surface,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9.2f),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            WrapMode = DataGridViewTriState.True,
            SelectionBackColor = UiTheme.Selection,
            SelectionForeColor = UiTheme.Navy,
            Padding = new Padding(5)
        };
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.Sidebar,
            ForeColor = Color.White,
            Font = UiTheme.Font(9.5f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            SelectionBackColor = UiTheme.Sidebar,
            SelectionForeColor = Color.White
        };

        var periodColumn = new DataGridViewTextBoxColumn
        {
            Name = "Period",
            HeaderText = "節次",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            Width = 126,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            ReadOnly = true,
            Frozen = true
        };
        _grid.Columns.Add(periodColumn);
        for (int day = 1; day <= 6; day++)
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = $"Day{day}",
                HeaderText = TimetableCatalog.DayNames[day],
                FillWeight = 100,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                ReadOnly = true
            });
        }

        _grid.CellClick += (_, e) => SelectCell(e.RowIndex, e.ColumnIndex);
        _grid.CellDoubleClick += (_, e) =>
        {
            SelectCell(e.RowIndex, e.ColumnIndex);
            if (_selectedEntry != null)
                EditSelectedEntry();
            else if (e.RowIndex >= 0 && e.ColumnIndex > 0)
                AddEntry();
        };
        _grid.CellPainting += GridCellPainting;
    }

    private Control BuildDetailsCard()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(18, 10, 18, 10),
            Margin = new Padding(0, 10, 0, 0)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

        var selected = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        selected.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        selected.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _selectionTitle.Dock = DockStyle.Fill;
        _selectionTitle.AutoSize = false;
        _selectionTitle.AutoEllipsis = true;
        _selectionTitle.ForeColor = UiTheme.Navy;
        _selectionTitle.Font = UiTheme.Font(11, FontStyle.Bold);
        _selectionTitle.TextAlign = ContentAlignment.MiddleLeft;
        _selectionDetail.Dock = DockStyle.Fill;
        _selectionDetail.AutoSize = false;
        _selectionDetail.AutoEllipsis = true;
        _selectionDetail.ForeColor = UiTheme.Muted;
        _selectionDetail.Font = UiTheme.Font(9);
        _selectionDetail.TextAlign = ContentAlignment.TopLeft;
        selected.Controls.Add(_selectionTitle, 0, 0);
        selected.Controls.Add(_selectionDetail, 0, 1);

        var today = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.PrimarySoft,
            Margin = new Padding(12, 0, 0, 0),
            Padding = new Padding(14, 7, 10, 7)
        };
        var todayLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = today.BackColor
        };
        // 「今日提示」由卡片上方開始排列，第二行緊接在標題下方。
        todayLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        todayLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        todayLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _todayHintTitle.Dock = DockStyle.Fill;
        _todayHintTitle.AutoSize = false;
        _todayHintTitle.Margin = Padding.Empty;
        _todayHintTitle.TextAlign = ContentAlignment.MiddleLeft;
        _todayHintTitle.ForeColor = UiTheme.PrimaryDark;
        _todayHintTitle.Font = UiTheme.Font(9.3f, FontStyle.Bold);
        _todayHintDetail.Dock = DockStyle.Fill;
        _todayHintDetail.AutoSize = false;
        _todayHintDetail.Margin = Padding.Empty;
        _todayHintDetail.TextAlign = ContentAlignment.TopLeft;
        _todayHintDetail.ForeColor = UiTheme.PrimaryDark;
        _todayHintDetail.Font = UiTheme.Font(8.8f);
        todayLayout.Controls.Add(_todayHintTitle, 0, 0);
        todayLayout.Controls.Add(_todayHintDetail, 0, 1);
        today.Controls.Add(todayLayout);

        layout.Controls.Add(selected, 0, 0);
        layout.Controls.Add(today, 1, 0);
        card.Controls.Add(layout);
        return card;
    }

    private void InitializeSemester(string? selectCode = null)
    {
        _loadingSemester = true;
        string desired = selectCode ?? _service.Data.Settings.LastTimetableSemester;
        _semesterCombo.Items.Clear();
        foreach (TimetableSemester semester in _service.Data.TimetableSemesters)
            _semesterCombo.Items.Add(new SemesterOption(semester.Code, semester.DisplayName));
        _semesterCombo.DisplayMember = nameof(SemesterOption.Display);

        int index = _semesterCombo.Items
            .Cast<SemesterOption>()
            .Select((item, itemIndex) => new { item.Code, Index = itemIndex })
            .FirstOrDefault(item => string.Equals(item.Code, desired, StringComparison.OrdinalIgnoreCase))?.Index ?? 0;
        if (_semesterCombo.Items.Count > 0)
            _semesterCombo.SelectedIndex = index;
        _loadingSemester = false;
        RefreshSemesterBadge();
    }

    private void RefreshSemesterBadge()
    {
        int count = _service.Data.TimetableSemesters.Count;
        _semesterBadgeTitle.Text = "學期課表管理";
        _semesterBadgeDetail.Text = $"目前共 {count} 個學期，可新增、刪除或匯入 CSV。";
    }

    private void SemesterChanged()
    {
        if (_loadingSemester || _semesterCombo.SelectedItem is not SemesterOption option)
            return;

        _selectedEntry = null;
        _service.Data.Settings.LastTimetableSemester = option.Code;
        try
        {
            _service.Save();
        }
        catch
        {
            // 學期偏好存檔失敗不應阻止課表切換。
        }
        RefreshTimetable();
    }

    private void BuildGridRows(List<TimetableEntry> entries)
    {
        _grid.SuspendLayout();
        try
        {
            _grid.Rows.Clear();
            for (int period = 1; period <= 10; period++)
            {
                int rowIndex = _grid.Rows.Add();
                DataGridViewRow row = _grid.Rows[rowIndex];
                row.Height = 66;
                DataGridViewCell periodCell = row.Cells[0];
                periodCell.Value = $"第 {period} 節\n{TimetableCatalog.PeriodTimes[period].Start}–{TimetableCatalog.PeriodTimes[period].End}";
                periodCell.Style = new DataGridViewCellStyle
                {
                    BackColor = UiTheme.PrimaryDark,
                    ForeColor = Color.White,
                    SelectionBackColor = UiTheme.PrimaryDark,
                    SelectionForeColor = Color.White,
                    Font = UiTheme.Font(9.2f, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    WrapMode = DataGridViewTriState.True,
                    Padding = new Padding(4)
                };

                for (int day = 1; day <= 6; day++)
                {
                    DataGridViewCell cell = row.Cells[day];
                    TimetableEntry? entry = entries.FirstOrDefault(item =>
                        item.DayIndex == day &&
                        period >= item.StartPeriod &&
                        period <= item.EndPeriod);

                    if (entry == null)
                    {
                        cell.Value = string.Empty;
                        cell.Tag = null;
                        cell.ToolTipText = $"{TimetableCatalog.DayNames[day]}　{TimetableCatalog.PeriodDisplay(period)}\n雙擊空白格可新增課程。";
                        cell.Style = EmptyCellStyle();
                        continue;
                    }

                    Color accent = ParseColor(entry.ColorHex, UiTheme.Primary);
                    Color background = Blend(accent, UiTheme.Surface, 0.82f);
                    Color selected = Blend(accent, UiTheme.Surface, 0.62f);
                    cell.Value = string.IsNullOrWhiteSpace(entry.Location)
                        ? entry.CourseName
                        : $"{entry.CourseName}\n{entry.Location}";
                    cell.Tag = entry;
                    cell.ToolTipText = BuildTooltip(entry);
                    cell.Style = new DataGridViewCellStyle
                    {
                        BackColor = background,
                        ForeColor = UiTheme.Navy,
                        SelectionBackColor = selected,
                        SelectionForeColor = UiTheme.Navy,
                        Font = UiTheme.Font(9.1f, period == entry.StartPeriod ? FontStyle.Bold : FontStyle.Regular),
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        WrapMode = DataGridViewTriState.True,
                        Padding = new Padding(5)
                    };
                }
            }
        }
        finally
        {
            _grid.ResumeLayout();
        }
    }

    private static DataGridViewCellStyle EmptyCellStyle() => new()
    {
        BackColor = UiTheme.Surface,
        ForeColor = UiTheme.Slate,
        SelectionBackColor = UiTheme.PrimarySoft,
        SelectionForeColor = UiTheme.Navy,
        Font = UiTheme.Font(9.1f),
        Alignment = DataGridViewContentAlignment.MiddleCenter,
        WrapMode = DataGridViewTriState.True,
        Padding = new Padding(5)
    };

    private void RefreshMetrics(string semester, List<TimetableEntry> entries)
    {
        string display = TimetableCatalog.SemesterDisplay(_service.Data.TimetableSemesters, semester);
        _semesterMetric.Text = Regex.IsMatch(semester, @"^\d{3}-[12]$") ? semester : display;
        _semesterMetric.Font = UiTheme.Font(_semesterMetric.Text.Length > 12 ? 14 : 20, FontStyle.Bold);
        _courseMetric.Text = entries.Select(item => item.CourseName).Distinct().Count().ToString();
        _periodMetric.Text = entries.Sum(item => item.EndPeriod - item.StartPeriod + 1) + " 節";
        int todayIndex = TimetableCatalog.ToDayIndex(DateTime.Today.DayOfWeek);
        _todayMetric.Text = todayIndex == 0
            ? "0 門"
            : entries.Count(item => item.DayIndex == todayIndex) + " 門";
    }

    private void RefreshTodayHint(string semester, List<TimetableEntry> entries)
    {
        int day = TimetableCatalog.ToDayIndex(DateTime.Now.DayOfWeek);
        if (day == 0)
        {
            _todayHintTitle.Text = "今日提示";
            _todayHintDetail.Text = "週日沒有排定課程，適合整理筆記與規劃下週。";
            return;
        }

        int currentPeriod = TimetableCatalog.GetCurrentPeriod(DateTime.Now);
        TimetableEntry? current = currentPeriod == 0
            ? null
            : entries.FirstOrDefault(item => item.DayIndex == day && currentPeriod >= item.StartPeriod && currentPeriod <= item.EndPeriod);
        if (current != null)
        {
            _todayHintTitle.Text = "現在上課";
            _todayHintDetail.Text = string.IsNullOrWhiteSpace(current.Location)
                ? current.CourseName
                : $"{current.CourseName}　{current.Location}";
            return;
        }

        TimeSpan now = DateTime.Now.TimeOfDay;
        TimetableEntry? next = entries
            .Where(item => item.DayIndex == day)
            .OrderBy(item => item.StartPeriod)
            .FirstOrDefault(item => TimeSpan.TryParse(TimetableCatalog.PeriodTimes[item.StartPeriod].Start, out TimeSpan start) && start > now);
        if (next == null)
        {
            _todayHintTitle.Text = "今日提示";
            _todayHintDetail.Text = "今天已沒有下一堂課。";
        }
        else
        {
            _todayHintTitle.Text = "下一堂課";
            _todayHintDetail.Text = $"{TimetableCatalog.PeriodTimes[next.StartPeriod].Start}　{next.CourseName}";
        }
    }

    private void SelectCell(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || columnIndex < 0)
            return;

        if (columnIndex > 0)
        {
            _lastSelectedDay = columnIndex;
            _lastSelectedPeriod = rowIndex + 1;
        }
        _selectedEntry = _grid.Rows[rowIndex].Cells[columnIndex].Tag as TimetableEntry;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        if (_selectedEntry == null)
        {
            _selectionTitle.Text = "選取一個課程格查看詳細資訊";
            _selectionDetail.Text = "可新增、編輯、刪除課程，也能建立或刪除學期、匯入 CSV 與匯出課表；雙擊空白格會帶入星期與節次。";
            return;
        }

        string instructor = string.IsNullOrWhiteSpace(_selectedEntry.Instructor)
            ? "未填授課者"
            : _selectedEntry.Instructor;
        string location = string.IsNullOrWhiteSpace(_selectedEntry.Location)
            ? "未填教室"
            : _selectedEntry.Location;
        _selectionTitle.Text = _selectedEntry.CourseName;
        _selectionDetail.Text =
            $"{TimetableCatalog.DayNames[_selectedEntry.DayIndex]}　{TimetableCatalog.PeriodRangeDisplay(_selectedEntry.StartPeriod, _selectedEntry.EndPeriod)}　｜　{location}　｜　{instructor}";
    }

    private void AddEntry()
    {
        if (_semesterCombo.SelectedItem is not SemesterOption semester)
            return;

        using var form = new TimetableEditorForm(
            _service,
            null,
            semester.Code,
            _lastSelectedDay,
            _lastSelectedPeriod);
        if (form.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        _service.Data.TimetableEntries.Add(form.ResultEntry);
        _service.Log(ActivityType.Created, "Timetable", form.ResultEntry.Id,
            $"新增課表課程：{form.ResultEntry.CourseName}");
        _service.SaveAndNotify();
        _selectedEntry = form.ResultEntry;
        RefreshTimetable();
    }

    private void EditSelectedEntry()
    {
        if (_selectedEntry == null)
        {
            MessageBox.Show("請先點選一個課程格。", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new TimetableEditorForm(_service, _selectedEntry);
        if (form.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        int index = _service.Data.TimetableEntries.FindIndex(item => item.Id == _selectedEntry.Id);
        if (index < 0)
            return;

        _service.Data.TimetableEntries[index] = form.ResultEntry;
        _service.Log(ActivityType.Updated, "Timetable", form.ResultEntry.Id,
            $"編輯課表課程：{form.ResultEntry.CourseName}");
        _selectedEntry = form.ResultEntry;
        _service.SaveAndNotify();
        RefreshTimetable();
    }

    private void DeleteSelectedEntry()
    {
        if (_selectedEntry == null)
        {
            MessageBox.Show("請先點選要刪除的課程格。", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        TimetableEntry deleting = _selectedEntry;
        bool confirmed = ConfirmDialog.Ask(
            FindForm()!,
            "刪除課表課程",
            $"確定刪除「{deleting.CourseName}」？\n" +
            $"{TimetableCatalog.DayNames[deleting.DayIndex]}　{TimetableCatalog.PeriodRangeDisplay(deleting.StartPeriod, deleting.EndPeriod)}",
            "刪除");
        if (!confirmed)
            return;

        _service.Data.TimetableEntries.RemoveAll(item => item.Id == deleting.Id);
        _service.Log(ActivityType.Deleted, "Timetable", deleting.Id,
            $"刪除課表課程：{deleting.CourseName}");
        _selectedEntry = null;
        _service.SaveAndNotify();
        RefreshTimetable();
    }

    private void AddSemester()
    {
        using var form = new TimetableSemesterEditorForm(_service.Data.TimetableSemesters);
        if (form.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        TimetableSemester semester = form.ResultSemester;
        _service.Data.TimetableSemesters.Add(semester);
        _service.Data.Settings.LastTimetableSemester = semester.Code;
        _service.Log(ActivityType.Created, "TimetableSemester", null,
            $"新增學期課表：{semester.DisplayName}");
        _service.SaveAndNotify();
        _selectedEntry = null;
        InitializeSemester(semester.Code);
        RefreshTimetable();

        DialogResult importNow = MessageBox.Show(
            $"「{semester.DisplayName}」已建立。\n\n是否立即匯入電腦中的 CSV 課表？",
            "學期建立完成",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information);
        if (importNow == DialogResult.Yes)
            ImportCsvIntoCurrentSemester();
    }

    private void DeleteCurrentSemester()
    {
        if (_semesterCombo.SelectedItem is not SemesterOption option)
            return;

        if (_service.Data.TimetableSemesters.Count <= 1)
        {
            MessageBox.Show("至少要保留一個學期課表。", "無法刪除",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        int courseCount = _service.Data.TimetableEntries.Count(item => item.SemesterCode == option.Code);
        bool confirmed = ConfirmDialog.Ask(
            FindForm()!,
            "刪除學期課表",
            $"確定刪除「{option.Display}」？\n\n此操作會同時刪除該學期的 {courseCount} 筆課程，且無法復原。",
            "刪除學期課表");
        if (!confirmed)
            return;

        int oldIndex = _semesterCombo.SelectedIndex;
        _service.Data.TimetableEntries.RemoveAll(item => item.SemesterCode == option.Code);
        _service.Data.TimetableSemesters.RemoveAll(item =>
            string.Equals(item.Code, option.Code, StringComparison.OrdinalIgnoreCase));
        TimetableSemester next = _service.Data.TimetableSemesters[Math.Clamp(oldIndex, 0, _service.Data.TimetableSemesters.Count - 1)];
        _service.Data.Settings.LastTimetableSemester = next.Code;
        _service.Log(ActivityType.Deleted, "TimetableSemester", null,
            $"刪除學期課表：{option.Display}（{courseCount} 筆課程）");
        _service.SaveAndNotify();
        _selectedEntry = null;
        InitializeSemester(next.Code);
        RefreshTimetable();
    }

    private void ImportCsvIntoCurrentSemester()
    {
        if (_semesterCombo.SelectedItem is not SemesterOption option)
            return;

        using var dialog = new OpenFileDialog
        {
            Filter = "CSV 課表 (*.csv)|*.csv|所有檔案 (*.*)|*.*",
            Title = $"匯入課表到 {option.Display}",
            Multiselect = false
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        bool replaceExisting = false;
        int existingCount = _service.Data.TimetableEntries.Count(item => item.SemesterCode == option.Code);
        if (existingCount > 0)
        {
            DialogResult mode = MessageBox.Show(
                $"「{option.Display}」目前已有 {existingCount} 筆課程。\n\n" +
                "按「是」：清空目前學期後匯入\n" +
                "按「否」：保留現有課程並合併匯入\n" +
                "按「取消」：停止匯入",
                "選擇匯入方式",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            if (mode == DialogResult.Cancel)
                return;
            replaceExisting = mode == DialogResult.Yes;
        }

        CsvImportResult result;
        try
        {
            result = ParseTimetableCsv(dialog.FileName, option.Code);
        }
        catch (Exception ex)
        {
            MessageBox.Show("讀取 CSV 失敗：\n" + ex.Message, "匯入失敗",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (result.ValidEntries.Count == 0)
        {
            string details = result.Errors.Count == 0
                ? "CSV 中沒有可匯入的課程資料。"
                : string.Join("\n", result.Errors.Take(6));
            MessageBox.Show(details, "沒有可匯入資料",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var occupied = replaceExisting
            ? new List<TimetableEntry>()
            : _service.Data.TimetableEntries.Where(item => item.SemesterCode == option.Code).ToList();
        var accepted = new List<TimetableEntry>();
        int conflictCount = 0;
        foreach (TimetableEntry entry in result.ValidEntries)
        {
            bool conflict = occupied.Concat(accepted).Any(existing =>
                existing.DayIndex == entry.DayIndex &&
                entry.StartPeriod <= existing.EndPeriod &&
                entry.EndPeriod >= existing.StartPeriod);
            if (conflict)
            {
                conflictCount++;
                continue;
            }
            accepted.Add(entry);
        }

        if (accepted.Count == 0)
        {
            MessageBox.Show("所有資料都與目前課表時段衝突，未匯入任何課程。", "匯入完成",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (replaceExisting)
            _service.Data.TimetableEntries.RemoveAll(item => item.SemesterCode == option.Code);
        _service.Data.TimetableEntries.AddRange(accepted);
        _service.Log(ActivityType.Imported, "Timetable", null,
            $"匯入課表 CSV：{option.Display}，新增 {accepted.Count} 筆");
        _service.SaveAndNotify();
        _selectedEntry = null;
        RefreshTimetable();

        var summary = new StringBuilder();
        summary.AppendLine($"已匯入 {accepted.Count} 筆課程到「{option.Display}」。");
        if (conflictCount > 0)
            summary.AppendLine($"略過 {conflictCount} 筆時段衝突。");
        if (result.Errors.Count > 0)
            summary.AppendLine($"另有 {result.Errors.Count} 列格式不完整而略過。");
        MessageBox.Show(summary.ToString().Trim(), "匯入完成",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RestoreSemester()
    {
        if (_semesterCombo.SelectedItem is not SemesterOption option)
            return;

        if (!TimetableCatalog.IsBuiltInSemester(option.Code))
        {
            MessageBox.Show(
                "自訂學期沒有內建範例可還原。你可以新增課程，或使用「匯入課表 CSV」。",
                "無內建範例",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (!ConfirmDialog.Ask(
                FindForm()!,
                "還原學期範例",
                $"這會以附圖整理出的原始課表取代「{option.Display}」目前所有課程。\n確定繼續嗎？",
                "還原"))
            return;

        _service.Data.TimetableEntries.RemoveAll(item => item.SemesterCode == option.Code);
        _service.Data.TimetableEntries.AddRange(TimetableSeedData.Create()
            .Where(item => item.SemesterCode == option.Code));
        _service.Log(ActivityType.Restored, "Timetable", null,
            $"還原課表範例：{option.Display}");
        _selectedEntry = null;
        _service.SaveAndNotify();
        RefreshTimetable();
    }

    private void ExportCsv()
    {
        if (_semesterCombo.SelectedItem is not SemesterOption option)
            return;

        using var dialog = new SaveFileDialog
        {
            Filter = "CSV 檔案 (*.csv)|*.csv",
            FileName = $"StudyFlowPro_課表_{SanitizeFileName(option.Display)}.csv"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        List<TimetableEntry> entries = _service.Data.TimetableEntries
            .Where(item => item.SemesterCode == option.Code)
            .OrderBy(item => item.DayIndex)
            .ThenBy(item => item.StartPeriod)
            .ToList();
        var lines = new List<string>
        {
            "學期代碼,學期名稱,星期,開始節次,結束節次,開始時間,結束時間,課程名稱,教室,授課者,顏色,備註"
        };
        lines.AddRange(entries.Select(item => string.Join(",", new[]
        {
            Csv(option.Code),
            Csv(option.Display),
            Csv(TimetableCatalog.DayNames[item.DayIndex]),
            item.StartPeriod.ToString(CultureInfo.InvariantCulture),
            item.EndPeriod.ToString(CultureInfo.InvariantCulture),
            Csv(TimetableCatalog.PeriodTimes[item.StartPeriod].Start),
            Csv(TimetableCatalog.PeriodTimes[item.EndPeriod].End),
            Csv(item.CourseName),
            Csv(item.Location),
            Csv(item.Instructor),
            Csv(item.ColorHex),
            Csv(item.Notes)
        })));
        File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(true));
        _service.Log(ActivityType.Exported, "Timetable", null,
            $"匯出課表 CSV：{option.Display}");
        _service.SaveAndNotify();
        MessageBox.Show("課表 CSV 已匯出。", "完成",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ExportPng()
    {
        if (_semesterCombo.SelectedItem is not SemesterOption option)
            return;

        using var dialog = new SaveFileDialog
        {
            Filter = "PNG 圖片 (*.png)|*.png",
            FileName = $"StudyFlowPro_課表_{SanitizeFileName(option.Display)}.png"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        using Bitmap image = RenderTimetableImage(option.Code, option.Display);
        image.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
        _service.Log(ActivityType.Exported, "Timetable", null,
            $"匯出課表 PNG：{option.Display}");
        _service.SaveAndNotify();
        MessageBox.Show("課表圖片已匯出。", "完成",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private CsvImportResult ParseTimetableCsv(string path, string targetSemesterCode)
    {
        string[] lines;
        using (var reader = new StreamReader(path, Encoding.UTF8, true))
        {
            var all = new List<string>();
            while (!reader.EndOfStream)
                all.Add(reader.ReadLine() ?? string.Empty);
            lines = all.ToArray();
        }

        var result = new CsvImportResult();
        if (lines.Length == 0)
            return result;

        List<string> headers = ParseCsvLine(lines[0]);
        var headerMap = headers
            .Select((header, index) => new { Header = NormalizeHeader(header), Index = index })
            .GroupBy(item => item.Header, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Index, StringComparer.OrdinalIgnoreCase);

        int Find(params string[] names)
        {
            foreach (string name in names)
            {
                if (headerMap.TryGetValue(NormalizeHeader(name), out int index))
                    return index;
            }
            return -1;
        }

        int dayIndex = Find("星期", "週次", "Day");
        int startIndex = Find("開始節次", "開始節", "StartPeriod");
        int endIndex = Find("結束節次", "結束節", "EndPeriod");
        int courseIndex = Find("課程名稱", "課程", "CourseName");
        int locationIndex = Find("教室", "地點", "Location");
        int instructorIndex = Find("授課者", "老師", "Instructor");
        int colorIndex = Find("顏色", "Color", "ColorHex");
        int notesIndex = Find("備註", "Notes");

        if (dayIndex < 0 || startIndex < 0 || endIndex < 0 || courseIndex < 0)
            throw new InvalidDataException("CSV 必須包含「星期、開始節次、結束節次、課程名稱」欄位。");

        string Value(IReadOnlyList<string> values, int index) =>
            index >= 0 && index < values.Count ? values[index].Trim() : string.Empty;

        string[] palette =
        {
            "#2563EB", "#7C3AED", "#059669", "#D97706", "#DB2777",
            "#0891B2", "#EA580C", "#0F766E", "#DC2626", "#4F46E5"
        };
        var colorByCourse = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (int lineNumber = 2; lineNumber <= lines.Length; lineNumber++)
        {
            string line = lines[lineNumber - 1];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            List<string> values = ParseCsvLine(line);
            string course = Value(values, courseIndex);
            if (string.IsNullOrWhiteSpace(course))
            {
                result.Errors.Add($"第 {lineNumber} 列：課程名稱為空白。");
                continue;
            }

            if (!TryParseDay(Value(values, dayIndex), out int day) ||
                !int.TryParse(Value(values, startIndex), NumberStyles.Integer, CultureInfo.InvariantCulture, out int start) ||
                !int.TryParse(Value(values, endIndex), NumberStyles.Integer, CultureInfo.InvariantCulture, out int end) ||
                day is < 1 or > 6 || start is < 1 or > 10 || end < start || end > 10)
            {
                result.Errors.Add($"第 {lineNumber} 列：星期或節次格式錯誤。");
                continue;
            }

            string color = Value(values, colorIndex);
            if (!IsValidHtmlColor(color))
            {
                if (!colorByCourse.TryGetValue(course, out color))
                {
                    color = palette[colorByCourse.Count % palette.Length];
                    colorByCourse[course] = color;
                }
            }

            result.ValidEntries.Add(new TimetableEntry
            {
                Id = Guid.NewGuid(),
                SemesterCode = targetSemesterCode,
                CourseName = course,
                DayIndex = day,
                StartPeriod = start,
                EndPeriod = end,
                Location = Value(values, locationIndex),
                Instructor = Value(values, instructorIndex),
                ColorHex = color,
                Notes = Value(values, notesIndex),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        return result;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var builder = new StringBuilder();
        bool quoted = false;
        for (int index = 0; index < line.Length; index++)
        {
            char current = line[index];
            if (current == '"')
            {
                if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                {
                    builder.Append('"');
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }
                continue;
            }

            if (current == ',' && !quoted)
            {
                values.Add(builder.ToString());
                builder.Clear();
                continue;
            }
            builder.Append(current);
        }
        values.Add(builder.ToString());
        return values;
    }

    private static string NormalizeHeader(string value) =>
        (value ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("/", string.Empty).ToLowerInvariant();

    private static bool TryParseDay(string value, out int day)
    {
        string normalized = (value ?? string.Empty).Trim()
            .Replace("星期", "週")
            .Replace("周", "週");
        day = normalized switch
        {
            "1" or "週一" or "mon" or "monday" => 1,
            "2" or "週二" or "tue" or "tuesday" => 2,
            "3" or "週三" or "wed" or "wednesday" => 3,
            "4" or "週四" or "thu" or "thursday" => 4,
            "5" or "週五" or "fri" or "friday" => 5,
            "6" or "週六" or "sat" or "saturday" => 6,
            _ => 0
        };
        return day > 0;
    }

    private static bool IsValidHtmlColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        try
        {
            _ = ColorTranslator.FromHtml(value.Trim());
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string SanitizeFileName(string value)
    {
        string result = value ?? "課表";
        foreach (char invalid in Path.GetInvalidFileNameChars())
            result = result.Replace(invalid, '_');
        return string.IsNullOrWhiteSpace(result) ? "課表" : result.Trim();
    }

    private Bitmap RenderTimetableImage(string semesterCode, string semesterDisplay)
    {
        const int width = 1680;
        const int height = 1210;
        const int margin = 50;
        const int titleHeight = 100;
        const int firstColumnWidth = 180;
        const int dayColumnWidth = 240;
        const int headerHeight = 62;
        const int rowHeight = 92;

        var bitmap = new Bitmap(width, height);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        graphics.Clear(UiTheme.Background);

        using var titleFont = UiTheme.Font(28, FontStyle.Bold);
        using var subtitleFont = UiTheme.Font(12);
        using var headerFont = UiTheme.Font(11, FontStyle.Bold);
        using var cellFont = UiTheme.Font(10, FontStyle.Bold);
        using var smallFont = UiTheme.Font(9);
        using var borderPen = new Pen(UiTheme.Border, 1);
        using var navyBrush = new SolidBrush(UiTheme.Navy);
        using var mutedBrush = new SolidBrush(UiTheme.Muted);

        graphics.DrawString($"StudyFlow Pro 課表｜{semesterDisplay}", titleFont, navyBrush, margin, 34);
        graphics.DrawString("可編輯課程、教室、節次與顏色｜匯出時間：" + DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
            subtitleFont, mutedBrush, margin, 78);

        int tableTop = margin + titleHeight;
        int tableLeft = margin;
        Rectangle topLeft = new(tableLeft, tableTop, firstColumnWidth, headerHeight);
        using var headerBrush = new SolidBrush(UiTheme.Sidebar);
        graphics.FillRectangle(headerBrush, topLeft);
        graphics.DrawRectangle(borderPen, topLeft);
        DrawCentered(graphics, "節次", headerFont, Color.White, topLeft);

        for (int day = 1; day <= 6; day++)
        {
            Rectangle rect = new(tableLeft + firstColumnWidth + (day - 1) * dayColumnWidth,
                tableTop, dayColumnWidth, headerHeight);
            graphics.FillRectangle(headerBrush, rect);
            graphics.DrawRectangle(borderPen, rect);
            DrawCentered(graphics, TimetableCatalog.DayNames[day], headerFont, Color.White, rect);
        }

        List<TimetableEntry> entries = _service.Data.TimetableEntries
            .Where(item => item.SemesterCode == semesterCode)
            .ToList();
        for (int period = 1; period <= 10; period++)
        {
            int y = tableTop + headerHeight + (period - 1) * rowHeight;
            Rectangle periodRect = new(tableLeft, y, firstColumnWidth, rowHeight);
            using var periodBrush = new SolidBrush(UiTheme.PrimaryDark);
            graphics.FillRectangle(periodBrush, periodRect);
            graphics.DrawRectangle(borderPen, periodRect);
            DrawCentered(graphics,
                $"第 {period} 節\n{TimetableCatalog.PeriodTimes[period].Start}–{TimetableCatalog.PeriodTimes[period].End}",
                smallFont, Color.White, periodRect);

            for (int day = 1; day <= 6; day++)
            {
                Rectangle rect = new(tableLeft + firstColumnWidth + (day - 1) * dayColumnWidth,
                    y, dayColumnWidth, rowHeight);
                TimetableEntry? entry = entries.FirstOrDefault(item =>
                    item.DayIndex == day && period >= item.StartPeriod && period <= item.EndPeriod);
                Color background = entry == null
                    ? UiTheme.Surface
                    : Blend(ParseColor(entry.ColorHex, UiTheme.Primary), UiTheme.Surface, 0.82f);
                using var backgroundBrush = new SolidBrush(background);
                graphics.FillRectangle(backgroundBrush, rect);
                graphics.DrawRectangle(borderPen, rect);
                if (entry != null)
                {
                    string text = string.IsNullOrWhiteSpace(entry.Location)
                        ? entry.CourseName
                        : entry.CourseName + "\n" + entry.Location;
                    DrawCentered(graphics, text, cellFont, UiTheme.Navy, rect);
                }
            }
        }

        return bitmap;
    }

    private void GridCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex <= 0 ||
            _semesterCombo.SelectedItem is not SemesterOption)
            return;

        int currentDay = TimetableCatalog.ToDayIndex(DateTime.Now.DayOfWeek);
        int currentPeriod = TimetableCatalog.GetCurrentPeriod(DateTime.Now);
        if (currentDay == 0 || currentPeriod == 0 ||
            e.ColumnIndex != currentDay || e.RowIndex != currentPeriod - 1)
            return;

        e.Paint(e.CellBounds, e.PaintParts);
        Rectangle rect = e.CellBounds;
        rect.Inflate(-2, -2);
        using var pen = new Pen(UiTheme.Warning, 3);
        e.Graphics.DrawRectangle(pen, rect);
        e.Handled = true;
    }

    private static string BuildTooltip(TimetableEntry entry)
    {
        var parts = new List<string>
        {
            entry.CourseName,
            TimetableCatalog.DayNames[entry.DayIndex] + "　" +
            TimetableCatalog.PeriodRangeDisplay(entry.StartPeriod, entry.EndPeriod)
        };
        if (!string.IsNullOrWhiteSpace(entry.Location))
            parts.Add("教室：" + entry.Location);
        if (!string.IsNullOrWhiteSpace(entry.Instructor))
            parts.Add("授課者：" + entry.Instructor);
        if (!string.IsNullOrWhiteSpace(entry.Notes))
            parts.Add("備註：" + entry.Notes);
        return string.Join("\n", parts);
    }

    private static string Csv(string value)
    {
        string safe = (value ?? string.Empty)
            .Replace("\r\n", " ")
            .Replace('\r', ' ')
            .Replace('\n', ' ');
        return '"' + safe.Replace("\"", "\"\"") + '"';
    }

    private static void DrawCentered(Graphics graphics, string text, Font font, Color color, Rectangle rect)
    {
        using var brush = new SolidBrush(color);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisWord,
            FormatFlags = StringFormatFlags.LineLimit
        };
        Rectangle inner = Rectangle.Inflate(rect, -8, -5);
        graphics.DrawString(text, font, brush, inner, format);
    }

    private static Color ParseColor(string? value, Color fallback)
    {
        try
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : ColorTranslator.FromHtml(value);
        }
        catch
        {
            return fallback;
        }
    }

    private static Color Blend(Color foreground, Color background, float backgroundWeight)
    {
        float weight = Math.Clamp(backgroundWeight, 0f, 1f);
        return Color.FromArgb(
            (int)(foreground.R * (1 - weight) + background.R * weight),
            (int)(foreground.G * (1 - weight) + background.G * weight),
            (int)(foreground.B * (1 - weight) + background.B * weight));
    }

    private sealed class CsvImportResult
    {
        public List<TimetableEntry> ValidEntries { get; } = new();
        public List<string> Errors { get; } = new();
    }

    private sealed record SemesterOption(string Code, string Display)
    {
        public override string ToString() => Display;
    }
}
