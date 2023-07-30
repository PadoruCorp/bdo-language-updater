using BDOLanguageUpdater.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;

namespace BDOLanguageUpdater.WPF;

public class StartupHelper
{
    private const string RUN_KEY = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

    private readonly ILogger<LanguageUpdaterService> logger;

    public StartupHelper(ILogger<LanguageUpdaterService> logger)
    {
        this.logger = logger;
    }

    public void SetStartupOnBoot(bool enable)
    {
        var appName = Assembly.GetExecutingAssembly().GetName().Name;
        var appPath = GetAppExecutablePath();

        try
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(RUN_KEY, true);

            if (enable)
            {
                var value = registryKey.GetValue(appName);

                if (value != null)
                {
                    return;
                }

                registryKey.SetValue(appName, appPath);
            }
            else
            {
                registryKey.DeleteValue(appName, false);
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation("Error setting startup: " + ex.Message, DateTimeOffset.Now);
        }
    }

    public static string GetAppExecutablePath()
    {
        try
        {
            var assemblyLocation = Assembly.GetEntryAssembly().Location;

            return Path.GetFullPath(assemblyLocation).Replace(".dll", ".exe");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error getting app executable path: " + ex.Message);
            return string.Empty;
        }
    }
}