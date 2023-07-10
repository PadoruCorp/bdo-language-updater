using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BDLanguageUpdater.Service;

public static class HostBuilderExtensions
{
    public static void RegisterUpdaterServices(this HostBuilderContext context, IServiceCollection services)
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

    public static IHostBuilder UseSerilog(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((_, configuration) =>
        {
            configuration
                .WriteTo.File(Constants.LOGS_FILE_NAME, LogEventLevel.Information)
                .WriteTo.Debug();
        });

        return hostBuilder;
    }
}