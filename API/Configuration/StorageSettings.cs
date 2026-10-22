namespace API.Configuration;

public class StorageSettings
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Storage backend: "Local" (filesystem) today; swap to AzureBlob, S3, etc. in production.
    /// </summary>
    public string Provider { get; set; } = "Local";

    /// <summary>
    /// Root folder for local file storage (relative to API content root, or absolute path).
    /// Development default: Uploads/
    /// </summary>
    public string RootPath { get; set; } = "Uploads";
}
