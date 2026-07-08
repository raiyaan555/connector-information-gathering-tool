namespace API.Models;

public class CustomerFormResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Token { get; set; } = string.Empty;
    public Dictionary<string, string> FormData { get; set; } = new();
    public DateTime SubmittedAt { get; set; }
}
