using System.IO;

namespace BDLanguageUpdater.Scripts
{
    public class UserPreferencesFile
    {
        private DirectoriesModel directoriesModel;

        public UserPreferencesFile(DirectoriesModel directoriesModel)
        {
            this.directoriesModel = directoriesModel;

            if (!File.Exists(directoriesModel.userPreferencesFilePath))
            {
                WriteFile(Constants.DEFAULT_BLACK_DESERT_CLIENT_PATH);
            }
            else
            {
                UpdateBDOClientPath(File.ReadAllText(directoriesModel.userPreferencesFilePath));
            }
        }

        public void WriteFile(string content)
        {
            UpdateBDOClientPath(content);

            File.WriteAllText(directoriesModel.userPreferencesFilePath, directoriesModel.blackDesertClientPath);
        }

        private void UpdateBDOClientPath(string path)
        {
            directoriesModel.blackDesertClientPath = path;

            directoriesModel.blackDesertFilesPath = Path.Combine(directoriesModel.blackDesertClientPath, Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);
        }
    }
}
