namespace BDOLanguageUpdater.Service;

public sealed class LanguageFileBackupStore
{
    private const string BackupFileSuffix = ".bdo-language-updater.bak";

    private readonly LanguageUpdateMetadataStore metadataStore;

    public LanguageFileBackupStore(LanguageUpdateMetadataStore metadataStore)
    {
        this.metadataStore = metadataStore;
    }

    public async Task<LanguageFileBackupResult> SaveBeforeUpdate(GameLanguageFile languageFile)
    {
        var backupPath = GetBackupPath(languageFile);

        try
        {
            var backupExists = File.Exists(backupPath);
            if (await metadataStore.MatchesCurrentOutput(languageFile).ConfigureAwait(false))
            {
                return backupExists
                    ? LanguageFileBackupResult.Preserved(backupPath)
                    : LanguageFileBackupResult.NotCreated("The current language file is already managed by the updater.");
            }

            await CopyFile(languageFile.FullPath, backupPath).ConfigureAwait(false);
            return LanguageFileBackupResult.Created(backupPath);
        }
        catch (Exception exception)
        {
            return LanguageFileBackupResult.Failure($"Could not create a backup before updating: {exception.Message}");
        }
    }

    public bool HasBackup(GameLanguageFile languageFile)
    {
        return File.Exists(GetBackupPath(languageFile));
    }

    public async Task<LanguageBackupRestoreResult> Restore(GameLanguageFile languageFile)
    {
        var backupPath = GetBackupPath(languageFile);
        if (!File.Exists(backupPath))
        {
            return LanguageBackupRestoreResult.Failure(
                $"No backup was found for {languageFile.DisplayName}.",
                languageFile.Code);
        }

        try
        {
            await CopyFile(backupPath, languageFile.FullPath).ConfigureAwait(false);
            await metadataStore.Delete(languageFile).ConfigureAwait(false);

            return LanguageBackupRestoreResult.Success(languageFile, backupPath);
        }
        catch (Exception exception)
        {
            return LanguageBackupRestoreResult.Failure(
                $"Could not restore {languageFile.DisplayName}: {exception.Message}",
                languageFile.Code);
        }
    }

    public string GetBackupPath(GameLanguageFile languageFile)
    {
        return languageFile.FullPath + BackupFileSuffix;
    }

    private static async Task CopyFile(string sourcePath, string destinationPath)
    {
        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        await using var sourceStream = new FileStream(
            sourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1024 * 128,
            useAsync: true);

        await using var destinationStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 1024 * 128,
            useAsync: true);

        await sourceStream.CopyToAsync(destinationStream).ConfigureAwait(false);
    }
}

public sealed record LanguageFileBackupResult(
    bool Succeeded,
    bool CreatedNewBackup,
    string? BackupFilePath,
    string Message)
{
    public static LanguageFileBackupResult Created(string backupFilePath)
    {
        return new LanguageFileBackupResult(true, true, backupFilePath, "Backup created.");
    }

    public static LanguageFileBackupResult Preserved(string backupFilePath)
    {
        return new LanguageFileBackupResult(true, false, backupFilePath, "Existing backup preserved.");
    }

    public static LanguageFileBackupResult NotCreated(string message)
    {
        return new LanguageFileBackupResult(true, false, null, message);
    }

    public static LanguageFileBackupResult Failure(string message)
    {
        return new LanguageFileBackupResult(false, false, null, message);
    }
}

public sealed record LanguageBackupRestoreResult(
    bool Succeeded,
    string Message,
    string SourceLanguageCode,
    string? RestoredFilePath)
{
    public static LanguageBackupRestoreResult Success(GameLanguageFile languageFile, string backupFilePath)
    {
        return new LanguageBackupRestoreResult(
            true,
            $"Restored {languageFile.DisplayName} from backup '{backupFilePath}'.",
            languageFile.Code,
            languageFile.FullPath);
    }

    public static LanguageBackupRestoreResult Failure(string message, string sourceLanguageCode = "")
    {
        return new LanguageBackupRestoreResult(false, message, sourceLanguageCode, null);
    }
}
