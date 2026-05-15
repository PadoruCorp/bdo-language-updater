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
    private readonly LanguageFileWatcher watcher;
    private readonly IServiceProvider serviceProvider;

    public event Action? OnFileUpdateStart;
    public event Action? OnFileUpdateFinish;
    public event Action<LanguageUpdateResult>? OnFileUpdateComplete;

    public LanguageUpdaterService(ILogger<LanguageUpdaterService> logger,
                  LanguageFileWatcher watcher,
                  IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.watcher = watcher;
        this.serviceProvider = serviceProvider;

        watcher.OnFileChanged += OnFileChanged;
    }

    public async Task<LanguageUpdateResult> UpdateLanguage(string? languageCodeToReplace = null)
    {
        OnFileUpdateStart?.Invoke();
        watcher.OnFileChanged -= OnFileChanged;
        
        logger.LogInformation("Updating file: {time}", DateTimeOffset.Now);

        LanguageUpdateResult result;
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var fileUpdater = scope.ServiceProvider.GetRequiredService<LanguageFileUpdater>();

            result = await fileUpdater.UpdateFile(languageCodeToReplace).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not update language file.");
            result = LanguageUpdateResult.Failure($"Could not update the language file: {exception.Message}");
        }
        finally
        {
            watcher.OnFileChanged += OnFileChanged;
        }

        logger.LogInformation("File update completed: {time}", DateTimeOffset.Now);

        if (result.Succeeded)
        {
            OnFileUpdateFinish?.Invoke();
        }

        OnFileUpdateComplete?.Invoke(result);

        return result;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private async void OnFileChanged()
    {
        await UpdateLanguage().ConfigureAwait(false);
    }
}
