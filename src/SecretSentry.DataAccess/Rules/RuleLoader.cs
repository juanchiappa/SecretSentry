using System.Reflection;
using System.Text.RegularExpressions;
using SecretSentry.Entities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SecretSentry.DataAccess.Rules;


public class RuleLoader
{
    private const string EmbeddedDefaultRulesResourceName =
        "SecretSentry.DataAccess.Rules.rules.default.yaml";

    private readonly IDeserializer _deserializer;

    public RuleLoader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public List<CompiledRule> LoadRules(string? customRulesPath = null)
    {
        var defaultRules = LoadDefaultRules();
        var merged = new Dictionary<string, SecretRule>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in defaultRules)
            merged[rule.Id] = rule;

        if (!string.IsNullOrWhiteSpace(customRulesPath))
        {
            var customRules = LoadRulesFromFile(customRulesPath, source: "custom");
            foreach (var rule in customRules)
                merged[rule.Id] = rule;
        }

        return CompileAndValidate(merged.Values);
    }

    public List<SecretRule> LoadDefaultRules()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(EmbeddedDefaultRulesResourceName);

        if (stream is null)
        {
            throw new InvalidOperationException(
                $"No se encontró el recurso embebido '{EmbeddedDefaultRulesResourceName}'. " +
                "Verificá que rules.default.yaml tenga Build Action = EmbeddedResource en el .csproj.");
        }

        using var reader = new StreamReader(stream);
        return ParseYaml(reader.ReadToEnd(), source: "default");
    }

    private List<SecretRule> LoadRulesFromFile(string path, string source)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"No se encontró el archivo de reglas custom: {path}");

        var yaml = File.ReadAllText(path);
        return ParseYaml(yaml, source);
    }

    private List<SecretRule> ParseYaml(string yaml, string source)
    {
        RulesFile? parsed;
        try
        {
            parsed = _deserializer.Deserialize<RulesFile>(yaml);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error parseando YAML de reglas (source: {source}): {ex.Message}", ex);
        }

        if (parsed?.Rules is null || parsed.Rules.Count == 0)
            throw new InvalidOperationException($"El archivo de reglas '{source}' no contiene ninguna regla.");

        foreach (var rule in parsed.Rules)
            rule.Source = source;

        return parsed.Rules;
    }

    private List<CompiledRule> CompileAndValidate(IEnumerable<SecretRule> rules)
    {
        var compiled = new List<CompiledRule>();
        var errors = new List<string>();

        foreach (var rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule.Id))
            {
                errors.Add("Hay una regla sin 'id' definido.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(rule.Pattern))
            {
                errors.Add($"Regla '{rule.Id}': falta 'pattern'.");
                continue;
            }

            try
            {
                var regex = new Regex(rule.Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
                compiled.Add(new CompiledRule(rule, regex));
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Regla '{rule.Id}': el pattern '{rule.Pattern}' no es un regex válido ({ex.Message}).");
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Se encontraron errores al cargar reglas:\n" + string.Join("\n", errors));
        }

        return compiled;
    }

    private class RulesFile
    {
        public List<SecretRule> Rules { get; set; } = new();
    }
}