using BDOLanguageUpdater.Service;

namespace BDOLanguageUpdater.Tests;

public sealed class LanguageUpdateMetadataStoreTests
{
    [Fact]
    public async Task MatchesCurrentFile_WhenMetadataHashMatches_ReturnsTrue()
    {
        using var tempDirectory = new TemporaryDirectory();
        var languageFile = CreateLanguageFile(tempDirectory.Path);
        var store = new LanguageUpdateMetadataStore();

        await File.WriteAllTextAsync(languageFile.FullPath, "patched english file");
        await store.Write(languageFile, "12345");

        Assert.True(await store.MatchesCurrentFile(languageFile, "12345"));
    }

    [Fact]
    public async Task MatchesCurrentFile_WhenGameOverwritesFile_ReturnsFalse()
    {
        using var tempDirectory = new TemporaryDirectory();
        var languageFile = CreateLanguageFile(tempDirectory.Path);
        var store = new LanguageUpdateMetadataStore();

        await File.WriteAllTextAsync(languageFile.FullPath, "patched english file");
        await store.Write(languageFile, "12345");
        await File.WriteAllTextAsync(languageFile.FullPath, "launcher overwrote file");

        Assert.False(await store.MatchesCurrentFile(languageFile, "12345"));
    }

    [Fact]
    public async Task MatchesCurrentFile_WhenTargetVersionChanges_ReturnsFalse()
    {
        using var tempDirectory = new TemporaryDirectory();
        var languageFile = CreateLanguageFile(tempDirectory.Path);
        var store = new LanguageUpdateMetadataStore();

        await File.WriteAllTextAsync(languageFile.FullPath, "patched english file");
        await store.Write(languageFile, "12345");

        Assert.False(await store.MatchesCurrentFile(languageFile, "67890"));
    }

    private static GameLanguageFile CreateLanguageFile(string directory)
    {
        var path = Path.Combine(directory, "languagedata_es.loc");
        return new GameLanguageFile("es", "languagedata_es.loc", "Spanish (es)", path);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
