using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace BDLanguageUpdater.Scripts
{
    public class LanguageFileUpdater
    {
        private DirectoriesModel directoriesModel;
        private ConfigFile configFile;

        private string version;

        public event Action OnFileDownloadStart;
        public event Action OnFileReplaceStart;
        public event Action OnUpdateFinish;
        public event Action<DownloadProgressChangedEventArgs> OnDownloadProgressChanged;

        public LanguageFileUpdater(DirectoriesModel directoriesModel, ConfigFile configFile)
        {
            this.directoriesModel = directoriesModel;
            this.configFile = configFile;
        }

        public async void UpdateFile()
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions
            try
            {
                var versionUrl = configFile.versionUrl;

                // Create a New HttpClient object.
                HttpClient client = new HttpClient();

                HttpResponseMessage response = await client.GetAsync(versionUrl);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var numbers = GetStringNumbers(responseBody);

                version = numbers[2];
                directoriesModel.downloadedFilePath = directoriesModel.downloadedFilePath.Replace("###", version);

                DownloadFile();

                // Need to call dispose on the HttpClient object
                // when done using it, so the app doesn't leak resources
                client.Dispose();
            }
            catch (HttpRequestException e)
            {
                ErrorDialog.Show(e.Message);
            }
        }

        private void DownloadFile()
        {
            var finalFileLink = configFile.fileUrl.Replace("###", version);

            OnFileDownloadStart?.Invoke();

            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFileCompleted += (a, b) => ReplaceFile();
                    client.DownloadProgressChanged += (a, args) => OnDownloadProgressChanged?.Invoke(args);
                    client.DownloadFileAsync(new Uri(finalFileLink), directoriesModel.downloadedFilePath);
                }
                catch (Exception e)
                {
                    ErrorDialog.Show(e.Message);
                }
            }
        }

        private void ReplaceFile()
        {
            OnFileReplaceStart?.Invoke();

            var oldFile = Path.Combine(directoriesModel.blackDesertFilesPath, Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace("##", "es"));
            var newFile = Path.Combine(directoriesModel.desktopPath, Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace("##", "en"));

            File.Delete(oldFile);

            File.Copy(newFile, oldFile);

            File.Delete(newFile);

            OnUpdateFinish?.Invoke();
        }

        private string[] GetStringNumbers(string value)
        {
            var numbers = Regex.Split(value, configFile.regex);
            return numbers;
        }
    }
}
