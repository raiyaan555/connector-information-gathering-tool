using API.DTOs;
using API.Helpers;
using API.Models;
using API.Repositories;

namespace API.Services;

public interface IProjectService
{
    Task<ApiResponse<IEnumerable<ProjectDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ProjectDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProjectDto>> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProjectDto>> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MessageResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GenerateLinkResponse>> GenerateLinkAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProjectDto>> SetJiraTicketAsync(Guid id, string? ticket, CancellationToken cancellationToken = default);
}

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUser _currentUser;

    public ProjectService(
        IProjectRepository projectRepository,
        IClientRepository clientRepository,
        IConfiguration configuration,
        ICurrentUser currentUser)
    {
        _projectRepository = projectRepository;
        _clientRepository = clientRepository;
        _configuration = configuration;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<IEnumerable<ProjectDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var ownerId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
            return ApiResponse<IEnumerable<ProjectDto>>.Fail("You are not signed in.");

        var projects = await _projectRepository.GetAllAsync(ownerId, cancellationToken);
        return ApiResponse<IEnumerable<ProjectDto>>.Ok(projects.Select(MapToDto));
    }

    public async Task<ApiResponse<ProjectDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ownerId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
            return ApiResponse<ProjectDto>.Fail("You are not signed in.");

        var project = await _projectRepository.GetByIdAsync(id, ownerId, cancellationToken);
        if (project is null)
            return ApiResponse<ProjectDto>.Fail("Project not found.");

        return ApiResponse<ProjectDto>.Ok(MapToDto(project));
    }

    public async Task<ApiResponse<ProjectDto>> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var ownerId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
            return ApiResponse<ProjectDto>.Fail("You are not signed in.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<ProjectDto>.Fail("Project name is required.");

        Guid? clientId = null;
        if (!string.IsNullOrWhiteSpace(request.ClientName))
        {
            var clients = await _clientRepository.GetAllAsync(ownerId, cancellationToken);
            clientId = clients
                .FirstOrDefault(c => string.Equals(c.CompanyName, request.ClientName, StringComparison.OrdinalIgnoreCase))
                ?.Id;
        }

        var now = DateTime.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ClientName = request.ClientName,
            ClientId = clientId,
            ApplicationName = request.ApplicationName,
            Description = request.Description,
            ImplementationEngineer = request.ImplementationEngineer,
            Priority = request.Priority,
            ExpectedCompletionDate = request.ExpectedCompletionDate,
            CompletionPercent = 0,
            Status = ProjectMapper.ParseStatus(request.Status),
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = _currentUser.Email ?? request.ImplementationEngineer ?? ownerId,
            OwnerUserId = ownerId
        };

        await _projectRepository.AddAsync(project, cancellationToken);
        return ApiResponse<ProjectDto>.Ok(MapToDto(project), "Project created successfully.");
    }

    public async Task<ApiResponse<ProjectDto>> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var ownerId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
            return ApiResponse<ProjectDto>.Fail("You are not signed in.");

        var project = await _projectRepository.GetByIdAsync(id, ownerId, cancellationToken);
        if (project is null)
            return ApiResponse<ProjectDto>.Fail("Project not found.");

        project.Name = request.Name;
        project.ClientName = request.ClientName;
        project.ApplicationName = request.ApplicationName;
        project.Status = ProjectMapper.ParseStatus(request.Status);

        if (!string.IsNullOrWhiteSpace(request.ClientName))
        {
            var clients = await _clientRepository.GetAllAsync(ownerId, cancellationToken);
            project.ClientId = clients
                .FirstOrDefault(c => string.Equals(c.CompanyName, request.ClientName, StringComparison.OrdinalIgnoreCase))
                ?.Id;
        }

        await _projectRepository.UpdateAsync(project, cancellationToken);
        return ApiResponse<ProjectDto>.Ok(MapToDto(project), "Project updated successfully.");
    }

    public async Task<ApiResponse<MessageResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ownerId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
            return ApiResponse<MessageResponse>.Fail("You are not signed in.");

        if (!await _projectRepository.DeleteAsync(id, ownerId, cancellationToken))
            return ApiResponse<MessageResponse>.Fail("Project not found.");

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Project deleted successfully." },
            "Project deleted successfully.");
    }

    public async Task<ApiResponse<GenerateLinkResponse>> GenerateLinkAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ownerId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
            return ApiResponse<GenerateLinkResponse>.Fail("You are not signed in.");

        var project = await _projectRepository.GetByIdAsync(id, ownerId, cancellationToken);
        if (project is null)
            return ApiResponse<GenerateLinkResponse>.Fail("Project not found.");

        var frontendBase = _configuration["App:FrontendBaseUrl"] ?? "http://localhost:4200";
        var token = TokenGenerator.GenerateToken();
        project.FormToken = token;
        project.FormLink = $"{frontendBase.TrimEnd('/')}/form/{token}";
        await _projectRepository.UpdateAsync(project, cancellationToken);

        return ApiResponse<GenerateLinkResponse>.Ok(new GenerateLinkResponse
        {
            Token = token,
            FormLink = project.FormLink
        }, "Customer form link generated successfully.");
    }

    public async Task<ApiResponse<ProjectDto>> SetJiraTicketAsync(Guid id, string? ticket, CancellationToken cancellationToken = default)
    {
        var ownerId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
            return ApiResponse<ProjectDto>.Fail("You are not signed in.");

        var project = await _projectRepository.GetByIdAsync(id, ownerId, cancellationToken);
        if (project is null)
            return ApiResponse<ProjectDto>.Fail("Project not found.");

        if (!JiraTicketRules.TryNormalize(ticket, out var normalized, out var error))
            return ApiResponse<ProjectDto>.Fail(error ?? "Enter a Jira issue key such as ABC-123, or a Jira link.");

        project.JiraTicket = normalized;
        await _projectRepository.UpdateAsync(project, cancellationToken);
        var message = normalized is null
            ? "Jira ticket removed from this application."
            : "Jira ticket saved on this application.";
        return ApiResponse<ProjectDto>.Ok(MapToDto(project), message);
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
        JiraTicket = project.JiraTicket,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt,
        CreatedBy = project.CreatedBy
    };
}
