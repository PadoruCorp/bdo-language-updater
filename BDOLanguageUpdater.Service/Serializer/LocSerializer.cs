using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDOLanguageUpdater.Service.Serializer;

public static class LocSerializer
{
    private const string Utf16LeBom = "\ufeff";

    public static async Task<string> Decompress(byte[] bytes)
    {
        var dataDecompress = await InflateAsync(bytes);
        var dataDecrypt = Decrypt(dataDecompress);
        return dataDecrypt;
    }

    public static async Task<byte[]> Compress(string content)
    {
        var dataEncrypt = await Encrypt(content);
        var dataCompress = await Deflate(dataEncrypt);
        return dataCompress;
    }

    private static string TrimStart(string str, string chars)
    {
        if (str.StartsWith(chars))
        {
            str = str.Substring(chars.Length);
        }

        return str;
    }

    private static string AddSingleQuote(string str)
    {
        if (!new[] { '+', '=', '-' }.All(ch => !str.StartsWith(ch)))
        {
            str = $"'{str}";
        }

        return str;
    }

    private static async Task<byte[]> InflateAsync(byte[] buffer)
    {
        using var memoryStream = new MemoryStream(buffer, 6, buffer.Length - 6);
        await using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        await deflateStream.CopyToAsync(resultStream);
        return resultStream.ToArray();
    }

    private static async Task<byte[]> Deflate(byte[] buffer)
    {
        using var memoryStream = new MemoryStream();
        await using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true);
        await deflateStream.WriteAsync(buffer);
        return memoryStream.ToArray();
    }
    
    private static async Task<byte[]> Encrypt(string fileContent)
    {
        var chunks = new List<byte[]>();
        var allLines = fileContent.Split("\n");

        foreach (var line in allLines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine))
                continue;

            var content = trimmedLine.Split('\t').Select((e, i) =>
            {
                return i > 4 ? TrimStart(e.Replace("<lf>", "\n").Replace("\"", "").Replace("<quot>", "\""), "'") : e;
            }).ToArray();
            
            int strSize = content[5].Length;
            using var ms = new MemoryStream();
            await using var writer = new BinaryWriter(ms);

            writer.Write(strSize);
            writer.Write(Convert.ToInt32(content[0]));
            writer.Write(Convert.ToInt32(content[1]));
            writer.Write(Convert.ToInt16(content[2]));
            writer.Write(Convert.ToByte(content[3]));
            writer.Write(Convert.ToByte(content[4]));
            writer.Write(Encoding.Unicode.GetBytes(content[5]));

            chunks.Add(ms.ToArray());
        }

        return chunks.SelectMany(chunk => chunk).ToArray();
    }

    private static string Decrypt(byte[] buffer)
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
}