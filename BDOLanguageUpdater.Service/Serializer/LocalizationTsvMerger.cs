namespace BDOLanguageUpdater.Service.Serializer;

internal static class LocalizationTsvMerger
{
    public static string Merge(string fromFileTsv, string toFileTsv)
    {
        var replacements = FileContentToDictionary(fromFileTsv);
        return ApplyReplacements(replacements, toFileTsv);
    }

    private static Dictionary<LocRecordKey, string> FileContentToDictionary(string fileContent)
    {
        var idToTextMapping = new Dictionary<LocRecordKey, string>(CountLines(fileContent));
        var position = 0;
        var content = fileContent.AsSpan();

        while (TsvLineReader.TryReadLine(content, ref position, out var line))
        {
            if (!TsvLineReader.TryParseLine(line, out var key, out var textStart))
            {
                continue;
            }

            idToTextMapping[key] = line[textStart..].ToString();
        }

        return idToTextMapping;
    }

    private static string ApplyReplacements(Dictionary<LocRecordKey, string> replacements, string targetFileContent)
    {
        var outputLength = GetMergedLength(replacements, targetFileContent);

        return string.Create(
            outputLength,
            (Replacements: replacements, TargetFileContent: targetFileContent),
            static (destination, state) => WriteMergedContent(destination, state.Replacements, state.TargetFileContent));
    }

    private static int GetMergedLength(Dictionary<LocRecordKey, string> replacements, string targetFileContent)
    {
        var length = 0;
        var position = 0;
        var wroteLine = false;
        var content = targetFileContent.AsSpan();

        while (TsvLineReader.TryReadLine(content, ref position, out var line))
        {
            if (!TsvLineReader.TryParseLine(line, out var key, out var textStart))
            {
                continue;
            }

            if (wroteLine)
            {
                length++;
            }

            if (replacements.TryGetValue(key, out var replacement))
            {
                length = checked(length + textStart + replacement.Length);
            }
            else
            {
                length = checked(length + line.Length);
            }

            wroteLine = true;
        }

        return length;
    }

    private static void WriteMergedContent(
        Span<char> destination,
        Dictionary<LocRecordKey, string> replacements,
        string targetFileContent)
    {
        var destinationIndex = 0;
        var position = 0;
        var wroteLine = false;
        var content = targetFileContent.AsSpan();

        while (TsvLineReader.TryReadLine(content, ref position, out var line))
        {
            if (!TsvLineReader.TryParseLine(line, out var key, out var textStart))
            {
                continue;
            }

            if (wroteLine)
            {
                destination[destinationIndex++] = '\n';
            }

            line[..textStart].CopyTo(destination[destinationIndex..]);
            destinationIndex += textStart;

            if (replacements.TryGetValue(key, out var replacement))
            {
                replacement.AsSpan().CopyTo(destination[destinationIndex..]);
                destinationIndex += replacement.Length;
            }
            else
            {
                line[textStart..].CopyTo(destination[destinationIndex..]);
                destinationIndex += line.Length - textStart;
            }

            wroteLine = true;
        }
    }

    private static int CountLines(string fileContent)
    {
        var count = 0;
        var position = 0;
        var content = fileContent.AsSpan();

        while (TsvLineReader.TryReadLine(content, ref position, out var line))
        {
            if (!line.IsEmpty)
            {
                count++;
            }
        }

        return count;
    }
}
