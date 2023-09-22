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
        try
        {
            var dataDecompress = await InflateAsync(bytes);
            var dataDecrypt = Decrypt(dataDecompress);
            return dataDecrypt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static async Task<byte[]> Compress(string content)
    {
        try
        {
            var dataEncrypt = Encrypt(content);
            var dataCompress = await Deflate(dataEncrypt);
            return dataCompress;
        }
        catch (Exception ex)
        {
            throw ex;
        }
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
        using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true);
        await deflateStream.WriteAsync(buffer, 0, buffer.Length);
        return memoryStream.ToArray();
    }
    
    private static byte[] Encrypt(string fileContent)
    {
        var lines = new List<string>();

        fileContent = TrimStart(fileContent, Utf16LeBom);

        foreach (var line in fileContent.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split('\t');
            var content = new List<object>();

            for (var i = 0; i < parts.Length; i++)
            {
                if (i > 4)
                {
                    content.Add(
                        TrimStart(parts[i].Replace("<lf>", "\n").Replace("\"", "").Replace("<quot>", "\""), "'"));
                }
                else
                {
                    content.Add(Convert.ToInt16((parts[i])));
                }
            }

            var strSize = Encoding.Unicode.GetByteCount((string)content[5]);
            var size = 4 + 4 + 4 + 2 + 1 + 1 + (strSize * 2) + 4;
            var buffer = new byte[size];
            var index = 0;

            BitConverter.GetBytes(Convert.ToInt32(strSize)).CopyTo(buffer, index);
            index += 4;
            BitConverter.GetBytes(Convert.ToInt32(content[0])).CopyTo(buffer, index);
            index += 4;
            BitConverter.GetBytes(Convert.ToInt32(content[1])).CopyTo(buffer, index);
            index += 4;
            BitConverter.GetBytes((ushort)content[2]).CopyTo(buffer, index);
            index += 2;
            buffer[index] = (byte)content[3];
            index += 1;
            buffer[index] = (byte)content[4];
            index += 1;
            Encoding.Unicode.GetBytes((string)content[5]).CopyTo(buffer, index);
            index += strSize * 2;
            BitConverter.GetBytes(strSize).CopyTo(buffer, index);

            lines.Add(Encoding.Unicode.GetString(buffer));
        }

        var result = string.Join("\n", lines);
        return Encoding.Unicode.GetBytes(Utf16LeBom + result);
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
            var str = AddSingleQuote(Encoding.Unicode.GetString(strBytes).Replace("\n", "<lf>")
                .Replace("\"", "<quot>"));
            index += 4;

            result.Add($"{strType}\t{strID1}\t{strID2}\t{strID3}\t{strID4}\t{str}");
        }

        return Utf16LeBom + string.Join("\n", result);
    }
}