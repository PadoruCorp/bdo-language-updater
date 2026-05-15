using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BDOLanguageUpdater.Service;
using BDOLanguageUpdater.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;

namespace BDOLanguageUpdater.WPF;

public class App : Application
{
    public const string INITIALIZE_ON_TRAY_ARG = "--background";

    private readonly IHost host;
    private IClassicDesktopStyleApplicationLifetime? desktop;
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

    private void SetupMainWindow(string[]? args)
    {
        Desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;

        var hasToShowWindow = args is null ||
                              args.Length <= 0 ||
                              !string.Equals(args[0], INITIALIZE_ON_TRAY_ARG, StringComparison.OrdinalIgnoreCase);

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

        var mainWindow = myMainWindow ?? throw new InvalidOperationException("Main window could not be created.");
        mainWindow.WindowState = WindowState.Normal;
        mainWindow.Show();
    }

    private void CloseApplication()
    {
        if (myMainWindow is MainWindow mainWindow)
        {
            mainWindow.ExitingFromTray = true;
            mainWindow.Close();
        }
    }

    private void InitMainWindow()
    {
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        Desktop.MainWindow = mainWindow;
        myMainWindow = mainWindow;
    }

    private IClassicDesktopStyleApplicationLifetime Desktop =>
        desktop ?? throw new InvalidOperationException("The desktop application lifetime is not available.");
}
