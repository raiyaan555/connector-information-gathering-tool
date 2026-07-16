using API.DTOs;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MessageResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MessageResponse>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MessageResponse>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MessageResponse>> LogoutAsync(string? userId, string? refreshToken, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserProfileDto>> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MessageResponse>> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}

public class AuthService : IAuthService
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin", "Engineer", "Viewer"
    };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (!EmailValidator.IsValidArconEmail(request.Email))
            return ApiResponse<AuthResponse>.Fail("Invalid email address. Email must end with @arconnet.com.");

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return ApiResponse<AuthResponse>.Fail("Account is locked. Try again later.");
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");
        }

        return ApiResponse<AuthResponse>.Ok(await BuildAuthResponseAsync(user, cancellationToken), "Login successful.");
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!EmailValidator.IsValidArconEmail(request.Email))
            return ApiResponse<AuthResponse>.Fail("Invalid email address. Email must end with @arconnet.com.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return ApiResponse<AuthResponse>.Fail("Password must be at least 8 characters.");

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return ApiResponse<AuthResponse>.Fail("First name and last name are required.");

        var email = request.Email.Trim().ToLowerInvariant();
        var username = string.IsNullOrWhiteSpace(request.Username)
            ? email.Split('@')[0]
            : request.Username.Trim();

        if (await _userManager.FindByEmailAsync(email) is not null)
            return ApiResponse<AuthResponse>.Fail("A user with this email already exists.");

        if (await _userManager.FindByNameAsync(username) is not null)
            return ApiResponse<AuthResponse>.Fail("Username is already taken.");

        var role = string.IsNullOrWhiteSpace(request.Role) ? "Engineer" : request.Role.Trim();
        if (!AllowedRoles.Contains(role))
            return ApiResponse<AuthResponse>.Fail("Invalid role. Allowed roles: Admin, Engineer, Viewer.");

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return ApiResponse<AuthResponse>.Fail(
                "Registration failed.",
                createResult.Errors.Select(e => e.Description).ToList());
        }

        await _userManager.AddToRoleAsync(user, role);
        _logger.LogInformation("Registered user {Email} with role {Role}", email, role);

        return ApiResponse<AuthResponse>.Ok(
            await BuildAuthResponseAsync(user, cancellationToken),
            "Registration successful. Please verify your email.");
    }

    public async Task<ApiResponse<MessageResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (!EmailValidator.IsValidArconEmail(request.Email))
            return ApiResponse<MessageResponse>.Fail("Invalid email address. Email must end with @arconnet.com.");

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation("Password reset token generated for {Email} (placeholder — email not sent). Token length: {Length}",
                user.Email, token.Length);
        }

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "If the email exists, a password reset link has been sent." },
            "Password reset email sent.");
    }

    public async Task<ApiResponse<MessageResponse>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (!EmailValidator.IsValidArconEmail(request.Email))
            return ApiResponse<MessageResponse>.Fail("Invalid email address. Email must end with @arconnet.com.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return ApiResponse<MessageResponse>.Fail("Password must be at least 8 characters.");

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return ApiResponse<MessageResponse>.Fail("Invalid reset request.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            return ApiResponse<MessageResponse>.Fail(
                "Password reset failed.",
                result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Password has been reset successfully." },
            "Password reset successful.");
    }

    public async Task<ApiResponse<MessageResponse>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        if (!EmailValidator.IsValidArconEmail(request.Email))
            return ApiResponse<MessageResponse>.Fail("Invalid email address. Email must end with @arconnet.com.");

        if (string.IsNullOrWhiteSpace(request.VerificationCode))
            return ApiResponse<MessageResponse>.Fail("Verification code is required.");

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return ApiResponse<MessageResponse>.Fail("User not found.");

        // Placeholder: accept any non-empty code until email provider is wired.
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Email verified successfully." },
            "Email verified successfully.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return ApiResponse<AuthResponse>.Fail("Refresh token is required.");

        var stored = await _jwtTokenService.GetActiveRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (stored is null || !stored.IsActive || stored.User is null)
            return ApiResponse<AuthResponse>.Fail("Invalid or expired refresh token.");

        var user = stored.User;
        var newRefresh = await _jwtTokenService.CreateRefreshTokenAsync(user, cancellationToken);
        await _jwtTokenService.RevokeRefreshTokenAsync(stored, newRefresh.Token, cancellationToken);

        var response = await BuildAuthResponseAsync(user, cancellationToken, newRefresh);
        return ApiResponse<AuthResponse>.Ok(response, "Token refreshed.");
    }

    public async Task<ApiResponse<MessageResponse>> LogoutAsync(string? userId, string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var stored = await _jwtTokenService.GetActiveRefreshTokenAsync(refreshToken, cancellationToken);
            if (stored is not null)
                await _jwtTokenService.RevokeRefreshTokenAsync(stored, cancellationToken: cancellationToken);
        }

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Logged out successfully." },
            "Logged out successfully.");
    }

    public async Task<ApiResponse<UserProfileDto>> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ApiResponse<UserProfileDto>.Fail("User not found.");

        return ApiResponse<UserProfileDto>.Ok(await MapProfileAsync(user));
    }

    public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ApiResponse<UserProfileDto>.Fail("User not found.");

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return ApiResponse<UserProfileDto>.Fail("First name and last name are required.");

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !string.Equals(request.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (!EmailValidator.IsValidArconEmail(request.Email))
                return ApiResponse<UserProfileDto>.Fail("Invalid email address. Email must end with @arconnet.com.");

            var emailOwner = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (emailOwner is not null && emailOwner.Id != user.Id)
                return ApiResponse<UserProfileDto>.Fail("Email is already in use.");

            user.Email = request.Email.Trim().ToLowerInvariant();
            user.EmailConfirmed = false;
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ApiResponse<UserProfileDto>.Fail(
                "Profile update failed.",
                result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse<UserProfileDto>.Ok(await MapProfileAsync(user), "Profile updated successfully.");
    }

    public async Task<ApiResponse<MessageResponse>> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ApiResponse<MessageResponse>.Fail("User not found.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return ApiResponse<MessageResponse>.Fail("New password must be at least 8 characters.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return ApiResponse<MessageResponse>.Fail(
                "Password change failed.",
                result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Password changed successfully." },
            "Password changed successfully.");
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken,
        RefreshToken? existingRefresh = null)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = await _jwtTokenService.CreateAccessTokenAsync(user, roles);
        var refresh = existingRefresh ?? await _jwtTokenService.CreateRefreshTokenAsync(user, cancellationToken);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refresh.Token,
            ExpiresAt = expiresAt,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Username = user.UserName,
            IsEmailVerified = user.EmailConfirmed,
            Roles = roles
        };
    }

    private async Task<UserProfileDto> MapProfileAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Username = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsEmailVerified = user.EmailConfirmed,
            Roles = roles
        };
    }
}
