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
}
