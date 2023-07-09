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

            var form = CreateForm();

            Application.Run(form);
        }

        private static Form1 CreateForm()
        {
            var form = new Form1();

            return form;
        }
    }
}
