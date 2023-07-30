using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;

namespace BDOLanguageUpdater.Service;

public class LanguageFileWatcher
{
    private readonly ILogger<LanguageUpdaterService> logger;
    private FileSystemWatcher? watcher;
    public Action? OnFileChanged;
    private readonly Stopwatch stopwatch;

    public LanguageFileWatcher(ILogger<LanguageUpdaterService> logger, IOptionsSnapshot<UserPreferencesOptions> userPreferencesOptions)
    {
        this.logger = logger;
        stopwatch = Stopwatch.StartNew();

        var path = Path.Combine(userPreferencesOptions.Value.BDOClientPath, Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);
        SetPath(path);
    }

    public void SetPath(string path)
    {
        if (!Directory.Exists(path))
        {
            logger.LogDebug($"Could not create file watcher. Path does not exist: {path}");
            return;
        }

        if (watcher != null)
        {
            watcher.Changed -= OnFileChangedCallback;
            watcher.Dispose();
        }

        watcher = new FileSystemWatcher(path);

        watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Size;

        watcher.Changed += OnFileChangedCallback;

        watcher.Filter = $"*{Constants.BLACK_DESERT_LANGUAGE_FILE_EXTENSION}";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
    }

    private void OnFileChangedCallback(object sender, FileSystemEventArgs e)
    {
        if (stopwatch.ElapsedMilliseconds < 1000) return;
        
        this.logger.LogInformation("Language file changed");
        OnFileChanged?.Invoke();
        
        stopwatch.Restart();
    }
}
