using API.DTOs;
using API.Models;
using API.Repositories;

namespace API.Services;

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

    public ApiResponse<CustomerFormDto> GetFormByToken(string token)
    {
        var project = _projectRepository.GetByToken(token);
        if (project is null)
            return ApiResponse<CustomerFormDto>.Fail("Invalid or expired form token.");

        var existingResponse = _customerFormRepository.GetByToken(token);

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

    public ApiResponse<CustomerFormResponseDto> SubmitForm(string token, SubmitCustomerFormRequest request)
    {
        var project = _projectRepository.GetByToken(token);
        if (project is null)
            return ApiResponse<CustomerFormResponseDto>.Fail("Invalid or expired form token.");

        if (request.FormData is null || request.FormData.Count == 0)
            return ApiResponse<CustomerFormResponseDto>.Fail("Form data is required.");

        var existingResponse = _customerFormRepository.GetByToken(token);
        if (existingResponse is not null)
            return ApiResponse<CustomerFormResponseDto>.Fail("This form has already been submitted.");

        var response = new CustomerFormResponse
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Token = token,
            FormData = request.FormData,
            SubmittedAt = DateTime.UtcNow
        };

        _customerFormRepository.Add(response);

        return ApiResponse<CustomerFormResponseDto>.Ok(
            MapToDto(response),
            "Form submitted successfully.");
    }

    public ApiResponse<IEnumerable<CustomerFormResponseDto>> GetResponsesByProjectId(Guid projectId)
    {
        var project = _projectRepository.GetById(projectId);
        if (project is null)
            return ApiResponse<IEnumerable<CustomerFormResponseDto>>.Fail("Project not found.");

        var responses = _customerFormRepository.GetByProjectId(projectId).Select(MapToDto);
        return ApiResponse<IEnumerable<CustomerFormResponseDto>>.Ok(responses);
    }

    private static CustomerFormResponseDto MapToDto(CustomerFormResponse response) => new()
    {
        Id = response.Id,
        ProjectId = response.ProjectId,
        Token = response.Token,
        FormData = response.FormData,
        SubmittedAt = response.SubmittedAt
    };
}
