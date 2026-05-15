using System.Globalization;

namespace BDOLanguageUpdater.Service.Serializer;

internal static class TsvLineReader
{
    public static bool TryReadLine(ReadOnlySpan<char> content, ref int position, out ReadOnlySpan<char> line)
    {
        if (position >= content.Length)
        {
            line = default;
            return false;
        }

        var remaining = content[position..];
        var lineFeedIndex = remaining.IndexOf('\n');
        if (lineFeedIndex < 0)
        {
            line = remaining;
            position = content.Length;
        }
        else
        {
            line = remaining[..lineFeedIndex];
            position += lineFeedIndex + 1;
        }

        if (!line.IsEmpty && line[^1] == '\r')
        {
            line = line[..^1];
        }

        return true;
    }

    public static bool TryParseLine(ReadOnlySpan<char> line, out LocRecordKey key, out int textStart)
    {
        key = default;
        textStart = 0;

        if (line.IsEmpty)
        {
            return false;
        }

        var firstTab = line.IndexOf('\t');
        var secondTab = firstTab < 0 ? -1 : line[(firstTab + 1)..].IndexOf('\t');
        var thirdTab = secondTab < 0 ? -1 : line[(firstTab + secondTab + 2)..].IndexOf('\t');
        var fourthTab = thirdTab < 0 ? -1 : line[(firstTab + secondTab + thirdTab + 3)..].IndexOf('\t');
        var fifthTab = fourthTab < 0 ? -1 : line[(firstTab + secondTab + thirdTab + fourthTab + 4)..].IndexOf('\t');

        if (fifthTab < 0)
        {
            return false;
        }

        secondTab += firstTab + 1;
        thirdTab += secondTab + 1;
        fourthTab += thirdTab + 1;
        fifthTab += fourthTab + 1;

        if (!TryParseKey(line, firstTab, secondTab, thirdTab, fourthTab, fifthTab, out key))
        {
            return false;
        }

        textStart = fifthTab + 1;
        return true;
    }

    private static bool TryParseKey(
        ReadOnlySpan<char> line,
        int firstTab,
        int secondTab,
        int thirdTab,
        int fourthTab,
        int fifthTab,
        out LocRecordKey key)
    {
        key = default;

        if (!uint.TryParse(line[..firstTab], NumberStyles.None, CultureInfo.InvariantCulture, out var strType) ||
            !uint.TryParse(line[(firstTab + 1)..secondTab], NumberStyles.None, CultureInfo.InvariantCulture, out var strId1) ||
            !ushort.TryParse(line[(secondTab + 1)..thirdTab], NumberStyles.None, CultureInfo.InvariantCulture, out var strId2) ||
            !byte.TryParse(line[(thirdTab + 1)..fourthTab], NumberStyles.None, CultureInfo.InvariantCulture, out var strId3) ||
            !byte.TryParse(line[(fourthTab + 1)..fifthTab], NumberStyles.None, CultureInfo.InvariantCulture, out var strId4))
        {
            return false;
        }

        key = new LocRecordKey(strType, strId1, strId2, strId3, strId4);
        return true;
    }
}
