using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BDLanguageUpdater.Service;

public class LanguageUpdaterService : BackgroundService
{
    private readonly ILogger<LanguageUpdaterService> logger;
    private readonly IWritableOptions<UserPreferencesOptions> userPreferencesOptions;
    private readonly IServiceProvider serviceProvider;
    private bool hasToUpdateFile;

    public event Action? OnFileUpdateStart;
    public event Action? OnFileUpdateFinish;

    public LanguageUpdaterService(ILogger<LanguageUpdaterService> logger,
                  IWritableOptions<UserPreferencesOptions> userPreferencesOptions,
                  LanguageFileWatcher watcher,
                  IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.userPreferencesOptions = userPreferencesOptions;
        this.serviceProvider = serviceProvider;

        watcher.OnFileChanged += OnFileChanged;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (hasToUpdateFile)
            {
                OnFileUpdateStart?.Invoke();

                logger.LogInformation("Updating file: {time}", DateTimeOffset.Now);

                await using var scope = serviceProvider.CreateAsyncScope();
                var fileUpdater = scope.ServiceProvider.GetRequiredService<LanguageFileUpdater>();

                await fileUpdater.UpdateFile();

                logger.LogInformation("File updated: {time}", DateTimeOffset.Now);

                hasToUpdateFile = false;

                OnFileUpdateFinish?.Invoke();
            }

            await Task.Delay(userPreferencesOptions.Value.FileCheckInterval, stoppingToken);
        }
    }

    private void OnFileChanged()
    {
        hasToUpdateFile = true;
    }
}