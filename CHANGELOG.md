## v7.17 專注計時重設視窗關閉鍵修正
- 修正重設確認視窗右上角 X 無法點擊。
- 改用支援取消與標題列關閉的自訂確認視窗。

## v7.16 考古題科目識別色顯示修正
- 修正編輯考古題科目並更換識別顏色後，左側科目分類沒有顯示新顏色的問題。
- 科目列會精準套用使用者選擇的原始色碼，選取時也不會被預設選取色覆蓋。
- 文字會依背景亮度自動切換黑色或白色，確保可讀性。

## v7.15 新增學期課表視窗版面修正
- 改用固定 ClientSize，避免標題列與 Windows DPI 縮放吃掉內容高度。
- 提示區改為固定高度，可完整顯示兩行命名與匯入說明。
- 調整提示內距與靠上對齊，避免文字被壓縮或裁切。
- 保留取消與建立學期按鈕的固定尺寸與右下角排列。

## v7.14 手動備份／還原功能移除
- 從「設定與資料管理」移除「建立備份」與「從備份還原」按鈕。
- 移除 SettingsForm 中對應的手動備份與還原事件及方法。
- 保留「匯出資料檔／匯入資料檔」、last-good 自動備援與滾動快照。
- 同步更新使用說明中的設定與資料管理介紹。

## v7.13 學習分析課程進度修正
- 「學習分析中心 → 各課程完成率」改為依課程底下所有任務的實際進度平均計算。
- 例如課程只有一項進度 13% 的任務，分析中心會立即顯示 13%，不必等任務勾選完成。

## v7.12 使用說明 Research Center 名稱顯示修正
- 修正使用說明左側第 11 項因寬度不足只顯示「Research」的問題，現在會完整顯示「Research Center」。

## v7.11 Research Center 按鈕精簡
- 移除 Research Center 底部的「重新整理」按鈕。
- 其餘按鈕會自動向左補位，不留下空白。

## v7.10 智慧排程持久化與帳號隔離修正
- 智慧排程按下「重新產生」後，立即將開始時間、可用分鐘、休息分鐘與完整區塊結果保存到目前帳號的 `studyflow-data.json`。
- 使用右上角 X 或「關閉」離開排程視窗後，主控台的「今日作戰計畫」會顯示最後一次重新產生的排程，不再退回舊版排序。
- 不同帳號各自保存智慧排程、任務、課程、專注紀錄、課表、考古題、視覺風格、分析與備份設定。
- 「寄信詢問教授」仍使用程式內建的元智資工教授共用名錄，不受帳號切換影響。

## v7.9 任務與專注紀錄 CSV 匯入補齊
- 任務管理新增「匯入任務 CSV」，可重新載入本系統匯出的任務資料。
- 任務 CSV 匯入會保留課程、期限、優先級、進度、難度、精力、標籤與說明；CSV 中不存在的課程會自動建立。
- 專注計時新增「匯入紀錄 CSV」，可重新載入專注時間、任務、課程、品質、分心次數與備註。
- 匯入專注紀錄時會避免重複累加任務專注分鐘，找不到原任務時仍保留 CSV 內的任務與課程名稱。
- 課表按鈕「匯出 CSV」改名為「匯出課表 CSV」，避免與任務及專注紀錄 CSV 混淆。
- CSV 匯入新增必要欄位、日期、數值與重複資料檢查，錯誤列不會中斷其他正常資料的匯入。

## v7.8 側邊導覽列圖示與文字對齊修正
- 全部 14 個側邊導覽按鈕改用固定圖示欄與固定文字起點。
- 修正「寄信詢問教授」及「設定與備份」因圖示字寬不同而顯得偏左的問題。
- 統一圖示大小、文字位置、垂直置中與高 DPI 顯示效果。

## v7.7 智慧排程按鈕與課程任務清單修正
- 加寬「使用現在時間」按鈕並調整內距與字型，避免高 DPI 顯示縮放時文字被裁切。
- 課程／專案頁新增下方任務清單；點選任一課程後，立即列出歸類於該課程的所有任務。
- 任務清單顯示狀態、名稱、優先級、期限、進度與智慧分數，雙擊任務可直接編輯。
- 更新資料時保留目前選取的課程，避免畫面跳回第一列。

## v7.6 課程原始色碼精準顯示修正
- 課程／專案列表整列直接使用使用者選取的原始 RGB／HEX 顏色。
- 移除選取資料列時自動加深或變亮的處理，避免亮黃色顯示成一般黃、一般黃色顯示成暗黃。
- 一般狀態與選取狀態維持完全相同的背景色，只依亮度切換黑字或白字。

## v7.5 課程整列識別色修正
- 課程／專案頁面的識別顏色改為套用到整個資料列，包括課程名稱、老師／負責人、地點、未完成任務與完成率。
- 整列直接使用使用者選擇的識別色，並依底色亮度自動切換黑／白文字；選取時以同色系深淺變化顯示。
- 修改課程顏色並儲存後，整列會立即同步更新。

