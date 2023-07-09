namespace BDLanguageUpdater.WorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IWritableOptions<UrlMetadataOptions> urlMetadataOptions;
    private readonly IWritableOptions<UserPreferencesOptions> userPreferencesOptions;
    private readonly LanguageFileUpdater fileUpdater;
    private FileSystemWatcher watcher;

    public Worker(ILogger<Worker> logger, 
                  IWritableOptions<UrlMetadataOptions> urlMetadataOptions,
                  IWritableOptions<UserPreferencesOptions> userPreferencesOptions,
                  LanguageFileUpdater fileUpdater)
    {
        _logger = logger;
        this.urlMetadataOptions = urlMetadataOptions;
        this.userPreferencesOptions = userPreferencesOptions;
        this.fileUpdater = fileUpdater;

        CreateWatcher();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
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
        watcher.Changed -= OnFileChanged;

        await fileUpdater.UpdateFile();

        watcher.Changed += OnFileChanged;
    }
}