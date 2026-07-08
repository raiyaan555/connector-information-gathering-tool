using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public ActionResult<ApiResponse<IEnumerable<ProjectDto>>> GetAll()
    {
        var result = _projectService.GetAll();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ApiResponse<ProjectDto>> GetById(Guid id)
    {
        var result = _projectService.GetById(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public ActionResult<ApiResponse<ProjectDto>> Create([FromBody] CreateProjectRequest request)
    {
        var result = _projectService.Create(request);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<ApiResponse<ProjectDto>> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var result = _projectService.Update(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:guid}")]
    public ActionResult<ApiResponse<MessageResponse>> Delete(Guid id)
    {
        var result = _projectService.Delete(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:guid}/generate-link")]
    public ActionResult<ApiResponse<GenerateLinkResponse>> GenerateLink(Guid id)
    {
        var result = _projectService.GenerateLink(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
