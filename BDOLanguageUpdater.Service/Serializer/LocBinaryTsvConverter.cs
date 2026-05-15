using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;

namespace BDOLanguageUpdater.Service.Serializer;

internal static class LocBinaryTsvConverter
{
    private const int RecordHeaderLength = 16;
    private const int RecordFooterLength = 4;
    private const string EscapedLineFeed = "<lf>";
    private const string EscapedQuote = "<quot>";

    public static string ToTsv(byte[] buffer)
    {
        var expectedLength = LocCompression.GetDeclaredInflatedLength(buffer);
        using var lengthStream = LocCompression.OpenInflateStream(buffer);
        var stringLength = GetTsvLength(lengthStream, expectedLength);

        return string.Create(
            stringLength,
            (Buffer: buffer, ExpectedLength: expectedLength),
            static (destination, state) =>
            {
                using var writeStream = LocCompression.OpenInflateStream(state.Buffer);
                WriteTsv(destination, writeStream, state.ExpectedLength);
            });
    }

    public static byte[] FromTsv(string fileContent)
    {
        var contentInfo = GetEncryptedContentInfo(fileContent);

        return LocCompression.Deflate(
            contentInfo.TotalByteCount,
            deflateStream => WriteEncryptedContent(deflateStream, fileContent, contentInfo.MaxLineByteCount));
    }

    private static int GetTsvLength(Stream inflatedStream, int inflatedLength)
    {
        var length = 0;
        var inflatedBytesRead = 0;
        var isFirstLine = true;

        while (inflatedBytesRead < inflatedLength)
        {
            EnsureRecordCanFit(inflatedLength, inflatedBytesRead);
            var record = ReadRecordHeader(inflatedStream);
            inflatedBytesRead += RecordHeaderLength;

            var stringByteCount = checked((int)record.TextLength * sizeof(char));
            EnsureRecordTextCanFit(inflatedLength, inflatedBytesRead, stringByteCount);

            if (!isFirstLine)
            {
                length++;
            }

            length += CountDigits(record.Key.StrType) + 1;
            length += CountDigits(record.Key.StrId1) + 1;
            length += CountDigits(record.Key.StrId2) + 1;
            length += CountDigits(record.Key.StrId3) + 1;
            length += CountDigits(record.Key.StrId4) + 1;
            length += ReadEscapedTextLength(inflatedStream, stringByteCount);

            inflatedBytesRead += stringByteCount;
            ReadRecordFooter(inflatedStream);
            inflatedBytesRead += RecordFooterLength;
            isFirstLine = false;
        }

        return length;
    }

    private static void WriteTsv(Span<char> destination, Stream inflatedStream, int inflatedLength)
    {
        var destinationIndex = 0;
        var inflatedBytesRead = 0;
        var isFirstLine = true;

        while (inflatedBytesRead < inflatedLength)
        {
            EnsureRecordCanFit(inflatedLength, inflatedBytesRead);
            var record = ReadRecordHeader(inflatedStream);
            inflatedBytesRead += RecordHeaderLength;

            var stringByteCount = checked((int)record.TextLength * sizeof(char));
            EnsureRecordTextCanFit(inflatedLength, inflatedBytesRead, stringByteCount);

            if (!isFirstLine)
            {
                destination[destinationIndex++] = '\n';
            }

            WriteFormatted(destination, ref destinationIndex, record.Key.StrType);
            destination[destinationIndex++] = '\t';
            WriteFormatted(destination, ref destinationIndex, record.Key.StrId1);
            destination[destinationIndex++] = '\t';
            WriteFormatted(destination, ref destinationIndex, record.Key.StrId2);
            destination[destinationIndex++] = '\t';
            WriteFormatted(destination, ref destinationIndex, record.Key.StrId3);
            destination[destinationIndex++] = '\t';
            WriteFormatted(destination, ref destinationIndex, record.Key.StrId4);
            destination[destinationIndex++] = '\t';
            WriteEscapedText(destination, ref destinationIndex, inflatedStream, stringByteCount);

            inflatedBytesRead += stringByteCount;
            ReadRecordFooter(inflatedStream);
            inflatedBytesRead += RecordFooterLength;
            isFirstLine = false;
        }
    }

    private static void WriteEncryptedContent(Stream destination, string fileContent, int maxLineByteCount)
    {
        var position = 0;
        var content = fileContent.AsSpan();
        var lineBuffer = maxLineByteCount == 0
            ? Array.Empty<byte>()
            : ArrayPool<byte>.Shared.Rent(maxLineByteCount);

        try
        {
            while (TsvLineReader.TryReadLine(content, ref position, out var line))
            {
                if (line.IsEmpty)
                {
                    continue;
                }

                var lineByteCount = GetEncryptedLineByteCount(line);
                var bytesWritten = WriteEncryptedLine(line, lineBuffer.AsSpan(0, lineByteCount));
                destination.Write(lineBuffer, 0, bytesWritten);
            }
        }
        finally
        {
            if (maxLineByteCount != 0)
            {
                ArrayPool<byte>.Shared.Return(lineBuffer);
            }
        }
    }

