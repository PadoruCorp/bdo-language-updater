using BDOLanguageUpdater.Service;
using BDOLanguageUpdater.Service.Serializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    public async Task LanguageFileUpdater_UpdateFile_ShouldUpdateFile()
    {
        // Arrange
        var languageFileUpdater = this.ServiceProvider!.GetRequiredService<LanguageFileUpdater>();

        // Act
        var exception = Record.ExceptionAsync(languageFileUpdater.UpdateFile);

        // Assert
        Assert.Null(exception);
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