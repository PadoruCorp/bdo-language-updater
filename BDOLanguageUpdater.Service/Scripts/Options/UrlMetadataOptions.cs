namespace BDLanguageUpdater.Service;

public class UrlMetadataOptions
{
    public const string UrlMetadata = "UrlMetadata";

    public string Regex { get; set; } = Constants.DEFAULT_REGEX;
    public string VersionUrl { get; set; } = Constants.DEFAULT_VERSION_URL;
    public string FileUrl { get; set; } = Constants.DEFAULT_FILE_URL;
    public string StringToReplaceOnUrl { get; set; } = Constants.DEFAULT_STRING_TO_REPLACE_ON_URL;
    public string StringToReplaceOnFile { get; set; } = Constants.DEFAULT_STRING_TO_REPLACE_ON_FILE;
    public int VersionNumberIndex { get; set; } = Constants.DEFAULT_VERSION_NUMBER_INDEX;
}
