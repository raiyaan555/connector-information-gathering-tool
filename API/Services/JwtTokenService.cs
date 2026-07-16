using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Configuration;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public interface IJwtTokenService
{
    Task<(string AccessToken, DateTime ExpiresAt)> CreateAccessTokenAsync(ApplicationUser user, IList<string> roles);
    Task<RefreshToken> CreateRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(RefreshToken token, string? replacedBy = null, CancellationToken cancellationToken = default);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly ApplicationDbContext _db;

    public JwtTokenService(IOptions<JwtSettings> settings, ApplicationDbContext db)
    {
        _settings = settings.Value;
        _db = db;
    }

    public Task<(string AccessToken, DateTime ExpiresAt)> CreateAccessTokenAsync(ApplicationUser user, IList<string> roles)
    {
        var expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return Task.FromResult((new JwtSecurityTokenHandler().WriteToken(token), expires));
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var refresh = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync(cancellationToken);
        return refresh;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken token, string? replacedBy = null, CancellationToken cancellationToken = default)
    {
        token.IsRevoked = true;
        token.ReplacedByToken = replacedBy;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
