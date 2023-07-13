using System;
using System.IO;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BDOLanguageUpdater.Service;

public class LanguageFileUpdater
{
    private ILogger<LanguageUpdaterService> logger;
    private UrlMetadataOptions urlMetadataOptions;
    private readonly HttpClient httpClient;
    private string? version;
    private string downloadedFilePath;
    private string blackDesertFilesPath;
    private string desktopPath;

    public LanguageFileUpdater(
        ILogger<LanguageUpdaterService> logger,
        IOptionsSnapshot<UrlMetadataOptions> urlMetadataOptions,
        IOptionsSnapshot<UserPreferencesOptions> userPreferencesOptions,
        IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        this.urlMetadataOptions = urlMetadataOptions.Value;
        this.httpClient = httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);
        
        desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        downloadedFilePath = Path.Combine(desktopPath, Constants.DOWNLOADED_FILE_NAME);
        blackDesertFilesPath = Path.Combine(userPreferencesOptions.Value.BDOClientPath, Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);
    }

    public async Task UpdateFile()
    {
        if (!Directory.Exists(blackDesertFilesPath))
        {
            logger.LogError($"Cannot download file for unexisting path: {blackDesertFilesPath}");
            return;
        }

        var latestVersion = await GetVersion();

        downloadedFilePath = downloadedFilePath.Replace(urlMetadataOptions.StringToReplaceOnUrl, latestVersion);

        await DownloadFile();

        MoveFile();
    }

    private async Task<string> GetVersion()
    {
        var response = await this.httpClient.GetAsync(urlMetadataOptions.VersionUrl);
        
        if (!response.IsSuccessStatusCode)
        {
            this.logger.LogError("Language version metadata download failed. Error: {statusCode}", response.StatusCode);
            throw new HttpRequestException($"Language version metadata download failed. Error: {response.StatusCode}");
        }
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var numbers = GetStringNumbers(responseBody);

        version = numbers[urlMetadataOptions.VersionNumberIndex];

        return version;
    }

    private async Task DownloadFile()
    {
        var finalFileLink = urlMetadataOptions.FileUrl.Replace(urlMetadataOptions.StringToReplaceOnUrl, version);

        var response = await this.httpClient.GetAsync(new Uri(finalFileLink));

        if (!response.IsSuccessStatusCode)
        {
            this.logger.LogError("Language file download failed. Error: {statusCode}", response.StatusCode);
            throw new HttpRequestException($"Language file download failed. Error: {response.StatusCode}");
        }

        await using var fs = new FileStream(downloadedFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        await response.Content.CopyToAsync(fs);
    }

    private void MoveFile()
    {
        var oldFile = Path.Combine(blackDesertFilesPath, Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace(Constants.DEFAULT_STRING_TO_REPLACE_ON_FILE, "es"));
        var newFile = Path.Combine(desktopPath, Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace(Constants.DEFAULT_STRING_TO_REPLACE_ON_FILE, "en"));

        File.Delete(oldFile);

        File.Copy(newFile, oldFile);

        File.Delete(newFile);
    }

    private string[] GetStringNumbers(string value)
    {
        var numbers = Regex.Split(value, urlMetadataOptions.Regex);
        return numbers;
    }
}