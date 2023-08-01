using BDOLanguageUpdater.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using File = System.IO.File;

namespace BDOLanguageUpdater.WPF;

public class StartupHelper
{
    private readonly ILogger<LanguageUpdaterService> logger;

    public StartupHelper(ILogger<LanguageUpdaterService> logger)
    {
        this.logger = logger;
    }

    public void SetStartupOnBoot(bool enable)
    {
        var appExePath = Assembly.GetEntryAssembly()!.Location.Replace(".dll", ".exe");
        var appFolder = Path.GetDirectoryName(appExePath);
        var shortcutPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup),
            Assembly.GetExecutingAssembly().GetName().Name + ".lnk");

        var directory = new DirectoryInfo(appFolder);
        var shortcutGeneratorPath = directory.GetFiles("*.exe").First(f => f.Name.Contains("ShortcutGenerator")).FullName;

        var argumentList = new List<string>
        {
            $"\"{shortcutPath}\"",
            $"\"{appExePath}\"",
            $"\"{appFolder}\"",
            App.INITIALIZE_ON_TRAY_ARG
        };

        var arguments = string.Join(' ', argumentList);

        if (enable)
        {
            DeleteIfExists(shortcutPath);

            var startInfo = new ProcessStartInfo(shortcutGeneratorPath)
            {
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                var result = Process.Start(startInfo);
                result?.WaitForExit();
                result?.Close();
            }
            catch (Win32Exception ex)
            {
                this.logger.LogError("Shortcut generator errored: {error}", ex.Message);
            }
        }
        else
        {
            DeleteIfExists(shortcutPath);
        }
    }

    private void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}