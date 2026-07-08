using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/attachments")]
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpGet("project/{projectId:guid}")]
    public ActionResult<ApiResponse<IEnumerable<AttachmentDto>>> GetByProjectId(Guid projectId)
    {
        var result = _attachmentService.GetByProjectId(projectId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("project/{projectId:guid}")]
    public ActionResult<ApiResponse<AttachmentDto>> Upload(
        Guid projectId,
        [FromBody] UploadAttachmentRequest request)
    {
        var result = _attachmentService.Upload(projectId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public ActionResult<ApiResponse<MessageResponse>> Delete(Guid id)
    {
        var result = _attachmentService.Delete(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
