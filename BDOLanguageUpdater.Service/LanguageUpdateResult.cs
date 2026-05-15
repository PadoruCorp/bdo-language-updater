namespace BDOLanguageUpdater.Service;

public sealed record LanguageUpdateResult(
    bool Succeeded,
    string Message,
    string SourceLanguageCode,
    string TargetLanguageCode,
    string? UpdatedFilePath,
    string? BackupFilePath)
{
    public static LanguageUpdateResult Success(GameLanguageFile sourceLanguage, LanguageFileBackupResult backupResult)
    {
        var backupMessage = backupResult.BackupFilePath is null
            ? " No original-language backup was created because the current file was already managed by the updater."
            : backupResult.CreatedNewBackup
                ? $" Backup saved to '{backupResult.BackupFilePath}'."
                : $" Existing backup preserved at '{backupResult.BackupFilePath}'.";

        return new LanguageUpdateResult(
            true,
            $"Updated {sourceLanguage.DisplayName} with the latest official English localization.{backupMessage}",
            sourceLanguage.Code,
            Constants.DEFAULT_TARGET_LANGUAGE_CODE,
            sourceLanguage.FullPath,
            backupResult.BackupFilePath);
    }

    public static LanguageUpdateResult Skipped(GameLanguageFile sourceLanguage)
    {
        return new LanguageUpdateResult(
            true,
            $"{sourceLanguage.DisplayName} already contains the latest official English localization. No update was needed.",
            sourceLanguage.Code,
            Constants.DEFAULT_TARGET_LANGUAGE_CODE,
            sourceLanguage.FullPath,
            null);
    }

    public static LanguageUpdateResult Failure(string message, string sourceLanguageCode = "")
    {
        return new LanguageUpdateResult(
            false,
            message,
            sourceLanguageCode,
            Constants.DEFAULT_TARGET_LANGUAGE_CODE,
            null,
            null);
    }
}
