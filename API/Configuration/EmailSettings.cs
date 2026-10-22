namespace API.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string ConnectorTeamAddress { get; set; } = "connector-team@arconnet.com";
    public string FromDisplayName { get; set; } = "CIGT";
}
