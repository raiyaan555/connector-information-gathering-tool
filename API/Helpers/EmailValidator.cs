namespace API.Helpers;

public static class EmailValidator
{
    public const string AllowedDomain = "@arconnet.com";

    public static bool IsValidArconEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var trimmed = email.Trim();
        var at = trimmed.IndexOf('@');
        if (at <= 0 || at != trimmed.LastIndexOf('@'))
            return false;

        var local = trimmed[..at];
        var host = trimmed[(at + 1)..];
        if (local.Length == 0 || host.Length == 0)
            return false;

        if (local.Any(char.IsWhiteSpace) || host.Any(char.IsWhiteSpace))
            return false;

        return host.Equals("arconnet.com", StringComparison.OrdinalIgnoreCase);
    }

    [Obsolete("Use IsValidArconEmail instead.")]
    public static bool IsValidConnectorEmail(string email) => IsValidArconEmail(email);
}
