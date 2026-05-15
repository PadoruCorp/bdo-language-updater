using System.Diagnostics;
using BDOLanguageUpdater.Service;

namespace BDOLanguageUpdater.WPF;

public sealed class GameLauncher
{
    public GameLaunchResult LaunchSteam()
    {
        return LaunchTarget(
            Constants.STEAM_GAME_LAUNCH_TARGET,
            AppContext.BaseDirectory,
            "Launching Black Desert Online through Steam.");
    }

    public GameLaunchResult LaunchStandalone(string? bdoClientPath)
    {
        if (string.IsNullOrWhiteSpace(bdoClientPath))
        {
            return GameLaunchResult.Failure("Select the Black Desert Online folder before launching the game.");
        }

        var clientPath = GetClientRootPath(bdoClientPath);
        var launcherPath = Path.Combine(clientPath, Constants.BLACK_DESERT_LAUNCHER_FILE_NAME);
        if (!File.Exists(launcherPath))
        {
            return GameLaunchResult.Failure($"Could not find '{Constants.BLACK_DESERT_LAUNCHER_FILE_NAME}' in '{clientPath}'.");
        }

        return LaunchTarget(
            launcherPath,
            clientPath,
            "Launching Black Desert Online through the BDO launcher.");
    }

    public GameLaunchResult LaunchCustomTarget(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return GameLaunchResult.Failure("The game launch target is empty.");
        }

        return LaunchTarget(
            target.Trim(),
            AppContext.BaseDirectory,
            $"Launching Black Desert Online with '{target.Trim()}'.");
    }

    private static GameLaunchResult LaunchTarget(string target, string workingDirectory, string successMessage)
    {
        try
        {
            Process.Start(new ProcessStartInfo(target)
            {
                UseShellExecute = true,
                WorkingDirectory = workingDirectory,
            });

            return GameLaunchResult.Success(successMessage);
        }
        catch (Exception exception)
        {
            return GameLaunchResult.Failure($"Could not launch Black Desert Online: {exception.Message}");
        }
    }

    private static string GetClientRootPath(string bdoClientPath)
    {
        var trimmedPath = bdoClientPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var directoryName = Path.GetFileName(trimmedPath);
        if (!directoryName.Equals(Constants.BLACK_DESERT_LANGUAGE_FILES_PATH, StringComparison.OrdinalIgnoreCase))
        {
            return trimmedPath;
        }

        return Directory.GetParent(trimmedPath)?.FullName ?? trimmedPath;
    }
}

public sealed record GameLaunchResult(bool Succeeded, string Message)
{
    public static GameLaunchResult Success(string message)
    {
        return new GameLaunchResult(true, message);
    }

    public static GameLaunchResult Failure(string message)
    {
        return new GameLaunchResult(false, message);
    }
}
