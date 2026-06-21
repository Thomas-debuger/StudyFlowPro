namespace StudyFlowPro.Services;

public static class DocxPreviewService
{
    public static string ConvertToHtml(string docxPath)
    {
        using ZipArchive archive = ZipFile.OpenRead(docxPath);
        ZipArchiveEntry documentEntry = archive.GetEntry("word/document.xml")
            ?? throw new InvalidDataException("DOCX 中找不到 word/document.xml。");

        XDocument document;
        using (Stream stream = documentEntry.Open())
            document = XDocument.Load(stream);

        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        XNamespace a = "http://schemas.openxmlformats.org/drawingml/2006/main";
        XNamespace r = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        Dictionary<string, string> relationships = LoadRelationships(archive);
        NumberingContext numbering = NumberingContext.Load(archive, w);
        XElement? body = document.Root?.Element(w + "body");
        if (body == null)
            throw new InvalidDataException("DOCX 文件內容為空。");

        var html = new StringBuilder();
        html.Append("""
<!doctype html>
<html lang="zh-Hant">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<style>
:root{color-scheme:light;}
body{margin:0;background:#eef2f7;color:#0f172a;font-family:'Microsoft JhengHei UI','Noto Sans TC',sans-serif;line-height:1.75;}
.page{max-width:920px;margin:28px auto;background:white;min-height:1120px;padding:64px 72px;box-sizing:border-box;box-shadow:0 14px 40px rgba(15,23,42,.12);border:1px solid #dbe3ef;}
p{margin:0 0 12px;white-space:pre-wrap;}h1,h2,h3{line-height:1.35;margin:22px 0 12px;}h1{font-size:26px;}h2{font-size:22px;}h3{font-size:18px;}
.numbered{display:flex;align-items:flex-start;gap:8px;white-space:normal;}.numbered .num-marker{flex:0 0 auto;min-width:2.2em;text-align:right;font-weight:600;color:#0f172a;}.numbered .num-body{min-width:0;flex:1;white-space:pre-wrap;}
.level-1{padding-left:1.6em;}.level-2{padding-left:3.2em;}.level-3{padding-left:4.8em;}.level-4{padding-left:6.4em;}.level-5,.level-6,.level-7,.level-8{padding-left:8em;}
table{border-collapse:collapse;width:100%;margin:16px 0;}td,th{border:1px solid #94a3b8;padding:8px 10px;vertical-align:top;}td p:last-child{margin-bottom:0;}img{max-width:100%;height:auto;display:block;margin:18px auto;}
.note{background:#eff6ff;border-left:4px solid #2563eb;padding:10px 14px;color:#334155;margin-bottom:22px;font-size:14px;}
@media(max-width:800px){.page{margin:0;padding:28px 24px;box-shadow:none;border:0;}.level-1,.level-2,.level-3,.level-4,.level-5,.level-6,.level-7,.level-8{padding-left:0;}}
</style>
</head><body><main class="page"><div class="note">DOCX 由 StudyFlow Pro 轉為閱讀預覽；已支援 Word 自動題號與多層編號，複雜頁首頁尾或特殊排版仍可能與 Word 略有差異。</div>
""");

        foreach (XElement element in body.Elements())
        {
            if (element.Name == w + "p")
                html.Append(RenderParagraph(element, archive, relationships, numbering, w, a, r));
            else if (element.Name == w + "tbl")
                html.Append(RenderTable(element, archive, relationships, numbering, w, a, r));
        }

        html.Append("</main></body></html>");
        return html.ToString();
    }

