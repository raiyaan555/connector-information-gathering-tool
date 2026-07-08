using API.DTOs;
using API.Models;

namespace API.Services;

public interface IProjectService
{
    ApiResponse<IEnumerable<ProjectDto>> GetAll();
    ApiResponse<ProjectDto> GetById(Guid id);
    ApiResponse<ProjectDto> Create(CreateProjectRequest request);
    ApiResponse<ProjectDto> Update(Guid id, UpdateProjectRequest request);
    ApiResponse<MessageResponse> Delete(Guid id);
    ApiResponse<GenerateLinkResponse> GenerateLink(Guid id);
}
