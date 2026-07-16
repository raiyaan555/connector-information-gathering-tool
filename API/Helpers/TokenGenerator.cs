using System.Security.Cryptography;

namespace API.Helpers;

public static class TokenGenerator
{
    /// <summary>
    /// Generates a URL-safe token for customer form links.
    /// </summary>
    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
