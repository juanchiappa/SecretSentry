namespace SecretSentry.BusinessLogic.Scanning;

public static class SecretMasker
{
    private const int VisiblePrefixLength = 4;
    private const int VisibleSuffixLength = 2;

    public static string Mask(string rawSecret)
    {
        if (string.IsNullOrEmpty(rawSecret))
            return string.Empty;

        var minLengthToPartiallyReveal = VisiblePrefixLength + VisibleSuffixLength + 4;

        if (rawSecret.Length < minLengthToPartiallyReveal)
            return new string('*', rawSecret.Length);

        var prefix = rawSecret[..VisiblePrefixLength];
        var suffix = rawSecret[^VisibleSuffixLength..];
        var maskedMiddleLength = rawSecret.Length - VisiblePrefixLength - VisibleSuffixLength;

        return $"{prefix}{new string('*', maskedMiddleLength)}{suffix}";
    }
}