# Changelog

## v7.4 - 課程色彩、側邊欄順序與使用說明

- 課程／專案列表新增「識別顏色」欄位，直接以背景色與十六進位色碼呈現。
- 修改課程識別顏色後，課程列表與任務表格會立即重新整理並顯示新顏色。
- 主畫面側邊欄將「課程／專案」移到「主控台」與「任務管理」之間。
- 使用說明改為 14 個獨立主題，順序與主畫面左側 14 個按鍵一致。
- 新增智慧排程、分析中心、Research Center、設定與備份、使用說明及登出帳號的獨立操作介紹。

## v7.3 - 嚴格帳號資料隔離

- 為每份 `studyflow-data.json` 加入 `OwnerAccountId` 與 `OwnerUsername`，避免其他帳號資料被誤載入。
- 新增每帳號獨立的 `profile-settings.json`，保存視覺風格、專注偏好、每日目標、提醒與最後課表學期。
- 登入時先載入目前帳號的獨立偏好再建立主視窗，修正 A／B 帳號切換後沿用上一位使用者主題的問題。
- 任務、課程／專案、專注紀錄、活動歷程、課表、考古題索引與原始檔、備份與快照均限定在目前帳號 GUID 資料夾。
- 不同登入帳號名稱的 JSON 備份不可匯入或還原；相同帳號名稱可在換電腦後重新綁定。
- 清除資料與重建 DEMO 只影響目前帳號，並保留該帳號自己的視覺與提醒偏好。
- 新帳號改為建立空白個人學習空間，避免多個帳號看起來共用相同展示內容。
- 考古題庫不再自動匯入共用 DemoAssets；每個帳號須自行匯入自己的題庫包。
- 修正設定頁一處重複程式碼造成的潛在編譯問題。

## v7.2 課表頁標題副文字裁切修正
- 修正課表頁面上方標題卡在高 DPI / Windows 顯示縮放下，『課表』下方說明文字被遮住的問題。
- 提高課表頁標題區高度，並調整副標題對齊方式與內距，避免文字貼齊底部造成裁切。

## v7.1 - 帳號名稱同步與註冊畫面修正

- 修正註冊頁面底部「返回登入」與「建立帳號並登入」按鈕在 DPI 縮放下被裁切的問題。
- 左上角品牌區改顯示註冊時輸入的顯示名稱，不再顯示登入帳號。
- 主畫面的早安／午安／晚安會每分鐘自動依系統時間更新。
- 設定與備份中的顯示名稱與帳號資料同步；修改後會同時更新帳號、側邊欄、問候語與視窗標題。
- 匯入舊資料或承接舊版 JSON 時，不再讓舊資料內的名稱覆蓋目前登入帳號的顯示名稱。

# v7.0 Login, Registration and Per-User Data

- Added a required login screen before the main application starts.
- Added local account registration with display name, username, password confirmation, and validation.
- Passwords are stored using random salts and PBKDF2-SHA256 hashes; plaintext passwords are never written to disk.
- Added isolated per-account data directories for tasks, courses, schedules, focus sessions, exam files, backups, snapshots, and visual styles.
- Added automatic one-time migration of legacy shared data to the first registered account.
- Added a sidebar logout command that saves current data and returns to the login screen.
- Added the current username to the sidebar brand area.

# v6.4 Sidebar Theme Restore Fixed

- 修正從 Spotify、Visual Studio Code 等深色風格切回 Facebook 時，左側導覽列下半部變成淺色或白色的問題。
- 側邊欄容器、Logo、全部導覽按鈕與「使用說明」按鈕改用明確的語意配色重新套用，不再依靠相近 RGB 顏色推測角色。
- 保留目前頁面的選取狀態，切換風格後仍會正確顯示藍色作用中按鈕。
- 僅修改顏色重套邏輯，沒有改動任何版面、字型、位置、大小、資料或功能。

# v6.2 - Visual Studio Style

- Added Visual Studio-inspired dark theme with black, charcoal, and purple semantic colors.
- Added VisualStudio to VisualStyleKind without changing existing enum values.
- Updated the style selector to a scroll-safe 3-by-2 card layout.
- Updated help text and project metadata.
- Theme switching still changes colors only.

# Changelog

## v6.0.0 — Visual Styles

- 新增左側「視覺風格」頁面。
- 新增 Facebook、Spotify、YouTube 三套全系統配色。
- Facebook 配色為預設值。
- 風格切換立即套用至目前頁面與已開啟視窗。
- 後續開啟的視窗會自動套用目前風格。
- 風格設定寫入本機 JSON，下次啟動自動沿用。
- 保留課程自訂顏色與文件原始內容。
- 新增 `Ctrl + Shift + V` 快捷鍵。
- 將主要表面、背景、文字、邊框、表格、側欄及自訂繪圖顏色集中到動態主題系統。
