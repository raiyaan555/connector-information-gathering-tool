using API.DTOs;
using API.Models;

namespace API.Services;

public interface IAuthService
{
    ApiResponse<AuthResponse> Login(LoginRequest request);
    ApiResponse<AuthResponse> Register(RegisterRequest request);
    ApiResponse<MessageResponse> ForgotPassword(ForgotPasswordRequest request);
    ApiResponse<MessageResponse> VerifyEmail(VerifyEmailRequest request);
}
