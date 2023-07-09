using System.Windows.Forms;

namespace BDLanguageUpdater.Scripts
{
    public static class ErrorDialog
    {
        public static void Show(string text)
        {
            MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
