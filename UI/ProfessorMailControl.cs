using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class ProfessorMailControl : UserControl
{
    private readonly DataService _service;
    private readonly IReadOnlyList<ProfessorContact> _professors;

    private readonly TextBox _searchBox = new();
    private readonly ComboBox _categoryCombo = new();
    private readonly DataGridView _grid = new();
    private readonly Label _resultLabel = new();

    private readonly Label _nameLabel = new();
    private readonly Label _roleLabel = new();
    private readonly Label _contactLabel = new();
    private readonly Label _emailLabel = new();
    private readonly Label _degreeLabel = new();
    private readonly Label _researchLabel = new();

    private readonly ComboBox _templateCombo = new();
    private readonly TextBox _subjectBox = new();
    private readonly TextBox _bodyBox = new();

    private ProfessorContact _selectedProfessor;

    public ProfessorMailControl(DataService service)
    {
        _service = service;
        _professors = ProfessorDirectoryService.GetAll();

        Dock = DockStyle.Fill;
        BackColor = UiTheme.Background;
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(820, 620);

        BuildInterface();
        WireEvents();
        RefreshDirectory();
    }

    public void RefreshDirectory()
    {
        string query = _searchBox.Text.Trim();
        string category = _categoryCombo.SelectedItem?.ToString() ?? "全部領域";

        List<ProfessorContact> filtered = _professors
            .Where(professor =>
                string.IsNullOrWhiteSpace(query)
                || professor.ChineseName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || professor.EnglishName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || professor.Email.Contains(query, StringComparison.OrdinalIgnoreCase)
                || professor.ResearchAreas.Contains(query, StringComparison.OrdinalIgnoreCase)
                || professor.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Where(professor =>
                category == "全部領域"
                || ProfessorDirectoryService.GetCategory(professor) == category)
            .OrderByDescending(professor => professor.IsFeatured)
            .ThenBy(professor => professor.ChineseName)
            .ToList();

        _grid.DataSource = filtered
            .Select(professor => new ProfessorGridRow(professor))
            .ToList();

        _resultLabel.Text = $"共 {filtered.Count} 位教授";
        if (_grid.Rows.Count > 0)
        {
            _grid.Rows[0].Selected = true;
            _grid.CurrentCell = _grid.Rows[0].Cells["ChineseName"];
            ShowProfessor(filtered[0], applyTemplate: _selectedProfessor == null);
        }
        else
        {
            ShowProfessor(null, applyTemplate: false);
        }
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Background,
            Padding = new Padding(24, 18, 24, 22),
            ColumnCount = 1,
            RowCount = 3
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildToolbar(), 0, 1);
        root.Controls.Add(BuildMainArea(), 0, 2);

        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(20, 12, 18, 12),
            BorderStyle = BorderStyle.FixedSingle
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            ColumnCount = 2,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label
        {
            Text = "寄信詢問教授",
            Dock = DockStyle.Fill,
            AutoSize = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(23, FontStyle.Bold)
        }, 0, 0);

        layout.Controls.Add(new Label
        {
            Text = "搜尋元智資工教授，套用正式信件範本，並直接開啟 Gmail 撰寫頁面。",
            Dock = DockStyle.Fill,
            AutoSize = false,
            AutoEllipsis = false,
            Margin = Padding.Empty,
            Padding = new Padding(2, 0, 0, 0),
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9.5f)
        }, 0, 2);

        var featured = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.PrimarySoft,
            Margin = new Padding(14, 0, 0, 0),
            Padding = new Padding(14, 8, 14, 8)
        };
        var featuredLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = featured.BackColor,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        featuredLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 44));
        featuredLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 56));
        featuredLayout.Controls.Add(new Label
        {
            Text = "★ 本課程教師置頂",
            Dock = DockStyle.Fill,
            AutoSize = false,
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.BottomLeft,
            ForeColor = UiTheme.Primary,
            Font = UiTheme.Font(9, FontStyle.Bold)
        }, 0, 0);
        featuredLayout.Controls.Add(new Label
        {
            Text = "陳琨 講師｜視窗程式設計（二）",
            Dock = DockStyle.Fill,
            AutoSize = false,
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(10, FontStyle.Bold)
        }, 0, 1);
        featured.Controls.Add(featuredLayout);

        layout.SetRowSpan(featured, 3);
        layout.Controls.Add(featured, 1, 0);

        card.Controls.Add(layout);
        return card;
    }

    private Control BuildToolbar()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(12, 10, 12, 10),
            BorderStyle = BorderStyle.FixedSingle
        };

        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            ColumnCount = 7,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

        _searchBox.Dock = DockStyle.Fill;
        _searchBox.BorderStyle = BorderStyle.FixedSingle;
        _searchBox.Font = UiTheme.Font(10);
        _searchBox.PlaceholderText = "搜尋姓名、信箱或研究領域";
        _searchBox.Margin = new Padding(0, 2, 12, 2);

        _categoryCombo.Dock = DockStyle.Fill;
        _categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _categoryCombo.Font = UiTheme.Font(10);
        _categoryCombo.Margin = new Padding(0, 2, 12, 2);
        _categoryCombo.Items.AddRange(ProfessorDirectoryService.GetCategories().Cast<object>().ToArray());
        _categoryCombo.SelectedIndex = 0;

        Button clearButton = UiTheme.SecondaryButton("清除篩選");
        clearButton.AutoSize = false;
        clearButton.Dock = DockStyle.Fill;
        clearButton.Margin = new Padding(0, 2, 12, 2);
        clearButton.TextAlign = ContentAlignment.MiddleCenter;
        clearButton.Click += (_, _) =>
        {
            _searchBox.Clear();
            _categoryCombo.SelectedIndex = 0;
        };

        _resultLabel.Dock = DockStyle.Fill;
        _resultLabel.AutoSize = false;
        _resultLabel.TextAlign = ContentAlignment.MiddleRight;
        _resultLabel.ForeColor = UiTheme.Muted;
        _resultLabel.Font = UiTheme.Font(9.5f, FontStyle.Bold);

        row.Controls.Add(InlineLabel("搜尋"), 0, 0);
        row.Controls.Add(_searchBox, 1, 0);
        row.Controls.Add(InlineLabel("領域"), 2, 0);
        row.Controls.Add(_categoryCombo, 3, 0);
        row.Controls.Add(clearButton, 4, 0);
        row.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface }, 5, 0);
        row.Controls.Add(_resultLabel, 6, 0);

        card.Controls.Add(row);
        return card;
    }

    private Control BuildMainArea()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Background,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(BuildDirectoryCard(), 0, 0);
        layout.Controls.Add(BuildComposeCard(), 1, 0);
        return layout;
    }

    private Control BuildDirectoryCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0, 0, 8, 0),
            Padding = new Padding(1),
            BorderStyle = BorderStyle.FixedSingle
        };

        UiTheme.StyleGrid(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.RowTemplate.Height = 44;
        _grid.ColumnHeadersHeight = 44;
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Clear();

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Featured",
            DataPropertyName = "Featured",
            HeaderText = "",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            Width = 38,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                ForeColor = UiTheme.Warning,
                Font = UiTheme.Font(11, FontStyle.Bold)
            }
        });
        _grid.Columns.Add(UiTheme.TextColumn("ChineseName", "教授姓名", 110));
        _grid.Columns.Add(UiTheme.TextColumn("Title", "職稱", 115));
        _grid.Columns.Add(new DataGridViewLinkColumn
        {
            Name = "Email",
            DataPropertyName = "Email",
            HeaderText = "電子郵件",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 165,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.Automatic,
            LinkColor = UiTheme.Primary,
            ActiveLinkColor = UiTheme.PrimaryDark,
            VisitedLinkColor = UiTheme.Primary,
            TrackVisitedState = false
        });
        _grid.Columns.Add(UiTheme.TextColumn(
            "Office",
            "辦公室",
            70,
            DataGridViewAutoSizeColumnMode.Fill,
            DataGridViewContentAlignment.MiddleCenter));

        card.Controls.Add(_grid);
        return card;
    }

    private Control BuildComposeCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Margin = new Padding(8, 0, 0, 0),
            Padding = new Padding(16, 14, 16, 14),
            BorderStyle = BorderStyle.FixedSingle
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 184));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));

        layout.Controls.Add(BuildProfessorDetails(), 0, 0);
        layout.Controls.Add(new Label
        {
            Text = "信件草稿",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(11, FontStyle.Bold)
        }, 0, 1);
        layout.Controls.Add(BuildComposer(), 0, 2);
        layout.Controls.Add(BuildComposeButtons(), 0, 3);

        card.Controls.Add(layout);
        return card;
    }

    private Control BuildProfessorDetails()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            ColumnCount = 1,
            RowCount = 6,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _nameLabel.Dock = DockStyle.Fill;
        _nameLabel.AutoSize = false;
        _nameLabel.TextAlign = ContentAlignment.MiddleLeft;
        _nameLabel.ForeColor = UiTheme.Navy;
        _nameLabel.Font = UiTheme.Font(16, FontStyle.Bold);
        _nameLabel.AutoEllipsis = true;

        _roleLabel.Dock = DockStyle.Fill;
        _roleLabel.AutoSize = false;
        _roleLabel.TextAlign = ContentAlignment.MiddleLeft;
        _roleLabel.ForeColor = UiTheme.Primary;
        _roleLabel.Font = UiTheme.Font(9.5f, FontStyle.Bold);
        _roleLabel.AutoEllipsis = true;

        _contactLabel.Dock = DockStyle.Fill;
        _contactLabel.AutoSize = false;
        _contactLabel.TextAlign = ContentAlignment.MiddleLeft;
        _contactLabel.ForeColor = UiTheme.Slate;
        _contactLabel.Font = UiTheme.Font(9);
        _contactLabel.AutoEllipsis = true;

        _emailLabel.Dock = DockStyle.Fill;
        _emailLabel.AutoSize = false;
        _emailLabel.TextAlign = ContentAlignment.MiddleLeft;
        _emailLabel.ForeColor = UiTheme.Primary;
        _emailLabel.Font = UiTheme.Font(9, FontStyle.Bold);
        _emailLabel.Cursor = Cursors.Hand;
        _emailLabel.AutoEllipsis = true;

        _degreeLabel.Dock = DockStyle.Fill;
        _degreeLabel.AutoSize = false;
        _degreeLabel.TextAlign = ContentAlignment.MiddleLeft;
        _degreeLabel.ForeColor = UiTheme.Muted;
        _degreeLabel.Font = UiTheme.Font(8.8f);
        _degreeLabel.AutoEllipsis = true;

        _researchLabel.Dock = DockStyle.Fill;
        _researchLabel.AutoSize = false;
        _researchLabel.TextAlign = ContentAlignment.TopLeft;
        _researchLabel.ForeColor = UiTheme.Slate;
        _researchLabel.Font = UiTheme.Font(9);
        _researchLabel.Padding = new Padding(0, 3, 0, 0);

        layout.Controls.Add(_nameLabel, 0, 0);
        layout.Controls.Add(_roleLabel, 0, 1);
        layout.Controls.Add(_contactLabel, 0, 2);
        layout.Controls.Add(_emailLabel, 0, 3);
        layout.Controls.Add(_degreeLabel, 0, 4);
        layout.Controls.Add(_researchLabel, 0, 5);
        return layout;
    }

    private Control BuildComposer()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            ColumnCount = 1,
            RowCount = 6,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _templateCombo.Dock = DockStyle.Fill;
        _templateCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _templateCombo.Font = UiTheme.Font(9.5f);
        _templateCombo.Margin = new Padding(0, 0, 0, 4);
        _templateCombo.Items.AddRange(new object[]
        {
            "課程問題",
            "作業／成績",
            "預約討論",
            "專題／實驗室",
            "空白信件"
        });
        _templateCombo.SelectedIndex = 0;

        ConfigureInput(_subjectBox, multiline: false);
        _subjectBox.Margin = new Padding(0, 0, 0, 4);

        ConfigureInput(_bodyBox, multiline: true);
        _bodyBox.ScrollBars = ScrollBars.Vertical;
        _bodyBox.AcceptsReturn = true;
        _bodyBox.AcceptsTab = true;

        layout.Controls.Add(FormLabel("信件範本"), 0, 0);
        layout.Controls.Add(_templateCombo, 0, 1);
        layout.Controls.Add(FormLabel("主旨"), 0, 2);
        layout.Controls.Add(_subjectBox, 0, 3);
        layout.Controls.Add(FormLabel("內容"), 0, 4);
        layout.Controls.Add(_bodyBox, 0, 5);
        return layout;
    }

    private Control BuildComposeButtons()
    {
        var host = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        var row = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoScroll = false,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 10)
        };

        Button gmailButton = UiTheme.PrimaryButton("使用 Gmail 撰寫");
        gmailButton.AutoSize = false;
        gmailButton.Size = new Size(166, 40);
        gmailButton.Margin = Padding.Empty;
        gmailButton.Padding = Padding.Empty;
        gmailButton.TextAlign = ContentAlignment.MiddleCenter;
        gmailButton.Click += (_, _) => OpenGmail();

        Button copyButton = UiTheme.SecondaryButton("複製信箱");
        copyButton.AutoSize = false;
        copyButton.Size = new Size(112, 40);
        copyButton.Margin = new Padding(0, 0, 10, 0);
        copyButton.Padding = Padding.Empty;
        copyButton.TextAlign = ContentAlignment.MiddleCenter;
        copyButton.Click += (_, _) => CopySelectedEmail();

        row.Controls.Add(gmailButton);
        row.Controls.Add(copyButton);
        host.Controls.Add(row);
        return host;
    }

    private void WireEvents()
    {
        _searchBox.TextChanged += (_, _) => RefreshDirectory();
        _categoryCombo.SelectedIndexChanged += (_, _) => RefreshDirectory();

        _grid.SelectionChanged += (_, _) =>
        {
            ProfessorGridRow row = _grid.CurrentRow?.DataBoundItem as ProfessorGridRow;
            if (row != null)
                ShowProfessor(row.Professor, applyTemplate: false);
        };

        _grid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (_grid.Columns[e.ColumnIndex].Name == "Email")
            {
                ProfessorGridRow row = _grid.Rows[e.RowIndex].DataBoundItem as ProfessorGridRow;
                if (row != null)
                {
                    ShowProfessor(row.Professor, applyTemplate: false);
                    OpenGmail();
                }
            }
        };

        _grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex < 0)
                return;

            ProfessorGridRow row = _grid.Rows[e.RowIndex].DataBoundItem as ProfessorGridRow;
            if (row != null)
            {
                ShowProfessor(row.Professor, applyTemplate: false);
                OpenGmail();
            }
        };

        _templateCombo.SelectedIndexChanged += (_, _) => ApplyTemplate();
        _emailLabel.Click += (_, _) => OpenGmail();
    }

    private void ShowProfessor(ProfessorContact professor, bool applyTemplate)
    {
        _selectedProfessor = professor;

        if (professor == null)
        {
            _nameLabel.Text = "沒有符合條件的教授";
            _roleLabel.Text = string.Empty;
            _contactLabel.Text = string.Empty;
            _emailLabel.Text = string.Empty;
            _degreeLabel.Text = string.Empty;
            _researchLabel.Text = "請調整搜尋關鍵字或研究領域篩選。";
            return;
        }

        _nameLabel.Text = professor.DisplayName;
        _roleLabel.Text = professor.IsFeatured
            ? $"★ 本課程教師｜{professor.Title}"
            : $"{professor.Title}｜{ProfessorDirectoryService.GetCategory(professor)}";
        _contactLabel.Text = professor.ContactSummary;
        _emailLabel.Text = professor.Email;
        _degreeLabel.Text = professor.Degree;
        _researchLabel.Text = "研究領域：" + professor.ResearchAreas;

        if (applyTemplate || string.IsNullOrWhiteSpace(_bodyBox.Text))
            ApplyTemplate();
    }

    private void ApplyTemplate()
    {
        if (_selectedProfessor == null)
            return;

        string userName = _service.Data.Settings.UserName?.Trim();
        if (string.IsNullOrWhiteSpace(userName))
            userName = "＿＿＿＿";

        string template = _templateCombo.SelectedItem?.ToString() ?? "課程問題";
        string greeting = $"{_selectedProfessor.ChineseName}老師您好：";
        string closing =
            $"\r\n\r\n謝謝老師撥空閱讀。\r\n\r\n敬祝　順心\r\n{userName}";

        switch (template)
        {
            case "作業／成績":
                _subjectBox.Text = "關於作業或成績問題的請教";
                _bodyBox.Text =
                    greeting
                    + $"\r\n\r\n我是元智大學資訊工程學系的{userName}，想向老師請教以下作業或成績問題："
                    + "\r\n\r\n（請在此說明課程、作業名稱與問題）"
                    + closing;
                break;

            case "預約討論":
                _subjectBox.Text = "預約討論時間";
                _bodyBox.Text =
                    greeting
                    + $"\r\n\r\n我是元智大學資訊工程學系的{userName}，想向老師預約時間討論以下事項："
                    + "\r\n\r\n討論主題：\r\n可配合時段："
                    + closing;
                break;

            case "專題／實驗室":
                _subjectBox.Text = "詢問專題或實驗室研究機會";
                _bodyBox.Text =
                    greeting
                    + $"\r\n\r\n我是元智大學資訊工程學系的{userName}，對老師的研究領域「{_selectedProfessor.ResearchAreas}」很有興趣，想請教專題或實驗室參與機會。"
                    + "\r\n\r\n（請在此簡短介紹背景、修課經驗與想研究的方向）"
                    + closing;
                break;

            case "空白信件":
                _subjectBox.Clear();
                _bodyBox.Text = greeting + "\r\n\r\n" + closing.TrimStart();
                break;

            default:
                _subjectBox.Text = _selectedProfessor.IsFeatured
                    ? "關於視窗程式設計（二）的問題請教"
                    : "關於課程問題的請教";
                _bodyBox.Text =
                    greeting
                    + $"\r\n\r\n我是元智大學資訊工程學系的{userName}，想向老師請教以下問題："
                    + "\r\n\r\n（請在此輸入問題內容）"
                    + closing;
                break;
        }
    }

    private void OpenGmail()
    {
        if (_selectedProfessor == null)
        {
            MessageBox.Show(
                this,
                "請先選擇一位教授。",
                "尚未選擇收件人",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        try
        {
            string url = ProfessorDirectoryService.BuildGmailComposeUrl(
                _selectedProfessor.Email,
                _subjectBox.Text.Trim(),
                _bodyBox.Text);

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            try
            {
                string mailTo = ProfessorDirectoryService.BuildMailToUrl(
                    _selectedProfessor.Email,
                    _subjectBox.Text.Trim(),
                    _bodyBox.Text);

                Process.Start(new ProcessStartInfo
                {
                    FileName = mailTo,
                    UseShellExecute = true
                });
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "無法開啟 Gmail 或預設郵件程式。\r\n\r\n"
                    + "請確認電腦有可用的瀏覽器，或直接複製教授信箱。\r\n\r\n"
                    + exception.Message,
                    "無法開啟寄信頁面",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }

    private void CopySelectedEmail()
    {
        if (_selectedProfessor == null)
            return;

        try
        {
            Clipboard.SetText(_selectedProfessor.Email);
            MessageBox.Show(
                this,
                $"已複製：{_selectedProfessor.Email}",
                "信箱已複製",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show(
                this,
                "目前無法存取剪貼簿，請稍後再試。",
                "複製失敗",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private static Label InlineLabel(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        AutoSize = false,
        TextAlign = ContentAlignment.MiddleLeft,
        ForeColor = UiTheme.Slate,
        Font = UiTheme.Font(9, FontStyle.Bold)
    };

    private static Label FormLabel(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        AutoSize = false,
        TextAlign = ContentAlignment.MiddleLeft,
        ForeColor = UiTheme.Slate,
        Font = UiTheme.Font(8.8f, FontStyle.Bold)
    };

    private static void ConfigureInput(TextBox textBox, bool multiline)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.Multiline = multiline;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = UiTheme.Font(9.5f);
        textBox.BackColor = UiTheme.Surface;
        textBox.ForeColor = UiTheme.Slate;
    }

    private sealed class ProfessorGridRow
    {
        public ProfessorGridRow(ProfessorContact professor)
        {
            Professor = professor;
        }

        [Browsable(false)]
        public ProfessorContact Professor { get; }

        public string Featured => Professor.IsFeatured ? "★" : string.Empty;
        public string ChineseName => Professor.ChineseName;
        public string Title => Professor.Title;
        public string Email => Professor.Email;
        public string Office => string.IsNullOrWhiteSpace(Professor.Office) ? "—" : Professor.Office;
    }
}
