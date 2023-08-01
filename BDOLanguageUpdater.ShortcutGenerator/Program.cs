using IWshRuntimeLibrary;

namespace BDOLanguageUpdater.ShortcutGenerator;

internal class Program
{
    private static void Main(string[] args)
    {
        var shortcutPath = args[0];
        var appExePath = args[1];
        var appFolder = args[2];
        var trayArgument = args[3];
        
        var shell = new WshShell();
        var link = (IWshShortcut)shell.CreateShortcut(shortcutPath);
        link.IconLocation = appExePath;
        link.TargetPath = appExePath;
        link.Arguments = trayArgument;
        link.WorkingDirectory = appFolder;
        link.Save();
    }
}
