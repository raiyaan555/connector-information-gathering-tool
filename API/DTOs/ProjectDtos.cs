using API.Models;

namespace API.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImplementationEngineer { get; set; }
    public string? Priority { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public int CompletionPercent { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FormToken { get; set; }
    public string? FormLink { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImplementationEngineer { get; set; }
    public string? Priority { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public string Status { get; set; } = "Draft";
}

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class GenerateLinkResponse
{
    public string Token { get; set; } = string.Empty;
    public string FormLink { get; set; } = string.Empty;
}
