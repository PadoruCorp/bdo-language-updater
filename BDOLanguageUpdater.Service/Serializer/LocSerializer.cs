using System.Buffers;
using System.IO.Compression;
using System.Text;
using Padoru.Core.Files;

namespace BDOLanguageUpdater.Service.Serializer;

public class LocSerializer : ISerializer  
{
    private static readonly char[] sourceArray = ['+', '=', '-'];

    public async Task<byte[]> Serialize(object value)
    {
        var content = value.ToString() ?? throw new InvalidCastException();
        var dataEncrypt = Encrypt(content);
        var dataCompress = await Deflate(dataEncrypt);
        return dataCompress;    
    }

    public async Task<object> Deserialize(Type type, byte[] bytes, string uri)
    {
        var dataDecompress = await InflateAsync(bytes);
        var dataDecrypt = Decrypt(dataDecompress);
        return dataDecrypt;
        
    }

    private string TrimStart(string str, string chars)
    {
        if (str.StartsWith(chars))
        {
            str = str.Substring(chars.Length);
        }

        return str;
    }

    private string AddSingleQuote(string str)
    {
        if (sourceArray.Any(ch => str.StartsWith(ch)))
        {
            str = $"'{str}";
        }

        return str;
    }

    public async Task<byte[]> InflateAsync(byte[] buffer)
    {
        using var memoryStream = new MemoryStream(buffer, 6, buffer.Length - 6);
        await using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        await deflateStream.CopyToAsync(resultStream);
        return resultStream.ToArray();
    }

    public async Task<byte[]> Deflate(byte[] buffer)
    {
        var sizeBuffer = new byte[6];
        var i = 0;
        WriteUInt32LE(sizeBuffer,ref i, (uint)buffer.Length);

        sizeBuffer[4] = 0x78;
        sizeBuffer[5] = 0x01;

        using var compressedStream = new MemoryStream();
        using var originalStream = new MemoryStream(buffer);
        await using (var deflateStream = new DeflateStream(compressedStream, CompressionLevel.Fastest))
        {
            await originalStream.CopyToAsync(deflateStream);
        }

        var compressedArray = compressedStream.ToArray();
        var concatenatedBytes = new byte[sizeBuffer.Length + compressedArray.Length];
        Array.Copy(sizeBuffer, concatenatedBytes, sizeBuffer.Length);
        Array.Copy(compressedArray, 0, concatenatedBytes, sizeBuffer.Length, compressedArray.Length);

        return concatenatedBytes;
    }

    private byte[] Encrypt(string fileContent)
    {
        var chunks = new List<byte[]>();

        var allLines = fileContent.Replace("\r","").Split("\n");
        foreach (var line in allLines)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            var content = line.Split('\t').Select((e, i) =>
            {
                return i > 4
                    ? TrimStart(e.Replace("<lf>", "\n").Replace("\"", "").Replace("<quot>", "\""), "'")
                    : e;
            }).ToArray();
            
            int strSize = content[5].Length;
            int size = 4+4+4+2+1+1+(strSize*2)+4;
            byte[] buffer = new byte[size];
            int i = 0;

            // Write values to the buffer using little-endian encoding
            WriteUInt32LE(buffer, ref i, (uint)strSize);
            WriteUInt32LE(buffer, ref i, Convert.ToUInt32(content[0]));
            WriteUInt32LE(buffer, ref i, Convert.ToUInt32(content[1]));
            WriteUInt16LE(buffer, ref i, Convert.ToUInt16(content[2]));
            WriteUInt8(buffer, ref i, Convert.ToByte(content[3]));
            WriteUInt8(buffer, ref i, Convert.ToByte(content[4]));

            // Write string content to the buffer as UTF-16LE
            var destination = ArrayPool<byte>.Shared.Rent(content[5].Length * 4);
            Encoding.Unicode.GetBytes(content[5], destination);
            ArrayPool<byte>.Shared.Return(destination);

            chunks.Add(buffer);
        }

        return chunks.SelectMany(bytes => bytes).ToArray();
    }

    private string Decrypt(byte[] buffer)
    {
        var result = new List<string>();
        var index = 0;

        while (index < buffer.Length)
        {
            var strSize = BitConverter.ToUInt32(buffer, index);
            index += 4;
            var strType = BitConverter.ToUInt32(buffer, index);
            index += 4;
            var strID1 = BitConverter.ToUInt32(buffer, index);
            index += 4;
            var strID2 = BitConverter.ToUInt16(buffer, index);
            index += 2;
            var strID3 = buffer[index];
            index += 1;
            var strID4 = buffer[index];
            index += 1;
            var strBytes = new byte[strSize * 2];
            Array.Copy(buffer, index, strBytes, 0, strSize * 2);
            index += (int)strSize * 2;
            var str = AddSingleQuote(Encoding.Unicode.GetString(strBytes).Replace("\n", "<lf>").Replace("\"", "<quot>"));
            index += 4;

            result.Add($"{strType}\t{strID1}\t{strID2}\t{strID3}\t{strID4}\t{str}");
        }

        return string.Join("\n", result);
    }
    
    static void WriteUInt32LE(byte[] buffer, ref int offset, uint value)
    {
        buffer[offset++] = (byte)(value & 0xFF);
        buffer[offset++] = (byte)((value >> 8) & 0xFF);
        buffer[offset++] = (byte)((value >> 16) & 0xFF);
        buffer[offset++] = (byte)((value >> 24) & 0xFF);
    }

    static void WriteUInt16LE(byte[] buffer, ref int offset, ushort value)
    {
        buffer[offset++] = (byte)(value & 0xFF);
        buffer[offset++] = (byte)((value >> 8) & 0xFF);
    }

    static void WriteUInt8(byte[] buffer, ref int offset, byte value)
    {
        buffer[offset++] = value;
    }
}