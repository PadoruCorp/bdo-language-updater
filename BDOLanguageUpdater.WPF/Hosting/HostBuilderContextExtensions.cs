using BDLanguageUpdater.Service;
using BDOLanguageUpdater.WPF.ViewModels;
using BDOLanguageUpdater.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BDOLanguageUpdater.WPF.Hosting;

public static class HostBuilderContextExtensions
{
    public static void RegisterServices(this HostBuilderContext context, IServiceCollection services)
    {
        // Language Updater
        services.AddHostedService((sp) => sp.GetRequiredService<LanguageUpdaterService>());
        services.AddSingleton<LanguageUpdaterService>();
        
        services.AddSingleton<LanguageFileWatcher>();
        services.AddScoped<LanguageFileUpdater>();
        services.AddHostedService<NotificationsManager>();
        
        // UI
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        // Options
        services.ConfigureWritable<UserPreferencesOptions>(context.Configuration.GetSection(UserPreferencesOptions.UserPreferences));
        services.ConfigureWritable<UrlMetadataOptions>(context.Configuration.GetSection(UrlMetadataOptions.UrlMetadata));
    }
}