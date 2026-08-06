using SecretSentry.DataAccess.Rules;
using SecretSentry.Entities;

namespace SecretSentry.BusinessLogic.Scanning;

public class CurrentStateScanner : IRepositoryScanner
{
    private static readonly HashSet<string> IgnoredDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "bin", "obj", "node_modules", ".vs", ".vscode", "packages"
    };

    private static readonly HashSet<string> IgnoredExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".exe", ".pdb", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".bmp",
        ".zip", ".7z", ".rar", ".tar", ".gz", ".pdf", ".woff", ".woff2", ".ttf",
        ".mp3", ".mp4", ".avi", ".mov", ".bin", ".dat", ".db", ".sqlite"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly List<CompiledRule> _rules;

    public CurrentStateScanner(List<CompiledRule> rules)
    {
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));

        if (_rules.Count == 0)
            throw new ArgumentException("La lista de reglas no puede estar vacía.", nameof(rules));
    }

    public List<Finding> Scan(string repositoryPath)
    {
        if (!Directory.Exists(repositoryPath))
            throw new DirectoryNotFoundException($"No se encontró el repositorio en: {repositoryPath}");

        var findings = new List<Finding>();

        foreach (var filePath in EnumerateScannableFiles(repositoryPath))
        {
            findings.AddRange(ScanFile(repositoryPath, filePath));
        }

        return findings;
    }

    private IEnumerable<string> EnumerateScannableFiles(string rootPath)
    {
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(rootPath);

        while (pendingDirectories.Count > 0)
        {
            var currentDirectory = pendingDirectories.Pop();

            IEnumerable<string> subDirectories;
            IEnumerable<string> filesInDirectory;

            try
            {
                subDirectories = Directory.EnumerateDirectories(currentDirectory);
                filesInDirectory = Directory.EnumerateFiles(currentDirectory);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var subDirectory in subDirectories)
            {
                var directoryName = Path.GetFileName(subDirectory);
                if (!IgnoredDirectoryNames.Contains(directoryName))
                    pendingDirectories.Push(subDirectory);
            }

            foreach (var file in filesInDirectory)
            {
                if (IsScannable(file))
                    yield return file;
            }
        }
    }

    private bool IsScannable(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (IgnoredExtensions.Contains(extension))
            return false;

        try
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > MaxFileSizeBytes)
                return false;
        }
        catch (IOException)
        {
            return false;
        }

        return true;
    }

    private List<Finding> ScanFile(string repositoryRoot, string absoluteFilePath)
    {
        var findings = new List<Finding>();

        string[] lines;
        try
        {
            lines = File.ReadAllLines(absoluteFilePath);
        }
        catch (IOException)
        {
            return findings;
        }
        catch (UnauthorizedAccessException)
        {
            return findings;
        }

        var relativePath = Path.GetRelativePath(repositoryRoot, absoluteFilePath);

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];

            foreach (var compiledRule in _rules)
            {
                var match = compiledRule.Regex.Match(line);
                if (!match.Success)
                    continue;

                findings.Add(new Finding
                {
                    RuleId = compiledRule.Rule.Id,
                    RuleDescription = compiledRule.Rule.Description,
                    Severity = compiledRule.Rule.Severity,
                    FilePath = relativePath,
                    LineNumber = lineIndex + 1,
                    CommitHash = null,
                    CommitAuthor = null,
                    CommitDate = null,
                    MaskedSecret = SecretMasker.Mask(match.Value),
                    NeedsReview = compiledRule.Rule.RequiresContext
                });
            }
        }

        return findings;
    }
}