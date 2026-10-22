using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Services;

public interface ICurrentUser
{
    string? UserId { get; }
    string? Email { get; }
}

public sealed class CurrentUser : ICurrentUser
{
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        var principal = httpContextAccessor.HttpContext?.User;
        UserId = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal?.FindFirstValue("nameid");
        Email = principal?.FindFirstValue(ClaimTypes.Email);
    }

    public string? UserId { get; }
    public string? Email { get; }
}
