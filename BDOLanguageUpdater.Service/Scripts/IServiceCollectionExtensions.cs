using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BDLanguageUpdater.Service;

public static class IServiceCollectionExtensions
{
    public static void RegisterUpdaterServices(this IServiceCollection services, HostBuilderContext context)
    {
        // HTTP
        services.AddHttpClient(Constants.HTTP_CLIENT_NAME);
        
        // Services
        services.AddHostedService((sp) => sp.GetRequiredService<LanguageUpdaterService>());
        services.AddSingleton<LanguageUpdaterService>();
        services.AddSingleton<LanguageFileWatcher>();
        services.AddScoped<LanguageFileUpdater>();
        
        // Options
        services.ConfigureWritable<UserPreferencesOptions>(context.Configuration.GetSection(UserPreferencesOptions.UserPreferences));
        services.ConfigureWritable<UrlMetadataOptions>(context.Configuration.GetSection(UrlMetadataOptions.UrlMetadata));
    }
}