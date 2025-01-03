﻿using Avalonia;
using Avalonia.ReactiveUI;

namespace BDOLanguageUpdater.WPF;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
       var appBuilder = BuildAvaloniaApp();
       appBuilder.StartWithClassicDesktopLifetime(args);

       if (appBuilder?.Instance is null)
       {
           throw new InvalidOperationException();
       }
       
       var app = (App)appBuilder.Instance;
       app.Shutdown();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}