namespace SecretSentry.Entities;

public class Finding
{
    public string RuleId { get; set; } = string.Empty;
    public string RuleDescription { get; set; } = string.Empty;
    public Severity Severity { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public int LineNumber { get; set; }


    public string? CommitHash { get; set; }

    public string? CommitAuthor { get; set; }

    public DateTimeOffset? CommitDate { get; set; }

    public string MaskedSecret { get; set; } = string.Empty;


    public bool NeedsReview { get; set; }

    public override string ToString()
    {
        var location = CommitHash is null
            ? $"{FilePath}:{LineNumber}"
            : $"{FilePath}:{LineNumber} (commit {CommitHash[..7]})";

        return $"[{Severity}] {RuleId} en {location} -> {MaskedSecret}";
    }
}