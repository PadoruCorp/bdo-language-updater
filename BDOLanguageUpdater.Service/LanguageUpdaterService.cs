using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BDOLanguageUpdater.Service;

public class LanguageUpdaterService
{
    private readonly ILogger<LanguageUpdaterService> logger;
    private readonly IServiceProvider serviceProvider;

    public LanguageUpdaterService(ILogger<LanguageUpdaterService> logger,
                  IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public async Task<LanguageUpdateResult> UpdateLanguage(string? languageCodeToReplace = null)
    {
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
        logger.LogInformation("File update completed: {time}", DateTimeOffset.Now);

        return result;
    }

    public async Task<LanguageBackupRestoreResult> RestoreLanguageBackup(string? languageCodeToReplace = null)
    {
        logger.LogInformation("Restoring language backup: {time}", DateTimeOffset.Now);

        LanguageBackupRestoreResult result;
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var fileUpdater = scope.ServiceProvider.GetRequiredService<LanguageFileUpdater>();

            result = await fileUpdater.RestoreBackup(languageCodeToReplace).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not restore language backup.");
            result = LanguageBackupRestoreResult.Failure($"Could not restore the language backup: {exception.Message}");
        }

        logger.LogInformation("Language backup restore completed: {time}", DateTimeOffset.Now);

        return result;
    }
}
