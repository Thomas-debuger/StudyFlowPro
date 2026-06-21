using StudyFlowPro.Models;

namespace StudyFlowPro.Services;

public static class TimetableCatalog
{
    public static readonly (string Code, string Display)[] Semesters =
    {
        ("113-1", "113 年第 1 學期"),
        ("113-2", "113 年第 2 學期"),
        ("114-1", "114 年第 1 學期"),
        ("114-2", "114 年第 2 學期")
    };

    public static List<TimetableSemester> CreateDefaultSemesters() => Semesters
        .Select(item => new TimetableSemester
        {
            Code = item.Code,
            DisplayName = item.Display,
            IsBuiltIn = true,
            CreatedAt = DateTime.Now
        })
        .ToList();

    public static readonly string[] DayNames =
    {
        string.Empty,
        "週一",
        "週二",
        "週三",
        "週四",
        "週五",
        "週六"
    };

    public static readonly (string Start, string End)[] PeriodTimes =
    {
        (string.Empty, string.Empty),
        ("08:10", "09:00"),
        ("09:10", "10:00"),
        ("10:10", "11:00"),
        ("11:10", "12:00"),
        ("12:10", "13:00"),
        ("13:10", "14:00"),
        ("14:10", "15:00"),
        ("15:10", "16:00"),
        ("16:10", "17:00"),
        ("17:10", "18:00")
    };

    public static string SemesterDisplay(string code)
    {
        foreach ((string semesterCode, string display) in Semesters)
        {
            if (semesterCode == code)
                return display;
        }
        return code;
    }

    public static string SemesterDisplay(IEnumerable<TimetableSemester> semesters, string code)
    {
        TimetableSemester? semester = semesters?.FirstOrDefault(item =>
            string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));
        return semester?.DisplayName ?? SemesterDisplay(code);
    }

    public static string CreateSemesterCode(string displayName, IEnumerable<string> existingCodes)
    {
        var used = new HashSet<string>(existingCodes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        Match match = Regex.Match(displayName ?? string.Empty, @"(?<year>\d{3})\s*(?:年)?\s*(?:第)?\s*(?<term>[12])\s*(?:學期)?");
        if (match.Success)
        {
            string natural = $"{match.Groups["year"].Value}-{match.Groups["term"].Value}";
            if (!used.Contains(natural))
                return natural;
        }

        string baseCode = "custom-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        string candidate = baseCode;
        int suffix = 2;
        while (used.Contains(candidate))
            candidate = baseCode + "-" + suffix++;
        return candidate;
    }

    public static bool IsBuiltInSemester(string code) => Semesters.Any(item => item.Code == code);

    public static string PeriodDisplay(int period)
    {
        int safe = Math.Clamp(period, 1, 10);
        (string start, string end) = PeriodTimes[safe];
        return $"第 {safe} 節  {start}–{end}";
    }

    public static string PeriodRangeDisplay(int startPeriod, int endPeriod)
    {
        int start = Math.Clamp(startPeriod, 1, 10);
        int end = Math.Clamp(endPeriod, start, 10);
        return start == end
            ? PeriodDisplay(start)
            : $"第 {start}–{end} 節  {PeriodTimes[start].Start}–{PeriodTimes[end].End}";
    }

    public static int GetCurrentPeriod(DateTime now)
    {
        for (int period = 1; period <= 10; period++)
        {
            if (!TimeSpan.TryParse(PeriodTimes[period].Start, out TimeSpan start) ||
                !TimeSpan.TryParse(PeriodTimes[period].End, out TimeSpan end))
                continue;

            if (now.TimeOfDay >= start && now.TimeOfDay <= end)
                return period;
        }

        return 0;
    }

    public static int ToDayIndex(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => 1,
        DayOfWeek.Tuesday => 2,
        DayOfWeek.Wednesday => 3,
        DayOfWeek.Thursday => 4,
        DayOfWeek.Friday => 5,
        DayOfWeek.Saturday => 6,
        _ => 0
    };
}

