namespace API.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string ConnectorTeamAddress { get; set; } = "connector-team@arconnet.com";
    public string FromDisplayName { get; set; } = "CIGT";
}

public class StorageSettings
{
    public const string SectionName = "Storage";

    public string RootPath { get; set; } = "App_Data";
}
