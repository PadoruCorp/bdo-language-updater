using Microsoft.Extensions.Options;
using System.Net;
using System.Text.RegularExpressions;

namespace BDLanguageUpdater.WorkerService;

public class LanguageFileUpdater
{
    private ILogger<Worker> logger;
    private UrlMetadataOptions urlMetadataOptions;
    private UserPreferencesOptions userPreferencesOptions;
    private string version;
    private string downloadedFilePath;
    private string blackDesertFilesPath;
    private string desktopPath;

    public event Action OnFileDownloadStart;
    public event Action OnFileReplaceStart;
    public event Action OnUpdateFinish;
    public event Action<DownloadProgressChangedEventArgs> OnDownloadProgressChanged;

    public LanguageFileUpdater(ILogger<Worker> logger, IOptions<UrlMetadataOptions> urlMetadataOptions, IOptions<UserPreferencesOptions> userPreferencesOptions)
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
        HttpClient client = new HttpClient();

        HttpResponseMessage response = await client.GetAsync(urlMetadataOptions.VersionUrl);
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

        OnFileDownloadStart?.Invoke();

        var client = new WebClient();

        var downloadFinished = false;

        client.DownloadFileCompleted += (a, b) => downloadFinished = true;
        client.DownloadProgressChanged += (a, args) => OnDownloadProgressChanged?.Invoke(args);
        client.DownloadFileAsync(new Uri(finalFileLink), downloadedFilePath);

        while (!downloadFinished)
        {
            await Task.Yield();
        }

        client.Dispose();
    }

    private void MoveFile()
    {
        OnFileReplaceStart?.Invoke();

        var oldFile = Path.Combine(blackDesertFilesPath, Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace(Constants.DEFAULT_STRING_TO_REPLACE_ON_FILE, "es"));
        var newFile = Path.Combine(desktopPath, Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace(Constants.DEFAULT_STRING_TO_REPLACE_ON_FILE, "en"));

        File.Delete(oldFile);

        File.Copy(newFile, oldFile);

        File.Delete(newFile);

        OnUpdateFinish?.Invoke();
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

        var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Constants.APP_LOCAL_FOLDER);

        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }
    }
}
