using System.Text.RegularExpressions;

namespace API.Helpers;

public static partial class JiraTicketRules
{
    public const int MaxLength = 500;

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_]*-\d+$", RegexOptions.CultureInvariant)]
    private static partial Regex IssueKeyPattern();

    [GeneratedRegex(@"/browse/([A-Za-z][A-Za-z0-9_]*-\d+)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex BrowseKeyPattern();

    public static bool TryNormalize(string? raw, out string? normalized, out string? error)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            normalized = null;
            error = null;
            return true;
        }

        var trimmed = raw.Trim();
        if (trimmed.Length > MaxLength || trimmed.Any(char.IsControl))
        {
            normalized = null;
            error = trimmed.Length > MaxLength
                ? "Jira ticket must be 500 characters or fewer."
                : "Enter a Jira issue key such as ABC-123, or a Jira link.";
            return false;
        }

        if (IssueKeyPattern().IsMatch(trimmed))
        {
            normalized = trimmed;
            error = null;
            return true;
        }

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            && !string.IsNullOrEmpty(uri.Host))
        {
            normalized = trimmed;
            error = null;
            return true;
        }

        normalized = null;
        error = "Enter a Jira issue key such as ABC-123, or a Jira link.";
        return false;
    }

    public static string? IssueKeyOrNull(string? ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket))
            return null;

        var trimmed = ticket.Trim();
        if (IssueKeyPattern().IsMatch(trimmed))
            return trimmed;

        var browse = BrowseKeyPattern().Match(trimmed);
        return browse.Success ? browse.Groups[1].Value : null;
    }
}
