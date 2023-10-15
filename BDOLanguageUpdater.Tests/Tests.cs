using BDOLanguageUpdater.Service;
using BDOLanguageUpdater.Service.Serializer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Padoru.Core.Files;

namespace BDOLanguageUpdater.Tests;

public class Tests : BaseHostTests
{
    protected override void ConfigureBoostrapServices(HostBuilderContext context, IServiceCollection bootstrapServices)
    {
        base.ConfigureBoostrapServices(context, bootstrapServices);

        context.RegisterUpdaterServices(bootstrapServices);
    }

    [Fact]
    public async Task LanguageFileUpdater_GetVersion_ShouldReturnValid()
    {
        // Arrange
        var languageFileUpdater = this.ServiceProvider!.GetRequiredService<LanguageFileUpdater>();
        var versionUri = Constants.HTTPS_STRING_PROTOCOL_HEADER + Constants.DEFAULT_VERSION_URL;

        // Act
        var version = await languageFileUpdater.GetVersion(versionUri);

        // Assert
        Assert.True(int.TryParse(version, out _));
    }

    [Fact]
    public async Task LanguageFileUpdater_DownloadFile_ShouldReturnValidFile()
    {
        // Arrange
        var languageFileUpdater = this.ServiceProvider!.GetRequiredService<LanguageFileUpdater>();
        var fileManager = this.ServiceProvider!.GetRequiredService<IFileManager>();
        var versionUri = Constants.HTTPS_STRING_PROTOCOL_HEADER + Constants.DEFAULT_VERSION_URL;
        var version = await languageFileUpdater.GetVersion(versionUri);
        var downloadedFileUri = Constants.LOCAL_NOT_SERIALIZED_TEST_PROTOCOL_HEADER +
                                Path.Combine(Path.GetTempPath(), Constants.DOWNLOADED_FILE_NAME)
                                    .Replace(Constants.DEFAULT_STRING_TO_REPLACE_ON_URL, version);
        var finalFileUri = Constants.HTTPS_NOT_SERIALIZED_PROTOCOL_HEADER +
                           Constants.DEFAULT_FILE_URL.Replace(Constants.DEFAULT_STRING_TO_REPLACE_ON_URL, version);

        // Act
        await languageFileUpdater.DownloadFile(finalFileUri, downloadedFileUri);

        // Assert
        Assert.True(await fileManager.Exists(downloadedFileUri));
    }

    [Fact]
    public async Task LanguageFileUpdater_MoveFile_ShouldMoveFileCorrectly()
    {
        // Arrange
        var languageFileUpdater = this.ServiceProvider!.GetRequiredService<LanguageFileUpdater>();
        var fileManager = this.ServiceProvider!.GetRequiredService<IFileManager>();
        var testFileContentA = "This is a test file A"u8.ToArray();
        var testFileContentB = "This is a test file B"u8.ToArray();
        var fileAUri = Constants.LOCAL_NOT_SERIALIZED_TEST_PROTOCOL_HEADER + Path.Combine(Path.GetTempPath(), "fileA");
        var fileBUri = Constants.LOCAL_NOT_SERIALIZED_TEST_PROTOCOL_HEADER + Path.Combine(Path.GetTempPath(), "fileB");
        await fileManager.Write(fileBUri, testFileContentA);
        await fileManager.Write(fileAUri, testFileContentB);

        // Act
        await languageFileUpdater.MoveFile(fileAUri, fileBUri);

        // Assert
        Assert.True(await fileManager.Exists(fileAUri));
        Assert.True(!await fileManager.Exists(fileBUri));
    }

    [Fact]
    public async Task LanguageFileUpdater_UpdateFile_ShouldUpdateFile()
    {
        // Arrange
        var languageFileUpdater = this.ServiceProvider!.GetRequiredService<LanguageFileUpdater>();
        
        // Act
        await languageFileUpdater.UpdateFile();
        
        // Assert
    }

    [Fact]
    public void DictionaryMerge()
    {
        var merged = DictionaryUtils.Merge(TestData.TsvFrom, TestData.TsvTo);
        Assert.Equal(TestData.TsvMerged, merged);
    }
}

public static class TestData
{
    public const string TsvTo = "1\t2\t3\t4\t5\tFrom\n2\t0\t0\t0\t0\tFrom\n3\t0\t0\t0\t0\tFrom\n4\t0\t0\t0\t0\tFrom";

    public const string TsvFrom = "1\t2\t3\t4\t5\tTo\n2\t0\t0\t0\t0\tTo\n3\t0\t0\t0\t0\tTo";

    public const string TsvMerged = "1\t2\t3\t4\t5\tTo\n2\t0\t0\t0\t0\tTo\n3\t0\t0\t0\t0\tTo\n4\t0\t0\t0\t0\tFrom";
}