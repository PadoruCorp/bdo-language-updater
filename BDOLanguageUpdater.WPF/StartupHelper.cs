using BDOLanguageUpdater.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
#if ON_W10
using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
#endif

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
#if ON_W10
        var appName = Assembly.GetExecutingAssembly().GetName().Name;
        var appPath = Path.GetFullPath(Assembly.GetEntryAssembly().Location).Replace(".dll", ".exe");

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

                var command = $"\"{appPath}\" {App.INITIALIZE_ON_TRAY_ARG}";

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
#endif
    }
}