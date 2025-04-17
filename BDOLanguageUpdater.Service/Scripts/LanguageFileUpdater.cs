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
    private UrlMetadataOptions urlMetadataOptions;
    private string blackDesertFilesPath;

    public LanguageFileUpdater(
        ILogger<LanguageUpdaterService> logger,
        IOptionsSnapshot<UrlMetadataOptions> urlMetadataOptions,
        IOptionsSnapshot<UserPreferencesOptions> userPreferencesOptions,
        IFileManager fileManager)
    {
        this.logger = logger;
        this.fileManager = fileManager;
        this.urlMetadataOptions = urlMetadataOptions.Value;

        blackDesertFilesPath = Path.Combine(userPreferencesOptions.Value.BDOClientPath,
            Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);
    }

    public async Task UpdateFile()
    {
        if (!Directory.Exists(blackDesertFilesPath))
        {
            logger.LogError($"Cannot download file for unexisting path: {blackDesertFilesPath}");
            return;
        }
        
        var versionUri = Constants.HTTPS_STRING_PROTOCOL_HEADER + urlMetadataOptions.VersionUrl;
        var version = await GetVersion(versionUri);

        var downloadedFileUri = Constants.HTTPS_LOCALIZATION_PROTOCOL_HEADER +
                            urlMetadataOptions.FileUrl.Replace(urlMetadataOptions.StringToReplaceOnUrl, version);
        var localFileUri = Constants.LOCAL_LOCALIZATION_PROTOCOL_HEADER + Path.Combine(blackDesertFilesPath,
            Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace(Constants.DEFAULT_STRING_TO_REPLACE_ON_FILE, "es"));

        var downloadedFileContent = fileManager.Read<string>(downloadedFileUri);
        var localFileContent = fileManager.Read<string>(localFileUri);

        await Task.WhenAll(downloadedFileContent, localFileContent);

        var finalFileContent = DictionaryUtils.Merge(downloadedFileContent.Result.Data, localFileContent.Result.Data);

        await fileManager.Write(localFileUri, finalFileContent);
    }

    public async Task<string> GetVersion(string uri)
    {
        var file = await fileManager.Read<string>(uri);

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