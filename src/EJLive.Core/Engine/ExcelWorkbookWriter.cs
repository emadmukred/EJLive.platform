using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace EJLive.Core.Engine;

/// <summary>
/// Minimal, dependency-free OOXML (<c>.xlsx</c>) writer — one bold header row plus data
/// rows per sheet, inline strings, sane column widths. Excel export is a standing operator
/// requirement (Analysis Log Studio, SS-10.5) while the platform bans Office interop, web
/// libraries and new NuGet surface; a SpreadsheetML package written directly with
/// <see cref="ZipArchive"/> satisfies both. Files produced here open in Excel 2016+, LibreOffice
/// and Calc without repair prompts (validated by the Excel round-trip test).
/// </summary>
public static class ExcelWorkbookWriter
{
    private const string MainNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    public static void Write(string path, ExcelSheet sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        Write(path, new[] { sheet });
    }

    public static void Write(string path, IReadOnlyList<ExcelSheet> sheets)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (sheets is null || sheets.Count == 0)
            throw new ArgumentException("At least one sheet is required.", nameof(sheets));

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        // Atomic publish: write beside the target, then replace, so a failed export never
        // overwrites the previous good file.
        var tempPath = path + ".tmp";
        if (File.Exists(tempPath))
            File.Delete(tempPath);

