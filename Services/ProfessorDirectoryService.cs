using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public static class ProfessorDirectoryService
{
    private static readonly IReadOnlyList<ProfessorContact> Professors = new List<ProfessorContact>
    {
        new ProfessorContact
        {
            ChineseName = "陳琨",
            EnglishName = "Chen Kun",
            Title = "講師",
            Degree = "銘傳大學資工碩士/現職元智大學資訊工程學系博士生",
            Office = "",
            Email = "s1139102@mail.yzu.edu.tw",
            Phone = "",
            ResearchAreas = "視窗程式設計(二)",
            IsFeatured = true
        },
        new ProfessorContact
        {
            ChineseName = "蔡侑庭",
            EnglishName = "Yu-Ting Tsai",
            Title = "副教授兼系主任",
            Degree = "國立交通大學 資訊工程博士",
            Office = "R60904",
            Email = "yttsai@saturn.yzu.edu.tw",
            Phone = "03-4638800轉3008",
            ResearchAreas = "三維電腦繪圖、三維繪圖資料處理、通用繪圖處理器計算與應用",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "王任瓚",
            EnglishName = "Ran-Zan Wang",
            Title = "特聘教授兼資訊長",
            Degree = "國立交通大學 資訊科學博士",
            Office = "R61014",
            Email = "rzwang@saturn.yzu.edu.tw",
            Phone = "03-4638800轉3003",
            ResearchAreas = "深度學習、影像處理與電腦視覺、人機介面設計、互動多媒體應用",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "艾哈邁德",
            EnglishName = "Afaroj Ahamad",
            Title = "助理教授",
            Degree = "深度學習與邊緣計算博士，國立虎尾科技大學光電工程系",
            Office = "R60903",
            Email = "aahmed09@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2364",
            ResearchAreas = "邊緣計算、深度學習、計算機視覺、語義分割、FPGA加速",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "伊桑",
            EnglishName = "Ihsan Ullah",
            Title = "助理教授",
            Degree = "PhD in Department of Electrical and Computer Engineering from Sungkyunkwan University, South Korea",
            Office = "R60913",
            Email = "ihsan@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2353",
            ResearchAreas = "深度強化學習、資料聚合和資料融合、虛擬網路嵌入（VNE）和網路切片（5G）、物聯網（IoT）、雲端運算和無線感測器網路",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "周志岳",
            EnglishName = "Chih-Yueh Chou",
            Title = "教授",
            Degree = "國立中央大學 資訊工程博士",
            Office = "R1408",
            Email = "cychou@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2362",
            ResearchAreas = "大數據學習分析、智慧型教育代理人、人工智慧在教育應用、電腦輔助學習、電腦輔助程式學習、數位遊戲式學習",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "林佑政",
            EnglishName = "Yu-Cheng Lin",
            Title = "助理教授",
            Degree = "中原大學電子所資工組博士",
            Office = "R1312",
            Email = "linyu@saturn.yzu.edu.tw",
            Phone = "03-4638800轉3010",
            ResearchAreas = "積體電路設計自動化、演算法、系統開發、VLSI實體設計",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "林明義",
            EnglishName = "Ming-Yi Lin",
            Title = "助理教授",
            Degree = "國立中央大學 資訊工程博士",
            Office = "R1306",
            Email = "lmy@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2355",
            ResearchAreas = "嵌入式系統、嵌入式AI、智慧感測網路、工業物聯網、邊緣運算",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "林榮彬",
            EnglishName = "Rung-Bin Lin",
            Title = "教授兼資訊學院院長",
            Degree = "美國明尼蘇達大學 資訊科學博士",
            Office = "R1406",
            Email = "csrlin@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2365",
            ResearchAreas = "積體電路設計自動化、VLSI實體設計、元件庫設計、低功率設計方法、靜態時序分析等",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "阿諾保羅",
            EnglishName = "Anal Paul (IEEE Senior Member)",
            Title = "助理教授",
            Degree = "PhD in Information Technology from Indian Institute of Engineering Science and Technology, Shibpur, India.",
            Office = "R60905",
            Email = "apaul@saturn.yzu.edu.tw",
            Phone = "03-4638800轉3006",
            ResearchAreas = "B5G/6G Networks, Application of AI&ML in Wireless Communications, Quantum Machine Learning in Wireless Networks, Digital Twin of Wireless networks.",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "胡光泰",
            EnglishName = "Quang-Thai Ho",
            Title = "助理教授",
            Degree = "元智大學資訊工程博士",
            Office = "R1302",
            Email = "hqthai@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2371",
            ResearchAreas = "生物資訊、機器學習、深度學習、雲計算服務",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "徐逸懷",
            EnglishName = "Yi-Huai Hsu (IEEE Senior Member)",
            Title = "助理教授兼研究發展處研發行政組組長",
            Degree = "國立交通大學 資訊科學與工程博士",
            Office = "R1320",
            Email = "yhhsu@saturn.yzu.edu.tw",
            Phone = "03-4638800轉3005",
            ResearchAreas = "無線行動通訊網路、邊緣運算、智慧資源管理配置",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "陳李書滕",
            EnglishName = "Chen Lee Shu-Teng",
            Title = "助理教授",
            Degree = "國立交通大學 資訊科學與工程博士",
            Office = "R61004",
            Email = "shuteng@saturn.yzu.edu.tw",
            Phone = "03-4638800轉3011",
            ResearchAreas = "雲端運算、資訊安全",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "陳尚寬",
            EnglishName = "Shang-Kuan Chen",
            Title = "助理教授",
            Degree = "交通大學資訊科學與工程研究所博士",
            Office = "R61001",
            Email = "cotachen@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2356",
            ResearchAreas = "QR碼的編碼與應用、圖像資安、深度學習、機器學習、最佳化排程、指紋辨識、應用數學。",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "許博凱",
            EnglishName = "Justin BoKai Hsu",
            Title = "助理教授",
            Degree = "國立交通大學 生物資訊及系統生物學博士",
            Office = "R1314",
            Email = "justin.bokai@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2358",
            ResearchAreas = "大數據資料分析、生物資料庫建構及資料探勘、生物資訊、腫瘤微環境與臨床資料分析",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "張經略",
            EnglishName = "Ching-Lueh Chang",
            Title = "教授",
            Degree = "國立台灣大學 資訊工程博士",
            Office = "R61016",
            Email = "clchang@saturn.yzu.edu.tw",
            Phone = "03-4638800轉3009",
            ResearchAreas = "理論計算機科學,包括離散數學、演算法、圖論、計算複雜度",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "曾王道",
            EnglishName = "Wang-Dauh Tseng",
            Title = "副教授",
            Degree = "國立交通大學 資訊科學博士",
            Office = "R1410",
            Email = "wdtseng@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2376",
            ResearchAreas = "超大型積體電路測試、容錯計算",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "黃依賢",
            EnglishName = "I-Shyan Hwang",
            Title = "教授",
            Degree = "美國紐約州立大學水牛城分校 電機暨資訊工程博士",
            Office = "R1412",
            Email = "ishwang@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2375",
            ResearchAreas = "光纖高速網路、無線行動計算、網路分析與評估、測試演算法設計",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "黃怡錚",
            EnglishName = "Yi-Jheng Huang",
            Title = "副教授兼資訊學院英語學士班主任",
            Degree = "國立交通大學 資訊科學與工程博士",
            Office = "R1318",
            Email = "yjhuang@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2360",
            ResearchAreas = "計算機圖學、混合實境、人機互動",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "黃鈺峰",
            EnglishName = "Yu-Feng Huang",
            Title = "助理教授",
            Degree = "國立臺灣大學 資訊工程博士",
            Office = "R1310",
            Email = "yfhuang@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2366",
            ResearchAreas = "生物資訊、次世代定序技術與分析、機器學習",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "黃毅然",
            EnglishName = "Yieh-Ran Haung",
            Title = "副教授",
            Degree = "國立交通大學 資訊工程博士",
            Office = "R1402",
            Email = "yrhaung@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2374",
            ResearchAreas = "行動多媒體網路、整合服務網際網路、無線網際網路存取",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "楊正仁",
            EnglishName = "Cheng-Zen Yang",
            Title = "教授",
            Degree = "國立台灣大學 資訊工程博士",
            Office = "R1414",
            Email = "czyang@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2361",
            ResearchAreas = "文本探勘、 資訊擷取、 機器學習、 智慧計算、 軟體測試",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "歐昱言",
            EnglishName = "Yu-Yen Ou",
            Title = "教授",
            Degree = "國立台灣大學 資訊工程博士",
            Office = "R60914",
            Email = "yien@saturn.yzu.edu.tw",
            Phone = "03-4638800轉2185",
            ResearchAreas = "生物資訊、機器學習、資料探勘",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "簡廷因",
            EnglishName = "Ting-Ying Chien",
            Title = "副教授",
            Degree = "國立台灣大學 資訊工程所博士",
            Office = "R1404",
            Email = "tinin@saturn.yzu.edu.tw",
            Phone = "03-4638800轉3004",
            ResearchAreas = "資料探勘&機器學習、大數據資料分析、智慧製造",
            IsFeatured = false
        },
        new ProfessorContact
        {
            ChineseName = "魏得恩",
            EnglishName = "Wilbur Wei",
            Title = "助理教授",
            Degree = "國立臺灣科技大學 資訊工程學系 博士",
            Office = "R61005",
            Email = "wilbur.wei@saturn.yzu.edu.tw",
            Phone = "03-4638800轉3007",
            ResearchAreas = "資訊安全、人工智慧安全",
            IsFeatured = false
        }
    };

    public static IReadOnlyList<ProfessorContact> GetAll() => Professors;

    public static IEnumerable<string> GetCategories() =>
        Professors
            .Select(GetCategory)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .Prepend("全部領域");

    public static string GetCategory(ProfessorContact professor)
    {
        string text = professor.ResearchAreas ?? string.Empty;

        if (ContainsAny(text, "生物資訊", "次世代定序", "臨床資料", "腫瘤"))
            return "生物資訊";
        if (ContainsAny(text, "資訊安全", "資安", "安全"))
            return "資訊安全";
        if (ContainsAny(text, "無線", "網路", "物聯網", "5G", "6G", "通訊", "雲端"))
            return "網路與雲端";
        if (ContainsAny(text, "VLSI", "積體電路", "FPGA", "硬體", "嵌入式", "時序分析"))
            return "硬體與嵌入式";
        if (ContainsAny(text, "圖學", "繪圖", "影像", "電腦視覺", "人機", "混合實境", "多媒體"))
            return "圖學與人機互動";
        if (ContainsAny(text, "演算法", "圖論", "複雜度", "離散數學", "理論計算機"))
            return "理論與演算法";
        if (ContainsAny(text, "教育", "學習分析", "電腦輔助學習", "遊戲式學習"))
            return "智慧教育";
        if (ContainsAny(text, "深度學習", "機器學習", "資料探勘", "人工智慧", "AI", "大數據"))
            return "人工智慧與資料科學";

        return "其他";
    }

    public static string BuildGmailComposeUrl(string email, string subject, string body)
    {
        return "https://mail.google.com/mail/?view=cm&fs=1"
            + "&to=" + Uri.EscapeDataString(email ?? string.Empty)
            + "&su=" + Uri.EscapeDataString(subject ?? string.Empty)
            + "&body=" + Uri.EscapeDataString(body ?? string.Empty);
    }

    public static string BuildMailToUrl(string email, string subject, string body)
    {
        return "mailto:" + Uri.EscapeDataString(email ?? string.Empty)
            + "?subject=" + Uri.EscapeDataString(subject ?? string.Empty)
            + "&body=" + Uri.EscapeDataString(body ?? string.Empty);
    }

    private static bool ContainsAny(string source, params string[] keywords) =>
        keywords.Any(keyword =>
            source.Contains(keyword, StringComparison.OrdinalIgnoreCase));
}
