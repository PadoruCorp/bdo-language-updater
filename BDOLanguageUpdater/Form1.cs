using System;
using System.Windows.Forms;

namespace BDLanguageUpdater
{
    public partial class Form1 : Form
    {
        private bool downloading;

        public Form1()
        {
            InitializeComponent();

            SetupEvents();

            // TODO: Get the path from the options
            //this.label4.Text = directoriesModel.blackDesertClientPath;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            /* TODO: Get the path from the options
            if (!Directory.Exists(blackDesertClientPath))
            {
                ErrorDialog.Show("The given path does not exist");
                return;
            }

            if (!Directory.Exists(Path.Combine(blackDesertFilesPath, "ads"))) // TODO: Stop hardcoding "ads"
            {
                ErrorDialog.Show("Invalid path");
                return;
            }
            */
            if (downloading) return;

            downloading = true;

            this.label2.Text = "Fetching objects";

            // TODO: Get access to the updater
            //languageFileUpdater.UpdateFile();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();

            var newPath = folderBrowserDialog1.SelectedPath;

            if (string.IsNullOrWhiteSpace(newPath)) return;

            this.label4.Text = newPath;

            // TODO: Update the options
            //preferencesFile.WriteFile(newPath);
            // TODO: Create a new file watcher or update the path on the current one
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void SetupEvents()
        {
            /* TODO: Get access to the updater
            languageFileUpdater.OnDownloadProgressChanged += (args) => this.progressBar1.Value = args.ProgressPercentage;
            languageFileUpdater.OnFileDownloadStart += () => this.label2.Text = $"Downloading files";
            languageFileUpdater.OnFileReplaceStart += () => this.label2.Text = $"Replacing files";
            languageFileUpdater.OnUpdateFinish += () =>
            {
                this.label2.Text = $"Done";
                downloading = false;

                Application.Exit();
            };
            */
        }
    }
}
