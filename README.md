# StudyFlow Pro Research Edition v7.9 — CSV 雙向匯入匯出修正

- 任務管理可「匯入任務 CSV」與「匯出任務 CSV」。
- 專注計時可「匯入紀錄 CSV」與「匯出紀錄 CSV」。
- 課表匯出按鈕明確標示為「匯出課表 CSV」。
- 建議以本系統匯出的 CSV 作為匯入範本；匯入時會檢查欄位、日期、數值與重複紀錄。

# StudyFlow Pro Research Edition v7.4 — 課程色彩與導覽說明修正

## v7.4 課程色彩與導覽說明修正

- 「課程／專案」列表新增識別顏色欄位；修改顏色並儲存後會立即顯示色塊與色碼。
- 任務表格中的課程名稱會使用該課程的識別顏色，方便快速辨認。
- 主畫面左側順序調整為「主控台 → 課程／專案 → 任務管理 → 專注計時」。
- 使用說明左側改為與主畫面完全相同的 14 項順序，並逐項介紹每個功能。


## v7.3 嚴格帳號資料隔離

- 每個帳號使用獨立 GUID 資料夾，任務、課程／專案、專注紀錄、活動紀錄、課表、考古題索引與 PDF／DOCX 原始檔完全分開。
- `studyflow-data.json` 新增帳號所有權標記；若偵測到其他帳號的資料檔，系統會阻止載入並隔離保存，避免誤覆蓋。
- 視覺風格、專注時間、每日目標、提醒設定與上次課表學期改存於每個帳號自己的 `profile-settings.json`。A 使用 Netflix、B 使用 Facebook 時，兩者登出再登入都會回到各自上次的風格。
- 備份、last-good 與滾動快照都位於目前帳號專屬資料夾；其他帳號不受清除、重建 DEMO、匯入或還原影響。
- 新註冊帳號從乾淨的個人空間開始，不再自動得到和其他人相同的展示任務與課表；需要展示資料時可於設定頁按「重建 DEMO 資料」。
- 舊 v7.2 以前的帳號資料會在第一次開啟時自動補上所有權與獨立偏好檔，不必手動搬移。



## v7.1 顯示名稱與介面修正

- 修正註冊頁面底部按鈕在 Windows DPI 縮放下被視窗下緣裁切的問題。
- 左上角 `PRO • ...` 改為顯示註冊時輸入的顯示名稱，而不是登入帳號。
- 主控台的早安／午安／晚安會每分鐘依目前系統時間自動更新。
- 設定與備份的顯示名稱與帳號資料完全同步；在設定頁修改後，側邊欄、主控台與視窗標題會一起更新。
- 匯入或承接舊資料時，舊 JSON 內的名稱不會再覆蓋目前帳號的顯示名稱。

## v7.0 登入、註冊與登出

- 程式啟動後先顯示登入畫面，登入成功才會進入主系統。
- 沒有帳號時可按「建立新帳號」，輸入顯示名稱、登入帳號與密碼完成註冊。
- 註冊完成後會直接登入；側邊欄底部提供「登出帳號」，登出前自動儲存資料。
- 每個帳號都有獨立的任務、課程、課表、專注紀錄、考古題、備份、快照與視覺風格。
- 密碼不以明碼保存，使用隨機鹽值與 PBKDF2-SHA256 雜湊後寫入本機帳號檔。
- 升級前若已有舊版資料，第一個建立的帳號會自動承接原有任務、課表與考古題檔案。

## v6.4 修正

已修正深色風格切回 Facebook 後，左側導覽列下半部顏色未完整還原的問題。側邊欄現在使用明確語意配色重繪，不會再因不同主題的相近深灰色而誤判。

# StudyFlow Pro Research Edition v4.0 — Exam Vault

StudyFlow Pro 是以 **C#、.NET 8、Windows Forms** 製作的離線智慧學習與專案管理系統。v4.0 新增完整的「考古題庫」，將任務、專注紀錄、智慧排程、學習分析與 PDF／DOCX 文件管理整合成同一套桌面應用程式。

## v4.0 核心亮點

### 考古題庫 Exam Vault

