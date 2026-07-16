namespace API.Helpers;

public static class EmailValidator
{
    public const string AllowedDomain = "@arconnet.com";

    public static bool IsValidArconEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var trimmed = email.Trim();
        if (!trimmed.Contains('@'))
            return false;

        return trimmed.EndsWith(AllowedDomain, StringComparison.OrdinalIgnoreCase);
    }

    [Obsolete("Use IsValidArconEmail instead.")]
    public static bool IsValidConnectorEmail(string email) => IsValidArconEmail(email);
}
