namespace Common.Localization;

public sealed class GameLanguageOptions
{
    public string DefaultLanguage { get; init; } = "ja-JP";

    public bool UseJapanese =>
        DefaultLanguage.StartsWith("ja", StringComparison.OrdinalIgnoreCase);
}
