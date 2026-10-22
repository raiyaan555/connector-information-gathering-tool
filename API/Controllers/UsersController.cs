using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;

    public UsersController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ManagedUserDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _userAdminService.GetAllAsync(CurrentUserId(), cancellationToken);
        if (IsForbidden(result.Message))
            return StatusCode(StatusCodes.Status403Forbidden, result);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ManagedUserDto>>> Create(
        [FromBody] CreateManagedUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userAdminService.CreateAsync(CurrentUserId(), request, cancellationToken);
        if (IsForbidden(result.Message))
            return StatusCode(StatusCodes.Status403Forbidden, result);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    private string? CurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue("nameid");

    private static bool IsForbidden(string? message) =>
        !string.IsNullOrEmpty(message) && message.StartsWith("Only an Admin", StringComparison.Ordinal);
}
