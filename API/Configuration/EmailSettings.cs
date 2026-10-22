namespace API.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string ConnectorTeamAddress { get; set; } = "ciconnectorsteam@arconnet.com";
    public string ConnectorTeamName { get; set; } = "CI Connectors Team";
    public string FromDisplayName { get; set; } = "CIGT";
}
