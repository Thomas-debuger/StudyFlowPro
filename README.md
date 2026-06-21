# StudyFlow Pro Research Edition

> 一套以 **C#、.NET 8 與 Windows Forms** 開發的離線個人智慧學習管理系統，整合帳號管理、任務規劃、專注計時、課表、考古題、智慧排程、學習分析、研究管理與資料安全功能。

---

## 目錄

- [專案簡介](#專案簡介)
- [系統特色](#系統特色)
- [執行環境](#執行環境)
- [執行方式](#執行方式)
- [系統截圖](#系統截圖)
- [第一次使用](#第一次使用)
- [左側工具列 14 個功能](#左側工具列-14-個功能)
- [學習分析中心](#學習分析中心)
- [Research Center](#research-center)
- [設定與資料管理](#設定與資料管理)
- [資料儲存與帳號隔離](#資料儲存與帳號隔離)
- [CSV 與其他匯出功能](#csv-與其他匯出功能)
- [快捷鍵](#快捷鍵)
- [專案結構](#專案結構)

---

## 專案簡介

StudyFlow Pro 是一套為學生設計的個人化學習系統。使用者可以建立自己的帳號，管理課程、任務、課表與考古題，並利用專注計時、智慧分數與智慧排程安排每天的學習順序。

系統所有核心資料皆儲存在本機，不需要登入雲端服務，也不需要網路才能使用。不同帳號會使用不同的資料空間，任務、課表、考古題、專注紀錄、智慧排程、視覺風格與個人設定都不會互相混用。

---

## 系統特色

- **個人帳號系統**：支援註冊、登入、登出與顯示名稱。
- **帳號資料隔離**：每個帳號擁有獨立的任務、課程、課表、專注紀錄、考古題與設定。
- **課程與任務管理**：先建立課程／專案，再將具體任務歸類到對應課程。
- **專注計時**：可綁定任務、暫停、重設，並記錄品質、分心次數與反思。
- **智慧優先分數**：依期限、優先級、剩餘工作量、難度與進度產生 0～100 分。
- **智慧排程**：依可用時間、休息時間與任務條件產生當日學習路線。
- **課表管理**：管理多學期課表，支援 CSV 匯入、CSV 匯出與 PNG 匯出。
- **考古題庫**：管理 PDF／DOCX 考古題、科目分類、閱讀狀態、收藏與筆記。
- **學習分析中心**：視覺化呈現專注時間、品質、課程進度與任務優先級。
- **Research Center**：提供研究型摘要、稽核軌跡、智慧四象限、週報與行事曆匯出。
- **六套視覺風格**：Facebook、Spotify、YouTube、Netflix、Visual Studio、Visual Studio Code。
- **資料安全**：原子式存檔、last-good 保護、滾動快照、匯入／匯出與資料健檢。

---

## 執行環境

- Windows 10 或 Windows 11
- Visual Studio 2022
- .NET 8 SDK
- Visual Studio 工作負載：`.NET desktop development`
- Microsoft Edge WebView2 Runtime  
  用於程式內的 PDF 預覽，一般 Windows 10／11 通常已安裝

專案使用的主要 NuGet 套件：

```xml
<PackageReference Include="Microsoft.Web.WebView2" Version="1.0.4022.49" />
```

---

## 執行方式

### 使用 Visual Studio 執行

1. 將專案 ZIP 完整解壓縮。
2. 使用 Visual Studio 2022 開啟 `StudyFlowPro.sln`。
3. 等待 Visual Studio 自動還原 NuGet 套件。
4. 選擇上方選單「建置 → 重建方案」。
5. 按 `F5` 或上方綠色開始按鈕執行。
6. 程式會先顯示登入頁面；第一次使用請按「建立新帳號」。

### 使用已編譯版本執行

若資料夾內已有完整發佈版本，直接雙擊：

```text
StudyFlowPro.exe
```

請勿只複製單一 `.exe`，應保留同一資料夾內的 DLL、設定檔與相依檔案。

---

## 系統截圖

### 主控台

<img width="1918" height="1140" alt="image" src="https://github.com/user-attachments/assets/1bbce95a-fe7d-4c79-9f82-f3c16c32dfff" />

### 課程/專案

<img width="1918" height="1137" alt="image" src="https://github.com/user-attachments/assets/2ae36fd1-3884-4823-8b53-90cb82101284" />

### 任務管理

<img width="1918" height="1140" alt="image" src="https://github.com/user-attachments/assets/83ef26a8-57d0-41e5-8ff9-7a3b827d9272" />

### 專注計時

<img width="1918" height="1141" alt="image" src="https://github.com/user-attachments/assets/59c3b145-6ada-4849-823f-dd6299630f15" />

### 課表

<img width="1918" height="1138" alt="image" src="https://github.com/user-attachments/assets/7b18ba78-b16d-44ad-8143-21fe841e17ba" />

### 考古題庫

<img width="1918" height="1142" alt="image" src="https://github.com/user-attachments/assets/a725e9fa-60ba-4999-960d-7e2f0a03f7b3" />

### 寄信詢問教授

<img width="1918" height="1140" alt="image" src="https://github.com/user-attachments/assets/64e80e68-2bcf-4c6d-9342-f25c5f80d55a" />

### 視覺風格

<img width="1918" height="1140" alt="image" src="https://github.com/user-attachments/assets/15eab0f3-b59a-453c-98f8-e02f07697f15" />

### 智慧排程

<img width="1918" height="1138" alt="image" src="https://github.com/user-attachments/assets/bc4cd988-adb4-46bd-a467-b04a46587b50" />

### 分析中心

<img width="1918" height="1137" alt="image" src="https://github.com/user-attachments/assets/2d2aef14-f2bb-45f0-bdd4-6cf55e0e6f38" />

### Research Center

<img width="1918" height="1138" alt="image" src="https://github.com/user-attachments/assets/093fdcc1-54b4-41e7-9899-9d91c31c42f8" />

### 設定與備份

<img width="1918" height="1137" alt="image" src="https://github.com/user-attachments/assets/ebdf7b20-bcc2-47a7-abc2-256dd52d6bb4" />

### 使用說明

<img width="1918" height="1140" alt="image" src="https://github.com/user-attachments/assets/e728642a-be91-442c-9407-e4845a2ec401" />

### 登出帳號

<img width="515" height="256" alt="image" src="https://github.com/user-attachments/assets/6d5bea60-c2e0-4f55-9249-31ae92d18ea3" />

---

## 第一次使用

1. 在登入頁面按「還沒有帳號？建立新帳號」。
2. 輸入顯示名稱、登入帳號、密碼與確認密碼。
3. 按「建立帳號並登入」。
4. 先到「課程／專案」建立課程，例如演算法、作業系統或專題研究。
5. 到「任務管理」新增具體工作，並設定課程、期限、優先級、預估時間與進度。
6. 可到「課表」建立個人學期課表。
7. 使用「專注計時」執行任務並留下學習紀錄。
8. 回到「主控台」、「分析中心」或「Research Center」查看成果。

---

## 左側工具列 14 個功能

| 順序 | 功能 | 功能說明 |
|---:|---|---|
| 1 | **主控台** | 顯示今日學習總覽，包括未完成任務、逾期任務、完成率、今日專注、智慧建議、今日作戰計畫與下一步任務。 |
| 2 | **課程／專案** | 建立課程或長期專案，設定老師、地點與識別顏色。點選課程後，下方會列出歸類在該課程底下的任務。 |
| 3 | **任務管理** | 新增、搜尋、篩選、編輯、完成或刪除任務。可設定課程、期限、優先級、預估時間、難度、精力、標籤與目前進度，也支援任務 CSV 匯入與匯出。 |
| 4 | **專注計時** | 選擇任務後開始計時，可暫停、繼續、完成或重設。完成時可記錄專注品質、分心次數與反思，也支援專注紀錄 CSV 匯入與匯出。 |
| 5 | **課表** | 建立多個學期、加入課程、編輯上課時段與教室；支援匯入課表 CSV、匯出課表 CSV 與匯出課表 PNG。 |
| 6 | **考古題庫** | 建立科目、匯入 PDF／DOCX、搜尋考古題、收藏、切換閱讀狀態、記錄筆記與開啟次數，並支援科目識別顏色。 |
| 7 | **寄信詢問教授** | 使用元智大學資工系共用教授名錄，選擇教授、建立正式郵件內容，並交由 Gmail 或預設郵件程式寄出。程式不保存 Gmail 密碼。 |
| 8 | **視覺風格** | 切換 Facebook、Spotify、YouTube、Netflix、Visual Studio 與 Visual Studio Code 六套配色。每個帳號會記住自己的選擇。 |
| 9 | **智慧排程** | 輸入開始時間、可用分鐘與休息分鐘，系統依任務智慧分數和剩餘工作量產生當日學習路線。重新產生後會立即保存到目前帳號。 |
| 10 | **分析中心** | 顯示生產力指數、本週專注、專注品質、估時準確度、近七天趨勢、各課程完成率與未完成任務優先級分布。 |
| 11 | **Research Center** | 顯示學習一致性、研究型摘要、稽核軌跡，並提供 HTML 週報、智慧四象限、資料健檢與 iCalendar 匯出。 |
| 12 | **設定與備份** | 修改個人學習設定，查看資料位置與快照，匯入／匯出資料、執行資料健檢、重建 DEMO 或清除目前帳號資料。 |
| 13 | **使用說明** | 開啟內建使用指南，依左側 14 個功能的實際順序逐項介紹操作方式與快捷鍵。 |
| 14 | **登出帳號** | 儲存目前資料並返回登入頁。下一位使用者應登入自己的帳號，避免共用個人資料。 |

---

## 學習分析中心

學習分析中心用來回答：「我最近學得怎麼樣？」

### 上方四個指標

#### 1. 生產力指數

生產力指數是 0～100 分的綜合分數，計算內容包括：

```text
生產力指數
＝任務完成率 × 30%
＋學習一致性 × 25%
＋專注目標達成率 × 25%
＋專注品質 × 20%
```

分數越高，代表任務完成、學習規律、專注目標與專注品質的整體表現越好。

#### 2. 本週專注

統計本週星期一到今天，所有有效專注紀錄的分鐘總和。

例如本週四次紀錄為 45、35、50、19 分鐘，畫面就會顯示：

```text
149 分
```

#### 3. 專注品質

每次專注完成後，使用者可給予 1～5 分的自我評分。系統將本週平均分數換算為百分比：

```text
專注品質＝平均品質分數 ÷ 5 × 100%
```

畫面中的 `Q4.0`、`Q5.0` 代表該日平均 Quality：

- `Q4.0`：當日平均專注品質 4.0／5
- `Q5.0`：當日平均專注品質 5.0／5

#### 4. 估時準確度

比較「任務原本預估分鐘」與「實際專注分鐘」的接近程度。只有已完成、具有預估時間且有實際專注紀錄的任務會納入計算。

```text
單一任務準確度＝較小時間 ÷ 較大時間 × 100%
```

越接近 100%，表示使用者越能準確估計任務所需時間；資料不足時顯示「待收集」。

### 其他圖表

- **近 7 天專注分鐘與品質**：長條高度代表每日專注分鐘，`Q` 代表每日平均品質。
- **各課程完成率**：依該課程底下所有任務的實際進度平均值計算；完成任務以 100% 計算。
- **未完成任務優先級分布**：將任務分為立即處理、高度優先、中度優先與可排程。

---

## Research Center

Research Center 用來回答：「我的研究或長期學習流程是否穩定、資料是否完整？」

### 上方四個指標

#### 1. 生產力指數

與學習分析中心相同，用來呈現任務完成率、學習一致性、專注目標與專注品質的綜合表現。

#### 2. 學習一致性

計算本週已經過的天數中，有多少天至少留下過一筆有效專注紀錄：

```text
學習一致性
＝有專注紀錄的天數 ÷ 本週已經過天數 × 100%
```

它重視「是否規律學習」，而不是單純比較總分鐘。

#### 3. 專注品質

顯示本週專注品質的平均百分比，計算方式與學習分析中心相同。

#### 4. 估時準確度

顯示已完成任務的預估時間與實際專注時間是否接近；資料不足時顯示「待收集」。

### 研究型摘要

研究型摘要會依生產力指數、本週與上週專注變化、學習一致性和專注品質，自動組合成一段建議文字。

它不是生成式 AI，而是根據統計條件產生的可解釋摘要，內容可能包含：

- 本週整體表現
- 專注時間是否成長或下降
- 學習節奏是否穩定
- 是否需要降低同時進行的任務數
- 本週專注分鐘
- 深度工作分鐘

深度工作定義為：

```text
單次專注至少 25 分鐘，且分心次數不超過 1 次
```

### 稽核軌跡

稽核軌跡會記錄最近的系統操作，例如：

- 新增或修改任務
- 開啟考古題
- 更新考古題閱讀狀態
- 切換視覺風格
- 匯入、匯出或修改資料

可用來回顧資料何時被修改以及進行過哪些操作。

### Research Center 底部按鈕

| 按鈕 | 功能 |
|---|---|
| **輸出 HTML 週報** | 將生產力指數、專注時間、課程進度、研究摘要與任務資料整理成離線 HTML 報告，可使用瀏覽器開啟、列印或另存為 PDF。 |
| **智慧四象限** | 依任務的重要性與緊急程度，自動分成重要且緊急、重要但不緊急、不重要但緊急、不重要且不緊急四類。 |
| **資料健檢** | 檢查帳號所有權、任務與課程關聯、重複 ID、課表衝突、考古題檔案、專注紀錄與數值範圍等資料問題。此功能只檢查，不會擅自修改資料。 |
| **匯出 iCalendar** | 將未完成任務與截止時間輸出為 `.ics` 行事曆檔，可匯入 Google Calendar、Outlook 或 Apple Calendar。 |
| **關閉** | 關閉 Research Center 並返回主系統，不會登出或結束程式。 |

---

## 設定與資料管理

### 個人設定

- **顯示名稱**：同步顯示於主畫面問候語、左上角帳號區與視窗標題。
- **預設專注分鐘**：設定專注計時的預設時長。
- **每日專注目標**：用於計算專注目標達成率與生產力指數。
- **啟動程式時顯示目前最緊急任務**：登入後主動提醒。
- **即將到期判定範圍**：設定幾小時內到期的任務要視為即將到期。

### 資料安全中心按鈕

| 按鈕 | 功能 |
|---|---|
| **開啟資料位置** | 開啟目前登入帳號的專屬資料夾，可查看主要 JSON、個人偏好與考古題資料夾。 |
| **開啟快照資料夾** | 查看系統自動建立的滾動快照。資料誤改或損壞時，可尋找較早版本。 |
| **匯出資料檔** | 將目前帳號的任務、課程、課表、專注紀錄、設定與考古題索引輸出成 JSON，適合保存或跨電腦移轉。 |
| **匯入資料檔** | 載入先前匯出的 JSON，並套用到目前登入帳號。匯入前系統會保護目前資料。 |
| **執行資料健檢** | 檢查資料完整性、帳號所有權、關聯、重複 ID、檔案路徑、課表衝突與數值範圍。 |
| **重建 DEMO 資料** | 為目前帳號建立展示用任務、課程、課表與專注紀錄，方便快速測試系統。 |
| **清除全部資料** | 清除目前登入帳號的任務、課程、課表、專注紀錄、考古題與活動資料。此操作不影響其他帳號，執行前會要求確認。 |
| **儲存設定** | 儲存目前帳號的顯示名稱、專注時間、每日目標與提醒設定。 |
| **關閉** | 關閉設定視窗並回到主系統。 |

> 匯出的 JSON 主要保存資料與考古題索引。若需要連同 PDF／DOCX 原始檔一起移轉，請使用考古題庫的完整題庫包匯出功能。

---

## 資料儲存與帳號隔離

系統資料預設儲存在：

```text
%LocalAppData%\StudyFlowPro\
```

主要結構：

```text
%LocalAppData%\StudyFlowPro\
├─ Auth\
│  └─ accounts.json
└─ Users\
   └─ <帳號ID>\
      ├─ studyflow-data.json
      ├─ studyflow-data.lastgood.json
      ├─ profile-settings.json
      ├─ profile-settings.lastgood.json
      ├─ Snapshots\
      └─ ExamLibrary\
         ├─ Files\
         └─ PreviewCache\
```

- `accounts.json`：保存帳號資料與密碼雜湊，不保存明碼密碼。
- `studyflow-data.json`：保存任務、課程、課表、專注紀錄、智慧排程、考古題索引與活動紀錄。
- `profile-settings.json`：保存目前帳號的視覺風格、專注設定、提醒與個人偏好。
- `Snapshots`：保存自動滾動快照。
- `ExamLibrary\Files`：保存匯入的 PDF／DOCX 原始檔。

### 帳號隔離原則

不同帳號的下列資料均互相獨立：

- 課程／專案與任務
- 專注紀錄
- 課表與學期
- 考古題科目、閱讀狀態與原始檔
- 智慧排程
- 視覺風格
- 學習分析與 Research Center 統計
- 個人設定、匯入資料與快照

「寄信詢問教授」的元智資工教授名錄為全帳號共用，因為所有使用者皆使用相同學校教授資料；寄件者顯示名稱仍會依目前登入帳號帶入。

---

## CSV 與其他匯出功能

| 功能 | 匯入 | 匯出 |
|---|---:|---:|
| 任務管理 | 匯入任務 CSV | 匯出任務 CSV |
| 專注計時 | 匯入紀錄 CSV | 匯出紀錄 CSV |
| 課表 | 匯入課表 CSV | 匯出課表 CSV、匯出課表 PNG |
| Research Center | — | HTML 週報、iCalendar |
| 設定與資料管理 | 匯入 JSON 資料檔 | 匯出 JSON 資料檔 |
| 考古題庫 | 匯入 PDF／DOCX、題庫包 | 原始檔、題庫包 |

建議先使用系統匯出的 CSV 作為匯入範本，避免欄位名稱、日期或資料格式不一致。

---

## 快捷鍵

| 快捷鍵 | 功能 |
|---|---|
| `Ctrl + N` | 新增任務 |
| `Ctrl + F` | 搜尋任務 |
| `Ctrl + E` | 編輯任務 |
| `Ctrl + I` | 查看智慧分數優先原因 |
| `Ctrl + R` | 開啟 Research Center |
| `Ctrl + M` | 開啟智慧四象限 |
| `Ctrl + P` | 開啟智慧排程 |
| `Ctrl + T` | 開啟課表 |
| `Ctrl + L` | 開啟考古題庫 |
| `Ctrl + G` | 開啟寄信詢問教授 |
| `Ctrl + Shift + V` | 開啟視覺風格 |
| `Ctrl + S` | 立即儲存 |
| `F5` | 重新整理目前畫面 |

---

## 專案結構

```text
StudyFlowPro/
├─ Models/
│  ├─ AppModels.cs
│  ├─ ProfessorContact.cs
│  ├─ UserAccount.cs
│  ├─ UserProfilePreferences.cs
│  └─ ViewModels.cs
├─ Services/
│  ├─ AccountService.cs
│  ├─ CsvService.cs
│  ├─ DataService.cs
│  ├─ DiagnosticsService.cs
│  ├─ DocxPreviewService.cs
│  ├─ ExamLibraryService.cs
│  ├─ HtmlReportService.cs
│  ├─ IcsExportService.cs
│  ├─ ProfessorDirectoryService.cs
│  ├─ ResearchMetricsService.cs
│  └─ SmartPlanner.cs
├─ UI/
│  ├─ LoginForm.cs
│  ├─ RegisterForm.cs
│  ├─ MainForm.cs
│  ├─ AnalyticsForm.cs
│  ├─ ResearchCenterForm.cs
│  ├─ SmartScheduleForm.cs
│  ├─ SettingsForm.cs
│  ├─ ExamLibraryControl.cs
│  └─ ...
├─ Docs/
├─ DemoAssets/
├─ Program.cs
├─ StudyFlowApplicationContext.cs
├─ StudyFlowPro.csproj
└─ StudyFlowPro.sln
```

---

## 使用注意事項

- 本系統為 Windows 桌面應用程式，不支援 macOS 或 Linux 直接執行。
- 第一次開啟方案時需等待 NuGet 還原完成。
- PDF 內嵌預覽需要 Microsoft Edge WebView2 Runtime。
- 系統資料位於使用者的 `%LocalAppData%`，不會因壓縮原始碼專案而自動包含在 ZIP 中。
- 交作業前建議先建立新帳號，測試登入、任務、課表、專注計時、智慧排程、分析中心與資料匯出功能。
- 請勿將 `.vs`、`bin`、`obj` 或 `.git` 資料夾放入繳交 ZIP。

---

## 技術資訊

- Language：C#
- Framework：.NET 8
- UI：Windows Forms
- PDF Preview：Microsoft Edge WebView2
- Data Format：JSON、CSV、HTML、ICS
- Storage：LocalAppData
- Architecture：Models／Services／UI 分層設計
