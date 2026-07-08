using API.DTOs;
using API.Helpers;
using API.Models;
using API.Repositories;

namespace API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public ApiResponse<AuthResponse> Login(LoginRequest request)
    {
        if (!EmailValidator.IsValidConnectorEmail(request.Email))
        {
            return ApiResponse<AuthResponse>.Fail(
                "Invalid email address. Email must end with @theconnector.com.");
        }

        var user = _userRepository.GetByEmail(request.Email);
        if (user is null || user.Password != request.Password)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");
        }

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = TokenGenerator.GenerateAuthToken(user.Email),
            Email = user.Email,
            FullName = user.FullName,
            IsEmailVerified = user.IsEmailVerified
        }, "Login successful.");
    }

    public ApiResponse<AuthResponse> Register(RegisterRequest request)
    {
        if (!EmailValidator.IsValidConnectorEmail(request.Email))
        {
            return ApiResponse<AuthResponse>.Fail(
                "Invalid email address. Email must end with @theconnector.com.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return ApiResponse<AuthResponse>.Fail("Password must be at least 8 characters.");
        }

        if (_userRepository.GetByEmail(request.Email) is not null)
        {
            return ApiResponse<AuthResponse>.Fail("A user with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _userRepository.Add(user);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = TokenGenerator.GenerateAuthToken(user.Email),
            Email = user.Email,
            FullName = user.FullName,
            IsEmailVerified = user.IsEmailVerified
        }, "Registration successful. Please verify your email.");
    }

    public ApiResponse<MessageResponse> ForgotPassword(ForgotPasswordRequest request)
    {
        if (!EmailValidator.IsValidConnectorEmail(request.Email))
        {
            return ApiResponse<MessageResponse>.Fail(
                "Invalid email address. Email must end with @theconnector.com.");
        }

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "If the email exists, a password reset link has been sent." },
            "Password reset email sent.");
    }

    public ApiResponse<MessageResponse> VerifyEmail(VerifyEmailRequest request)
    {
        if (!EmailValidator.IsValidConnectorEmail(request.Email))
        {
            return ApiResponse<MessageResponse>.Fail(
                "Invalid email address. Email must end with @theconnector.com.");
        }

        var user = _userRepository.GetByEmail(request.Email);
        if (user is null)
        {
            return ApiResponse<MessageResponse>.Fail("User not found.");
        }

        if (string.IsNullOrWhiteSpace(request.VerificationCode))
        {
            return ApiResponse<MessageResponse>.Fail("Verification code is required.");
        }

        _userRepository.Update(request.Email, u => u.IsEmailVerified = true);

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Email verified successfully." },
            "Email verified successfully.");
    }
}