- 自訂不同科目，例如演算法、計算機組織、線性代數。
- 匯入 PDF 與 DOCX，支援多選及拖曳匯入。
- 匯入後把原始檔複製到本機題庫，程式下次啟動會自動讀取，不必重新新增。
- PDF 使用 Microsoft Edge WebView2 內嵌閱讀。
- DOCX 由程式解析 XML，轉為具有標題、表格、粗體與圖片的 HTML 閱讀預覽。
- 可收藏、標記未開始／複習中／已完成、記錄開啟次數與最後閱讀時間。
- 搜尋標題、年份、類型、標籤、筆記及原始檔名。
- 重複檔案以 SHA-256 偵測，避免同一份考古題重複匯入。
- 「智慧抽一份」優先選擇尚未完成、開啟次數較少或最久未閱讀的文件。
- 可匯出原始檔，也可建立完整 `.sfexam` 可攜題庫包。
- 題庫包內含科目、索引、閱讀狀態與全部 PDF／DOCX，可在另一台電腦直接匯入。
- 系統資料健檢會檢查考古題 ID、科目關聯與原始檔完整性。

### 既有高分功能

- 任務、課程與專案 CRUD
- 可解釋智慧優先分數（0～100）
- 智慧排程與今日學習路線
- 專注計時、品質、分心次數與反思
- 分析中心與 GDI+ 自製圖表
- Research Center、四象限、稽核軌跡與資料健檢
- CSV、HTML 週報與 iCalendar 匯出
- JSON 原子式存檔、last-good 備份、滾動快照與損壞復原

---

## 執行環境

- Windows 10 或 Windows 11
- Visual Studio 2022
- .NET 8 SDK
- Visual Studio 工作負載：`.NET desktop development`
- Microsoft Edge WebView2 Runtime（一般 Windows 10／11 與 Edge 環境通常已具備）

專案使用官方 NuGet 套件：

```xml
<PackageReference Include="Microsoft.Web.WebView2" Version="1.0.4022.49" />
```

第一次開啟方案時，Visual Studio 會還原 NuGet 套件。

## 執行方式

1. 使用 Visual Studio 開啟 `StudyFlowPro.sln`。
2. 選擇「建置 → 清除方案」。
3. 選擇「建置 → 重建方案」。
4. 按 `F5` 執行。
5. 第一次啟動會先顯示登入畫面；按「建立新帳號」完成註冊後即可使用。
6. 新帳號會建立乾淨且獨立的個人學習空間；若偵測到舊版資料，第一個帳號會自動承接。需要展示資料時可在設定頁按「重建 DEMO 資料」。

---

## 考古題資料如何保存

### 同一台電腦下次啟動

系統會依帳號保存以下內容：

```text
%LocalAppData%\StudyFlowPro\Auth\accounts.json
%LocalAppData%\StudyFlowPro\Users\<帳號ID>\studyflow-data.json
%LocalAppData%\StudyFlowPro\Users\<帳號ID>\profile-settings.json
%LocalAppData%\StudyFlowPro\Users\<帳號ID>\Backups\
%LocalAppData%\StudyFlowPro\Users\<帳號ID>\ExamLibrary\Files\
```

- `Auth\accounts.json` 保存帳號名稱與密碼雜湊，不保存明碼密碼。
- `studyflow-data.json` 保存目前帳號的任務、課程／專案、課表、專注紀錄、活動紀錄、考古題索引與帳號所有權。
- `profile-settings.json` 保存目前帳號自己的視覺風格、專注時間、每日目標、提醒設定與上次課表學期。
- `ExamLibrary\Files` 保存實際 PDF／DOCX。
- 啟動時會自動讀取，因此不用重新新增科目或重新匯入。

### 換電腦或交作業給老師

只把專案 ZIP 交給老師時，`LocalAppData` 不會跟著進入 ZIP。因此請在「考古題庫」按：

```text
匯出題庫包 → ExamLibrary.sfexam
```

`.sfexam` 內含完整科目、考古題資料與所有原始檔，可採兩種方式：

1. 老師執行後，在考古題庫按「匯入題庫包」。
2. 為避免不同帳號自動得到相同題庫，v7.3 起不再從 `DemoAssets` 自動匯入；需要展示時請在目前帳號的考古題庫頁面手動按「匯入題庫包」。

建議繳交 ZIP 結構：

```text
StudyFlowPro/
├─ StudyFlowPro.sln
├─ StudyFlowPro.csproj
├─ DemoAssets/
│  └─ ExamLibrary.sfexam
├─ Models/
├─ Services/
├─ UI/
└─ Docs/
```

---

## 主要架構

