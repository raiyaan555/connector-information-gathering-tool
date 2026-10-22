namespace API.DTOs;

public class GeneratePdfRequest
{
    public Dictionary<string, string> FormData { get; set; } = new();
}

public class ShareComposeResponse
{
    public string To { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string AttachmentFileName { get; set; } = string.Empty;
}

public class PdfSectionDto
{
    public string Title { get; set; } = string.Empty;
    public List<PdfFieldDto> Fields { get; set; } = new();
}

public class PdfFieldDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