        using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            WriteEntry(zip, "[Content_Types].xml", BuildContentTypes(sheets.Count));
            WriteEntry(zip, "_rels/.rels", RootRels);
            WriteEntry(zip, "docProps/app.xml", AppProps);
            WriteEntry(zip, "xl/workbook.xml", BuildWorkbook(sheets));
            WriteEntry(zip, "xl/_rels/workbook.xml.rels", BuildWorkbookRels(sheets.Count));
            WriteEntry(zip, "xl/styles.xml", Styles);
            for (var i = 0; i < sheets.Count; i++)
                WriteEntry(zip, $"xl/worksheets/sheet{i + 1}.xml", BuildSheet(sheets[i], i + 1));
        }

        if (File.Exists(path))
            File.Delete(path);
        File.Move(tempPath, path, overwrite: true);
    }

    // ── parts ────────────────────────────────────────────────────────────────
    private const string RootRels =
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
        <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
        <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties" Target="docProps/app.xml"/>
        </Relationships>
        """;

    private const string AppProps =
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties">
        <Application>EJLive ReportingExport</Application>
        </Properties>
        """;

    private const string Styles =
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
        <fonts count="2"><font><sz val="10"/><name val="Calibri"/></font><font><b/><sz val="10"/><name val="Calibri"/></font></fonts>
        <fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills>
        <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
        <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
        <cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/></cellXfs>
        <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
        </styleSheet>
        """;

    private static string BuildContentTypes(int sheetCount)
    {
        var sb = new StringBuilder();
        sb.Append("""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
            <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
            <Default Extension="xml" ContentType="application/xml"/>
            <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
            <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
            """);
        for (var i = 1; i <= sheetCount; i++)
            sb.Append($"<Override PartName=\"/xl/worksheets/sheet{i}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
        sb.Append("<Override PartName=\"/docProps/app.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.extended-properties+xml\"/>");
        sb.Append("</Types>");
        return sb.ToString();
    }

    private static string BuildWorkbook(IReadOnlyList<ExcelSheet> sheets)
    {
        var sb = new StringBuilder();
        sb.Append($"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"{MainNs}\" xmlns:r=\"{RelNs}\"><sheets>");
        for (var i = 0; i < sheets.Count; i++)
            sb.Append($"<sheet name=\"{XmlEscape(SanitizeSheetName(sheets[i].Name, i))}\" sheetId=\"{i + 1}\" r:id=\"rId{i + 1}\"/>");
        sb.Append("</sheets></workbook>");
        return sb.ToString();
    }

    private static string BuildWorkbookRels(int sheetCount)
    {
        var sb = new StringBuilder();
        sb.Append("""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
            """);
        for (var i = 1; i <= sheetCount; i++)
            sb.Append($"<Relationship Id=\"rId{i}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{i}.xml\"/>");
        sb.Append("<Relationship Id=\"rId")
          .Append(sheetCount + 1)
          .Append("\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>");
        sb.Append("</Relationships>");
        return sb.ToString();
    }

    private static string BuildSheet(ExcelSheet sheet, int sheetIndex)
    {
        var headers = sheet.Headers ?? Array.Empty<string>();
        var columns = headers.Length;
        foreach (var row in sheet.Rows ?? Array.Empty<IReadOnlyList<string?>>())
            columns = Math.Max(columns, row.Count);

        var sb = new StringBuilder();
        sb.Append($"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"{MainNs}\">");

        if (columns > 0)
        {
            sb.Append("<cols>");
            for (var c = 1; c <= columns; c++)
                sb.Append("<col min=\"").Append(c).Append("\" max=\"").Append(c).Append("\" width=\"").Append(ColumnWidthFor(headers, c - 1)).Append("\" customWidth=\"1\"/>");
            sb.Append("</cols>");
        }

        sb.Append("<sheetData>");
        var rowIndex = 1;
        if (headers.Length > 0)
        {
            var headerRow = rowIndex++;
            sb.Append("<row r=\"").Append(headerRow).Append("\">");
            for (var c = 0; c < headers.Length; c++)
                AppendCell(sb, c, headerRow, headers[c], bold: true);
            sb.Append("</row>");
        }

        foreach (var row in sheet.Rows ?? Array.Empty<IReadOnlyList<string?>>())
        {
            sb.Append("<row r=\"").Append(rowIndex).Append("\">");
            for (var c = 0; c < row.Count; c++)
                AppendCell(sb, c, rowIndex, row[c], bold: false);
            sb.Append("</row>");
            rowIndex++;
        }
        sb.Append("</sheetData></worksheet>");
        return sb.ToString();
    }

    private static void AppendCell(StringBuilder sb, int columnIndex, int rowIndex, string? value, bool bold)
    {
        var reference = $"{ColumnName(columnIndex)}{rowIndex}";
        var style = bold ? " s=\"1\"" : string.Empty;
        if (string.IsNullOrEmpty(value))
        {
            sb.Append(bold ? $"<c r=\"{reference}\"{style}/>" : string.Empty);
            return;
        }

        if (!bold && IsPlainNumber(value))
        {
            sb.Append($"<c r=\"{reference}\"><v>{value}</v></c>");
            return;
        }

        sb.Append($"<c r=\"{reference}\" t=\"inlineStr\"{style}><is><t xml:space=\"preserve\">{XmlEscape(value)}</t></is></c>");
    }

    private static int ColumnWidthFor(IReadOnlyList<string> headers, int index)
    {
        if (index < 0 || index >= headers.Count)
            return 16;
        return Math.Clamp(headers[index].Length + 4, 10, 42);
    }

    /// <summary>1 → A, 26 → AA style column names (spreadsheet references).</summary>
    public static string ColumnName(int index)
    {
        var column = index + 1;
        var name = new StringBuilder();
        while (column > 0)
        {
            var rem = (column - 1) % 26;
            name.Insert(0, (char)('A' + rem));
            column = (column - 1) / 26;
        }
        return name.ToString();
    }

    private static bool IsPlainNumber(string value)
    {
        if (value.Length is 0 or > 15)
            return false;
        var start = value[0] == '-' ? 1 : 0;
        if (start >= value.Length)
            return false;
        var digits = 0;
        var seenDot = false;
        for (var i = start; i < value.Length; i++)
        {
            var ch = value[i];
            if (ch == '.')
            {
                if (seenDot)
                    return false;
                seenDot = true;
                continue;
            }
            if (!char.IsAsciiDigit(ch))
                return false;
            digits++;
        }
        return digits > 0 && char.IsAsciiDigit(value[^1]);
    }

    /// <summary>Excel forbids : \ / ? * [ ] in sheet names and caps them at 31 characters.</summary>
    public static string SanitizeSheetName(string? name, int fallbackIndex)
    {
        var raw = string.IsNullOrWhiteSpace(name) ? $"Sheet{fallbackIndex}" : name.Trim();
        var sb = new StringBuilder(raw.Length);
        foreach (var ch in raw)
            sb.Append(ch is ':' or '\\' or '/' or '?' or '*' or '[' or ']' ? ' ' : ch);
        var cleaned = sb.ToString();
        return cleaned.Length <= 31 ? cleaned : cleaned[..31];
    }

    public static string XmlEscape(string value) => value
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;");

    private static void WriteEntry(ZipArchive zip, string name, string content)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(content);
    }
}

/// <summary>
/// One worksheet: header labels + rows of stringified cells. Numbers may be passed as text —
/// the writer detects plain numerals and emits them as numeric cells so Excel sums them.
/// </summary>
public sealed class ExcelSheet
{
    public ExcelSheet(string name, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string?>> rows)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Headers = headers ?? Array.Empty<string>();
        Rows = rows ?? Array.Empty<IReadOnlyList<string?>>();
    }

    public string Name { get; }
    public IReadOnlyList<string> Headers { get; }
    public IReadOnlyList<IReadOnlyList<string?>> Rows { get; }
}
