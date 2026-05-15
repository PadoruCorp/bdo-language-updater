using Avalonia;
using Avalonia.ReactiveUI;
using BDOLanguageUpdater.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BDOLanguageUpdater.WPF;

class Program
{
    private const string UpdateArgument = "--update";
    private const string UpdateAndLaunchArgument = "--update-and-launch";
    private const string RestoreBackupArgument = "--restore-backup";
    private const string QuietArgument = "--quiet";
    private const string LanguageArgumentPrefix = "--language=";
    private const string LaunchModeArgumentPrefix = "--launch=";
    private const string LaunchTargetArgumentPrefix = "--launch-target=";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Contains(UpdateArgument, StringComparer.OrdinalIgnoreCase) ||
            args.Contains(UpdateAndLaunchArgument, StringComparer.OrdinalIgnoreCase))
        {
            return RunHeadlessUpdate(args, launchAfterUpdate:
                    args.Contains(UpdateAndLaunchArgument, StringComparer.OrdinalIgnoreCase))
                .GetAwaiter()
                .GetResult();
        }

        if (args.Contains(RestoreBackupArgument, StringComparer.OrdinalIgnoreCase))
        {
            return RunHeadlessRestore(args)
                .GetAwaiter()
                .GetResult();
        }

        var appBuilder = BuildAvaloniaApp();
        var exitCode = appBuilder.StartWithClassicDesktopLifetime(args);

        if (appBuilder.Instance is null)
        {
            throw new InvalidOperationException();
        }

        var app = (App)appBuilder.Instance;
        app.Shutdown();

        return exitCode;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    private static async Task<int> RunHeadlessUpdate(string[] args, bool launchAfterUpdate)
    {
        var quiet = args.Contains(QuietArgument, StringComparer.OrdinalIgnoreCase);
        var languageCode = args.FirstOrDefault(arg => arg.StartsWith(LanguageArgumentPrefix, StringComparison.OrdinalIgnoreCase))
            ?.Substring(LanguageArgumentPrefix.Length);
        var launchMode = args.FirstOrDefault(arg => arg.StartsWith(LaunchModeArgumentPrefix, StringComparison.OrdinalIgnoreCase))
            ?.Substring(LaunchModeArgumentPrefix.Length);
        var launchTarget = args.FirstOrDefault(arg => arg.StartsWith(LaunchTargetArgumentPrefix, StringComparison.OrdinalIgnoreCase))
            ?.Substring(LaunchTargetArgumentPrefix.Length);

        using var host = ApplicationHost.Create(args);
        await host.StartAsync().ConfigureAwait(false);

        try
        {
            var languageUpdaterService = host.Services.GetRequiredService<LanguageUpdaterService>();
            var result = await languageUpdaterService.UpdateLanguage(languageCode).ConfigureAwait(false);

            if (result.Succeeded && launchAfterUpdate)
            {
                var options = host.Services.GetRequiredService<IWritableOptions<UserPreferencesOptions>>();
                var gameLauncher = host.Services.GetRequiredService<GameLauncher>();
                var launchResult = LaunchGame(gameLauncher, options.Value.BDOClientPath, launchMode, launchTarget);

                if (!quiet)
                {
                    Console.WriteLine(result.Message);
                    Console.WriteLine(launchResult.Message);
                }

                return launchResult.Succeeded ? 0 : 1;
            }

            if (!quiet)
            {
                Console.WriteLine(result.Message);
            }

            return result.Succeeded ? 0 : 1;
        }
        finally
        {
            await host.StopAsync().ConfigureAwait(false);
        }
    }

    private static GameLaunchResult LaunchGame(
        GameLauncher gameLauncher,
        string bdoClientPath,
        string? launchMode,
        string? launchTarget)
    {
        if (!string.IsNullOrWhiteSpace(launchTarget))
        {
            return gameLauncher.LaunchCustomTarget(launchTarget);
        }

        if (string.IsNullOrWhiteSpace(launchMode) ||
            launchMode.Equals("steam", StringComparison.OrdinalIgnoreCase))
        {
            return gameLauncher.LaunchSteam();
        }

        if (launchMode.Equals("launcher", StringComparison.OrdinalIgnoreCase) ||
            launchMode.Equals("standalone", StringComparison.OrdinalIgnoreCase))
        {
            return gameLauncher.LaunchStandalone(bdoClientPath);
        }

        return GameLaunchResult.Failure($"Unknown launch mode '{launchMode}'. Use 'steam' or 'launcher'.");
    }

    private static async Task<int> RunHeadlessRestore(string[] args)
    {
        var quiet = args.Contains(QuietArgument, StringComparer.OrdinalIgnoreCase);
        var languageCode = args.FirstOrDefault(arg => arg.StartsWith(LanguageArgumentPrefix, StringComparison.OrdinalIgnoreCase))
            ?.Substring(LanguageArgumentPrefix.Length);

        using var host = ApplicationHost.Create(args);
        await host.StartAsync().ConfigureAwait(false);

        try
        {
            var languageUpdaterService = host.Services.GetRequiredService<LanguageUpdaterService>();
            var result = await languageUpdaterService.RestoreLanguageBackup(languageCode).ConfigureAwait(false);

            if (!quiet)
            {
                Console.WriteLine(result.Message);
            }

            return result.Succeeded ? 0 : 1;
        }
        finally
        {
            await host.StopAsync().ConfigureAwait(false);
        }
    }
}
