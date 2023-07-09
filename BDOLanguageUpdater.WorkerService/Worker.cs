namespace BDLanguageUpdater.WorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly IWritableOptions<UrlMetadataOptions> urlMetadataOptions;
    private readonly IWritableOptions<UserPreferencesOptions> userPreferencesOptions;
    private readonly LanguageFileUpdater fileUpdater;
    private FileSystemWatcher watcher;
    private bool hasToUpdateFile;

    public Worker(ILogger<Worker> logger,
                  IWritableOptions<UrlMetadataOptions> urlMetadataOptions,
                  IWritableOptions<UserPreferencesOptions> userPreferencesOptions,
                  LanguageFileUpdater fileUpdater)
    {
        this.logger = logger;
        this.urlMetadataOptions = urlMetadataOptions;
        this.userPreferencesOptions = userPreferencesOptions;
        this.fileUpdater = fileUpdater;

        CreateWatcher();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (hasToUpdateFile)
            {
                logger.LogInformation("Updating file: {time}", DateTimeOffset.Now);

                await fileUpdater.UpdateFile();

                logger.LogInformation("File updated: {time}", DateTimeOffset.Now);

                hasToUpdateFile = false;
            }

            await Task.Delay(userPreferencesOptions.Value.FileCheckInterval, stoppingToken);
        }
    }
    
    private void CreateWatcher()
    {
        var path = Path.Combine(userPreferencesOptions.Value.BDOClientPath, Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);

        watcher = new FileSystemWatcher(path);

        watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Size;

        watcher.Changed += OnFileChanged;

        watcher.Filter = $"*{Constants.BLACK_DESERT_LANGUAGE_FILE_EXTENSION}";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
    }

    private async void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        hasToUpdateFile = true;
    }
}