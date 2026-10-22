using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DrawingBlip = DocumentFormat.OpenXml.Drawing.Blip;

namespace API.Services;

/// <summary>
/// Renders an uploaded DOCX into PDF pages so it can be appended to the generated connector PDF.
/// </summary>
public static class DocxPdfConverter
{
    private const string BrandRed = "#E30613";
    private const string BrandRedDark = "#B80510";

    static DocxPdfConverter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[]? ToPdf(string absolutePath, string title)
    {
        var blocks = ReadBlocks(absolutePath);
        if (blocks.Count == 0)
            return null;

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(style => style.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Column(header =>
                {
                    header.Item().Text(title).Bold().FontSize(13).FontColor(BrandRed);
                    header.Item().PaddingTop(4).LineHorizontal(1).LineColor(BrandRed);
                });

                page.Content().PaddingVertical(12).Column(column =>
                {
                    column.Spacing(8);
                    foreach (var block in blocks)
                    {
                        switch (block.Kind)
                        {
                            case DocxBlockKind.Heading:
                                column.Item().PaddingTop(4).Text(block.Text ?? string.Empty)
                                    .Bold().FontSize(12).FontColor(BrandRedDark);
                                break;
                            case DocxBlockKind.Paragraph:
                                column.Item().Text(block.Text ?? string.Empty).FontSize(10);
                                break;
                            case DocxBlockKind.Table:
                                AppendTable(column, block.Rows);
                                break;
                            case DocxBlockKind.Image:
                                if (block.ImageBytes is { Length: > 0 })
                                    column.Item().Image(block.ImageBytes).FitWidth();
                                break;
                        }
                    }
                });

                page.Footer().AlignCenter().Text(footer =>
                {
                    footer.Span(title).FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void AppendTable(ColumnDescriptor column, IReadOnlyList<string[]>? rows)
    {
        if (rows is null || rows.Count == 0)
            return;

        var columnCount = rows.Max(row => row.Length);
        if (columnCount == 0)
            return;

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                for (var i = 0; i < columnCount; i++)
                    columns.RelativeColumn();
            });

            foreach (var row in rows)
            {
                for (var i = 0; i < columnCount; i++)
                {
                    var value = i < row.Length ? row[i] : string.Empty;
                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(value).FontSize(9);
                }
            }
        });
    }

    private static List<DocxBlock> ReadBlocks(string absolutePath)
    {
        var blocks = new List<DocxBlock>();
        using var word = WordprocessingDocument.Open(absolutePath, false);
        var main = word.MainDocumentPart;
        var body = main?.Document?.Body;
        if (main is null || body is null)
            return blocks;

        Walk(body, main, blocks);
        return blocks;
    }

    private static void Walk(OpenXmlElement element, MainDocumentPart main, List<DocxBlock> blocks)
    {
        foreach (var child in element.ChildElements)
        {
            switch (child)
            {
                case Paragraph paragraph:
                    AddParagraph(paragraph, main, blocks);
                    break;
                case Table table:
                    AddTable(table, main, blocks);
                    break;
                default:
                    if (child.HasChildren)
                        Walk(child, main, blocks);
                    break;
            }
        }
    }

    private static void AddParagraph(Paragraph paragraph, MainDocumentPart main, List<DocxBlock> blocks)
    {
        var text = paragraph.InnerText.Trim();
        if (text.Length > 0)
        {
            var style = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value ?? string.Empty;
            var heading = style.StartsWith("Heading", StringComparison.OrdinalIgnoreCase);
            blocks.Add(heading ? DocxBlock.Heading(text) : DocxBlock.Paragraph(text));
        }

        foreach (var image in ReadImages(paragraph, main))
            blocks.Add(DocxBlock.Picture(image));
    }

    private static void AddTable(Table table, MainDocumentPart main, List<DocxBlock> blocks)
    {
        var rows = new List<string[]>();
        foreach (var row in table.Elements<TableRow>())
        {
            var cells = row.Elements<TableCell>().Select(cell => cell.InnerText.Trim()).ToArray();
            if (cells.Any(cell => cell.Length > 0))
                rows.Add(cells);
        }

        if (rows.Count > 0)
            blocks.Add(DocxBlock.Table(rows));

        foreach (var image in ReadImages(table, main))
            blocks.Add(DocxBlock.Picture(image));
    }

    private static IEnumerable<byte[]> ReadImages(OpenXmlElement element, MainDocumentPart main)
    {
        foreach (var blip in element.Descendants<DrawingBlip>())
        {
            var id = blip.Embed?.Value;
            if (string.IsNullOrEmpty(id))
                continue;
            if (main.GetPartById(id) is not ImagePart part)
                continue;
            if (!IsPngOrJpeg(part.ContentType))
                continue;

            using var stream = part.GetStream();
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var bytes = buffer.ToArray();
            if (bytes.Length > 0 && IsPngOrJpegBytes(bytes))
                yield return bytes;
        }
    }

    private static bool IsPngOrJpeg(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;
        var type = contentType.ToLowerInvariant();
        return type.Contains("png") || type.Contains("jpeg") || type.Contains("jpg");
    }

    private static bool IsPngOrJpegBytes(byte[] bytes)
    {
        if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return true;
        return bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
    }

    private enum DocxBlockKind { Heading, Paragraph, Table, Image }

    private sealed record DocxBlock(DocxBlockKind Kind, string? Text, IReadOnlyList<string[]>? Rows, byte[]? ImageBytes)
    {
        public static DocxBlock Heading(string text) => new(DocxBlockKind.Heading, text, null, null);
        public static DocxBlock Paragraph(string text) => new(DocxBlockKind.Paragraph, text, null, null);
        public static DocxBlock Table(IReadOnlyList<string[]> rows) => new(DocxBlockKind.Table, null, rows, null);
        public static DocxBlock Picture(byte[] image) => new(DocxBlockKind.Image, null, null, image);
    }
}
