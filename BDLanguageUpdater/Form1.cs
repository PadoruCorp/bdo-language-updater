using System;
using System.Windows.Forms;
using System.IO;
using BDLanguageUpdater.Scripts;

namespace BDLanguageUpdater
{
    public partial class Form1 : Form
    {
        private DirectoriesModel directoriesModel;
        private UserPreferencesFile preferencesFile;
        private LanguageFileUpdater languageFileUpdater;

        private bool downloading;

        public Form1(DirectoriesModel directoriesModel, UserPreferencesFile preferencesFile, LanguageFileUpdater languageFileUpdater)
        {
            InitializeComponent();

            this.directoriesModel = directoriesModel;
            this.preferencesFile = preferencesFile;
            this.languageFileUpdater = languageFileUpdater;

            SetupEvents();

            this.label4.Text = directoriesModel.blackDesertClientPath;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(directoriesModel.blackDesertClientPath))
            {
                ErrorDialog.Show("The given path does not exist");
                return;
            }

            if (!Directory.Exists(directoriesModel.blackDesertFilesPath))
            {
                ErrorDialog.Show("Invalid path");
                return;
            }

            if (!Directory.Exists(directoriesModel.blackDesertClientPath))
            {
                ErrorDialog.Show("The given path does not exist");
                return;
            }

            if (downloading) return;

            downloading = true;

            this.label2.Text = "Fetching objects";

            languageFileUpdater.UpdateFile();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();

            var newPath = folderBrowserDialog1.SelectedPath;

            if (string.IsNullOrWhiteSpace(newPath)) return;

            this.label4.Text = newPath;

            preferencesFile.WriteFile(newPath);
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void SetupEvents()
        {
            languageFileUpdater.OnDownloadProgressChanged += (args) => this.progressBar1.Value = args.ProgressPercentage;
            languageFileUpdater.OnFileDownloadStart += () => this.label2.Text = $"Downloading files";
            languageFileUpdater.OnFileReplaceStart += () => this.label2.Text = $"Replacing files";
            languageFileUpdater.OnUpdateFinish += () =>
            {
                this.label2.Text = $"Done";
                downloading = false;

                Application.Exit();
            };
        }
    }
}
