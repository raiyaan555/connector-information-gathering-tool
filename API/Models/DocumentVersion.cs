namespace API.Models;

public class DocumentVersion
{
    public Guid Id { get; set; }
    public Guid ProjectDocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string SaveType { get; set; } = "Official";
    public string FormDataJson { get; set; } = "{}";
    public string AttachmentsJson { get; set; } = "[]";
    public int CompletionPercent { get; set; }
    public string? GeneratedDocumentsJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public ProjectDocument? ProjectDocument { get; set; }
}
