using API.DTOs;
using API.Models;

namespace API.Helpers;

public static class ProjectMapper
{
    public static ProjectDto ToDto(Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        ClientName = project.ClientName,
        ApplicationName = project.ApplicationName,
        Status = ToStatusString(project.Status),
        FormToken = project.FormToken,
        FormLink = project.FormLink,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt,
        CreatedBy = project.CreatedBy
    };

    public static string ToStatusString(ProjectStatus status) => status switch
    {
        ProjectStatus.Draft => "Draft",
        ProjectStatus.InProgress => "In Progress",
        ProjectStatus.Completed => "Completed",
        ProjectStatus.PendingReview => "Pending Review",
        _ => status.ToString()
    };

    public static ProjectStatus ParseStatus(string status) => status switch
    {
        "Draft" => ProjectStatus.Draft,
        "In Progress" => ProjectStatus.InProgress,
        "Completed" => ProjectStatus.Completed,
        "Pending Review" => ProjectStatus.PendingReview,
        _ => ProjectStatus.Draft
    };
}
