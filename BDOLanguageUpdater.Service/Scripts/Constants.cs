namespace BDOLanguageUpdater.Service;

public class Constants
{
    // Updater
    public const string HTTP_CLIENT_NAME = "bdo-language-updater";
    public const string LOGS_FILE_NAME = "logs.txt";
    
    // Defaults
    public const string DEFAULT_VERSION_URL = "naeu-o-dn.playblackdesert.com/UploadData/ads_files";
    public const string DEFAULT_FILE_URL = "naeu-o-dn.playblackdesert.com/UploadData/ads/languagedata_en/###/languagedata_en.loc";
    public const string DEFAULT_REGEX = @"\D+";
    public const string DEFAULT_BLACK_DESERT_CLIENT_PATH = "E:\\Steam\\steamapps\\common\\Black Desert Online";
    public const string DEFAULT_STRING_TO_REPLACE_ON_URL = "###";
    public const string DEFAULT_STRING_TO_REPLACE_ON_FILE = "##";
    public const int DEFAULT_VERSION_NUMBER_INDEX = 2;
    public const bool DEFAULT_MINIMIZE_TO_TRAY = true;
    public const bool DEFAULT_OPEN_ON_STARTUP = false;

    // Localization file
    public const string DOWNLOADED_FILE_NAME = "languagedata_en.loc";
    public const string BLACK_DESERT_LANGUAGE_FILES_PATH = "ads";
    public const string BLACK_DESERT_LANGUAGE_FILE_NAME = "languagedata_##.loc";
    public const string BLACK_DESERT_LANGUAGE_FILE_EXTENSION = ".loc";

    // Preferences
    public const string APP_LOCAL_FOLDER = "BDOLanguageUpdater";
    public const string USER_PREFERENCES_FILE_NAME = "UserPreferences.txt";
    public const string CONFIG_FILE_NAME = "Config.json";
    
    // File Manager
    public const string HTTPS_STRING_PROTOCOL_HEADER = "httpsString://";
    public const string HTTPS_NOT_SERIALIZED_PROTOCOL_HEADER = "httpsNotSerialized://";
    public const string LOCAL_NOT_SERIALIZED_PROTOCOL_HEADER = "localNotSerialized://";
    public const string LOCAL_NOT_SERIALIZED_TEST_PROTOCOL_HEADER = "localNotSerializedTest://";
    public const string LOCAL_LOCALIZATION_PROTOCOL_HEADER = "localLocalization://";
    public const string HTTPS_LOCALIZATION_PROTOCOL_HEADER = "httpsLocalization://";

}
