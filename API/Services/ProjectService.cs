using API.DTOs;
using API.Helpers;
using API.Models;
using API.Repositories;

namespace API.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public ApiResponse<IEnumerable<ProjectDto>> GetAll()
    {
        var projects = _projectRepository.GetAll().Select(MapToDto);
        return ApiResponse<IEnumerable<ProjectDto>>.Ok(projects);
    }

    public ApiResponse<ProjectDto> GetById(Guid id)
    {
        var project = _projectRepository.GetById(id);
        if (project is null)
            return ApiResponse<ProjectDto>.Fail("Project not found.");

        return ApiResponse<ProjectDto>.Ok(MapToDto(project));
    }

    public ApiResponse<ProjectDto> Create(CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<ProjectDto>.Fail("Project name is required.");

        var now = DateTime.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ClientName = request.ClientName,
            ApplicationName = request.ApplicationName,
            Description = request.Description,
            ImplementationEngineer = request.ImplementationEngineer,
            Priority = request.Priority,
            ExpectedCompletionDate = request.ExpectedCompletionDate,
            CompletionPercent = 0,
            Status = ProjectMapper.ParseStatus(request.Status),
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = request.ImplementationEngineer ?? "admin@theconnector.com"
        };

        _projectRepository.Add(project);
        return ApiResponse<ProjectDto>.Ok(MapToDto(project), "Project created successfully.");
    }

    public ApiResponse<ProjectDto> Update(Guid id, UpdateProjectRequest request)
    {
        var project = _projectRepository.GetById(id);
        if (project is null)
            return ApiResponse<ProjectDto>.Fail("Project not found.");

        project.Name = request.Name;
        project.ClientName = request.ClientName;
        project.ApplicationName = request.ApplicationName;
        project.Status = ProjectMapper.ParseStatus(request.Status);
        _projectRepository.Update(project);

        return ApiResponse<ProjectDto>.Ok(MapToDto(project), "Project updated successfully.");
    }

    public ApiResponse<MessageResponse> Delete(Guid id)
    {
        if (!_projectRepository.Delete(id))
            return ApiResponse<MessageResponse>.Fail("Project not found.");

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Project deleted successfully." },
            "Project deleted successfully.");
    }

    public ApiResponse<GenerateLinkResponse> GenerateLink(Guid id)
    {
        var project = _projectRepository.GetById(id);
        if (project is null)
            return ApiResponse<GenerateLinkResponse>.Fail("Project not found.");

        var token = TokenGenerator.GenerateToken();
        project.FormToken = token;
        project.FormLink = $"http://localhost:4200/form/{token}";
        _projectRepository.Update(project);

        return ApiResponse<GenerateLinkResponse>.Ok(new GenerateLinkResponse
        {
            Token = token,
            FormLink = project.FormLink
        }, "Customer form link generated successfully.");
    }

    private static ProjectDto MapToDto(Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        ClientName = project.ClientName,
        ApplicationName = project.ApplicationName,
        Description = project.Description,
        ImplementationEngineer = project.ImplementationEngineer,
        Priority = project.Priority,
        ExpectedCompletionDate = project.ExpectedCompletionDate,
        CompletionPercent = project.CompletionPercent,
        Status = ProjectMapper.ToStatusString(project.Status),
        FormToken = project.FormToken,
        FormLink = project.FormLink,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt,
        CreatedBy = project.CreatedBy
    };
}
