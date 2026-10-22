using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;

namespace API.Helpers;

public static class AttachmentFileRules
{
    public const string RejectionMessage =
        "Only PDF, DOCX, JPG, PNG, and JSON files are allowed.";

    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".jpg", ".jpeg", ".png", ".json"
    };

    private static readonly Dictionary<string, string[]> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ["application/pdf"],
        [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/zip"],
        [".jpg"] = ["image/jpeg", "image/jpg", "image/pjpeg"],
        [".jpeg"] = ["image/jpeg", "image/jpg", "image/pjpeg"],
        [".png"] = ["image/png", "image/x-png"],
        [".json"] = ["application/json", "text/json", "application/ld+json", "text/plain"],
    };

    public static bool IsAllowedExtension(string? fileName)
    {
        var extension = ExtensionOf(fileName);
        return extension.Length > 0 && Extensions.Contains(extension);
    }

    public static bool TryValidate(
        string fileName,
        string? contentType,
        Stream content,
        out string canonicalContentType,
        out string error)
    {
        canonicalContentType = "application/octet-stream";
        error = RejectionMessage;

        var safeName = Path.GetFileName(fileName);
        var extension = ExtensionOf(safeName);
        if (!Extensions.Contains(extension))
            return false;

        var mediaType = MediaType(contentType);
        if (mediaType.Length > 0
            && !mediaType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase)
            && !ContentTypes[extension].Contains(mediaType, StringComparer.OrdinalIgnoreCase))
        {
            error = RejectionMessage;
            return false;
        }

        if (!content.CanSeek || !MatchesSignature(extension, content))
        {
            error = SignatureMessage(extension);
            return false;
        }

        canonicalContentType = CanonicalContentType(extension);
        content.Position = 0;
        error = string.Empty;
        return true;
    }

    public static string CanonicalContentType(string fileNameOrExtension)
    {
        var extension = fileNameOrExtension.StartsWith('.')
            ? fileNameOrExtension
            : ExtensionOf(fileNameOrExtension);

        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }

    private static bool MatchesSignature(string extension, Stream content)
    {
        content.Position = 0;
        try
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => HasPrefix(content, "%PDF"u8),
                ".png" => HasPrefix(content, new byte[] { 0x89, 0x50, 0x4E, 0x47 }),
                ".jpg" or ".jpeg" => HasPrefix(content, new byte[] { 0xFF, 0xD8, 0xFF }),
                ".docx" => IsDocxPackage(content),
                ".json" => IsJson(content),
                _ => false
            };
        }
        finally
        {
            content.Position = 0;
        }
    }

    private static bool HasPrefix(Stream content, ReadOnlySpan<byte> prefix)
    {
        content.Position = 0;
        var buffer = new byte[prefix.Length];
        var read = content.Read(buffer, 0, buffer.Length);
        content.Position = 0;
        return read == prefix.Length && buffer.AsSpan().SequenceEqual(prefix);
    }

    private static bool IsDocxPackage(Stream content)
    {
        if (!HasPrefix(content, new byte[] { 0x50, 0x4B, 0x03, 0x04 })
            && !HasPrefix(content, new byte[] { 0x50, 0x4B, 0x05, 0x06 })
            && !HasPrefix(content, new byte[] { 0x50, 0x4B, 0x07, 0x08 }))
            return false;

        content.Position = 0;
        using var copy = new MemoryStream();
        content.CopyTo(copy);
        content.Position = 0;
        copy.Position = 0;

        try
        {
            using var word = WordprocessingDocument.Open(copy, false);
            return word.MainDocumentPart is not null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsJson(Stream content)
    {
        content.Position = 0;
        try
        {
            using var document = JsonDocument.Parse(content, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 128
            });
            return document.RootElement.ValueKind != JsonValueKind.Undefined;
        }
        catch (JsonException)
        {
            return false;
        }
        finally
        {
            content.Position = 0;
        }
    }

    private static string ExtensionOf(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;
        return Path.GetExtension(Path.GetFileName(fileName.Trim()));
    }

    private static string MediaType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return string.Empty;
        var semi = contentType.IndexOf(';');
        return (semi >= 0 ? contentType[..semi] : contentType).Trim();
    }

    private static string SignatureMessage(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => "This file is not a valid PDF.",
        ".docx" => "This file is not a valid DOCX document.",
        ".jpg" or ".jpeg" => "This file is not a valid JPG image.",
        ".png" => "This file is not a valid PNG image.",
        ".json" => "This file is not valid JSON.",
        _ => RejectionMessage
    };
}
