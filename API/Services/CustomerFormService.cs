using API.DTOs;
using API.Models;
using API.Repositories;

namespace API.Services;

public interface ICustomerFormService
{
    Task<ApiResponse<CustomerFormDto>> GetFormByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerFormResponseDto>> SubmitFormAsync(string token, SubmitCustomerFormRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<CustomerFormResponseDto>>> GetResponsesByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
}

public class CustomerFormService : ICustomerFormService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICustomerFormRepository _customerFormRepository;

    public CustomerFormService(
        IProjectRepository projectRepository,
        ICustomerFormRepository customerFormRepository)
    {
        _projectRepository = projectRepository;
        _customerFormRepository = customerFormRepository;
    }

    public async Task<ApiResponse<CustomerFormDto>> GetFormByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByTokenAsync(token, cancellationToken);
        if (project is null)
            return ApiResponse<CustomerFormDto>.Fail("Invalid or expired form token.");

        var existingResponse = await _customerFormRepository.GetByTokenAsync(token, cancellationToken);

        return ApiResponse<CustomerFormDto>.Ok(new CustomerFormDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            ClientName = project.ClientName,
            ApplicationName = project.ApplicationName,
            Token = token,
            IsSubmitted = existingResponse is not null
        });
    }

    public async Task<ApiResponse<CustomerFormResponseDto>> SubmitFormAsync(
        string token,
        SubmitCustomerFormRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByTokenAsync(token, cancellationToken);
        if (project is null)
            return ApiResponse<CustomerFormResponseDto>.Fail("Invalid or expired form token.");

        if (request.FormData is null || request.FormData.Count == 0)
            return ApiResponse<CustomerFormResponseDto>.Fail("Form data is required.");

        var existingResponse = await _customerFormRepository.GetByTokenAsync(token, cancellationToken);
        if (existingResponse is not null)
            return ApiResponse<CustomerFormResponseDto>.Fail("This form has already been submitted.");

        var response = new CustomerForm
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Token = token,
            FormData = request.FormData,
            SubmittedAt = DateTime.UtcNow
        };

        await _customerFormRepository.AddAsync(response, cancellationToken);

        return ApiResponse<CustomerFormResponseDto>.Ok(
            MapToDto(response),
            "Form submitted successfully.");
    }

    public async Task<ApiResponse<IEnumerable<CustomerFormResponseDto>>> GetResponsesByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return ApiResponse<IEnumerable<CustomerFormResponseDto>>.Fail("Project not found.");

        var responses = await _customerFormRepository.GetByProjectIdAsync(projectId, cancellationToken);
        return ApiResponse<IEnumerable<CustomerFormResponseDto>>.Ok(responses.Select(MapToDto));
    }

    private static CustomerFormResponseDto MapToDto(CustomerForm response) => new()
    {
        Id = response.Id,
        ProjectId = response.ProjectId,
        Token = response.Token,
        FormData = response.FormData,
        SubmittedAt = response.SubmittedAt
    };
}
