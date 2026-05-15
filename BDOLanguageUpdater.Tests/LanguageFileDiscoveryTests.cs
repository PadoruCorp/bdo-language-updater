using BDOLanguageUpdater.Service;

namespace BDOLanguageUpdater.Tests;

public sealed class LanguageFileDiscoveryTests : IDisposable
{
    private readonly string tempDirectory = Path.Combine(Path.GetTempPath(), $"bdo-language-updater-{Guid.NewGuid():N}");

    [Fact]
    public void GetAvailableLanguages_ReturnsInstalledNonEnglishLanguageFiles()
    {
        var adsPath = Path.Combine(tempDirectory, Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);
        Directory.CreateDirectory(adsPath);
        File.WriteAllText(Path.Combine(adsPath, "languagedata_es.loc"), string.Empty);
        File.WriteAllText(Path.Combine(adsPath, "languagedata_kr.loc"), string.Empty);
        File.WriteAllText(Path.Combine(adsPath, "languagedata_pt.loc"), string.Empty);
        File.WriteAllText(Path.Combine(adsPath, "languagedata_en.loc"), string.Empty);
        File.WriteAllText(Path.Combine(adsPath, "not-a-language.loc"), string.Empty);
        var discovery = new LanguageFileDiscovery();

        var languages = discovery.GetAvailableLanguages(tempDirectory);

        Assert.Equal(["es", "kr", "pt"], languages.Select(language => language.Code).OrderBy(code => code));
        Assert.Contains(languages, language => language.Code == "kr" && language.DisplayName == "Korean (kr)");
        Assert.All(languages, language => Assert.DoesNotContain("en", language.Code));
    }

    [Fact]
    public void FindLanguage_ReturnsSelectedLanguageFile()
    {
        var adsPath = Path.Combine(tempDirectory, Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);
        Directory.CreateDirectory(adsPath);
        File.WriteAllText(Path.Combine(adsPath, "languagedata_pt.loc"), string.Empty);
        var discovery = new LanguageFileDiscovery();

        var language = discovery.FindLanguage(tempDirectory, "pt");

        Assert.NotNull(language);
        Assert.Equal("pt", language.Code);
        Assert.EndsWith(Path.Combine(Constants.BLACK_DESERT_LANGUAGE_FILES_PATH, "languagedata_pt.loc"), language.FullPath);
    }

    [Fact]
    public void GetAdsPath_AcceptsGameRootOrAdsFolder()
    {
        var discovery = new LanguageFileDiscovery();
        var adsPath = Path.Combine(tempDirectory, Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);

        Assert.Equal(adsPath, discovery.GetAdsPath(tempDirectory));
        Assert.Equal(adsPath, discovery.GetAdsPath(adsPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }
}
