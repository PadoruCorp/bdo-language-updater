namespace BDLanguageUpdater
{
    class Constants
    {
        public const string DEFAULT_VERSION_URL = "http://dn.sea.playblackdesert.com/UploadData/ads_files";
        public const string DEFAULT_FILE_URL = "http://dn.sea.playblackdesert.com/UploadData/ads/languagedata_en/###/languagedata_en.loc";
        public const string DEFAULT_REGEX = @"\D+";
        public const string DEFAULT_BLACK_DESERT_CLIENT_PATH = "C:/Pearlabyss/BlackDesert";
        public const string DEFAULT_STRING_TO_REPLACE_ON_URL = "###";
        public const string DEFAULT_STRING_TO_REPLACE_ON_FILE = "##";
        public const int DEFAULT_VERSION_NUMBER_INDEX = 2;

        public const string DOWNLOADED_FILE_NAME = "languagedata_en.loc";
        public const string BLACK_DESERT_LANGUAGE_FILES_PATH = "ads";
        public const string BLACK_DESERT_LANGUAGE_FILE_NAME = "languagedata_##.loc";

        public const string APP_LOCAL_FOLDER = "BDOLanguageUpdater";
        public const string USER_PREFERENCES_FILE_NAME = "UserPreferences.txt";
        public const string CONFIG_FILE_NAME = "Config.json";
    }
}
