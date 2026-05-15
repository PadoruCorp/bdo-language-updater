using System.Buffers.Binary;
using System.IO.Compression;

namespace BDOLanguageUpdater.Service.Serializer;

internal static class LocCompression
{
    private const int HeaderLength = 6;
    private const byte DeflateHeaderByte0 = 0x78;
    private const byte DeflateHeaderByte1 = 0x01;

    public static async Task<byte[]> InflateAsync(byte[] buffer)
    {
        var expectedLength = GetDeclaredInflatedLength(buffer);
        var result = GC.AllocateUninitializedArray<byte>(expectedLength);

        await using var deflateStream = OpenInflateStream(buffer);

        var totalRead = 0;
        while (totalRead < result.Length)
        {
            var read = await deflateStream.ReadAsync(result.AsMemory(totalRead)).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        if (totalRead != expectedLength)
        {
            throw new InvalidDataException(
                $"The localization file inflated to {totalRead} bytes, but its header declares {expectedLength} bytes.");
        }

        return result;
    }

    public static byte[] Deflate(byte[] buffer)
    {
        return Deflate(buffer.Length, stream => stream.Write(buffer));
    }

    public static byte[] Deflate(int inflatedLength, Action<Stream> writeInflatedContent)
    {
        using var compressedStream = new MemoryStream();
        WriteHeader(compressedStream, inflatedLength);

        using (var deflateStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, leaveOpen: true))
        {
            writeInflatedContent(deflateStream);
        }

        return compressedStream.ToArray();
    }

    public static int GetDeclaredInflatedLength(byte[] buffer)
    {
        if (buffer.Length < HeaderLength)
        {
            throw new InvalidDataException("The localization file is missing its compression header.");
        }

        return checked((int)BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(0, sizeof(uint))));
    }

    public static DeflateStream OpenInflateStream(byte[] buffer)
    {
        if (buffer.Length < HeaderLength)
        {
            throw new InvalidDataException("The localization file is missing its compression header.");
        }

        var memoryStream = new MemoryStream(buffer, HeaderLength, buffer.Length - HeaderLength, writable: false);
        return new DeflateStream(memoryStream, CompressionMode.Decompress);
    }

    private static void WriteHeader(Stream destination, int inflatedLength)
    {
        Span<byte> header = stackalloc byte[HeaderLength];
        BinaryPrimitives.WriteUInt32LittleEndian(header, checked((uint)inflatedLength));
        header[4] = DeflateHeaderByte0;
        header[5] = DeflateHeaderByte1;
        destination.Write(header);
    }
}
