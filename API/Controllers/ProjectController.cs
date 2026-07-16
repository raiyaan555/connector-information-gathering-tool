using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProjectDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _projectService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetByIdAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Update(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.DeleteAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:guid}/generate-link")]
    public async Task<ActionResult<ApiResponse<GenerateLinkResponse>>> GenerateLink(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.GenerateLinkAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
