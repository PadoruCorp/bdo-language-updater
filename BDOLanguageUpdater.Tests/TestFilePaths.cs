namespace BDOLanguageUpdater.Tests;

internal static class TestFilePaths
{
    public static string GetTestDataPath(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var directCandidate = Path.Combine(directory.FullName, fileName);
            if (File.Exists(directCandidate))
            {
                return directCandidate;
            }

            var projectCandidate = Path.Combine(directory.FullName, "BDOLanguageUpdater.Tests", fileName);
            if (File.Exists(projectCandidate))
            {
                return projectCandidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find test localization file '{fileName}'.");
    }
}
