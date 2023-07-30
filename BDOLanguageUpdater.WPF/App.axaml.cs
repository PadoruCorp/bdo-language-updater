using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BDOLanguageUpdater.Service;
using BDOLanguageUpdater.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ReactiveUI;
using System;

namespace BDOLanguageUpdater.WPF;

public class App : Application
{
    public const string INITIALIZE_ON_TRAY_ARG = "InitializeOnTray";

    private readonly IHost host;
    private IClassicDesktopStyleApplicationLifetime desktop;
    private Window? myMainWindow;

    public App()
    {
        this.host = Host.CreateDefaultBuilder()
            .UseSerilog()
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
            this.desktop = desktop;

            SetupMainWindow(desktop.Args);

            RegisterTrayIcon();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Shutdown()
    {
        this.host.Dispose();
    }

    private void RegisterTrayIcon()
    {
        var trayIcon = new TrayIcon
        {
            IsVisible = true,
            ToolTipText = "BDO Language Updater",
            Menu = new NativeMenu
            {
                Items =
                {
                    new NativeMenuItem
                    {
                        Header = "Exit",
                        Command = ReactiveCommand.Create(CloseApplication)
                    }
                }
            },
            Command = ReactiveCommand.Create(ShowApplication),
            Icon = new WindowIcon(new Bitmap("Assets/icon.png"))
        };

        var trayIcons = new TrayIcons
        {
            trayIcon
        };

        SetValue(TrayIcon.IconsProperty, trayIcons);
    }

    private void SetupMainWindow(string[] args)
    {
        desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;

        var hasToShowWindow = args.Length <= 0 ||
                              !args[0].ToLower().Equals(INITIALIZE_ON_TRAY_ARG.ToLower());

        if (hasToShowWindow)
        {
            InitMainWindow();
        }
    }

    private void ShowApplication()
    {
        if (myMainWindow == null)
        {
            InitMainWindow();
        }

        myMainWindow.WindowState = WindowState.Normal;
        myMainWindow.Show();
    }

    private void CloseApplication()
    {
        var mainWindow = (MainWindow)myMainWindow!;
        
        mainWindow.ExitingFromTray = true;
        myMainWindow?.Close();
    }

    private void InitMainWindow()
    {
        desktop.MainWindow = host.Services.GetService<MainWindow>();
        myMainWindow = desktop.MainWindow;
    }
}