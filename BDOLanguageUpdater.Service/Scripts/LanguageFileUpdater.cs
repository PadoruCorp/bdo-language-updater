using System;
using System.IO;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BDOLanguageUpdater.Service.Serializer;
using Microsoft.Extensions.Logging;
using Padoru.Core.Files;

namespace BDOLanguageUpdater.Service;

public class LanguageFileUpdater
{
    private ILogger<LanguageUpdaterService> logger;
    private readonly IFileManager fileManager;
    private readonly LanguageFileDiscovery languageFileDiscovery;
    private readonly LanguageUpdateMetadataStore metadataStore;
    private readonly LanguageFileBackupStore backupStore;
    private readonly UserPreferencesOptions userPreferencesOptions;
    private UrlMetadataOptions urlMetadataOptions;
    private string blackDesertFilesPath;

    public LanguageFileUpdater(
        ILogger<LanguageUpdaterService> logger,
        IOptionsSnapshot<UrlMetadataOptions> urlMetadataOptions,
        IOptionsSnapshot<UserPreferencesOptions> userPreferencesOptions,
        IFileManager fileManager,
        LanguageFileDiscovery languageFileDiscovery,
        LanguageUpdateMetadataStore metadataStore,
        LanguageFileBackupStore backupStore)
    {
        this.logger = logger;
        this.fileManager = fileManager;
        this.languageFileDiscovery = languageFileDiscovery;
        this.metadataStore = metadataStore;
        this.backupStore = backupStore;
        this.userPreferencesOptions = userPreferencesOptions.Value;
        this.urlMetadataOptions = urlMetadataOptions.Value;

        blackDesertFilesPath = languageFileDiscovery.GetAdsPath(this.userPreferencesOptions.BDOClientPath);
    }

    public async Task<LanguageUpdateResult> UpdateFile(string? languageCodeToReplace = null)
    {
        var selectedLanguageCode = string.IsNullOrWhiteSpace(languageCodeToReplace)
            ? userPreferencesOptions.LanguageCodeToReplace
            : languageCodeToReplace;

        if (!Directory.Exists(blackDesertFilesPath))
        {
            logger.LogError($"Cannot download file for unexisting path: {blackDesertFilesPath}");
            return LanguageUpdateResult.Failure(
                $"Could not find the Black Desert Online ads folder at '{blackDesertFilesPath}'. Select the game folder and scan again.",
                selectedLanguageCode);
        }

        var languageToReplace = languageFileDiscovery.FindLanguage(
            userPreferencesOptions.BDOClientPath,
            selectedLanguageCode);

        if (languageToReplace is null)
        {
            logger.LogError($"Could not find language file to replace: {selectedLanguageCode}");
            return LanguageUpdateResult.Failure(
                $"Could not find languagedata_{selectedLanguageCode}.loc in '{blackDesertFilesPath}'. Choose one of the detected installed languages.",
                selectedLanguageCode);
        }
        
        var versionUri = Constants.HTTPS_STRING_PROTOCOL_HEADER + urlMetadataOptions.VersionUrl;
        var version = await GetVersion(versionUri).ConfigureAwait(false);

        if (await metadataStore.MatchesCurrentFile(languageToReplace, version).ConfigureAwait(false))
        {
            logger.LogInformation("Skipping language update. {file} already matches target version {version}.",
                languageToReplace.FullPath,
                version);
            return LanguageUpdateResult.Skipped(languageToReplace);
        }

        var downloadedFileUri = Constants.HTTPS_LOCALIZATION_PROTOCOL_HEADER +
                            urlMetadataOptions.FileUrl.Replace(urlMetadataOptions.StringToReplaceOnUrl, version);
        var localFileUri = Constants.LOCAL_LOCALIZATION_PROTOCOL_HEADER + languageToReplace.FullPath;

        var downloadedFileContent = await fileManager.Read<string>(downloadedFileUri).ConfigureAwait(false);
        var localFileContent = await fileManager.Read<string>(localFileUri).ConfigureAwait(false);

        var finalFileContent = DictionaryUtils.Merge(downloadedFileContent.Data, localFileContent.Data);
        var backupResult = await backupStore.SaveBeforeUpdate(languageToReplace).ConfigureAwait(false);
        if (!backupResult.Succeeded)
        {
            logger.LogError("Language update cancelled because backup creation failed: {message}", backupResult.Message);
            return LanguageUpdateResult.Failure(
                $"{backupResult.Message} The language file was not changed.",
                selectedLanguageCode);
        }

        await fileManager.Write(localFileUri, finalFileContent).ConfigureAwait(false);

        try
        {
            await metadataStore.Write(languageToReplace, version).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not write language update metadata for {file}.", languageToReplace.FullPath);
        }

        return LanguageUpdateResult.Success(languageToReplace, backupResult);
    }

    public async Task<LanguageBackupRestoreResult> RestoreBackup(string? languageCodeToReplace = null)
    {
        var selectedLanguageCode = string.IsNullOrWhiteSpace(languageCodeToReplace)
            ? userPreferencesOptions.LanguageCodeToReplace
            : languageCodeToReplace;

        if (!Directory.Exists(blackDesertFilesPath))
        {
            logger.LogError($"Cannot restore backup for unexisting path: {blackDesertFilesPath}");
            return LanguageBackupRestoreResult.Failure(
                $"Could not find the Black Desert Online ads folder at '{blackDesertFilesPath}'. Select the game folder and scan again.",
                selectedLanguageCode);
        }

        var languageToRestore = ResolveLanguageForRestore(selectedLanguageCode);
        return await backupStore.Restore(languageToRestore).ConfigureAwait(false);
    }

    public async Task<string> GetVersion(string uri)
    {
        var file = await fileManager.Read<string>(uri).ConfigureAwait(false);

        if (file is null)
        {
            throw new InvalidOperationException("The language file version could not be obtained.");
        }

        return GetVersionNumber(file.Data);
    }

    private string GetVersionNumber(string value)
    {
        var match = Regex.Match(value, $@"{urlMetadataOptions.DefaultLanguageToTranslate}\.loc\s*\t\s*(\d+)");
        if (!match.Success) throw new InvalidOperationException("The language file version could not be obtained.");
        var number = int.Parse(match.Groups[1].Value);
        return number.ToString();
    }

    private GameLanguageFile ResolveLanguageForRestore(string languageCode)
    {
        var detectedLanguage = languageFileDiscovery.FindLanguage(userPreferencesOptions.BDOClientPath, languageCode);
        if (detectedLanguage is not null)
        {
            return detectedLanguage;
        }

        var fileName = Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace(
            Constants.DEFAULT_STRING_TO_REPLACE_ON_FILE,
            languageCode);
        var filePath = Path.Combine(blackDesertFilesPath, fileName);

        return new GameLanguageFile(languageCode, fileName, $"{languageCode} ({languageCode})", filePath);
    }
}
