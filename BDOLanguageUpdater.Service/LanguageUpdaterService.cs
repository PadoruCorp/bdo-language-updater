using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BDOLanguageUpdater.Service;

public class LanguageUpdaterService : BackgroundService
{
    private readonly ILogger<LanguageUpdaterService> logger;
    private readonly IWritableOptions<UserPreferencesOptions> userPreferencesOptions;
    private readonly LanguageFileWatcher watcher;
    private readonly IServiceProvider serviceProvider;

    public event Action? OnFileUpdateStart;
    public event Action? OnFileUpdateFinish;

    public LanguageUpdaterService(ILogger<LanguageUpdaterService> logger,
                  IWritableOptions<UserPreferencesOptions> userPreferencesOptions,
                  LanguageFileWatcher watcher,
                  IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.userPreferencesOptions = userPreferencesOptions;
        this.watcher = watcher;
        this.serviceProvider = serviceProvider;

        watcher.OnFileChanged += OnFileChanged;
    }

    public async Task UpdateLanguage()
    {
        OnFileUpdateStart?.Invoke();
        watcher.OnFileChanged -= OnFileChanged;
        
        logger.LogInformation("Updating file: {time}", DateTimeOffset.Now);

        await using var scope = serviceProvider.CreateAsyncScope();

        var fileUpdater = scope.ServiceProvider.GetRequiredService<LanguageFileUpdater>();

        await fileUpdater.UpdateFile();

        logger.LogInformation("File updated: {time}", DateTimeOffset.Now);

        OnFileUpdateFinish?.Invoke();
        
        watcher.OnFileChanged += OnFileChanged;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private async void OnFileChanged()
    {
        await UpdateLanguage();
    }
}