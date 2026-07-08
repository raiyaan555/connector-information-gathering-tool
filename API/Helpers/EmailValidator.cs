namespace API.Helpers;

public static class EmailValidator
{
  private const string AllowedDomain = "@theconnector.com";

  public static bool IsValidConnectorEmail(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
      return false;

    return email.Trim().EndsWith(AllowedDomain, StringComparison.OrdinalIgnoreCase);
  }
}
