using System.Text.RegularExpressions;
using SecretSentry.Entities;

namespace SecretSentry.DataAccess.Rules;

public class CompiledRule
{
    public SecretRule Rule { get; }
    public Regex Regex { get; }

    public CompiledRule(SecretRule rule, Regex regex)
    {
        Rule = rule;
        Regex = regex;
    }
}