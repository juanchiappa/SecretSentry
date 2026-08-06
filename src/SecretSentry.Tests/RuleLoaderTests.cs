using SecretSentry.DataAccess.Rules;

namespace SecretSentry.Tests;

public class RuleLoaderTests
{
    [Fact]
    public void LoadDefaultRules_CargaLasReglasDelYamlEmbebido()
    {
        var loader = new RuleLoader();

        var rules = loader.LoadDefaultRules();

        Assert.NotEmpty(rules);
        Assert.Equal(11, rules.Count);
    }

    [Fact]
    public void LoadRules_TodasLasReglasDefaultCompilanSinExcepcion()
    {
        var loader = new RuleLoader();

        var compiledRules = loader.LoadRules();

        Assert.Equal(11, compiledRules.Count);
        Assert.All(compiledRules, cr => Assert.NotNull(cr.Regex));
    }

    [Fact]
    public void LoadRules_CadaReglaTieneIdDescripcionYPatternNoVacios()
    {
        var loader = new RuleLoader();

        var compiledRules = loader.LoadRules();

        Assert.All(compiledRules, cr =>
        {
            Assert.False(string.IsNullOrWhiteSpace(cr.Rule.Id));
            Assert.False(string.IsNullOrWhiteSpace(cr.Rule.Description));
            Assert.False(string.IsNullOrWhiteSpace(cr.Rule.Pattern));
        });
    }

    [Fact]
    public void LoadRules_TodosLosIdsSonUnicos()
    {
        var loader = new RuleLoader();

        var compiledRules = loader.LoadRules();
        var ids = compiledRules.Select(cr => cr.Rule.Id).ToList();

        Assert.Equal(ids.Count, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Theory]
    [InlineData("aws-access-key-id", "AKIAIOSFODNN7EXAMPLE", true)]
    [InlineData("aws-access-key-id", "no-matchea-esto", false)]
    [InlineData("github-pat-classic", "ghp_1234567890abcdef1234567890abcdef1234", true)]
    [InlineData("gcp-api-key", "AIzaSyD-1234567890abcdefghijklmnopqrstuv", true)]
    [InlineData("private-key-block", "-----BEGIN RSA PRIVATE KEY-----", true)]
    public void LoadRules_PatronesDetectanElFormatoEsperado(string ruleId, string input, bool esperaMatch)
    {
        var loader = new RuleLoader();
        var compiledRules = loader.LoadRules();

        var rule = compiledRules.Single(cr => cr.Rule.Id == ruleId);

        Assert.Equal(esperaMatch, rule.Regex.IsMatch(input));
    }

    [Fact]
    public void LoadRules_StripeLiveSecretKey_Matchea()
    {
        // Se arma en runtime (no como literal) para que scanners de secretos
        // (incluido GitHub push protection) no lo confundan con una key real.
        var fakeStripeKey = "sk_" + "live_" + new string('a', 24);

        var loader = new RuleLoader();
        var compiledRules = loader.LoadRules();
        var rule = compiledRules.Single(cr => cr.Rule.Id == "stripe-live-secret-key");

        Assert.True(rule.Regex.IsMatch(fakeStripeKey));
    }

    [Fact]
    public void LoadRules_SlackToken_Matchea()
    {
        var fakeSlackToken = "xoxb" + "-1234567890-" + new string('a', 16);

        var loader = new RuleLoader();
        var compiledRules = loader.LoadRules();
        var rule = compiledRules.Single(cr => cr.Rule.Id == "slack-token");

        Assert.True(rule.Regex.IsMatch(fakeSlackToken));
    }

    [Fact]
    public void LoadRules_ReglaCustomConMismoIdSobreescribeALaDefault()
    {
        var customYamlPath = Path.GetTempFileName();
        File.WriteAllText(customYamlPath, """
            rules:
              - id: aws-access-key-id
                description: "Override custom para testear merge"
                pattern: 'CUSTOM_AKIA[0-9A-Z]{16}'
                severity: low
                requires_context: false
            """);

        try
        {
            var loader = new RuleLoader();
            var compiledRules = loader.LoadRules(customYamlPath);

            var rule = compiledRules.Single(cr => cr.Rule.Id == "aws-access-key-id");

            Assert.Equal("custom", rule.Rule.Source);
            Assert.Equal(Entities.Severity.Low, rule.Rule.Severity);
            Assert.True(rule.Regex.IsMatch("CUSTOM_AKIAIOSFODNN7EXAMPLE"));
        }
        finally
        {
            File.Delete(customYamlPath);
        }
    }

    [Fact]
    public void LoadRules_ArchivoCustomInexistente_TiraFileNotFoundException()
    {
        var loader = new RuleLoader();

        Assert.Throws<FileNotFoundException>(() => loader.LoadRules("no-existe.yaml"));
    }
}