using StudyFlowPro.Services;

namespace StudyFlowPro.UI;

public sealed class DiagnosticsForm : Form
{
    public DiagnosticsForm(DataService service)
    {
        Text = "系統資料健檢";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(860, 620);
        MinimumSize = new Size(760, 540);
        BackColor = UiTheme.Background;
        Font = UiTheme.Font(10);

        var results = DiagnosticsService.Run(service.Data, service.CurrentUser.Id).ToList();
        var examLibrary = new ExamLibraryService(service);
        int missingExamFiles = service.Data.ExamPapers.Count(paper => !examLibrary.FileExists(paper));
        results.Add(new DiagnosticItem
        {
            Passed = missingExamFiles == 0,
            Name = "考古題原始檔完整性",
            Detail = missingExamFiles == 0
                ? $"{service.Data.ExamPapers.Count} 份考古題的 PDF／DOCX 原始檔皆可讀取。"
                : $"有 {missingExamFiles} 份考古題原始檔遺失，請重新匯入或從 .sfexam 題庫包還原。"
        });

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        int passed = results.Count(item => item.Passed);
        Control header = UiTheme.StackedHeader(
            "系統資料健檢",
            $"通過 {passed}/{results.Count} 項檢查；用於展示資料完整性與防呆設計",
            out _,
            22);
        root.Controls.Add(header, 0, 0);

        var grid = new DataGridView { Dock = DockStyle.Fill };
        UiTheme.StyleGrid(grid);
        grid.Columns.AddRange(
            UiTheme.TextColumn("Status", "狀態", autoSizeMode: DataGridViewAutoSizeColumnMode.AllCells),
            UiTheme.TextColumn("Name", "檢查項目", 160),
            UiTheme.TextColumn("Detail", "檢查內容", 320));
        grid.DataSource = results.Select(item => new
        {
            Status = item.Passed ? "✓ 通過" : "✕ 失敗",
            item.Name,
            item.Detail
        }).ToList();
        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 ||
                grid.Columns[e.ColumnIndex].DataPropertyName != "Status")
            {
                return;
            }

            string value = e.Value?.ToString() ?? string.Empty;
            e.CellStyle.ForeColor = value.StartsWith("✓") ? UiTheme.Success : UiTheme.Danger;
        };
        root.Controls.Add(grid, 0, 1);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        Button closeButton = UiTheme.PrimaryButton("關閉");
        closeButton.Click += (_, _) => Close();
        bottom.Controls.Add(closeButton);
        root.Controls.Add(bottom, 0, 2);

        Controls.Add(root);
        AcceptButton = closeButton;
    }
}
