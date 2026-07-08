using System.Security.Cryptography;

namespace API.Helpers;

public static class TokenGenerator
{
    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public static string GenerateAuthToken(string email)
    {
        return $"mock-jwt-{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(email))}-{Guid.NewGuid():N}";
    }
}
