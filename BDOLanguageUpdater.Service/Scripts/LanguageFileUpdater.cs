using System;
using System.IO;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BDLanguageUpdater.Service;

public class LanguageFileUpdater
{
    private ILogger<LanguageUpdaterService> logger;
    private UrlMetadataOptions urlMetadataOptions;
    private UserPreferencesOptions userPreferencesOptions;
    private string version;
    private string downloadedFilePath;
    private string blackDesertFilesPath;
    private string desktopPath;

    public LanguageFileUpdater(ILogger<LanguageUpdaterService> logger, IOptionsSnapshot<UrlMetadataOptions> urlMetadataOptions, IOptionsSnapshot<UserPreferencesOptions> userPreferencesOptions)
    {
        this.logger = logger;
        this.urlMetadataOptions = urlMetadataOptions.Value;
        this.userPreferencesOptions = userPreferencesOptions.Value;

        InitializePaths();
    }

    public async Task UpdateFile()
    {
        try
        {
            if (!Directory.Exists(blackDesertFilesPath))
            {
                logger.LogError($"Cannot download file for unexisting path: {blackDesertFilesPath}");
                return;
            }

            var version = await GetVersion();

            downloadedFilePath = downloadedFilePath.Replace(urlMetadataOptions.StringToReplaceOnUrl, version);

            await DownloadFile();

            MoveFile();
        }
        catch (HttpRequestException e)
        {
            logger.LogError(e.Message);
        }
    }

    private async Task<string> GetVersion()
    {
        var client = new HttpClient();

        var response = await client.GetAsync(urlMetadataOptions.VersionUrl);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var numbers = GetStringNumbers(responseBody);

        version = numbers[urlMetadataOptions.VersionNumberIndex];

        // Need to call dispose on the HttpClient object
        // when done using it, so the app doesn't leak resources
        client.Dispose();

        return version;
    }

    private async Task DownloadFile()
    {
        var finalFileLink = urlMetadataOptions.FileUrl.Replace(urlMetadataOptions.StringToReplaceOnUrl, version);

        var client = new WebClient();

        var downloadFinished = false;

        client.DownloadFileCompleted += (a, b) => downloadFinished = true;
        client.DownloadFileAsync(new Uri(finalFileLink), downloadedFilePath);

        while (!downloadFinished)
        {
            await Task.Yield();
        }

        client.Dispose();
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

    private void InitializePaths()
    {
        desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        downloadedFilePath = Path.Combine(desktopPath, Constants.DOWNLOADED_FILE_NAME);

        blackDesertFilesPath = Path.Combine(userPreferencesOptions.BDOClientPath, Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);
    }
}
