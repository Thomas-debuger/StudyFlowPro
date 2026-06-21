using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class HelpForm : Form
{
    private readonly DataService _service;
    private readonly Dictionary<string, Button> _topicButtons = new();
    private readonly Label _topicTitle = new();
    private readonly Label _topicSubtitle = new();
    private readonly RichTextBox _content = new();

    public HelpForm(DataService service)
    {
        _service = service;

        Text = "使用說明與快捷鍵";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1000, 780);
        MinimumSize = new Size(900, 700);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        ShowTopic("dashboard");
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 164));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildBody(), 0, 1);
        root.Controls.Add(BuildFooter(), 0, 2);

        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(20, 14, 20, 12),
            Margin = new Padding(0, 0, 0, 12)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

        layout.Controls.Add(new Label
        {
            Text = "StudyFlow Pro 使用指南",
            Dock = DockStyle.Fill,
            AutoSize = false,
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(21, FontStyle.Bold),
            UseCompatibleTextRendering = true
        }, 0, 0);

        layout.Controls.Add(new Label
        {
            Text = "用最短的步驟完成任務管理、四學期課表、考古題閱讀、研究分析與資料備份",
            Dock = DockStyle.Fill,
            AutoSize = false,
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9.8f),
            UseCompatibleTextRendering = true
        }, 0, 1);

        var badges = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(0, 6, 0, 0)
        };
        badges.Controls.Add(CreateBadge("離線使用", UiTheme.Success));
        badges.Controls.Add(CreateBadge("可解釋排序", UiTheme.Primary));
        badges.Controls.Add(CreateBadge("自動備份", UiTheme.Purple));
        badges.Controls.Add(CreateBadge("可攜題庫", UiTheme.Warning));
        layout.Controls.Add(badges, 0, 2);

        card.Controls.Add(layout);
        return card;
    }

    private Control BuildBody()
    {
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 214));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        body.Controls.Add(BuildTopicMenu(), 0, 0);
        body.Controls.Add(BuildTopicContent(), 1, 0);
        return body;
    }

    private Control BuildTopicMenu()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 12, 0)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label
        {
            Text = "說明目錄",
            Dock = DockStyle.Fill,
            AutoSize = false,
            Margin = Padding.Empty,
            Padding = new Padding(8, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(12, FontStyle.Bold)
        }, 0, 0);

        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Margin = Padding.Empty,
            Padding = new Padding(0, 4, 0, 0)
        };

        // 與主畫面左側功能列保持完全相同的順序，方便新手對照操作。
        AddTopicButton(menu, "dashboard", "01  主控台");
        AddTopicButton(menu, "courses", "02  課程 / 專案");
        AddTopicButton(menu, "tasks", "03  任務管理");
        AddTopicButton(menu, "focus", "04  專注計時");
        AddTopicButton(menu, "timetable", "05  課表");
        AddTopicButton(menu, "exams", "06  考古題庫");
        AddTopicButton(menu, "mail", "07  寄信詢問教授");
        AddTopicButton(menu, "styles", "08  視覺風格");
        AddTopicButton(menu, "schedule", "09  智慧排程");
        AddTopicButton(menu, "analytics", "10  分析中心");
        AddTopicButton(menu, "research", "11  Research Center");
        AddTopicButton(menu, "settings", "12  設定與備份");
        AddTopicButton(menu, "help", "13  使用說明");
        AddTopicButton(menu, "logout", "14  登出帳號");

        layout.Controls.Add(menu, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private Control BuildTopicContent()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(0),
            Margin = Padding.Empty
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 98));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var topicHeader = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(22, 10, 20, 8),
            Margin = Padding.Empty,
            ColumnCount = 1,
            RowCount = 2
        };
        topicHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        topicHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _topicTitle.Dock = DockStyle.Fill;
        _topicTitle.AutoSize = false;
        _topicTitle.AutoEllipsis = true;
        _topicTitle.Margin = Padding.Empty;
        _topicTitle.ForeColor = UiTheme.Navy;
        _topicTitle.Font = UiTheme.Font(17, FontStyle.Bold);
        _topicTitle.TextAlign = ContentAlignment.MiddleLeft;
        _topicTitle.UseCompatibleTextRendering = true;

        _topicSubtitle.Dock = DockStyle.Fill;
        _topicSubtitle.AutoSize = false;
        _topicSubtitle.AutoEllipsis = true;
        _topicSubtitle.Margin = Padding.Empty;
        _topicSubtitle.ForeColor = UiTheme.Muted;
        _topicSubtitle.Font = UiTheme.Font(9.5f);
        _topicSubtitle.TextAlign = ContentAlignment.TopLeft;
        _topicSubtitle.UseCompatibleTextRendering = true;

        topicHeader.Controls.Add(_topicTitle, 0, 0);
        topicHeader.Controls.Add(_topicSubtitle, 0, 1);

        var contentHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(20, 10, 16, 16)
        };

        _content.Dock = DockStyle.Fill;
        _content.ReadOnly = true;
        _content.BorderStyle = BorderStyle.None;
        _content.BackColor = UiTheme.Surface;
        _content.ForeColor = UiTheme.Slate;
        _content.Font = UiTheme.Font(10.2f);
        _content.DetectUrls = false;
        _content.HideSelection = false;
        _content.ScrollBars = RichTextBoxScrollBars.Vertical;
        _content.TabStop = false;
        contentHost.Controls.Add(_content);

        layout.Controls.Add(topicHeader, 0, 0);
        layout.Controls.Add(contentHost, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private Control BuildFooter()
    {
        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 0)
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        footer.Controls.Add(new Label
        {
            Text = "各帳號資料都分開儲存在本機，核心功能不需要網路連線。",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(9)
        }, 0, 0);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        Button closeButton = UiTheme.PrimaryButton("完成");
        Button folderButton = UiTheme.SecondaryButton("開啟資料位置");
        Button researchButton = UiTheme.SecondaryButton("開啟 Research Center");

        closeButton.Click += (_, _) => Close();
        folderButton.Click += (_, _) => OpenDataLocation();
        researchButton.Click += (_, _) =>
        {
            using var form = new ResearchCenterForm(_service);
            form.ShowDialog(this);
        };

        buttons.Controls.Add(closeButton);
        buttons.Controls.Add(folderButton);
        buttons.Controls.Add(researchButton);
        footer.Controls.Add(buttons, 1, 0);
        return footer;
    }

    private void AddTopicButton(FlowLayoutPanel menu, string key, string text)
    {
        bool isResearchCenter = key == "research";
        var button = new Button
        {
            Text = text,
            Width = 174,
            Height = 44,
            Margin = new Padding(0, 0, 0, 6),
            // Research Center 的英文名稱較長，縮小該項左右內距與字級，避免只顯示 Research。
            Padding = isResearchCenter ? new Padding(6, 0, 0, 0) : new Padding(12, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = UiTheme.Font(isResearchCenter ? 8.8f : 9.5f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = UiTheme.Surface,
            ForeColor = UiTheme.Slate,
            AutoEllipsis = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = UiTheme.PrimarySoft;
        button.Click += (_, _) => ShowTopic(key);

        _topicButtons[key] = button;
        menu.Controls.Add(button);
    }

    private void ShowTopic(string key)
    {
        foreach ((string topicKey, Button button) in _topicButtons)
        {
            bool selected = topicKey == key;
            button.BackColor = selected ? UiTheme.Primary : UiTheme.Surface;
            button.ForeColor = selected ? Color.White : UiTheme.Slate;
        }

        _content.Clear();
        _content.SelectionIndent = 0;
        _content.SelectionRightIndent = 0;
        _content.SelectionHangingIndent = 0;
        _content.SelectionBackColor = UiTheme.Surface;

        switch (key)
        {
            case "courses":
                _topicTitle.Text = "課程 / 專案";
                _topicSubtitle.Text = "先建立學習分類，再把任務、進度與分析結果整理到正確課程。";
                RenderCoursesTopic();
                break;
            case "tasks":
                _topicTitle.Text = "任務管理";
                _topicSubtitle.Text = "新增具體工作、設定期限與進度，並交給智慧排序協助安排。";
                RenderTasksTopic();
                break;
            case "focus":
                _topicTitle.Text = "專注計時";
                _topicSubtitle.Text = "把專注時間、品質與反思轉成可分析的學習紀錄。";
                RenderFocusTopic();
                break;
            case "timetable":
                _topicTitle.Text = "課表";
                _topicSubtitle.Text = "管理學期、上課節次、教室與顏色，並匯入或匯出課表。";
                RenderTimetableTopic();
                break;
            case "exams":
                _topicTitle.Text = "考古題庫";
                _topicSubtitle.Text = "按科目匯入 PDF／DOCX、閱讀文件並記錄複習進度。";
                RenderExamTopic();
                break;
            case "mail":
                _topicTitle.Text = "寄信詢問教授";
                _topicSubtitle.Text = "搜尋教授、套用正式信件範本，再帶入 Gmail 撰寫頁面。";
                RenderProfessorMailTopic();
                break;
            case "styles":
                _topicTitle.Text = "視覺風格";
                _topicSubtitle.Text = "切換六套全系統配色，而且每個帳號會記住自己的選擇。";
                RenderVisualStyleTopic();
                break;
            case "schedule":
                _topicTitle.Text = "智慧排程";
                _topicSubtitle.Text = "依期限、優先級、進度與剩餘工作量，產生今天的建議順序。";
                RenderSmartScheduleTopic();
                break;
            case "analytics":
                _topicTitle.Text = "分析中心";
                _topicSubtitle.Text = "用圖表查看專注時間、完成率、品質與估時準確度。";
                RenderAnalyticsTopic();
                break;
            case "research":
                _topicTitle.Text = "Research Center";
                _topicSubtitle.Text = "查看研究型指標、稽核軌跡、週報與可解釋分析。";
                RenderResearchCenterTopic();
                break;
            case "settings":
                _topicTitle.Text = "設定與備份";
                _topicSubtitle.Text = "修改個人設定、匯入匯出資料並進行資料健檢。";
                RenderSettingsTopic();
                break;
            case "help":
                _topicTitle.Text = "使用說明";
                _topicSubtitle.Text = "依照主畫面相同順序，逐項查看每個功能的操作方法。";
                RenderHelpTopic();
                break;
            case "logout":
                _topicTitle.Text = "登出帳號";
                _topicSubtitle.Text = "儲存目前帳號資料並返回登入畫面，避免不同使用者資料混用。";
                RenderLogoutTopic();
                break;
            default:
                _topicTitle.Text = "主控台";
                _topicSubtitle.Text = "登入後的首頁，用來快速掌握今天最重要的學習資訊。";
                RenderDashboardTopic();
                break;
        }

        _content.SelectionStart = 0;
        _content.ScrollToCaret();
    }

    private void RenderDashboardTopic()
    {
        AppendSection("登入後先看這裡");
        AppendBullet("四張摘要卡片：未完成任務、逾期任務、任務完成率與今日專注分鐘。");
        AppendBullet("SMART PRIORITY 智慧建議：指出目前最值得優先處理的任務與原因。");
        AppendBullet("今日作戰計畫：列出建議先後順序；按「產生完整排程」可開啟智慧排程。");
        AppendBullet("下一步任務：顯示任務名稱、課程、期限、進度與智慧分數。");

        AppendSection("建議操作");
        AppendStep(1, "先看智慧建議", "確認系統建議的首要任務是否符合今天的目標。");
        AppendStep(2, "選擇下一步任務", "需要調整時，前往任務管理修改期限、優先級或進度。");
        AppendStep(3, "開始執行", "到專注計時綁定任務，完成後再回主控台查看數字更新。");
    }

    private void RenderCoursesTopic()
    {
        AppendSection("課程／專案是任務的分類");
        AppendParagraph("例如先建立『演算法』『視窗程式設計』『專題研究』，再把每一項具體任務歸入正確分類。");
        AppendStep(1, "新增課程", "按「＋ 新增課程」，輸入名稱、老師／負責人與教室／地點。");
        AppendStep(2, "選擇識別顏色", "點擊顏色按鈕選色；儲存後會立即顯示在課程列表與相關任務的課程欄位。");
        AppendStep(3, "編輯", "選取課程後按「編輯」，可更改名稱、老師、地點或識別顏色。");
        AppendStep(4, "查看進度", "列表會統計該課程的未完成任務數與完成率。");
        AppendTip("刪除提醒", "刪除課程不會刪除原本任務；這些任務會改成『未分類』。");
    }

    private void RenderSmartScheduleTopic()
    {
        AppendSection("產生今天的建議順序");
        AppendStep(1, "確認任務資料", "先在任務管理設定期限、優先級、預估分鐘、難度與目前進度。");
        AppendStep(2, "開啟智慧排程", "系統會依智慧分數排列尚未完成的任務。");
        AppendStep(3, "閱讀原因", "查看每項任務的建議等級、剩餘工作量與排序理由。");
        AppendStep(4, "開始執行", "依建議順序前往專注計時，或回任務管理調整不合理的條件。");

        AppendSection("排序會考慮");
        AppendBullet("截止時間、是否逾期、優先級與是否釘選。");
        AppendBullet("剩餘工作量、目前進度、難度與精力需求。");
        AppendBullet("長時間沒有更新的任務，以及完成後能帶來的進度提升。");
        AppendTip("不是強制命令", "智慧排程是建議工具；臨時有課堂、身體狀況或老師交辦事項時，仍可自行調整順序。");
    }

    private void RenderAnalyticsTopic()
    {
        AppendSection("看懂自己的學習狀況");
        AppendBullet("生產力指數：綜合任務完成與專注紀錄的摘要指標。");
        AppendBullet("本週專注與專注品質：了解投入時間及自評狀態。");
        AppendBullet("估時準確度：比較任務預估時間與實際專注時間。");
        AppendBullet("近七天圖表：觀察每天的專注分鐘與品質變化。");
        AppendBullet("課程完成率與優先級分布：看出工作集中在哪一門課或哪種急迫程度。");
        AppendTip("資料來源", "分析中心使用任務與專注計時的紀錄；資料越完整，圖表越有參考價值。");
    }

    private void RenderResearchCenterTopic()
    {
        AppendSection("研究型與可解釋指標");
        AppendBullet("學習一致性：本週有留下專注紀錄的天數比例。");
        AppendBullet("專注品質：將本週自評品質換算成百分比。");
        AppendBullet("估時準確度：比較已完成任務的預估時間與實際投入時間。");
        AppendBullet("稽核軌跡：保留新增、修改、完成、專注、匯入匯出與其他資料變更紀錄。");

        AppendSection("可用工具");
        AppendBullet("輸出 HTML 學習週報與 iCalendar 行事曆。");
        AppendBullet("開啟智慧四象限，依重要性與急迫性整理任務。");
        AppendBullet("執行資料健檢，確認 ID、參照關係、時間與數值是否合理。");
        AppendTip("報告重點", "這裡的指標都有清楚定義，可說明結果如何產生，不是無法解釋的黑箱分數。");
    }

    private void RenderSettingsTopic()
    {
        AppendSection("個人設定");
        AppendStep(1, "修改顯示名稱", "儲存後，左上角帳號名稱與主控台問候語會同步更新。");
        AppendStep(2, "設定專注習慣", "可調整預設專注分鐘、每日目標與即將到期提醒範圍。");
        AppendStep(3, "確認資料位置", "畫面會顯示目前帳號的正式資料與考古題原檔位置。");

        AppendSection("資料管理");
        AppendBullet("可匯出目前帳號的資料檔，或匯入先前匯出的資料檔。");
        AppendBullet("匯入、清除或重建 DEMO，只會影響目前登入的帳號。");
        AppendBullet("系統仍保留 last-good 與滾動快照，降低突然中斷造成的資料損壞風險。");
        AppendPath("目前帳號正式資料", _service.DataPath);
        AppendTip("建議", "正式展示或大量修改前，可先匯出一份資料檔保存。");
    }

    private void RenderHelpTopic()
    {
        AppendSection("如何使用這個說明視窗");
        AppendBullet("左側 14 個項目的順序與主畫面左側功能列完全一致。");
        AppendBullet("點選功能名稱後，右側會顯示用途、操作步驟與注意事項。");
        AppendBullet("不確定下一步時，可先閱讀『主控台』『課程／專案』『任務管理』『專注計時』。");

        AppendSection("常用快捷鍵");
        AppendShortcut("Ctrl + N", "新增任務");
        AppendShortcut("Ctrl + T", "開啟課表");
        AppendShortcut("Ctrl + L", "開啟考古題庫");
        AppendShortcut("Ctrl + G", "開啟寄信詢問教授");
        AppendShortcut("Ctrl + P", "開啟智慧排程");
        AppendShortcut("Ctrl + S", "立即儲存");
        AppendShortcut("F5", "重新整理目前畫面");
    }

    private void RenderLogoutTopic()
    {
        AppendSection("結束使用時正確登出");
        AppendStep(1, "確認自動儲存", "畫面最下方顯示『資料已同步並自動儲存』後即可登出。");
        AppendStep(2, "按登出帳號", "點選主畫面左下角『登出帳號』，系統會再儲存一次目前資料。");
        AppendStep(3, "返回登入畫面", "下一位使用者應使用自己的帳號登入，不要共用同一帳號。");

        AppendSection("登出後不會消失的內容");
        AppendBullet("目前帳號的課程、任務、課表、考古題與專注紀錄。");
        AppendBullet("目前帳號選擇的 Facebook、Netflix、Spotify 等視覺風格。");
        AppendBullet("目前帳號的個人設定、備份與分析資料。");
        AppendTip("多帳號隔離", "A 帳號登出後改由 B 登入，兩人的資料與視覺風格不會互相覆蓋。");
    }


    private void RenderTasksTopic()
    {
        AppendSection("建立一項真正要完成的工作");
        AppendStep(1, "新增任務", "按「＋ 新增任務」，輸入名稱並選擇所屬課程／專案。");
        AppendStep(2, "設定條件", "填寫優先級、截止時間、預估分鐘、難度、精力需求與進度。");
        AppendStep(3, "搜尋與篩選", "可依名稱、說明、標籤、逾期、今日到期、高優先與釘選狀態篩選。");
        AppendStep(4, "匯入或匯出", "按『匯入任務 CSV』可載入先前匯出的任務表；按『匯出任務 CSV』可交由 Excel 整理。");

        AppendSection("任務會影響哪些地方");
        AppendBullet("主控台會顯示未完成、逾期、完成率與下一步任務。");
        AppendBullet("智慧排程會依任務條件計算建議順序與智慧分數。");
        AppendBullet("專注計時可綁定任務，完成的分鐘數會回寫任務進度。");
        AppendTip("課程與任務的差別", "課程／專案是分類，例如『演算法』；任務是具體工作，例如『複習最小生成樹』。");
    }

    private void RenderFocusTopic()
    {
        AppendSection("開始一段專注");
        AppendStep(1, "選擇任務", "可綁定現有任務，也能使用自由專注。");
        AppendStep(2, "設定時間", "預設為 25 分鐘，也可依需要調整。");
        AppendStep(3, "開始與暫停", "計時期間可暫停、繼續或重設。");
        AppendStep(4, "完成並回顧", "完成後填寫專注品質、分心次數、成果與反思。");

        AppendSection("資料如何被使用");
        AppendBullet("專注分鐘會回寫到綁定任務的進度。");
        AppendBullet("品質與分心資料會進入分析中心與週報指標。");
        AppendBullet("反思內容會保留在專注紀錄中，方便日後回顧。");
        AppendBullet("『匯入紀錄 CSV』可載入既有紀錄；『匯出紀錄 CSV』可備份或交由 Excel 分析。");
        AppendTip("DEMO 技巧", "錄影時可設定 1 分鐘，執行 10 秒後按「完成並記錄」，即可快速展示完整流程。");
    }

    private void RenderTimetableTopic()
    {
        AppendSection("查看與編輯課表");
        AppendStep(1, "切換學期", "可選擇內建四學期，也能自行新增往後學期的空白課表。");
        AppendStep(2, "新增課程", "先點選星期與節次，再按「新增課程」；系統會自動帶入選取時段。");
        AppendStep(3, "編輯或刪除", "點選課程格查看資訊，雙擊可編輯；同一門課可使用一致或不同顏色。");
        AppendStep(4, "避免衝突", "儲存前會檢查同一星期、同一節次是否已經有其他課程。");

        AppendSection("實用工具");
        AppendBullet("可自行新增或刪除學期課表，並將既有 CSV 匯入目前學期。");
        AppendBullet("按『匯出課表 CSV』可供 Excel 整理，也可匯出高解析度 PNG 課表圖片。");
        AppendBullet("下次啟動會從本機 JSON 自動讀取課表，不需要重新輸入。");
        AppendTip("快捷鍵", "按 Ctrl + T 可直接開啟課表頁面；橘色外框會標示目前星期與節次。");
    }

    private void RenderExamTopic()
    {
        AppendSection("建立科目與匯入文件");
        AppendStep(1, "新增科目", "進入「考古題庫」，先建立演算法、計算機組織等科目分類。");
        AppendStep(2, "匯入考古題", "選取科目後按「匯入 PDF／DOCX」，也可以直接把檔案拖曳到列表。");
        AppendStep(3, "補充資訊", "可編輯年份、學期、類型、標籤、筆記、收藏與複習狀態。");
        AppendStep(4, "閱讀與匯出", "雙擊文件即可在新視窗預覽；可另存原檔或使用系統預設程式開啟。");

        AppendSection("自動保存與跨電腦移轉");
        AppendBullet("匯入後，系統會把原始 PDF／DOCX 複製到本機題庫資料夾，並把索引寫入 JSON；下次啟動會自動載入，不必重新新增。");
        AppendBullet("「匯出題庫包」會建立 .sfexam，內含科目、考古題資訊與所有原始檔；另一台電腦可直接匯入。");
        AppendBullet("交作業時，可將題庫包命名為 ExamLibrary.sfexam 並放入專案 DemoAssets 資料夾，老師第一次執行時會自動載入展示題庫。");
        AppendTip("閱讀策略", "使用「智慧抽一份」會優先挑選尚未完成、開啟次數較少或最久未看的考古題。快捷鍵 Ctrl+L 可直接開啟題庫。");
    }

    private void RenderProfessorMailTopic()
    {
        AppendSection("搜尋與選擇教授");
        AppendStep(1, "開啟頁面", "從左側選單進入「寄信詢問教授」；本課程的陳琨講師會固定置頂。");
        AppendStep(2, "搜尋", "可依中文姓名、英文姓名、信箱、職稱或研究領域搜尋。");
        AppendStep(3, "領域篩選", "可快速查看人工智慧、生物資訊、網路、資安、硬體等研究方向的教師。");

        AppendSection("撰寫郵件");
        AppendStep(1, "選擇範本", "提供課程問題、作業／成績、預約討論、專題／實驗室與空白信件。");
        AppendStep(2, "修改內容", "在系統中先修改主旨與內文，確認語氣及資訊完整。");
        AppendStep(3, "開啟 Gmail", "按「使用 Gmail 撰寫」，系統會帶入收件人、主旨與內容，但不會自動寄出。");
        AppendBullet("程式不保存 Google 密碼，也不會讀取使用者郵件。");
        AppendTip("快捷鍵", "按 Ctrl + G 可直接開啟寄信詢問教授頁面。");
    }

    private void RenderVisualStyleTopic()
    {
        AppendSection("六套顏色風格");
        AppendBullet("Facebook 顏色風格：預設的明亮藍色介面，適合一般使用與課堂展示。");
        AppendBullet("Spotify 顏色風格：黑色與深灰背景搭配綠色重點，適合夜間與沉浸式使用。");
        AppendBullet("YouTube 顏色風格：白色、炭黑與紅色重點，操作按鈕辨識度高。");
        AppendBullet("Netflix 顏色風格：黑色與炭黑背景搭配 Netflix 紅色重點，呈現沉浸式劇院質感。");
        AppendBullet("Visual Studio 顏色風格：黑灰色介面搭配紫色操作重點，呈現專業開發工具質感。");
        AppendBullet("Visual Studio Code 顏色風格：近黑工作區搭配深灰表面與 VS Code 藍色重點，呈現沉浸式程式開發質感。");

        AppendSection("切換方式");
        AppendStep(1, "開啟頁面", "從左側選單進入『視覺風格』。");
        AppendStep(2, "預覽配色", "每張卡片會顯示該風格的側欄、主色、背景與成功色。");
        AppendStep(3, "立即套用", "按『套用此風格』後，全系統目前頁面與之後開啟的視窗會同步換色。");
        AppendStep(4, "自動保存", "選擇會寫入本機資料檔，下次啟動仍沿用上次風格。");
        AppendTip("快捷鍵", "按 Ctrl + Shift + V 可直接開啟視覺風格頁面。");
    }




    private void OpenDataLocation()
    {
        Directory.CreateDirectory(_service.DataDirectory);
        Process.Start(new ProcessStartInfo
        {
            FileName = _service.DataDirectory,
            UseShellExecute = true
        });
    }

    private static Label CreateBadge(string text, Color accent)
    {
        return new Label
        {
            Text = "  " + text + "  ",
            AutoSize = true,
            Height = 24,
            Margin = new Padding(0, 0, 8, 0),
            Padding = new Padding(4, 3, 4, 3),
            BackColor = BlendWithWhite(accent, 0.90f),
            ForeColor = accent,
            Font = UiTheme.Font(8.5f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
    }

    private static Color BlendWithWhite(Color color, float whiteRatio)
    {
        whiteRatio = Math.Clamp(whiteRatio, 0f, 1f);
        Color surface = UiTheme.Surface;
        int red = (int)Math.Round(color.R * (1f - whiteRatio) + surface.R * whiteRatio);
        int green = (int)Math.Round(color.G * (1f - whiteRatio) + surface.G * whiteRatio);
        int blue = (int)Math.Round(color.B * (1f - whiteRatio) + surface.B * whiteRatio);
        return Color.FromArgb(red, green, blue);
    }

    private void AppendSection(string text)
    {
        if (_content.TextLength > 0)
            _content.AppendText(Environment.NewLine);

        _content.SelectionFont = UiTheme.Font(12.5f, FontStyle.Bold);
        _content.SelectionColor = UiTheme.Navy;
        _content.SelectionIndent = 0;
        _content.AppendText(text + Environment.NewLine);
    }

    private void AppendParagraph(string text)
    {
        _content.SelectionFont = UiTheme.Font(10.2f);
        _content.SelectionColor = UiTheme.Slate;
        _content.SelectionIndent = 0;
        _content.AppendText(text + Environment.NewLine);
    }

    private void AppendStep(int number, string title, string description)
    {
        _content.SelectionFont = UiTheme.Font(10.2f, FontStyle.Bold);
        _content.SelectionColor = UiTheme.Primary;
        _content.SelectionIndent = 8;
        _content.AppendText($"{number:00}  {title}");

        _content.SelectionFont = UiTheme.Font(10.2f);
        _content.SelectionColor = UiTheme.Slate;
        _content.AppendText("　" + description + Environment.NewLine);
    }

    private void AppendBullet(string text)
    {
        _content.SelectionFont = UiTheme.Font(10.2f);
        _content.SelectionColor = UiTheme.Slate;
        _content.SelectionIndent = 10;
        _content.SelectionHangingIndent = 14;
        _content.AppendText("•  " + text + Environment.NewLine);
    }

    private void AppendTip(string title, string text)
    {
        _content.AppendText(Environment.NewLine);
        _content.SelectionIndent = 8;
        _content.SelectionRightIndent = 8;
        _content.SelectionFont = UiTheme.Font(10, FontStyle.Bold);
        _content.SelectionColor = UiTheme.PrimaryDark;
        _content.AppendText("提示｜" + title + Environment.NewLine);

        _content.SelectionFont = UiTheme.Font(9.8f);
        _content.SelectionColor = UiTheme.Slate;
        _content.SelectionIndent = 18;
        _content.AppendText(text + Environment.NewLine);

        _content.SelectionIndent = 0;
        _content.SelectionRightIndent = 0;
        _content.SelectionBackColor = UiTheme.Surface;
    }

    private void AppendPath(string title, string path)
    {
        _content.SelectionFont = UiTheme.Font(9.8f, FontStyle.Bold);
        _content.SelectionColor = UiTheme.Slate;
        _content.SelectionIndent = 8;
        _content.AppendText(title + Environment.NewLine);

        _content.SelectionFont = new Font("Consolas", 9.2f);
        _content.SelectionColor = UiTheme.Muted;
        _content.SelectionBackColor = UiTheme.Background;
        _content.SelectionIndent = 18;
        _content.AppendText(path + Environment.NewLine);
        _content.SelectionBackColor = UiTheme.Surface;
        _content.SelectionIndent = 0;
    }

    private void AppendShortcut(string keys, string action)
    {
        _content.SelectionIndent = 8;
        _content.SelectionFont = new Font("Consolas", 9.5f, FontStyle.Bold);
        _content.SelectionColor = UiTheme.PrimaryDark;
        _content.AppendText(keys.PadRight(12));
        _content.SelectionFont = UiTheme.Font(10.2f);
        _content.SelectionColor = UiTheme.Slate;
        _content.AppendText(action + Environment.NewLine);
    }
}
