namespace API.Models;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImplementationEngineer { get; set; }
    public string? Priority { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public int CompletionPercent { get; set; }
    public ProjectStatus Status { get; set; }
    public string? FormToken { get; set; }
    public string? FormLink { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public Client? Client { get; set; }
    public ICollection<CustomerForm> CustomerForms { get; set; } = new List<CustomerForm>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ProjectDocument? ProjectDocument { get; set; }
}
