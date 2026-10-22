using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}")]
public class PdfController : ControllerBase
{
    private readonly IConnectorPdfService _pdfService;

    public PdfController(IConnectorPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    [HttpPost("generate-pdf")]
    public async Task<IActionResult> GeneratePdf(
        Guid projectId,
        [FromBody] GeneratePdfRequest request,
        CancellationToken cancellationToken)
    {
        var (success, error, fileBytes, fileName, contentType) = await _pdfService.GenerateAsync(
            projectId,
            request.FormData,
            cancellationToken);

        if (!success || fileBytes is null)
            return BadRequest(ApiResponse<object>.Fail(error ?? "PDF generation failed."));

        return File(fileBytes, contentType ?? "application/pdf", fileName ?? "ConnectorDocument.pdf");
    }

    [HttpPost("share-email")]
    public async Task<ActionResult<ApiResponse<ShareComposeResponse>>> ShareEmail(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var (success, error, compose) = await _pdfService.BuildShareComposeAsync(projectId, cancellationToken);
        if (!success || compose is null)
        {
            var failure = ApiResponse<ShareComposeResponse>.Fail(error ?? "Unable to prepare the Outlook message.");
            return error == "Project not found." ? NotFound(failure) : BadRequest(failure);
        }

        return Ok(ApiResponse<ShareComposeResponse>.Ok(compose));
    }
}
