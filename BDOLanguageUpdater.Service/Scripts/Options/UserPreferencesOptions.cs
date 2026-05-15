namespace BDOLanguageUpdater.Service;

public class UserPreferencesOptions
{
    public const string UserPreferences = "UserPreferences";

    public string BDOClientPath { get; set; } = Constants.DEFAULT_BLACK_DESERT_CLIENT_PATH;
    public string LanguageCodeToReplace { get; set; } = Constants.DEFAULT_LANGUAGE_CODE_TO_REPLACE;
    public bool HideToTrayOnClose { get; set; } = Constants.DEFAULT_MINIMIZE_TO_TRAY;
    public bool AutoRepairEnabled { get; set; } = Constants.DEFAULT_AUTO_REPAIR_ENABLED;
    public string AutoRepairDay { get; set; } = Constants.DEFAULT_AUTO_REPAIR_DAY;
}
