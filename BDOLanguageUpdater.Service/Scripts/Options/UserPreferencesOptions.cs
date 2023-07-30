namespace BDOLanguageUpdater.Service;

public class UserPreferencesOptions
{
    public const string UserPreferences = "UserPreferences";

    public string BDOClientPath { get; set; } = Constants.DEFAULT_BLACK_DESERT_CLIENT_PATH;
    public bool HideToTrayOnClose { get; set; } = Constants.DEFAULT_MINIMIZE_TO_TRAY;
    public bool OpenOnStartup { get; set; } = Constants.DEFAULT_OPEN_ON_STARTUP;
}
