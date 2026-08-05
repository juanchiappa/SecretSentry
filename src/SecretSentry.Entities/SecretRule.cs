namespace SecretSentry.Entities;

public class SecretRule
{
    public string Id { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Pattern { get; set; } = string.Empty;

    public Severity Severity { get; set; } = Severity.Medium;

    public List<string> Tags { get; set; } = new();


    public bool RequiresContext { get; set; } = false;

    public string Source { get; set; } = "default";
}