public static class TimetableSeedData
{
    public static List<TimetableEntry> Create()
    {
        var entries = new List<TimetableEntry>();

        void Add(
            string semester,
            string course,
            int day,
            int start,
            int end,
            string location,
            string color,
            string instructor = "",
            string notes = "")
        {
            entries.Add(new TimetableEntry
            {
                SemesterCode = semester,
                CourseName = course,
                DayIndex = day,
                StartPeriod = start,
                EndPeriod = end,
                Location = location,
                Instructor = instructor,
                ColorHex = color,
                Notes = notes,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        // 113 年第 1 學期
        Add("113-1", "英語（一）：進階英語", 2, 1, 2, "R3101", "#2563EB");
        Add("113-1", "資訊概論", 5, 2, 2, "R1401B", "#7C3AED");
        Add("113-1", "程式設計（一）", 2, 3, 4, "R1401B", "#0891B2");
        Add("113-1", "健康、休閒與生活", 3, 3, 4, "R1213", "#059669");
        Add("113-1", "資訊概論", 4, 3, 4, "R1401B", "#7C3AED");
        Add("113-1", "中文閱讀、思辨與表達（一）", 5, 3, 4, "R3104", "#DB2777");
        Add("113-1", "基礎程式設計－C++ 實習（一）", 1, 6, 7, "R1008", "#EA580C");
        Add("113-1", "醫學工程概論（一）", 2, 6, 7, "R1401B", "#9333EA");
        Add("113-1", "微積分（一）", 1, 8, 8, "R60312", "#D97706");
        Add("113-1", "微積分（一）", 2, 8, 8, "R60312", "#D97706");
        Add("113-1", "體育", 5, 8, 9, "R 體育場地", "#16A34A");
        Add("113-1", "程式設計（一）", 1, 9, 9, "R1401B", "#0891B2");
        Add("113-1", "微積分（一）", 2, 9, 9, "R60312", "#D97706");

        // 113 年第 2 學期
        Add("113-2", "英語（二）：進階英語", 2, 1, 2, "R70110", "#2563EB");
        Add("113-2", "醫療資訊學概論", 3, 1, 2, "R60312", "#0F766E");
        Add("113-2", "電子電路學", 5, 2, 2, "R70103", "#B45309");
        Add("113-2", "微積分（二）", 1, 3, 4, "R1401B", "#D97706");
        Add("113-2", "程式設計（二）", 2, 3, 4, "R1401B", "#0891B2");
        Add("113-2", "物理的科學", 3, 3, 4, "R2101", "#4F46E5");
        Add("113-2", "電子電路學", 4, 3, 4, "R70103", "#B45309");
        Add("113-2", "中文閱讀、思辨與表達（二）", 5, 3, 4, "R1115", "#DB2777");
        Add("113-2", "醫學工程概論（二）", 2, 6, 7, "R2008", "#9333EA");
        Add("113-2", "離散數學", 4, 6, 7, "R60104", "#DC2626");
        Add("113-2", "基礎程式設計－C++ 實習（二）", 5, 6, 7, "R1008", "#EA580C");
        Add("113-2", "網站程式設計實務", 3, 7, 9, "R1201A", "#2563EB");
        Add("113-2", "離散數學", 2, 8, 8, "R60104", "#DC2626");
        Add("113-2", "體育", 5, 8, 9, "R 體育場地", "#16A34A");
        Add("113-2", "程式設計（二）", 1, 9, 9, "R1401B", "#0891B2");
        Add("113-2", "微積分（二）", 2, 9, 9, "R1401B", "#D97706");

        // 114 年第 1 學期
        Add("114-1", "資料結構", 1, 2, 2, "R60104", "#0EA5E9");
        Add("114-1", "數位系統設計", 3, 2, 2, "R60312", "#7C3AED");
        Add("114-1", "數位系統實務", 5, 2, 4, "R1008", "#4F46E5");
        Add("114-1", "古典音樂欣賞", 1, 3, 4, "R5103", "#E11D48");
        Add("114-1", "醫用放射治療概論", 3, 3, 4, "R1102", "#0D9488");
        Add("114-1", "性別教育", 4, 3, 4, "R3104", "#DB2777");
        Add("114-1", "線性代數", 1, 6, 7, "R2115", "#D97706");
        Add("114-1", "線性代數", 4, 6, 6, "R60104", "#D97706");
        Add("114-1", "實用科技英文", 5, 6, 7, "R1213", "#059669");
        Add("114-1", "數位系統設計", 4, 7, 8, "R1401B", "#7C3AED");
        Add("114-1", "視窗程式設計", 1, 8, 10, "R1201A", "#2563EB");
        Add("114-1", "資料結構", 2, 8, 9, "R1102", "#0EA5E9");
        Add("114-1", "籃球", 5, 8, 9, "R 體育場地", "#16A34A");

        // 114 年第 2 學期
        Add("114-2", "演算法概論", 3, 2, 2, "R1401B", "#7C3AED");
        Add("114-2", "生命與身體活動", 1, 3, 4, "R70109", "#059669");
        Add("114-2", "作業系統概論", 3, 3, 4, "R60312", "#EA580C");
        Add("114-2", "演算法概論", 1, 6, 7, "R1401B", "#7C3AED");
        Add("114-2", "組合語言與計算機組織", 4, 6, 7, "R1501B", "#0F766E");
        Add("114-2", "機率與統計", 2, 7, 7, "R1501B", "#DC2626");
        Add("114-2", "視窗程式設計（二）", 3, 7, 9, "R1008", "#2563EB", "陳琨講師");
        Add("114-2", "作業系統概論", 2, 8, 8, "R70103", "#EA580C");
        Add("114-2", "機率與統計", 4, 8, 9, "R60104", "#DC2626");
        Add("114-2", "籃球", 5, 8, 9, "R 體育場地", "#16A34A");
        Add("114-2", "組合語言與計算機組織", 2, 9, 9, "R1102", "#0F766E");

        return entries;
    }
}
