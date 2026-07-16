namespace API.Models;

/// <summary>
/// Submitted customer requirement form for a project (tokenized public form).
/// </summary>
public class CustomerForm
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Token { get; set; } = string.Empty;
    public Dictionary<string, string> FormData { get; set; } = new();
    public DateTime SubmittedAt { get; set; }

    public Project? Project { get; set; }
}
