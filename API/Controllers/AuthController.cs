using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public ActionResult<ApiResponse<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = _authService.Login(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("register")]
    public ActionResult<ApiResponse<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = _authService.Register(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("forgot-password")]
    public ActionResult<ApiResponse<MessageResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = _authService.ForgotPassword(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("verify-email")]
    public ActionResult<ApiResponse<MessageResponse>> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var result = _authService.VerifyEmail(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