    private static string RenderParagraph(
        XElement paragraph,
        ZipArchive archive,
        Dictionary<string, string> relationships,
        NumberingContext numbering,
        XNamespace w,
        XNamespace a,
        XNamespace r)
    {
        XElement? paragraphProperties = paragraph.Element(w + "pPr");
        string styleId = paragraphProperties?.Element(w + "pStyle")?.Attribute(w + "val")?.Value ?? string.Empty;
        string tag = styleId.Contains("Heading1", StringComparison.OrdinalIgnoreCase) ? "h1"
            : styleId.Contains("Heading2", StringComparison.OrdinalIgnoreCase) ? "h2"
            : styleId.Contains("Heading3", StringComparison.OrdinalIgnoreCase) ? "h3"
            : "p";

        var content = new StringBuilder();
        foreach (XElement run in paragraph.Descendants(w + "r"))
        {
            XElement? properties = run.Element(w + "rPr");
            bool bold = properties?.Element(w + "b") != null;
            bool italic = properties?.Element(w + "i") != null;
            bool underline = properties?.Element(w + "u") != null;

            var runText = new StringBuilder();
            foreach (XElement node in run.Elements())
            {
                if (node.Name == w + "t")
                    runText.Append(WebUtility.HtmlEncode(node.Value));
                else if (node.Name == w + "tab")
                    runText.Append("&emsp;");
                else if (node.Name == w + "br")
                    runText.Append("<br>");
                else if (node.Name == w + "noBreakHyphen")
                    runText.Append("‑");
            }

            string formatted = runText.ToString();
            if (underline && formatted.Length > 0) formatted = $"<u>{formatted}</u>";
            if (italic && formatted.Length > 0) formatted = $"<em>{formatted}</em>";
            if (bold && formatted.Length > 0) formatted = $"<strong>{formatted}</strong>";
            content.Append(formatted);

            foreach (XElement blip in run.Descendants(a + "blip"))
            {
                string? relationId = blip.Attribute(r + "embed")?.Value;
                string? imageHtml = RenderImage(archive, relationships, relationId);
                if (imageHtml != null)
                    content.Append(imageHtml);
            }
        }

        NumberMarker? marker = numbering.GetMarker(paragraph, w);
        string body = content.Length == 0 ? "&nbsp;" : content.ToString();
        if (marker == null)
            return $"<{tag}>{body}</{tag}>";

        string encodedMarker = WebUtility.HtmlEncode(marker.Value.Text);
        return $"<{tag} class=\"numbered level-{Math.Clamp(marker.Value.Level, 0, 8)}\"><span class=\"num-marker\">{encodedMarker}</span><span class=\"num-body\">{body}</span></{tag}>";
    }

    private static string RenderTable(
        XElement table,
        ZipArchive archive,
        Dictionary<string, string> relationships,
        NumberingContext numbering,
        XNamespace w,
        XNamespace a,
        XNamespace r)
    {
        var html = new StringBuilder("<table>");
        foreach (XElement row in table.Elements(w + "tr"))
        {
            html.Append("<tr>");
            foreach (XElement cell in row.Elements(w + "tc"))
            {
                html.Append("<td>");
                foreach (XElement child in cell.Elements())
                {
                    if (child.Name == w + "p")
                        html.Append(RenderParagraph(child, archive, relationships, numbering, w, a, r));
                    else if (child.Name == w + "tbl")
                        html.Append(RenderTable(child, archive, relationships, numbering, w, a, r));
                }
                html.Append("</td>");
            }
            html.Append("</tr>");
        }
        html.Append("</table>");
        return html.ToString();
    }

    private static Dictionary<string, string> LoadRelationships(ZipArchive archive)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        ZipArchiveEntry? entry = archive.GetEntry("word/_rels/document.xml.rels");
        if (entry == null)
            return result;

