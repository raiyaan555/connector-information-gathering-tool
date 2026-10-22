using API.DTOs;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public interface IUserAdminService
{
    Task<ApiResponse<List<ManagedUserDto>>> GetAllAsync(string? actorUserId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ManagedUserDto>> CreateAsync(string? actorUserId, CreateManagedUserRequest request, CancellationToken cancellationToken = default);
}

public class UserAdminService : IUserAdminService
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin", "Engineer", "Viewer"
    };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(UserManager<ApplicationUser> userManager, ILogger<UserAdminService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApiResponse<List<ManagedUserDto>>> GetAllAsync(string? actorUserId, CancellationToken cancellationToken = default)
    {
        if (!await IsAdminAsync(actorUserId))
            return ApiResponse<List<ManagedUserDto>>.Fail("Only an Admin can view users.");

        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        var dtos = new List<ManagedUserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            dtos.Add(Map(user, roles));
        }

        return ApiResponse<List<ManagedUserDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<ManagedUserDto>> CreateAsync(
        string? actorUserId,
        CreateManagedUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await IsAdminAsync(actorUserId))
            return ApiResponse<ManagedUserDto>.Fail("Only an Admin can create users.");

        if (!EmailValidator.IsValidArconEmail(request.Email))
            return ApiResponse<ManagedUserDto>.Fail("Invalid email address. Email must be an @arconnet.com address.");

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return ApiResponse<ManagedUserDto>.Fail("First name and last name are required.");

        if (!IsAcceptablePassword(request.Password))
            return ApiResponse<ManagedUserDto>.Fail("Password must be at least 8 characters and include upper case, lower case, and a number.");

        var role = string.IsNullOrWhiteSpace(request.Role) ? "Engineer" : request.Role.Trim();
        if (!AllowedRoles.Contains(role))
            return ApiResponse<ManagedUserDto>.Fail("Invalid role. Allowed roles: Admin, Engineer, Viewer.");

        var email = request.Email.Trim().ToLowerInvariant();
        var username = email.Split('@')[0];

        if (await _userManager.FindByEmailAsync(email) is not null)
            return ApiResponse<ManagedUserDto>.Fail("A user with this email already exists.");

        if (await _userManager.FindByNameAsync(username) is not null)
            return ApiResponse<ManagedUserDto>.Fail("Username is already taken.");

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return ApiResponse<ManagedUserDto>.Fail(
                "Could not create the user.",
                createResult.Errors.Select(e => e.Description).ToList());
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            return ApiResponse<ManagedUserDto>.Fail(
                "User was created but the role could not be assigned.",
                roleResult.Errors.Select(e => e.Description).ToList());
        }

        _logger.LogInformation("Admin {ActorId} created user {Email} with role {Role}", actorUserId, email, role);
        var roles = await _userManager.GetRolesAsync(user);
        return ApiResponse<ManagedUserDto>.Ok(Map(user, roles), "User created.");
    }

    private async Task<bool> IsAdminAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var actor = await _userManager.FindByIdAsync(userId);
        return actor is not null && await _userManager.IsInRoleAsync(actor, "Admin");
    }

    private static bool IsAcceptablePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        return password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit);
    }

    private static ManagedUserDto Map(ApplicationUser user, IList<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        Username = user.UserName ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        FullName = user.FullName,
        CreatedAt = user.CreatedAt,
        IsEmailVerified = user.EmailConfirmed,
        Roles = roles
    };
}