```text
Models/
├─ AppModels.cs                 任務、課程、考古題資料模型
├─ UserAccount.cs               帳號與本機驗證資料模型
├─ UserProfilePreferences.cs    每帳號獨立視覺與個人偏好
└─ ViewModels.cs                表格顯示模型

StudyFlowApplicationContext.cs   登入、主畫面與登出之間的單一訊息迴圈切換

Services/
├─ AccountService.cs            註冊、登入、密碼雜湊與舊資料承接
├─ DataService.cs               每帳號 JSON 自動存讀、備份、快照、資料遷移
├─ ExamLibraryService.cs        檔案匯入、SHA-256、題庫包、複習狀態
├─ DocxPreviewService.cs        DOCX XML → HTML 預覽
├─ SmartPlanner.cs              可解釋智慧分數
└─ ...

UI/
├─ LoginForm.cs                 登入入口
├─ RegisterForm.cs              註冊新帳號
├─ MainForm.cs
├─ ExamLibraryControl.cs        考古題庫主頁
├─ ExamDocumentViewerForm.cs    PDF／DOCX 內嵌閱讀器
├─ ExamSubjectEditorForm.cs     科目新增與編輯
├─ ExamPaperEditorForm.cs       考古題資訊與筆記
├─ ExamSubjectPickerForm.cs     匯入科目選擇
└─ ...
```

## 快捷鍵

| 快捷鍵 | 功能 |
|---|---|
| Ctrl + N | 新增任務 |
| Ctrl + F | 搜尋任務 |
| Ctrl + E | 編輯任務 |
| Ctrl + L | 開啟考古題庫 |
| Ctrl + P | 開啟智慧排程 |
| Ctrl + I | 查看優先原因 |
| Ctrl + M | 智慧四象限 |
| Ctrl + R | Research Center |
| Ctrl + S | 手動儲存 |
| F5 | 重新整理 |

## 五分鐘 DEMO 建議

1. 展示登入畫面、註冊新帳號與個人資料隔離
2. 主控台與智慧分數
3. 新增任務與優先原因
3. 開啟考古題庫，新增科目
4. 拖曳匯入 PDF／DOCX
5. 雙擊文件展示內嵌預覽
6. 收藏、切換複習狀態與智慧抽題
7. 匯出 `.sfexam` 題庫包
8. 專注計時、分析中心與資料健檢

> 內嵌 PDF 閱讀採 WebView2。若個別電腦缺少 Runtime，程式會顯示清楚的替代畫面，仍可使用「外部開啟」與「匯出原檔」。


## v5.0 寄信詢問教授

左側選單新增「寄信詢問教授」，內建元智大學資訊工程學系教師資料。

功能包括：

- 陳琨講師置頂
- 教授姓名、英文名、職稱、信箱、辦公室與研究領域
- 搜尋與研究領域篩選
- Gmail 撰寫頁面
- 正式信件範本
- 複製教授信箱
- Gmail 無法開啟時使用預設郵件程式作為備援

程式不會保存 Gmail 密碼，也不會讀取或直接寄送郵件；按下「使用 Gmail 撰寫」後，會交由使用者的瀏覽器完成登入與寄送。

## v5.1 介面修正
- 寄信頁面標題與副標題增加清楚間距。
- 右下角寄信按鈕固定尺寸並完整顯示。


## v5.4 介面微調

本版修正課表頁面藍色提示卡的文字間距，以及刪除學期確認按鈕的完整顯示與垂直對齊。


## v5.5 介面微調
課表右上與右下提示卡已改為上對齊，由上往下閱讀。

## v6.0 視覺風格

左側「視覺風格」可切換 Facebook、Spotify、YouTube、Netflix、Visual Studio 與 Visual Studio Code 六套配色。切換只影響顏色，不改變既有版面、資料或功能，並會儲存到目前帳號專屬的 `profile-settings.json`。快捷鍵為 `Ctrl + Shift + V`。


## v6.2 Visual Studio 顏色風格

- 新增「Visual Studio 顏色風格」：黑色、深灰與紫色為主要配色。
- 使用語意化主題色同步更新所有已開啟頁面與後續新視窗。
- 切換時只改顏色；字型、控制項位置、尺寸、功能及資料保持不變。
- 視覺風格頁改為 3+2 卡片配置，窄視窗可垂直捲動，避免卡片文字被裁切。


## v7.14 資料管理介面簡化
- 移除「建立備份」與「從備份還原」兩個重複按鈕。
- 保留「匯出資料檔／匯入資料檔」、last-good 自動保護與滾動快照。