    private static LocalizationRecord ReadRecordHeader(Stream inflatedStream)
    {
        Span<byte> header = stackalloc byte[RecordHeaderLength];
        ReadExactly(inflatedStream, header);

        return new LocalizationRecord(
            BinaryPrimitives.ReadUInt32LittleEndian(header[..sizeof(uint)]),
            new LocRecordKey(
                BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(4, sizeof(uint))),
                BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(8, sizeof(uint))),
                BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(12, sizeof(ushort))),
                header[14],
                header[15]));
    }

    private static void ReadRecordFooter(Stream inflatedStream)
    {
        Span<byte> footer = stackalloc byte[RecordFooterLength];
        ReadExactly(inflatedStream, footer);
    }

    private static void EnsureRecordCanFit(int inflatedLength, int inflatedBytesRead)
    {
        if (inflatedLength - inflatedBytesRead < RecordHeaderLength + RecordFooterLength)
        {
            throw new InvalidDataException("The localization file contains an incomplete record.");
        }
    }

    private static void EnsureRecordTextCanFit(int inflatedLength, int inflatedBytesRead, int stringByteCount)
    {
        if (stringByteCount < 0 || stringByteCount > inflatedLength - inflatedBytesRead - RecordFooterLength)
        {
            throw new InvalidDataException("The localization file contains an invalid record text length.");
        }
    }

    private static void ReadExactly(Stream stream, Span<byte> buffer)
    {
        while (!buffer.IsEmpty)
        {
            var bytesRead = stream.Read(buffer);
            if (bytesRead == 0)
            {
                throw new EndOfStreamException("Unexpected end of localization file.");
            }

            buffer = buffer[bytesRead..];
        }
    }

    private static (int TotalByteCount, int MaxLineByteCount) GetEncryptedContentInfo(string fileContent)
    {
        var totalByteCount = 0;
        var maxLineByteCount = 0;
        var position = 0;
        var content = fileContent.AsSpan();

        while (TsvLineReader.TryReadLine(content, ref position, out var line))
        {
            if (line.IsEmpty)
            {
                continue;
            }

            var lineByteCount = GetEncryptedLineByteCount(line);
            totalByteCount = checked(totalByteCount + lineByteCount);
            maxLineByteCount = Math.Max(maxLineByteCount, lineByteCount);
        }

        return (totalByteCount, maxLineByteCount);
    }

    private static int GetEncryptedLineByteCount(ReadOnlySpan<char> line)
    {
        var text = GetTextField(line);
        return checked(RecordHeaderLength + (TransformEscapedText(text, Span<byte>.Empty) * sizeof(char)) + RecordFooterLength);
    }

    private static int WriteEncryptedLine(ReadOnlySpan<char> line, Span<byte> destination)
    {
        if (!TsvLineReader.TryParseLine(line, out var key, out var textStart))
        {
            throw new InvalidDataException("Localization TSV lines must contain five id fields and one text field.");
        }

        var text = line[textStart..];
        var textLength = TransformEscapedText(text, Span<byte>.Empty);
        var offset = 0;

        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(offset, sizeof(uint)), checked((uint)textLength));
        offset += sizeof(uint);
        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(offset, sizeof(uint)), key.StrType);
        offset += sizeof(uint);
        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(offset, sizeof(uint)), key.StrId1);
        offset += sizeof(uint);
        BinaryPrimitives.WriteUInt16LittleEndian(destination.Slice(offset, sizeof(ushort)), key.StrId2);
        offset += sizeof(ushort);
        destination[offset++] = key.StrId3;
        destination[offset++] = key.StrId4;

        offset += TransformEscapedText(text, destination.Slice(offset, textLength * sizeof(char))) * sizeof(char);
        destination.Slice(offset, RecordFooterLength).Clear();
        offset += RecordFooterLength;

        return offset;
    }

    private static ReadOnlySpan<char> GetTextField(ReadOnlySpan<char> line)
    {
        if (!TsvLineReader.TryParseLine(line, out _, out var textStart))
        {
            throw new InvalidDataException("Localization TSV lines must contain five id fields and one text field.");
        }

        return line[textStart..];
    }

    private static int TransformEscapedText(ReadOnlySpan<char> source, Span<byte> destination)
    {
        var charsWritten = 0;
        var byteOffset = 0;
        var isFirstOutputChar = true;

        for (var i = 0; i < source.Length; i++)
        {
            char ch;
            if (source[i] == '"')
            {
                continue;
            }

            if (source[i..].StartsWith(EscapedLineFeed, StringComparison.Ordinal))
            {
                ch = '\n';
                i += EscapedLineFeed.Length - 1;
            }
            else if (source[i..].StartsWith(EscapedQuote, StringComparison.Ordinal))
            {
                ch = '"';
                i += EscapedQuote.Length - 1;
            }
            else
            {
                ch = source[i];
            }

            if (isFirstOutputChar)
            {
                isFirstOutputChar = false;
                if (ch == '\'' && IsLeadingFormulaGuard(source, i))
                {
                    continue;
                }
            }

            if (!destination.IsEmpty)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(destination.Slice(byteOffset, sizeof(char)), ch);
                byteOffset += sizeof(char);
            }

            charsWritten++;
        }

        return charsWritten;
    }

    private static bool IsLeadingFormulaGuard(ReadOnlySpan<char> source, int quoteIndex)
    {
        var remaining = source[(quoteIndex + 1)..];

        while (!remaining.IsEmpty)
        {
            if (remaining[0] == '"')
            {
                remaining = remaining[1..];
                continue;
            }

            return remaining[0] is '+' or '=' or '-';
        }

        return false;
    }

    private static int ReadEscapedTextLength(Stream inflatedStream, int utf16ByteCount)
    {
        var length = 0;
        var remainingBytes = utf16ByteCount;
        var isFirstChar = true;
        Span<byte> buffer = stackalloc byte[8192];

        while (remainingBytes > 0)
        {
            var bytesToRead = Math.Min(remainingBytes, buffer.Length);
            ReadExactly(inflatedStream, buffer[..bytesToRead]);

            for (var i = 0; i < bytesToRead; i += sizeof(char))
            {
                var ch = ReadUtf16LittleEndianChar(buffer, i);
                if (isFirstChar)
                {
                    isFirstChar = false;
                    if (ch == '+' || ch == '=' || ch == '-')
                    {
                        length++;
                    }
                }

                length += GetEscapedCharLength(ch);
            }

            remainingBytes -= bytesToRead;
        }

        return length;
    }

    private static void WriteEscapedText(
        Span<char> destination,
        ref int destinationIndex,
        Stream inflatedStream,
        int utf16ByteCount)
    {
        var remainingBytes = utf16ByteCount;
        var isFirstChar = true;
        Span<byte> buffer = stackalloc byte[8192];

        while (remainingBytes > 0)
        {
            var bytesToRead = Math.Min(remainingBytes, buffer.Length);
            ReadExactly(inflatedStream, buffer[..bytesToRead]);

            for (var i = 0; i < bytesToRead; i += sizeof(char))
            {
                var ch = ReadUtf16LittleEndianChar(buffer, i);
                if (isFirstChar)
                {
                    isFirstChar = false;
                    if (ch == '+' || ch == '=' || ch == '-')
                    {
                        destination[destinationIndex++] = '\'';
                    }
                }

                WriteEscapedChar(destination, ref destinationIndex, ch);
            }

            remainingBytes -= bytesToRead;
        }
    }

    private static int GetEscapedCharLength(char ch)
    {
        return ch switch
        {
            '\n' => EscapedLineFeed.Length,
            '"' => EscapedQuote.Length,
            _ => 1
        };
    }

    private static void WriteEscapedChar(Span<char> destination, ref int destinationIndex, char ch)
    {
        switch (ch)
        {
            case '\n':
                EscapedLineFeed.AsSpan().CopyTo(destination[destinationIndex..]);
                destinationIndex += EscapedLineFeed.Length;
                break;
            case '"':
                EscapedQuote.AsSpan().CopyTo(destination[destinationIndex..]);
                destinationIndex += EscapedQuote.Length;
                break;
            default:
                destination[destinationIndex++] = ch;
                break;
        }
    }

    private static char ReadUtf16LittleEndianChar(ReadOnlySpan<byte> source, int offset)
    {
        return (char)BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(offset, sizeof(char)));
    }

    private static int CountDigits(uint value)
    {
        var digits = 1;
        while (value >= 10)
        {
            value /= 10;
            digits++;
        }

        return digits;
    }

    private static void WriteFormatted(Span<char> destination, ref int destinationIndex, uint value)
    {
        if (!value.TryFormat(destination[destinationIndex..], out var charsWritten, provider: CultureInfo.InvariantCulture))
        {
            throw new InvalidOperationException("Could not format localization id.");
        }

        destinationIndex += charsWritten;
    }

    private readonly record struct LocalizationRecord(uint TextLength, LocRecordKey Key);
}
