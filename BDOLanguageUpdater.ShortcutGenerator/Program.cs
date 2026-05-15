using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace BDOLanguageUpdater.ShortcutGenerator;

[SupportedOSPlatform("windows")]
internal class Program
{
    private static void Main(string[] args)
    {
        var shortcutPath = args[0];
        var appExePath = args[1];
        var appFolder = args[2];
        var trayArgument = args[3];

        object? shell = null;
        object? link = null;

        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell")
                            ?? throw new InvalidOperationException("WScript.Shell is not available.");

            shell = Activator.CreateInstance(shellType)
                    ?? throw new InvalidOperationException("Could not create WScript.Shell.");

            link = shellType.InvokeMember(
                "CreateShortcut",
                BindingFlags.InvokeMethod,
                binder: null,
                target: shell,
                args: [shortcutPath]);

            if (link is null)
            {
                throw new InvalidOperationException("Could not create the shortcut COM object.");
            }

            var linkType = link.GetType();
            SetProperty(linkType, link, "IconLocation", appExePath);
            SetProperty(linkType, link, "TargetPath", appExePath);
            SetProperty(linkType, link, "Arguments", trayArgument);
            SetProperty(linkType, link, "WorkingDirectory", appFolder);
            linkType.InvokeMember("Save", BindingFlags.InvokeMethod, binder: null, target: link, args: null);
        }
        finally
        {
            ReleaseComObject(link);
            ReleaseComObject(shell);
        }
    }

    private static void SetProperty(Type targetType, object target, string propertyName, object value)
    {
        targetType.InvokeMember(
            propertyName,
            BindingFlags.SetProperty,
            binder: null,
            target: target,
            args: [value]);
    }

    private static void ReleaseComObject(object? value)
    {
        if (value is not null && Marshal.IsComObject(value))
        {
            Marshal.FinalReleaseComObject(value);
        }
    }
}
