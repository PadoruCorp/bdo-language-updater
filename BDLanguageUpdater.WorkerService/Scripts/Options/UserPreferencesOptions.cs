namespace BDLanguageUpdater.WorkerService;

public class UserPreferencesOptions
{
    public const string UserPreferences = "UserPreferences";

    public string BDOClientPath { get; set; } = Constants.DEFAULT_BLACK_DESERT_CLIENT_PATH;
    public int FileCheckInterval { get; set; } = Constants.DEFAULT_FILE_CHECK_INTERVAL;
}
