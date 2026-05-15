using System.Security.Cryptography;
using System.Text.Json;

namespace BDOLanguageUpdater.Service;

public sealed class LanguageUpdateMetadataStore
{
    private const int CurrentMetadataVersion = 1;
    private const string MetadataFileSuffix = ".bdo-language-updater.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public async Task<bool> MatchesCurrentFile(GameLanguageFile languageFile, string targetVersion)
    {
        var metadata = await Read(languageFile).ConfigureAwait(false);
        if (!IsValidForLanguage(metadata, languageFile) ||
            !metadata!.TargetVersion.Equals(targetVersion, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return await MatchesCurrentOutputHash(languageFile, metadata).ConfigureAwait(false);
    }

    public async Task<bool> MatchesCurrentOutput(GameLanguageFile languageFile)
    {
        var metadata = await Read(languageFile).ConfigureAwait(false);
        if (!IsValidForLanguage(metadata, languageFile))
        {
            return false;
        }

        return await MatchesCurrentOutputHash(languageFile, metadata!).ConfigureAwait(false);
    }

    public async Task Write(GameLanguageFile languageFile, string targetVersion)
    {
        var metadata = new LanguageUpdateMetadata(
            CurrentMetadataVersion,
            languageFile.Code,
            Constants.DEFAULT_TARGET_LANGUAGE_CODE,
            targetVersion,
            Path.GetFileName(languageFile.FullPath),
            await ComputeSha256(languageFile.FullPath).ConfigureAwait(false),
            DateTimeOffset.UtcNow);

        var metadataPath = GetMetadataPath(languageFile);
        await using var stream = File.Create(metadataPath);
        await JsonSerializer.SerializeAsync(stream, metadata, JsonOptions).ConfigureAwait(false);
    }

    public async Task<LanguageUpdateMetadata?> Read(GameLanguageFile languageFile)
    {
        var metadataPath = GetMetadataPath(languageFile);
        if (!File.Exists(metadataPath))
        {
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(metadataPath);
            return await JsonSerializer.DeserializeAsync<LanguageUpdateMetadata>(stream, JsonOptions).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public string GetMetadataPath(GameLanguageFile languageFile)
    {
        return languageFile.FullPath + MetadataFileSuffix;
    }

    public Task Delete(GameLanguageFile languageFile)
    {
        var metadataPath = GetMetadataPath(languageFile);
        if (File.Exists(metadataPath))
        {
            File.Delete(metadataPath);
        }

        return Task.CompletedTask;
    }

    private static bool IsValidForLanguage(LanguageUpdateMetadata? metadata, GameLanguageFile languageFile)
    {
        return metadata is not null &&
               metadata.Version == CurrentMetadataVersion &&
               metadata.SourceLanguageCode.Equals(languageFile.Code, StringComparison.OrdinalIgnoreCase) &&
               metadata.TargetLanguageCode.Equals(Constants.DEFAULT_TARGET_LANGUAGE_CODE, StringComparison.OrdinalIgnoreCase) &&
               File.Exists(languageFile.FullPath);
    }

    private static async Task<bool> MatchesCurrentOutputHash(GameLanguageFile languageFile, LanguageUpdateMetadata metadata)
    {
        var currentFileHash = await ComputeSha256(languageFile.FullPath).ConfigureAwait(false);
        return currentFileHash.Equals(metadata.OutputSha256, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string> ComputeSha256(string path)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream).ConfigureAwait(false);
        return Convert.ToHexString(hash);
    }
}

public sealed record LanguageUpdateMetadata(
    int Version,
    string SourceLanguageCode,
    string TargetLanguageCode,
    string TargetVersion,
    string FileName,
    string OutputSha256,
    DateTimeOffset UpdatedAtUtc);
