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
    private readonly UserPreferencesOptions userPreferencesOptions;
    private UrlMetadataOptions urlMetadataOptions;
    private string blackDesertFilesPath;

    public LanguageFileUpdater(
        ILogger<LanguageUpdaterService> logger,
        IOptionsSnapshot<UrlMetadataOptions> urlMetadataOptions,
        IOptionsSnapshot<UserPreferencesOptions> userPreferencesOptions,
        IFileManager fileManager,
        LanguageFileDiscovery languageFileDiscovery)
    {
        this.logger = logger;
        this.fileManager = fileManager;
        this.languageFileDiscovery = languageFileDiscovery;
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

        var downloadedFileUri = Constants.HTTPS_LOCALIZATION_PROTOCOL_HEADER +
                            urlMetadataOptions.FileUrl.Replace(urlMetadataOptions.StringToReplaceOnUrl, version);
        var localFileUri = Constants.LOCAL_LOCALIZATION_PROTOCOL_HEADER + languageToReplace.FullPath;

        var downloadedFileContent = await fileManager.Read<string>(downloadedFileUri).ConfigureAwait(false);
        var localFileContent = await fileManager.Read<string>(localFileUri).ConfigureAwait(false);

        var finalFileContent = DictionaryUtils.Merge(downloadedFileContent.Data, localFileContent.Data);

        await fileManager.Write(localFileUri, finalFileContent).ConfigureAwait(false);

        return LanguageUpdateResult.Success(languageToReplace);
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
}
