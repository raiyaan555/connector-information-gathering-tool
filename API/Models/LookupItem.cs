namespace API.Models;

public class LookupItem
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
