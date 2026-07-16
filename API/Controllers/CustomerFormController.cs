using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/customer-form")]
public class CustomerFormController : ControllerBase
{
    private readonly ICustomerFormService _customerFormService;

    public CustomerFormController(ICustomerFormService customerFormService)
    {
        _customerFormService = customerFormService;
    }

    [AllowAnonymous]
    [HttpGet("{token}")]
    public async Task<ActionResult<ApiResponse<CustomerFormDto>>> GetForm(string token, CancellationToken cancellationToken)
    {
        var result = await _customerFormService.GetFormByTokenAsync(token, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [AllowAnonymous]
    [HttpPost("{token}")]
    public async Task<ActionResult<ApiResponse<CustomerFormResponseDto>>> SubmitForm(
        string token,
        [FromBody] SubmitCustomerFormRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _customerFormService.SubmitFormAsync(token, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpGet("project/{projectId:guid}/responses")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerFormResponseDto>>>> GetResponses(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _customerFormService.GetResponsesByProjectIdAsync(projectId, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
