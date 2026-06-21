using StudyFlowPro.Models;
using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class SettingsForm : Form
{
    private readonly DataService _service;
    private readonly TextBox _userNameBox = new();
    private readonly NumericUpDown _focusMinutes = new();
    private readonly NumericUpDown _dailyGoal = new();
    private readonly CheckBox _reminderCheckBox = new();
    private readonly NumericUpDown _dueSoonHours = new();

    public SettingsForm(DataService service)
    {
        _service = service;
        Text = "設定與資料管理";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(820, 840);
        MinimumSize = new Size(760, 720);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        LoadValues();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26),
            ColumnCount = 1,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));

        Control header = UiTheme.StackedHeader(
            "設定與資料管理",
            "個人化、智慧提醒、滾動快照、資料匯入匯出與健檢",
            out _,
            22);
        root.Controls.Add(header, 0, 0);

        _focusMinutes.Minimum = 1;
        _focusMinutes.Maximum = 180;
        _dailyGoal.Minimum = 10;
        _dailyGoal.Maximum = 1000;
        _dailyGoal.Increment = 10;
        _dueSoonHours.Minimum = 1;
        _dueSoonHours.Maximum = 168;

        var scrollHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = UiTheme.Background,
            Padding = Padding.Empty
        };

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 7,
            Height = 700,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 330));

        body.Controls.Add(CreateField("顯示名稱", _userNameBox), 0, 0);
        body.Controls.Add(CreateField("預設專注分鐘", _focusMinutes), 0, 1);
        body.Controls.Add(CreateField("每日專注目標（分鐘）", _dailyGoal), 0, 2);

        _reminderCheckBox.Text = "啟動程式時顯示目前最緊急任務";
        _reminderCheckBox.AutoSize = true;
        _reminderCheckBox.Font = UiTheme.Font(9.5f, FontStyle.Bold);
        _reminderCheckBox.ForeColor = UiTheme.Slate;
        _reminderCheckBox.Margin = new Padding(2, 12, 0, 0);
        body.Controls.Add(_reminderCheckBox, 0, 3);
        body.Controls.Add(CreateField("即將到期判定範圍（未來幾小時）", _dueSoonHours), 0, 4);

        var dataCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            BackColor = UiTheme.Surface
        };
        var dataLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        dataLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        dataLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        dataLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Label dataTitle = new()
        {
            Text = "資料安全中心",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Navy,
            Font = UiTheme.Font(15, FontStyle.Bold)
        };
        var pathLabel = new Label
        {
            Text = "目前帳號：" + _service.CurrentUser.Username + Environment.NewLine +
                   "個人資料：" + _service.DataPath + Environment.NewLine +
                   "個人偏好：" + _service.ProfileSettingsPath + Environment.NewLine +
                   "考古題原檔：" + _service.ExamFilesDirectory + Environment.NewLine +
                   "所有路徑都位於目前帳號專屬資料夾；不同帳號不會共用。",
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            ForeColor = UiTheme.Muted,
            Font = UiTheme.Font(8.8f),
            TextAlign = ContentAlignment.TopLeft
        };
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoScroll = true,
            Padding = new Padding(0, 8, 0, 0),
            Margin = Padding.Empty
        };

        Button openButton = UiTheme.SecondaryButton("開啟資料位置");
        Button snapshotButton = UiTheme.SecondaryButton("開啟快照資料夾");
        Button exportDataButton = UiTheme.SecondaryButton("匯出資料檔");
        Button importDataButton = UiTheme.SecondaryButton("匯入資料檔");
        Button diagnosticButton = UiTheme.SecondaryButton("執行資料健檢");
        Button demoButton = UiTheme.SecondaryButton("重建 DEMO 資料");
        Button clearButton = UiTheme.DangerButton("清除全部資料");

        openButton.Click += (_, _) => OpenFolder(_service.DataDirectory);
        snapshotButton.Click += (_, _) => OpenFolder(_service.SnapshotDirectory);
        exportDataButton.Click += (_, _) => ExportDataFile();
        importDataButton.Click += (_, _) => ImportDataFile();
        diagnosticButton.Click += (_, _) =>
        {
            using var form = new DiagnosticsForm(_service);
            form.ShowDialog(this);
        };
        demoButton.Click += (_, _) => ResetDemoData();
        clearButton.Click += (_, _) => ClearAllData();

        buttons.Controls.Add(openButton);
        buttons.Controls.Add(snapshotButton);
        buttons.Controls.Add(exportDataButton);
        buttons.Controls.Add(importDataButton);
        buttons.Controls.Add(diagnosticButton);
        buttons.Controls.Add(demoButton);
        buttons.Controls.Add(clearButton);

        dataLayout.Controls.Add(dataTitle, 0, 0);
        dataLayout.Controls.Add(pathLabel, 0, 1);
        dataLayout.Controls.Add(buttons, 0, 2);
        dataCard.Controls.Add(dataLayout);
        body.Controls.Add(dataCard, 0, 6);

        scrollHost.Controls.Add(body);
        root.Controls.Add(scrollHost, 0, 1);

        var bottomBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };
        Button saveButton = UiTheme.PrimaryButton("儲存設定");
        Button closeButton = UiTheme.SecondaryButton("關閉");
        saveButton.Click += (_, _) => SaveSettings();
        closeButton.Click += (_, _) => Close();
        bottomBar.Controls.Add(saveButton);
        bottomBar.Controls.Add(closeButton);
        root.Controls.Add(bottomBar, 0, 2);

        Controls.Add(root);
        AcceptButton = saveButton;
    }

    private static Panel CreateField(string labelText, Control control)
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        control.Dock = DockStyle.Fill;
        control.Font = UiTheme.Font(10);
        panel.Controls.Add(control);
        panel.Controls.Add(new Label
        {
            Text = labelText,
            Dock = DockStyle.Top,
            Height = 26,
            ForeColor = UiTheme.Slate,
            Font = UiTheme.Font(9, FontStyle.Bold)
        });
        return panel;
    }

    private void LoadValues()
    {
        _userNameBox.Text = string.IsNullOrWhiteSpace(_service.CurrentUser.DisplayName)
            ? "同學"
            : _service.CurrentUser.DisplayName;
        _focusMinutes.Value = Math.Clamp(
            _service.Data.Settings.DefaultFocusMinutes,
            (int)_focusMinutes.Minimum,
            (int)_focusMinutes.Maximum);
        _dailyGoal.Value = Math.Clamp(
            _service.Data.Settings.DailyGoalMinutes,
            (int)_dailyGoal.Minimum,
            (int)_dailyGoal.Maximum);
        _reminderCheckBox.Checked = _service.Data.Settings.ShowDueSoonReminder;
        _dueSoonHours.Value = Math.Clamp(
            _service.Data.Settings.DueSoonHours,
            (int)_dueSoonHours.Minimum,
            (int)_dueSoonHours.Maximum);
    }

    private void SaveSettings()
    {
        if (!_service.UpdateDisplayName(_userNameBox.Text, out string displayNameError))
        {
            MessageBox.Show(displayNameError, "顯示名稱無法儲存",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _userNameBox.Focus();
            return;
        }

        _service.Data.Settings.DefaultFocusMinutes = (int)_focusMinutes.Value;
        _service.Data.Settings.DailyGoalMinutes = (int)_dailyGoal.Value;
        _service.Data.Settings.ShowDueSoonReminder = _reminderCheckBox.Checked;
        _service.Data.Settings.DueSoonHours = (int)_dueSoonHours.Value;
        _service.Log(ActivityType.Updated, "Settings", null, "更新個人化與提醒設定");
        _service.SaveAndNotify();

        MessageBox.Show("設定已儲存。", "完成",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static void OpenFolder(string folder)
    {
        Directory.CreateDirectory(folder);
        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });
    }

    private void ExportDataFile()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "StudyFlow 資料檔 (*.json)|*.json",
            InitialDirectory = _service.BackupDirectory,
            FileName = $"StudyFlowPro-{_service.CurrentUser.Username}-data-{DateTime.Now:yyyyMMdd-HHmm}.json"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            _service.ExportRawJson(dialog.FileName);
            MessageBox.Show(
                "資料檔已匯出。下次可用「匯入資料檔」載入。" + Environment.NewLine + Environment.NewLine +
                "注意：JSON 只保存考古題索引；若要連同 PDF／DOCX 一起移轉，請到「考古題庫」匯出 .sfexam 題庫包。",
                "完成",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("匯出失敗：\n" + ex.Message, "錯誤",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImportDataFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "StudyFlow 資料檔或備份檔 (*.json)|*.json|所有檔案 (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        if (MessageBox.Show(
            "匯入後會把目前任務、課程、專注紀錄與考古題索引替換成所選檔案內容。" + Environment.NewLine + Environment.NewLine +
            "PDF／DOCX 原始檔需另外使用考古題庫的 .sfexam 題庫包匯入。" + Environment.NewLine + Environment.NewLine +
            "匯入前系統會先建立 last-good 備份，是否繼續？",
            "確認匯入資料檔",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _service.ImportDataFile(dialog.FileName);
            LoadValues();
            MessageBox.Show(
                "資料檔已匯入。之後重新開啟程式時，系統會自動讀取這份匯入後的資料。",
                "完成",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("匯入失敗：\n" + ex.Message, "錯誤",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ResetDemoData()
    {
        if (MessageBox.Show(
            "這會覆蓋目前資料並建立展示內容，是否繼續？",
            "重建 DEMO",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        _service.ResetToDemo();
        LoadValues();
        MessageBox.Show("DEMO 資料已重建。", "完成",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearAllData()
    {
        if (MessageBox.Show(
            "確定清除目前帳號的任務、課程／專案、專注紀錄、課表、活動紀錄與考古題庫？PDF／DOCX 原始檔也會刪除。其他帳號完全不受影響，但此動作無法復原。",
            "危險操作",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        _service.ClearAll();
        LoadValues();
        MessageBox.Show("資料已清除。", "完成",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
