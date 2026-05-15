using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BDOLanguageUpdater.WPF;

public sealed class AutoRepairScheduler
{
    private const string TaskName = "BDOLanguageUpdater Auto Update";
    private const string LegacyTaskName = "BDOLanguageUpdater Auto Repair";
    private const string StartTime = "08:00";
    private const string RepeatIntervalMinutes = "120";
    private const string RepeatDuration = "16:00";

    public bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public async Task<AutoRepairSchedulerResult> CreateOrUpdate(DayOfWeek day)
    {
        if (!IsSupported)
        {
            return AutoRepairSchedulerResult.Failure("Automatic update scheduling is only available on Windows.");
        }

        var executablePath = GetExecutablePath();
        if (!File.Exists(executablePath))
        {
            return AutoRepairSchedulerResult.Failure($"Could not find the application executable at '{executablePath}'.");
        }

        var result = await RunSchtasks(
            "/Create",
            "/TN", TaskName,
            "/TR", $"\"{executablePath}\" --update --quiet",
            "/SC", "WEEKLY",
            "/D", ToSchtasksDay(day),
            "/ST", StartTime,
            "/RI", RepeatIntervalMinutes,
            "/DU", RepeatDuration,
            "/F").ConfigureAwait(false);

        if (!result.Succeeded)
        {
            return AutoRepairSchedulerResult.Failure(result.Message);
        }

        await DeleteTaskIfExists(LegacyTaskName).ConfigureAwait(false);
        return AutoRepairSchedulerResult.Success($"Enabled. Runs every {day} from {StartTime} and retries every 2 hours that day.");
    }

    public async Task<AutoRepairSchedulerResult> Delete()
    {
        if (!IsSupported)
        {
            return AutoRepairSchedulerResult.Failure("Automatic update scheduling is only available on Windows.");
        }

        if (!await Exists().ConfigureAwait(false))
        {
            return AutoRepairSchedulerResult.Success("Automatic update is disabled.");
        }

        var result = await DeleteTaskIfExists(TaskName).ConfigureAwait(false);
        var legacyResult = await DeleteTaskIfExists(LegacyTaskName).ConfigureAwait(false);

        return result.Succeeded && legacyResult.Succeeded
            ? AutoRepairSchedulerResult.Success("Automatic update is disabled.")
            : AutoRepairSchedulerResult.Failure(string.Join(
                Environment.NewLine,
                new[] { result.Message, legacyResult.Message }.Where(message => !string.IsNullOrWhiteSpace(message))));
    }

    public async Task<bool> Exists()
    {
        if (!IsSupported)
        {
            return false;
        }

        return await TaskExists(TaskName).ConfigureAwait(false) ||
               await TaskExists(LegacyTaskName).ConfigureAwait(false);
    }

    private static async Task<AutoRepairSchedulerResult> DeleteTaskIfExists(string taskName)
    {
        if (!await TaskExists(taskName).ConfigureAwait(false))
        {
            return AutoRepairSchedulerResult.Success(string.Empty);
        }

        return await RunSchtasks("/Delete", "/TN", taskName, "/F").ConfigureAwait(false);
    }

    private static async Task<bool> TaskExists(string taskName)
    {
        var result = await RunSchtasks("/Query", "/TN", taskName).ConfigureAwait(false);
        return result.Succeeded;
    }

    private static string GetExecutablePath()
    {
        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(processPath) && Path.GetExtension(processPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return processPath;
        }

        var expectedAppHostPath = Path.Combine(AppContext.BaseDirectory, "BDOLanguageUpdater.exe");
        if (File.Exists(expectedAppHostPath))
        {
            return expectedAppHostPath;
        }

        return processPath ?? expectedAppHostPath;
    }

    private static string ToSchtasksDay(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "MON",
            DayOfWeek.Tuesday => "TUE",
            DayOfWeek.Wednesday => "WED",
            DayOfWeek.Thursday => "THU",
            DayOfWeek.Friday => "FRI",
            DayOfWeek.Saturday => "SAT",
            DayOfWeek.Sunday => "SUN",
            _ => throw new ArgumentOutOfRangeException(nameof(day), day, null),
        };
    }

    private static async Task<AutoRepairSchedulerResult> RunSchtasks(params string[] arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("schtasks.exe")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
        await process.WaitForExitAsync().ConfigureAwait(false);

        var message = string.IsNullOrWhiteSpace(error) ? output.Trim() : error.Trim();
        return process.ExitCode == 0
            ? AutoRepairSchedulerResult.Success(message)
            : AutoRepairSchedulerResult.Failure(message);
    }
}

public sealed record AutoRepairSchedulerResult(bool Succeeded, string Message)
{
    public static AutoRepairSchedulerResult Success(string message)
    {
        return new AutoRepairSchedulerResult(true, message);
    }

    public static AutoRepairSchedulerResult Failure(string message)
    {
        return new AutoRepairSchedulerResult(false, message);
    }
}
