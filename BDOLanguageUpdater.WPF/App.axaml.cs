using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BDOLanguageUpdater.WPF.Hosting;
using BDOLanguageUpdater.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BDOLanguageUpdater.WPF;

public class App : Application
{
    private readonly IHost host;

    public App()
    {
        this.host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => { context.RegisterServices(services); })
            .Build();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        await host.StartAsync();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = host.Services.GetService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Shutdown()
    {
        this.host.Dispose();
    }
}