using System.Globalization;

namespace BDOLanguageUpdater.Service;

public sealed class LanguageFileDiscovery
{
    private const string LanguageFilePrefix = "languagedata_";
    private static readonly IReadOnlyDictionary<string, string> KnownLanguageDisplayNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["br"] = "Portuguese - Brazil (br)",
            ["cn"] = "Simplified Chinese (cn)",
            ["jp"] = "Japanese (jp)",
            ["kr"] = "Korean (kr)",
            ["tw"] = "Traditional Chinese (tw)"
        };

    public IReadOnlyList<GameLanguageFile> GetAvailableLanguages(string bdoClientPath)
    {
        var adsPath = GetAdsPath(bdoClientPath);
        if (!Directory.Exists(adsPath))
        {
            return Array.Empty<GameLanguageFile>();
        }

        return Directory
            .EnumerateFiles(adsPath, $"{LanguageFilePrefix}*{Constants.BLACK_DESERT_LANGUAGE_FILE_EXTENSION}", SearchOption.TopDirectoryOnly)
            .Select(CreateLanguageFile)
            .Where(language => language is not null && !IsTargetEnglishLanguage(language.Code))
            .Select(language => language!)
            .OrderBy(language => language.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public GameLanguageFile? FindLanguage(string bdoClientPath, string languageCode)
    {
        return GetAvailableLanguages(bdoClientPath)
            .FirstOrDefault(language => language.Code.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
    }

    public string GetAdsPath(string bdoClientPath)
    {
        if (string.IsNullOrWhiteSpace(bdoClientPath))
        {
            return Constants.BLACK_DESERT_LANGUAGE_FILES_PATH;
        }

        var directoryName = Path.GetFileName(
            bdoClientPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        return directoryName.Equals(Constants.BLACK_DESERT_LANGUAGE_FILES_PATH, StringComparison.OrdinalIgnoreCase)
            ? bdoClientPath
            : Path.Combine(bdoClientPath, Constants.BLACK_DESERT_LANGUAGE_FILES_PATH);
    }

    private static GameLanguageFile? CreateLanguageFile(string path)
    {
        var fileName = Path.GetFileName(path);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

        if (!fileNameWithoutExtension.StartsWith(LanguageFilePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var code = fileNameWithoutExtension[LanguageFilePrefix.Length..];
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return new GameLanguageFile(code, fileName, GetDisplayName(code), path);
    }

    private static bool IsTargetEnglishLanguage(string code)
    {
        return code.Equals(Constants.DEFAULT_TARGET_LANGUAGE_CODE, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetDisplayName(string code)
    {
        if (KnownLanguageDisplayNames.TryGetValue(code, out var displayName))
        {
            return displayName;
        }

        var cultureCode = code.Replace('_', '-');

        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureCode);
            return $"{culture.EnglishName} ({cultureCode})";
        }
        catch (CultureNotFoundException)
        {
            return $"{code} ({code})";
        }
    }
}

public sealed record GameLanguageFile(
    string Code,
    string FileName,
    string DisplayName,
    string FullPath)
{
    public override string ToString()
    {
        return DisplayName;
    }
}
