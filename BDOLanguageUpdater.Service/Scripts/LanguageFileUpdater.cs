using System;
using System.IO;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BDOLanguageUpdater.Service.Serializer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Padoru.Core.Files;

namespace BDOLanguageUpdater.Service;

public class LanguageFileUpdater
{
    private ILogger<LanguageUpdaterService> logger;
    private readonly IFileManager fileManager;
    private UrlMetadataOptions urlMetadataOptions;
    private string downloadedFilePath;
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

        downloadedFilePath = Path.Combine(Path.GetTempPath(), Constants.DOWNLOADED_FILE_NAME);
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

        downloadedFilePath = Constants.LOCAL_NOT_SERIALIZED_PROTOCOL_HEADER + 
                             downloadedFilePath.Replace(urlMetadataOptions.StringToReplaceOnUrl, version);

        var finalFileUri = Constants.HTTPS_NOT_SERIALIZED_PROTOCOL_HEADER +
                            urlMetadataOptions.FileUrl.Replace(urlMetadataOptions.StringToReplaceOnUrl, version);
        
        await DownloadFile(finalFileUri, downloadedFilePath);
        
        var oldFile = Constants.LOCAL_LOCALIZATION_PROTOCOL_HEADER + Path.Combine(blackDesertFilesPath,
            Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace(Constants.DEFAULT_STRING_TO_REPLACE_ON_FILE, "es"));
        var newFile = Constants.LOCAL_LOCALIZATION_PROTOCOL_HEADER + Path.Combine(Path.GetTempPath(),
            Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace(Constants.DEFAULT_STRING_TO_REPLACE_ON_FILE, "en"));

        await MoveFile(oldFile, newFile);
        
        // TODO: Rework this logic with the steps listed bellow
        // 1. Get version
        // 2. Download file to memory with decryption
        // 3. Read current file with decryption
        // 4. Merge files
        // 5. Replace file
    }

    public async Task<string> GetVersion(string uri)
    {
        var file = await this.fileManager.Read<string>(uri);

        if (file is null)
        {
            throw new InvalidOperationException("The language file version could not be obtained.");
        }

        var numbers = GetStringNumbers(file.Data);

        var version = numbers[urlMetadataOptions.VersionNumberIndex];

        return version;
    }

    public async Task DownloadFile(string fileUri, string destinationFileUri)
    {
        var file = await this.fileManager.Read<byte[]>(fileUri);

        await this.fileManager.Write(destinationFileUri, file.Data);
    }

    public async Task MoveFile(string oldFileUri, string newFileUri)
    {
        var oldFile = await fileManager.Read<string>(oldFileUri);
        var newFile = await fileManager.Read<string>(newFileUri);
        var finalFile = DictionaryUtils.Merge(newFile.Data, oldFile.Data);
        await fileManager.Write(oldFileUri, finalFile);
        await fileManager.Delete(newFileUri);
    }

    private string[] GetStringNumbers(string value)
    {
        var numbers = Regex.Split(value, urlMetadataOptions.Regex);
        return numbers;
    }
}