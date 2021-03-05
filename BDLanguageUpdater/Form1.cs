using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;
using System.IO;
using System.IO.Compression;

namespace BDLanguageUpdater
{
    public partial class Form1 : Form
    {
        public string blackDesertClientPath = "C:/Pearlabyss/BlackDesert";
        public string blackDesertFilesPath;
        public string userPreferencesFile;
        
        private string downloadedFilePath;
        private string desktopPath;

        private string version;
        private bool downloading;

        public Form1()
        {
            InitializeComponent();

            desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            downloadedFilePath = Path.Combine(desktopPath, Constants.DOWNLOADED_FILE_NAME);

            userPreferencesFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                  Constants.USER_BLACK_DESERT_DIRECTORY_FILE);

            if (!File.Exists(userPreferencesFile))
            {
                File.WriteAllText(userPreferencesFile, blackDesertClientPath);
            }

            SetBDODirectory(File.ReadAllText(userPreferencesFile));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(blackDesertClientPath))
            {
                ShowError("The given path does not exist");
                return;
            }

            if (!Directory.Exists(blackDesertFilesPath))
            {
                ShowError("Invalid path");
                return;
            }

            if (!Directory.Exists(blackDesertClientPath))
            {
                ShowError("The given path does not exist");
                return;
            }

            if (downloading) return;

            downloading = true;

            this.label2.Text = "Fetching objects";

            GetVersion(Constants.VERSION_URL, DownloadFile);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();

            var newPath = folderBrowserDialog1.SelectedPath;

            if (string.IsNullOrWhiteSpace(newPath)) return;

            SetBDODirectory(newPath);
        }

        private void DownloadFile()
        {
            var finalFileLink = Constants.FILE_URL.Replace("###", version);

            this.label2.Text = $"Downloading files";

            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFileCompleted += (a, b) => ReplaceFile();
                    client.DownloadProgressChanged += (a, args) => RefreshProgressBar(args);
                    client.DownloadFileAsync(new Uri(finalFileLink), downloadedFilePath);
                }
                catch(Exception e)
                {
                    ShowError(e.Message);
                }
            }
        }

        private void RefreshProgressBar(DownloadProgressChangedEventArgs args)
        {
            this.progressBar1.Value = args.ProgressPercentage;
        }

        private void ReplaceFile()
        {
            this.label2.Text = $"Replacing files";

            var oldFile = Path.Combine(blackDesertFilesPath, Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace("##", "es"));
            var newFile = Path.Combine(desktopPath, Constants.BLACK_DESERT_LANGUAGE_FILE_NAME.Replace("##", "en"));
            
            File.Delete(oldFile);

            File.Copy(newFile, oldFile);

            File.Delete(newFile);

            downloading = false;

            this.label2.Text = $"Done";

            Application.Exit();
        }

        private async void GetVersion(string url, Action callback)
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions
            try
            {
                // Create a New HttpClient object.
                HttpClient client = new HttpClient();

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var numbers = GetStringNumbers(responseBody);

                version = numbers[2];
                downloadedFilePath = downloadedFilePath.Replace("###", version);

                callback?.Invoke();

                // Need to call dispose on the HttpClient object
                // when done using it, so the app doesn't leak resources
                client.Dispose();
            }
            catch (HttpRequestException e)
            {
                ShowError(e.Message);
            }
        }

        private string[] GetStringNumbers(string value)
        {
             return System.Text.RegularExpressions.Regex.Split(value, @"\D+");
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void ShowError(string text)
        {
            MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SetBDODirectory(string directory)
        {
            blackDesertClientPath = directory;

            blackDesertFilesPath = Path.Combine(directory, Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);

            this.label4.Text = directory;

            File.WriteAllText(userPreferencesFile, directory);
        }
    }
}
