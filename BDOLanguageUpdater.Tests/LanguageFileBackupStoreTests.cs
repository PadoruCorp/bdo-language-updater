using BDOLanguageUpdater.Service;

namespace BDOLanguageUpdater.Tests;

public sealed class LanguageFileBackupStoreTests
{
    [Fact]
    public async Task SaveBeforeUpdate_WhenFileIsNotManaged_CreatesBackup()
    {
        using var tempDirectory = new TemporaryDirectory();
        var languageFile = CreateLanguageFile(tempDirectory.Path);
        var store = new LanguageFileBackupStore(new LanguageUpdateMetadataStore());

        await File.WriteAllTextAsync(languageFile.FullPath, "original spanish file");

        var result = await store.SaveBeforeUpdate(languageFile);

        Assert.True(result.Succeeded);
        Assert.True(result.CreatedNewBackup);
        Assert.True(File.Exists(result.BackupFilePath));
        Assert.Equal("original spanish file", await File.ReadAllTextAsync(result.BackupFilePath!));
    }

    [Fact]
    public async Task SaveBeforeUpdate_WhenFileIsManaged_PreservesExistingBackup()
    {
        using var tempDirectory = new TemporaryDirectory();
        var languageFile = CreateLanguageFile(tempDirectory.Path);
        var metadataStore = new LanguageUpdateMetadataStore();
        var store = new LanguageFileBackupStore(metadataStore);

        await File.WriteAllTextAsync(languageFile.FullPath, "original spanish file");
        await store.SaveBeforeUpdate(languageFile);
        await File.WriteAllTextAsync(languageFile.FullPath, "patched english file");
        await metadataStore.Write(languageFile, "12345");

        var result = await store.SaveBeforeUpdate(languageFile);

        Assert.True(result.Succeeded);
        Assert.False(result.CreatedNewBackup);
        Assert.Equal("original spanish file", await File.ReadAllTextAsync(result.BackupFilePath!));
    }

    [Fact]
    public async Task Restore_ReplacesCurrentFileAndClearsMetadata()
    {
        using var tempDirectory = new TemporaryDirectory();
        var languageFile = CreateLanguageFile(tempDirectory.Path);
        var metadataStore = new LanguageUpdateMetadataStore();
        var store = new LanguageFileBackupStore(metadataStore);

        await File.WriteAllTextAsync(languageFile.FullPath, "original spanish file");
        await store.SaveBeforeUpdate(languageFile);
        await File.WriteAllTextAsync(languageFile.FullPath, "patched english file");
        await metadataStore.Write(languageFile, "12345");

        var result = await store.Restore(languageFile);

        Assert.True(result.Succeeded);
        Assert.Equal("original spanish file", await File.ReadAllTextAsync(languageFile.FullPath));
        Assert.False(File.Exists(metadataStore.GetMetadataPath(languageFile)));
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
