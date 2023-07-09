using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BDLanguageUpdater.Service;
using BDOLanguageUpdater.WPF.ViewModels;
using BDOLanguageUpdater.WPF.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BDOLanguageUpdater.WPF;

public class App : Application
{
    private readonly IHost host;
 
    public App()
    {
        host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context.Configuration, services);
            })
            .Build();
    }
 
    private void ConfigureServices(
        IConfiguration configuration, 
        IServiceCollection services)
    {
        services.AddHostedService<LanguageUpdaterService>();
        services.AddHostedService<NotificationsManager>();

        services.AddSingleton<LanguageFileUpdater>();
        services.ConfigureWritable<UserPreferencesOptions>(configuration.GetSection(UserPreferencesOptions.UserPreferences));
        services.ConfigureWritable<UrlMetadataOptions>(configuration.GetSection(UrlMetadataOptions.UrlMetadata));
    }
 
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }
        
        await host.StartAsync();
 
        base.OnFrameworkInitializationCompleted();
    }
}