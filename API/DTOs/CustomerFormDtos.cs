namespace API.DTOs;

public class CustomerFormDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool IsSubmitted { get; set; }
}

public class SubmitCustomerFormRequest
{
    public Dictionary<string, string> FormData { get; set; } = new();
}

public class CustomerFormResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Token { get; set; } = string.Empty;
    public Dictionary<string, string> FormData { get; set; } = new();
    public DateTime SubmittedAt { get; set; }
}
