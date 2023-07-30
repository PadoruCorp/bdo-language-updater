using BDOLanguageUpdater.Service;
using BDOLanguageUpdater.WPF.ViewModels;
using BDOLanguageUpdater.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BDOLanguageUpdater.WPF;

public static class HostBuilderContextExtensions
{
    public static void RegisterServices(this HostBuilderContext context, IServiceCollection services)
    {
        // Language Updater
        context.RegisterUpdaterServices(services);

        // UI
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<StartupHelper>();

        services.AddHostedService<NotificationsManager>();
    }
}