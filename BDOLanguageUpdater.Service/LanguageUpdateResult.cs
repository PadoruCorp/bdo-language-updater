namespace BDOLanguageUpdater.Service;

public sealed record LanguageUpdateResult(
    bool Succeeded,
    string Message,
    string SourceLanguageCode,
    string TargetLanguageCode,
    string? UpdatedFilePath)
{
    public static LanguageUpdateResult Success(GameLanguageFile sourceLanguage)
    {
        return new LanguageUpdateResult(
            true,
            $"Updated {sourceLanguage.DisplayName} with the latest official English localization.",
            sourceLanguage.Code,
            Constants.DEFAULT_TARGET_LANGUAGE_CODE,
            sourceLanguage.FullPath);
    }

    public static LanguageUpdateResult Failure(string message, string sourceLanguageCode = "")
    {
        return new LanguageUpdateResult(
            false,
            message,
            sourceLanguageCode,
            Constants.DEFAULT_TARGET_LANGUAGE_CODE,
            null);
    }
}
