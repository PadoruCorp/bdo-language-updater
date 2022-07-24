using BDLanguageUpdater.Scripts;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Forms;

namespace BDLanguageUpdater
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var directoriesModel = CreateDirectoriesModel();

            var preferencesFile = new UserPreferencesFile(directoriesModel);

            var configFile = CreateConfigFile(directoriesModel);

            var languageFileUpdater = new LanguageFileUpdater(directoriesModel, configFile);

            var form = CreateForm(directoriesModel, preferencesFile, languageFileUpdater);

            Application.Run(form);
        }

        private static Form1 CreateForm(DirectoriesModel directoriesModel, UserPreferencesFile preferencesFile, LanguageFileUpdater languageFileUpdater)
        {
            var form = new Form1(directoriesModel, preferencesFile, languageFileUpdater);

            return form;
        }

        private static DirectoriesModel CreateDirectoriesModel()
        {
            var directoriesModel = new DirectoriesModel();

            directoriesModel.desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            directoriesModel.downloadedFilePath = Path.Combine(directoriesModel.desktopPath, Constants.DOWNLOADED_FILE_NAME);

            var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Constants.APP_LOCAL_FOLDER);

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            directoriesModel.userPreferencesFilePath = Path.Combine(appFolder, Constants.USER_PREFERENCES_FILE_NAME);

            directoriesModel.configFilePath = Path.Combine(appFolder, Constants.CONFIG_FILE_NAME);

            return directoriesModel;
        }

        private static ConfigFile CreateConfigFile(DirectoriesModel directoriesModel)
        {
            if (!File.Exists(directoriesModel.configFilePath))
            {
                var configFile = new ConfigFile()
                {
                    regex = Constants.DEFAULT_REGEX,
                    versionUrl = Constants.DEFAULT_VERSION_URL,
                    fileUrl = Constants.DEFAULT_FILE_URL,
                    stringToReplaceOnUrl = Constants.DEFAULT_STRING_TO_REPLACE_ON_URL,
                    stringToReplaceOnFile = Constants.DEFAULT_STRING_TO_REPLACE_ON_FILE,
                    versionNumberIndex = Constants.DEFAULT_VERSION_NUMBER_INDEX,
                };

                File.WriteAllText(directoriesModel.configFilePath, JsonConvert.SerializeObject(configFile));

                return configFile;
            }

            var json = File.ReadAllText(directoriesModel.configFilePath);
            return JsonConvert.DeserializeObject<ConfigFile>(json);
        }
    }
}