        XDocument relationships;
        using (Stream stream = entry.Open())
            relationships = XDocument.Load(stream);
        XNamespace rel = "http://schemas.openxmlformats.org/package/2006/relationships";
        foreach (XElement item in relationships.Descendants(rel + "Relationship"))
        {
            string? id = item.Attribute("Id")?.Value;
            string? target = item.Attribute("Target")?.Value;
            if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(target))
                result[id] = target.Replace('\\', '/');
        }
        return result;
    }

    private static string? RenderImage(
        ZipArchive archive,
        Dictionary<string, string> relationships,
        string? relationId)
    {
        if (string.IsNullOrWhiteSpace(relationId) || !relationships.TryGetValue(relationId, out string? target))
            return null;

        string normalized = target.StartsWith("/")
            ? target.TrimStart('/')
            : "word/" + target.TrimStart('.').TrimStart('/');
        normalized = normalized.Replace("word/../", string.Empty, StringComparison.OrdinalIgnoreCase);
        ZipArchiveEntry? image = archive.GetEntry(normalized);
        if (image == null)
            return null;

        using Stream stream = image.Open();
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        string mime = Path.GetExtension(image.FullName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            _ => "image/jpeg"
        };
        return $"<img alt=\"文件圖片\" src=\"data:{mime};base64,{Convert.ToBase64String(memory.ToArray())}\">";
    }

    private readonly record struct NumberMarker(string Text, int Level);

    private sealed class NumberingContext
    {
        private readonly Dictionary<(string NumId, int Level), LevelDefinition> _definitions = new();
        private readonly Dictionary<string, int[]> _counters = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, (string NumId, int Level)> _styleNumbering = new(StringComparer.OrdinalIgnoreCase);

        public static NumberingContext Load(ZipArchive archive, XNamespace w)
        {
            var context = new NumberingContext();
            ZipArchiveEntry? entry = archive.GetEntry("word/numbering.xml");
            if (entry == null)
                return context;

            XDocument numberingDocument;
            using (Stream stream = entry.Open())
                numberingDocument = XDocument.Load(stream);

            var abstractLevels = new Dictionary<(string AbstractId, int Level), LevelDefinition>();
            foreach (XElement abstractNumbering in numberingDocument.Descendants(w + "abstractNum"))
            {
                string? abstractId = abstractNumbering.Attribute(w + "abstractNumId")?.Value;
                if (string.IsNullOrWhiteSpace(abstractId))
                    continue;

                foreach (XElement levelElement in abstractNumbering.Elements(w + "lvl"))
                {
                    int level = ParseInt(levelElement.Attribute(w + "ilvl")?.Value, 0);
                    int start = ParseInt(levelElement.Element(w + "start")?.Attribute(w + "val")?.Value, 1);
                    string format = levelElement.Element(w + "numFmt")?.Attribute(w + "val")?.Value ?? "decimal";
                    string text = levelElement.Element(w + "lvlText")?.Attribute(w + "val")?.Value ?? $"%{level + 1}.";
                    string suffix = levelElement.Element(w + "suff")?.Attribute(w + "val")?.Value ?? "tab";
                    abstractLevels[(abstractId, level)] = new LevelDefinition(format, text, Math.Max(1, start), suffix);
                }
            }

            foreach (XElement numberInstance in numberingDocument.Descendants(w + "num"))
            {
                string? numId = numberInstance.Attribute(w + "numId")?.Value;
                string? abstractId = numberInstance.Element(w + "abstractNumId")?.Attribute(w + "val")?.Value;
                if (string.IsNullOrWhiteSpace(numId) || string.IsNullOrWhiteSpace(abstractId))
                    continue;

                var startOverrides = new Dictionary<int, int>();
                foreach (XElement levelOverride in numberInstance.Elements(w + "lvlOverride"))
                {
                    int level = ParseInt(levelOverride.Attribute(w + "ilvl")?.Value, 0);
                    string? overrideValue = levelOverride.Element(w + "startOverride")?.Attribute(w + "val")?.Value;
                    if (!string.IsNullOrWhiteSpace(overrideValue))
                        startOverrides[level] = Math.Max(1, ParseInt(overrideValue, 1));

                    XElement? replacementLevel = levelOverride.Element(w + "lvl");
                    if (replacementLevel != null)
                    {
                        int start = ParseInt(replacementLevel.Element(w + "start")?.Attribute(w + "val")?.Value, 1);
                        string format = replacementLevel.Element(w + "numFmt")?.Attribute(w + "val")?.Value ?? "decimal";
                        string text = replacementLevel.Element(w + "lvlText")?.Attribute(w + "val")?.Value ?? $"%{level + 1}.";
                        string suffix = replacementLevel.Element(w + "suff")?.Attribute(w + "val")?.Value ?? "tab";
                        context._definitions[(numId, level)] = new LevelDefinition(format, text, Math.Max(1, start), suffix);
                    }
                }

                for (int level = 0; level <= 8; level++)
                {
                    if (context._definitions.ContainsKey((numId, level)))
                        continue;
                    if (!abstractLevels.TryGetValue((abstractId, level), out LevelDefinition definition))
                        continue;
                    if (startOverrides.TryGetValue(level, out int startOverride))
                        definition = definition with { Start = startOverride };
                    context._definitions[(numId, level)] = definition;
                }
            }

            context.LoadStyleNumbering(archive, w);
            return context;
        }

        public NumberMarker? GetMarker(XElement paragraph, XNamespace w)
        {
            XElement? paragraphProperties = paragraph.Element(w + "pPr");
            XElement? numberProperties = paragraphProperties?.Element(w + "numPr");
            string? numId = numberProperties?.Element(w + "numId")?.Attribute(w + "val")?.Value;
            int? explicitLevel = TryParseNullableInt(numberProperties?.Element(w + "ilvl")?.Attribute(w + "val")?.Value);

            string? styleId = paragraphProperties?.Element(w + "pStyle")?.Attribute(w + "val")?.Value;
            if (!string.IsNullOrWhiteSpace(styleId) && _styleNumbering.TryGetValue(styleId, out var styleNumbering))
            {
                if (string.IsNullOrWhiteSpace(numId))
                    numId = styleNumbering.NumId;
                explicitLevel ??= styleNumbering.Level;
            }

            if (string.IsNullOrWhiteSpace(numId) || numId == "0")
                return null;

            int level = Math.Clamp(explicitLevel ?? 0, 0, 8);
            LevelDefinition definition = GetDefinition(numId, level);
            if (!_counters.TryGetValue(numId, out int[]? counters))
            {
                counters = new int[9];
                _counters[numId] = counters;
            }

            counters[level] = counters[level] <= 0 ? definition.Start : counters[level] + 1;
            for (int deeper = level + 1; deeper < counters.Length; deeper++)
                counters[deeper] = 0;
            for (int parent = 0; parent < level; parent++)
            {
                if (counters[parent] <= 0)
                    counters[parent] = GetDefinition(numId, parent).Start;
            }

            string marker;
            if (definition.Format.Equals("bullet", StringComparison.OrdinalIgnoreCase))
            {
                marker = string.IsNullOrWhiteSpace(definition.LevelText) ? "•" : definition.LevelText;
            }
            else
            {
                marker = Regex.Replace(definition.LevelText, "%([1-9])", match =>
                {
                    int referencedLevel = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) - 1;
                    int number = counters[Math.Clamp(referencedLevel, 0, 8)];
                    if (number <= 0)
                        number = GetDefinition(numId, referencedLevel).Start;
                    return FormatNumber(number, GetDefinition(numId, referencedLevel).Format);
                });
                if (string.IsNullOrWhiteSpace(marker))
                    marker = FormatNumber(counters[level], definition.Format) + ".";
            }

            marker = marker.Replace("\uF0B7", "•", StringComparison.Ordinal);
            if (definition.Suffix.Equals("space", StringComparison.OrdinalIgnoreCase))
                marker += " ";
            return new NumberMarker(marker, level);
        }


        private void LoadStyleNumbering(ZipArchive archive, XNamespace w)
        {
            ZipArchiveEntry? entry = archive.GetEntry("word/styles.xml");
            if (entry == null)
                return;

            XDocument stylesDocument;
            using (Stream stream = entry.Open())
                stylesDocument = XDocument.Load(stream);

            var direct = new Dictionary<string, (string? NumId, int? Level)>(StringComparer.OrdinalIgnoreCase);
            var basedOn = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (XElement style in stylesDocument.Descendants(w + "style"))
            {
                string? type = style.Attribute(w + "type")?.Value;
                string? styleId = style.Attribute(w + "styleId")?.Value;
                if (!string.Equals(type, "paragraph", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(styleId))
                    continue;

                XElement? numPr = style.Element(w + "pPr")?.Element(w + "numPr");
                string? numId = numPr?.Element(w + "numId")?.Attribute(w + "val")?.Value;
                int? level = TryParseNullableInt(numPr?.Element(w + "ilvl")?.Attribute(w + "val")?.Value);
                direct[styleId] = (numId, level);

                string? parent = style.Element(w + "basedOn")?.Attribute(w + "val")?.Value;
                if (!string.IsNullOrWhiteSpace(parent))
                    basedOn[styleId] = parent;
            }

            var resolved = new Dictionary<string, (string? NumId, int? Level)>(StringComparer.OrdinalIgnoreCase);
            (string? NumId, int? Level) Resolve(string styleId, HashSet<string> visiting)
            {
                if (resolved.TryGetValue(styleId, out var cached))
                    return cached;
                if (!visiting.Add(styleId))
                    return (null, null);

                direct.TryGetValue(styleId, out var own);
                (string? NumId, int? Level) parentValue = (null, null);
                if (basedOn.TryGetValue(styleId, out string? parentId))
                    parentValue = Resolve(parentId, visiting);

                visiting.Remove(styleId);
                var result = (own.NumId ?? parentValue.NumId, own.Level ?? parentValue.Level);
                resolved[styleId] = result;
                return result;
            }

            foreach (string styleId in direct.Keys)
            {
                var value = Resolve(styleId, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(value.NumId) && value.NumId != "0")
                    _styleNumbering[styleId] = (value.NumId, Math.Clamp(value.Level ?? 0, 0, 8));
            }
        }

        private LevelDefinition GetDefinition(string numId, int level)
        {
            if (_definitions.TryGetValue((numId, Math.Clamp(level, 0, 8)), out LevelDefinition definition))
                return definition;
            return new LevelDefinition("decimal", $"%{Math.Clamp(level, 0, 8) + 1}.", 1, "tab");
        }

        private static string FormatNumber(int value, string format)
        {
            value = Math.Max(1, value);
            return format.ToLowerInvariant() switch
            {
                "decimalzero" => value.ToString("00", CultureInfo.InvariantCulture),
                "lowerletter" => ToLetters(value).ToLowerInvariant(),
                "upperletter" => ToLetters(value).ToUpperInvariant(),
                "lowerroman" => ToRoman(value).ToLowerInvariant(),
                "upperroman" => ToRoman(value),
                "chinesecounting" or "chinesecountingthousand" or "ideographtraditional" => ToChineseNumber(value),
                _ => value.ToString(CultureInfo.InvariantCulture)
            };
        }

        private static string ToLetters(int value)
        {
            var result = new StringBuilder();
            while (value > 0)
            {
                value--;
                result.Insert(0, (char)('A' + value % 26));
                value /= 26;
            }
            return result.ToString();
        }

        private static string ToRoman(int value)
        {
            (int Number, string Symbol)[] values =
            {
                (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
                (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
                (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
            };
            var result = new StringBuilder();
            foreach ((int number, string symbol) in values)
            {
                while (value >= number)
                {
                    result.Append(symbol);
                    value -= number;
                }
            }
            return result.ToString();
        }

        private static string ToChineseNumber(int value)
        {
            if (value <= 10)
                return new[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" }[value];
            if (value < 20)
                return "十" + ToChineseNumber(value % 10);
            if (value < 100)
            {
                int tens = value / 10;
                int ones = value % 10;
                return ToChineseNumber(tens) + "十" + (ones == 0 ? string.Empty : ToChineseNumber(ones));
            }
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static int ParseInt(string? value, int fallback)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : fallback;

        private static int? TryParseNullableInt(string? value)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : null;

        private readonly record struct LevelDefinition(string Format, string LevelText, int Start, string Suffix);
    }
}
